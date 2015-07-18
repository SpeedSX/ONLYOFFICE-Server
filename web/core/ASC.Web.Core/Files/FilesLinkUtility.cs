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
using System.Web;
using System.Web.Configuration;
using ASC.Core;
using ASC.Security.Cryptography;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Core.Files
{
    public static class FilesLinkUtility
    {
        public const string FilesBaseVirtualPath = "~/products/files/";
        public const string EditorPage = "doceditor.aspx";

        public static string FilesBaseAbsolutePath
        {
            get { return CommonLinkUtility.ToAbsolute(FilesBaseVirtualPath); }
        }

        public const string FileId = "fileid";
        public const string FolderId = "folderid";
        public const string Version = "version";
        public const string FileUri = "fileuri";
        public const string FileTitle = "title";
        public const string Action = "action";
        public const string DocShareKey = "doc";
        public const string TryParam = "try";
        public const string FolderUrl = "folderurl";
        public const string OutType = "outputtype";
        public const string AuthKey = "stream_auth";

        public static string FileHandlerPath
        {
            get { return FilesBaseAbsolutePath + "httphandlers/filehandler.ashx"; }
        }

        public static string DocServiceApiUrl
        {
            get { return GetUrlSetting("api"); }
            set { SetUrlSetting("api", value); }
        }

        public static string DocServiceConverterUrl
        {
            get { return GetUrlSetting("converter"); }
            set { SetUrlSetting("converter", value); }
        }

        public static string DocServiceStorageUrl
        {
            get { return GetUrlSetting("storage"); }
            set { SetUrlSetting("storage", value); }
        }

        public static string DocServiceCommandUrl
        {
            get { return GetUrlSetting("command"); }
            set { SetUrlSetting("command", value); }
        }

        public static string FileViewUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=view&" + FileId + "={0}"; }
        }

        public static string GetFileViewUrl(object fileId)
        {
            return GetFileViewUrl(fileId, 0);
        }

        public static string GetFileViewUrl(object fileId, int fileVersion)
        {
            return string.Format(FileViewUrlString, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? string.Empty : "&" + Version + "=" + fileVersion);
        }

        public static string FileDownloadUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=download&" + FileId + "={0}"; }
        }

        public static string GetFileDownloadUrl(object fileId)
        {
            return GetFileDownloadUrl(fileId, 0, string.Empty);
        }

        public static string GetFileDownloadUrl(object fileId, int fileVersion, string convertToExtension)
        {
            return string.Format(FileDownloadUrlString, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? "&" + Version + "=" + fileVersion : string.Empty)
                   + (string.IsNullOrEmpty(convertToExtension) ? string.Empty : "&" + OutType + "=" + convertToExtension);
        }

        public static string GetFileWebImageViewUrl(object fileId)
        {
            return FilesBaseAbsolutePath + "#preview/" + HttpUtility.UrlEncode(fileId.ToString());
        }

        public static string FileWebViewerUrlString
        {
            get { return FileWebEditorUrlString + "&" + Action + "=view"; }
        }

        public static string GetFileWebViewerUrlForMobile(object fileId, int fileVersion)
        {
            var viewerUrl = CommonLinkUtility.ToAbsolute("~/../products/files/") + EditorPage + "?" + FileId + "={0}";

            return string.Format(viewerUrl, HttpUtility.UrlEncode(fileId.ToString()))
                   + (fileVersion > 0 ? "&" + Version + "=" + fileVersion : string.Empty);
        }

        public static string FileWebViewerExternalUrlString
        {
            get { return FilesBaseAbsolutePath + EditorPage + "?" + FileUri + "={0}&" + FileTitle + "={1}&" + FolderUrl + "={2}"; }
        }

        public static string GetFileWebViewerExternalUrl(string fileUri, string fileTitle, string refererUrl)
        {
            return string.Format(FileWebViewerExternalUrlString, HttpUtility.UrlEncode(fileUri), HttpUtility.UrlEncode(fileTitle), HttpUtility.UrlEncode(refererUrl));
        }

        public static string FileWebEditorUrlString
        {
            get { return FilesBaseAbsolutePath + EditorPage + "?" + FileId + "={0}"; }
        }

        public static string GetFileWebEditorUrl(object fileId)
        {
            return string.Format(FileWebEditorUrlString, HttpUtility.UrlEncode(fileId.ToString()));
        }

        public static string GetFileWebEditorTryUrl(FileType fileType)
        {
            return FilesBaseAbsolutePath + EditorPage + "?" + TryParam + "=" + fileType;
        }

        public static string FileWebEditorExternalUrlString
        {
            get { return FileHandlerPath + "?" + Action + "=create&" + FileUri + "={0}&" + FileTitle + "={1}"; }
        }

        public static string GetFileWebEditorExternalUrl(string fileUri, string fileTitle)
        {
            return GetFileWebEditorExternalUrl(fileUri, fileTitle, false);
        }

        public static string GetFileWebEditorExternalUrl(string fileUri, string fileTitle, bool openFolder)
        {
            var url = string.Format(FileWebEditorExternalUrlString, HttpUtility.UrlEncode(fileUri), HttpUtility.UrlEncode(fileTitle));
            if (openFolder)
                url += "&openfolder=true";
            return url;
        }

        public static string GetFileWebPreviewUrl(string fileTitle, object fileId)
        {
            if (FileUtility.CanImageView(fileTitle))
                return GetFileWebImageViewUrl(fileId);

            if (FileUtility.CanWebView(fileTitle))
            {
                if (FileUtility.ExtsMustConvert.Contains(FileUtility.GetFileExtension(fileTitle)))
                    return string.Format(FileWebViewerUrlString, HttpUtility.UrlEncode(fileId.ToString()));
                return GetFileWebEditorUrl(fileId);
            }

            return GetFileViewUrl(fileId);
        }

        public static string GetFileRedirectPreviewUrl(object enrtyId, bool isFile)
        {
            return FileHandlerPath + "?" + Action + "=redirect&" + (isFile ? FileId : FolderId) + "=" + enrtyId;
        }

        public static string GetInitiateUploadSessionUrl(object folderId, object fileId, string fileName, long contentLength)
        {
            var queryString = string.Format("?initiate=true&name={0}&fileSize={1}&tid={2}&userid={3}",
                                            fileName, contentLength, TenantProvider.CurrentTenantID,
                                            HttpUtility.UrlEncode(InstanceCrypto.Encrypt(SecurityContext.CurrentAccount.ID.ToString())));

            if (fileId != null)
                queryString = queryString + "&fileid=" + fileId;

            if (folderId != null)
                queryString = queryString + "&folderid=" + folderId;

            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath(contentLength > 0) + queryString);
        }

        public static string GetUploadChunkLocationUrl(string uploadId, bool serviceUrl)
        {
            var queryString = "?uid=" + uploadId;
            return CommonLinkUtility.GetFullAbsolutePath(GetFileUploaderHandlerVirtualPath(serviceUrl) + queryString);
        }


        private static string GetFileUploaderHandlerVirtualPath(bool getServiceUrl)
        {
            string virtualPath = getServiceUrl
                                     ? (WebConfigurationManager.AppSettings["files.uploader.url"] ?? "~")
                                     : (WebConfigurationManager.AppSettings["files.uploader.url.local"] ?? "~/products/files");

            return virtualPath.EndsWith(".ashx") ? virtualPath : virtualPath.TrimEnd('/') + "/ChunkedUploader.ashx";
        }


        private static string GetUrlSetting(string key, string appSettingsKey = null)
        {
            var value = string.Empty;
            if (CoreContext.Configuration.Standalone)
            {
                value = CoreContext.Configuration.GetSetting(GetSettingsKey(key));
            }
            if (string.IsNullOrEmpty(value))
            {
                value = WebConfigurationManager.AppSettings["files.docservice.url." + (appSettingsKey ?? key)];
            }
            return value;
        }

        private static void SetUrlSetting(string key, string value)
        {
            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException("Method for server edition only.");
            }
            if (GetUrlSetting(key) != value)
                CoreContext.Configuration.SaveSetting(GetSettingsKey(key), value);
        }

        private static string GetSettingsKey(string key)
        {
            return "DocKey_" + key;
        }
    }
}