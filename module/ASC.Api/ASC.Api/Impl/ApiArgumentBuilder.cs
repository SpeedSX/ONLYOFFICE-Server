/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Routing;
using System.Xml.Linq;

namespace ASC.Api.Impl
{
    public class ApiArgumentBuilder : IApiArgumentBuilder
    {
        public IEnumerable<object> BuildCallingArguments(RequestContext context, IApiMethodCall methodToCall)
        {
            var callArg = new List<object>();
            var requestParams = GetRequestParams(context);

            var methodParams = methodToCall.GetParams().Where(x => !x.IsRetval).OrderBy(x => x.Position);


            foreach (var parameterInfo in methodParams)
            {
                if (requestParams[parameterInfo.Name] != null)
                {
                    //convert
                    var values = requestParams.GetValues(parameterInfo.Name);
                    if (values != null && values.Any())
                    {
                        try
                        {
                            callArg.Add(ConvertUtils.GetConverted(values.First(), parameterInfo));//NOTE; Get first value!
                        }
                        catch (ApiArgumentMismatchException)
                        {
                            //Failed to convert. Try bind
                            callArg.Add(Utils.Binder.Bind(parameterInfo.ParameterType, requestParams, parameterInfo.Name));
                        }
                    }
                }
                else
                {
                    //try get request param first. It may be form\url-encoded
                    if (!"GET".Equals(context.HttpContext.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(context.HttpContext.Request[parameterInfo.Name]))
                    {
                        //Drop to
                        callArg.Add(ConvertUtils.GetConverted(context.HttpContext.Request[parameterInfo.Name], parameterInfo));
                    }
                    else if (parameterInfo.ParameterType == typeof(ContentType) && !string.IsNullOrEmpty(context.HttpContext.Request.ContentType))
                    {
                        callArg.Add(new ContentType(context.HttpContext.Request.ContentType));
                    }
                    else if (parameterInfo.ParameterType == typeof(ContentDisposition) && !string.IsNullOrEmpty(context.HttpContext.Request.Headers["Content-Disposition"]))
                    {
                        var disposition = new ContentDisposition(context.HttpContext.Request.Headers["Content-Disposition"]);
                        disposition.FileName = HttpUtility.UrlDecode(disposition.FileName);//Decode uri name
                        callArg.Add(disposition);
                    }
                    else if (parameterInfo.ParameterType.IsSubclassOf(typeof(HttpPostedFile)) && context.HttpContext.Request.Files[parameterInfo.Name] != null)
                    {
                        callArg.Add(context.HttpContext.Request.Files[parameterInfo.Name]);
                    }
                    else if (Utils.Binder.IsCollection(parameterInfo.ParameterType) && parameterInfo.ParameterType.IsGenericType && parameterInfo.ParameterType.GetGenericArguments().First() == typeof(HttpPostedFileBase))
                    {
                        //File catcher
                        var files = new List<HttpPostedFileBase>(context.HttpContext.Request.Files.Count);
                        files.AddRange(from string key in context.HttpContext.Request.Files select context.HttpContext.Request.Files[key]);
                        callArg.Add(files);
                    }
                    else
                    {
                        if (parameterInfo.ParameterType.IsSubclassOf(typeof(Stream)) || parameterInfo.ParameterType == typeof(Stream))
                        {
                            //First try get files
                            var file = context.HttpContext.Request.Files[parameterInfo.Name];
                            callArg.Add(file != null ? file.InputStream : context.HttpContext.Request.InputStream);
                        }
                        else
                        {
                            //Try bind
                            //Note: binding moved here
                            if (IsTypeBindable(parameterInfo.ParameterType))
                            {
                                //Custom type
                                var binded = Utils.Binder.Bind(parameterInfo.ParameterType,
                                    requestParams,
                                    parameterInfo.Name);

                                if (binded != null)
                                {
                                    callArg.Add(binded);
                                    continue;//Go to next loop
                                }
                            }
                            //Create null
                            var obj = parameterInfo.ParameterType.IsValueType ? Activator.CreateInstance(parameterInfo.ParameterType) : null;
                            callArg.Add(obj);
                        }


                    }
                }
            }
            return callArg;
        }

        private static bool IsTypeBindable(Type parameterInfo)
        {
            if (Binder.IsCollection(parameterInfo))
            {
                return true;
            }
            return parameterInfo.Namespace != null && !parameterInfo.Namespace.StartsWith("System", StringComparison.Ordinal);
        }

        private static NameValueCollection GetRequestParams(RequestContext context)
        {
            var requestType = string.IsNullOrEmpty(context.HttpContext.Request.ContentType) ? new ContentType("text/plain") : new ContentType(context.HttpContext.Request.ContentType);
            var collection = context.RouteData.Values.ToNameValueCollection();

            switch (requestType.MediaType)
            {
                case Constants.XmlContentType:
                    {
                        using (var reader = new StreamReader(context.HttpContext.Request.InputStream))
                        {
                            FillCollectionFromXElement(XDocument.Load(reader).Root.Elements(), string.Empty, collection);
                        }
                    }
                    break;
                case Constants.JsonContentType:
                    {
                        using (var reader = new StreamReader(context.HttpContext.Request.InputStream))
                        {
                            var xdoc = JsonConvert.DeserializeXNode(reader.ReadToEnd(),"request",false);
                            FillCollectionFromXElement(xdoc.Root.Elements(), string.Empty,collection);
                        }
                    }
                    break;
                default:
                    collection.Add(context.HttpContext.Request.QueryString);
                    if (!"GET".Equals(context.HttpContext.Request.HttpMethod, StringComparison.InvariantCultureIgnoreCase))
                    {
                        collection.Add(context.HttpContext.Request.Form);
                    }
                    break;
            }
            return collection;
        }

        private static void FillCollectionFromXElement(IEnumerable<XElement> elements, string prefix, NameValueCollection collection)
        {
            foreach (var grouping in elements.GroupBy(x => x.Name))
            {
                if (grouping.Count() < 2)
                {
                    //Single element
                    var element = grouping.SingleOrDefault();
                    if (element != null)
                    {
                        if (!element.HasElements)
                        {
                            //Last one in tree
                            AddElement(prefix, collection, element);
                        }
                        else
                        {
                            FillCollectionFromXElement(element.Elements(), prefix + "." + element.Name.LocalName, collection);
                        }
                    }

                }
                else
                {
                    //Grouping has more than one
                    if (grouping.All(x => !x.HasElements))
                    {
                        //Simple collection
                        foreach (XElement element in grouping)
                        {
                            AddElement(prefix, collection, element);
                        }
                    }
                    else
                    {
                        var groupList = grouping.ToList();
                        for (int i = 0; i < groupList.Count; i++)
                        {
                            FillCollectionFromXElement(groupList[i].Elements(), prefix + "." + grouping.Key + "[" + i + "]", collection);
                        }
                    }
                }
            }
        }

        private static void AddElement(string prefix, NameValueCollection collection, XElement element)
        {
            if (string.IsNullOrEmpty(prefix))
                collection.Add(element.Name.LocalName, element.Value);
            else
            {
                var prefixes = prefix.TrimStart('.').Split('.');
                string additional = string.Empty;
                if (prefixes.Length > 1)
                {
                    additional = string.Join("", prefix.Skip(1).Select(x => "[" + x + "]").ToArray());
                }
                collection.Add(prefixes[0] + additional + "[" + element.Name.LocalName + "]", element.Value);
            }
        }
    }
}