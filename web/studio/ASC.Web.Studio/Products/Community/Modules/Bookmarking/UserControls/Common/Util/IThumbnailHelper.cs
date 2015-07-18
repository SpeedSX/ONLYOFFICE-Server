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


using ASC.Web.UserControls.Bookmarking.Common.Util;
using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace ASC.Web.UserControls.Bookmarking.Util
{
    public interface IThumbnailHelper
    {
        void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID);
        string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size);
        string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size);
        void DeleteThumbnail(string Url);
    }

    public class ThumbnailHelper
    {
        private static IThumbnailHelper _processHelper = new WebSiteThumbnailHelper();
        private static IThumbnailHelper _serviceHelper = new ServiceThumbnailHelper();

        public static bool HasService
        {
            get { return ConfigurationManager.AppSettings["bookmarking.thumbnail-url"] != null; }
        }

        public static IThumbnailHelper Instance
        {
            get
            {
                return HasService ? _serviceHelper : _processHelper;
            }
        }
    }

    internal class ServiceThumbnailHelper : IThumbnailHelper
    {
        private string ServiceFormatUrl
        {
            get { return ConfigurationManager.AppSettings["bookmarking.thumbnail-url"]; }
        }

        public void MakeThumbnail(string url, bool async, bool notOverride, HttpContext context, int tenantID)
        {

        }

        public string GetThumbnailUrl(string Url, BookmarkingThumbnailSize size)
        {
            var sizeValue = string.Format("{0}x{1}", size.Width, size.Height);
            return string.Format(ServiceFormatUrl, Url, sizeValue, Url.GetHashCode());
        }

        public string GetThumbnailUrlForUpdate(string Url, BookmarkingThumbnailSize size)
        {
            var url = GetThumbnailUrl(Url, size);
            try
            {
                var req = WebRequest.Create(url);
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        return url;
                    }
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public void DeleteThumbnail(string Url)
        {

        }
    }
}