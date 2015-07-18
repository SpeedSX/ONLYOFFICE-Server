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


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.UI;

namespace ASC.Thrdparty.Web
{
    public class BaseImportPage : Page
    {
        private readonly List<ContactInfo> _contacts = new List<ContactInfo>();

        public static string EncodeJsString(string s)
        {
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        var i = (int) c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }

        private const string CallbackJavascript =
            @"function snd(){{client.sendAndClose({0},{1});}} window.onload = snd;";

        protected void AddContactInfo(string name, IEnumerable<string> emails)
        {
            var lastname = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                if (name.Contains(' '))
                {
                    lastname = name.Substring(name.IndexOf(' ') + 1);
                    name = name.Substring(0, name.IndexOf(' '));
                }
            }

            AddContactInfo(name, lastname, emails);
        }

        protected void AddContactInfo(string name, string lastname, IEnumerable<string> emails)
        {
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(lastname))
            {
                var _name = emails.FirstOrDefault().Contains("@") ? emails.FirstOrDefault().Substring(0, emails.FirstOrDefault().IndexOf("@")).Split('.') : emails.FirstOrDefault().Split('.');
                if (_name.Length > 1)
                {
                    name = _name[0];
                    lastname = _name[1];
                }
            }

            var info = new ContactInfo
                {
                    FirstName = String.IsNullOrEmpty(name) ? String.Empty : name,
                    Email = String.IsNullOrEmpty(emails.FirstOrDefault()) ? String.Empty : emails.FirstOrDefault(),
                    LastName = String.IsNullOrEmpty(lastname) ? String.Empty : lastname
                };

            if (!string.IsNullOrEmpty(info.Email))
            {
                _contacts.Add(info);
            }
        }

        protected void SubmitData(string data, string errorMessage = null)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                   string.Format(CallbackJavascript,
                                                                 string.IsNullOrEmpty(data) ? "null" : data,
                                                                 string.IsNullOrEmpty(errorMessage) ? "null" : EncodeJsString(errorMessage)),
                                                   true);
        }

        protected void SubmitContacts()
        {
            SubmitData(ToJson(_contacts.Distinct().ToList()));
        }

        protected void SubmitEmailInfo(EmailAccessInfo emailInfo)
        {
            SubmitData(ToJson(emailInfo));
        }

        protected void SubmitError(string message)
        {
            SubmitData(string.Empty, message);
        }

        protected static string ToJson(object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int) ms.Length);
            }
        }
    }
}