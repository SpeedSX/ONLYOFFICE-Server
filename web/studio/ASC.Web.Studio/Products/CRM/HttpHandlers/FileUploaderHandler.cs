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
using System.Runtime.Remoting.Messaging;
using System.Web;
using ASC.Files.Core;
using ASC.Web.Studio.Controls.FileUploader;
using ASC.Web.Studio.Controls.FileUploader.HttpModule;
using ASC.Web.Studio.Core;
using ASC.Web.CRM.Resources;

namespace ASC.Web.CRM.Classes
{
    public class FileUploaderHandler : FileUploadHandler
    {
        public override FileUploadResult ProcessUpload(HttpContext context)
        {
            var fileUploadResult = new FileUploadResult();

            if (!FileToUpload.HasFilesToUpload(context)) return fileUploadResult;

            var file = new FileToUpload(context);

            if (String.IsNullOrEmpty(file.FileName) || file.ContentLength == 0)
                throw new InvalidOperationException(CRMErrorsResource.InvalidFile);

            if (0 < SetupInfo.MaxUploadSize && SetupInfo.MaxUploadSize < file.ContentLength)
                throw FileSizeComment.FileSizeException;

            if (CallContext.GetData("CURRENT_ACCOUNT") == null)
                CallContext.SetData("CURRENT_ACCOUNT", new Guid(context.Request["UserID"]));


            var fileName = file.FileName.LastIndexOf('\\') != -1
                               ? file.FileName.Substring(file.FileName.LastIndexOf('\\') + 1)
                               : file.FileName;

            var document = new File
                {
                    Title = fileName,
                    FolderID = Global.DaoFactory.GetFileDao().GetRoot(),
                    ContentLength = file.ContentLength
                };

            document = Global.DaoFactory.GetFileDao().SaveFile(document, file.InputStream);

            fileUploadResult.Data = document.ID;
            fileUploadResult.FileName = document.Title;
            fileUploadResult.FileURL = document.FileDownloadUrl;


            fileUploadResult.Success = true;


            return fileUploadResult;
        }
    }
}