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


using ASC.Collections;
using ASC.Core.Tenants;
using ASC.Files.Core;
using ASC.Web.Files.Classes;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using File = Microsoft.SharePoint.Client.File;
using Folder = Microsoft.SharePoint.Client.Folder;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Files.Thirdparty.SharePoint
{
    public class SharePointProviderInfo : IProviderInfo, IDisposable
    {
        private static readonly CachedDictionary<File> FileCache = new CachedDictionary<File>("shpoint-files", x => true);

        private static readonly CachedDictionary<Folder> FolderCache = new CachedDictionary<Folder>("shpoint-folders", x => true);

        private ClientContext clientContext;

        public int ID { get; set; }
        public string ProviderKey { get; private set; }
        public Guid Owner { get; private set; }
        public FolderType RootFolderType { get; private set; }
        public DateTime CreateOn { get; private set; }
        public string CustomerTitle { get; private set; }
        public object RootFolderId { get; private set; }

        public string SpRootFolderId = "/Shared Documents";

        public SharePointProviderInfo(int id, string providerKey, string customerTitle, AuthData authData, Guid owner,
                                      FolderType rootFolderType, DateTime createOn)
        {
            if (string.IsNullOrEmpty(providerKey))
                throw new ArgumentNullException("providerKey");
            if (!string.IsNullOrEmpty(authData.Login) && string.IsNullOrEmpty(authData.Password))
                throw new ArgumentNullException("password", "Password can't be null");

            ID = id;
            ProviderKey = providerKey;
            CustomerTitle = customerTitle;
            Owner = owner == Guid.Empty ? SecurityContext.CurrentAccount.ID : owner;
            RootFolderType = rootFolderType;
            CreateOn = createOn;
            RootFolderId = MakeId();

            InitClientContext(authData);
        }


        public bool CheckAccess()
        {
            try
            {
                clientContext.Load(clientContext.Web);
                clientContext.ExecuteQuery();
                return true;
            }
            catch (Exception e)
            {
                Global.Logger.Error("CheckAccess", e);
                return false;
            }
        }

        public void InvalidateStorage()
        {
            clientContext.Dispose();
            FolderCache.Clear();
            FileCache.Clear();
        }

        internal void UpdateTitle(string newtitle)
        {
            CustomerTitle = newtitle;
        }

        private void InitClientContext(AuthData authData)
        {
            var authUrl = authData.Url;
            ICredentials credentials = new NetworkCredential(authData.Login, authData.Password);

            if (authData.Login.EndsWith("onmicrosoft.com"))
            {
                var personalPath = string.Concat("/personal/", authData.Login.Replace("@", "_").Replace(".", "_").ToLower());
                SpRootFolderId = string.Concat(personalPath, "/Documents");

                var ss = new SecureString();
                foreach (var p in authData.Password)
                {
                    ss.AppendChar(p);
                }
                authUrl = string.Concat(authData.Url.TrimEnd('/'), personalPath);
                credentials = new SharePointOnlineCredentials(authData.Login, ss);

            }

            clientContext = new ClientContext(authUrl)
            {
                AuthenticationMode = ClientAuthenticationMode.Default,
                Credentials = credentials
            };
        }

        #region Files

        public File GetFileById(object id)
        {
            return FileCache.Get(MakeId(id), () =>
                {
                    var file = clientContext.Web.GetFileByServerRelativeUrl((string)id);
                    clientContext.Load(file);
                    clientContext.Load(file.ListItemAllFields);
                    
                    try
                    {
                        clientContext.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        FolderCache.Reset(MakeId(GetParentFolderId(id)));
                        return new SharePointFileErrorEntry(file.Context, file.Path) { Error = e.Message, ID = id };
                    }

                    return file;
                });
        }

        public Stream GetFileStream(object id)
        {
            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return null;
            var fileInfo = File.OpenBinaryDirect(clientContext, (string)id);
            clientContext.ExecuteQuery();
            return fileInfo.Stream;
        }

        public File CreateFile(string id, Stream stream)
        {
            byte[] b;

            using (var br = new BinaryReader(stream))
            {
                b = br.ReadBytes((int)stream.Length);
            }

            var file = clientContext.Web.RootFolder.Files.Add(new FileCreationInformation { Content = b, Url = id, Overwrite = true });
            clientContext.Load(file);
            clientContext.Load(file.ListItemAllFields);
            clientContext.ExecuteQuery();

            FileCache.Add(MakeId(id), file);
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            return file;
        }

        public void DeleteFile(string id)
        {
            FileCache.Reset(MakeId(id));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return;

            file.DeleteObject();
            clientContext.ExecuteQuery();
        }

        public object RenameFile(string id, string newTitle)
        {            
            FileCache.Reset(MakeId(id));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return MakeId();

            var newUrl = GetParentFolderId(file.ServerRelativeUrl) + "/" + newTitle;
            file.MoveTo(newUrl, MoveOperations.Overwrite);
            clientContext.ExecuteQuery();

            return MakeId(newUrl);
        }

        public object MoveFile(object id, object toFolderId)
        {
            FileCache.Reset(MakeId(id));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));
            FolderCache.Reset(MakeId(toFolderId));

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return MakeId();

            var newUrl = toFolderId + "/" + file.Name;
            file.MoveTo(newUrl, MoveOperations.Overwrite);
            clientContext.ExecuteQuery();

            return MakeId(newUrl);
        }

        public File CopyFile(object id, object toFolderId)
        {
            FolderCache.Reset(MakeId(toFolderId));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var file = GetFileById(id);

            if (file is SharePointFileErrorEntry) return file;

            var newUrl = toFolderId + "/" + file.Name;
            file.CopyTo(newUrl, false);
            clientContext.ExecuteQuery();

            return file;
        }

        public Core.File ToFile(File file)
        {
            if (file == null)
                return null;

            var errorFile = file as SharePointFileErrorEntry;
            if (errorFile != null)
                return new Core.File
                {
                    ID = MakeId(errorFile.ID),
                    FolderID = MakeId(GetParentFolderId(errorFile.ID)),
                    CreateBy = Owner,
                    CreateOn = DateTime.UtcNow,
                    ModifiedBy = Owner,
                    ModifiedOn = DateTime.UtcNow,
                    ProviderId = ID,
                    ProviderKey = ProviderKey,
                    RootFolderCreator = Owner,
                    RootFolderId = MakeId(RootFolder.ServerRelativeUrl),
                    RootFolderType = RootFolderType,
                    Title = MakeTitle(GetTitleById(errorFile.ID)),
                    Error = errorFile.Error
                };

            var result = new Core.File
            {
                ID = MakeId(file.ServerRelativeUrl),
                Access = Core.Security.FileShare.None,
                //ContentLength = file.Length,
                CreateBy = Owner,
                CreateOn = file.TimeCreated.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(file.TimeCreated) : file.TimeCreated,
                FileStatus = FileStatus.None,
                FolderID = MakeId(GetParentFolderId(file.ServerRelativeUrl)),
                ModifiedBy = Owner,
                ModifiedOn = file.TimeLastModified.Kind == DateTimeKind.Utc ? TenantUtil.DateTimeFromUtc(file.TimeLastModified) : file.TimeLastModified,
                NativeAccessor = file,
                ProviderId = ID,
                ProviderKey = ProviderKey,
                Title = MakeTitle(file.Name),
                RootFolderId = MakeId(SpRootFolderId),
                RootFolderType = RootFolderType,
                RootFolderCreator = Owner,
                SharedByMe = false,
                Version = 1
            };

            if (file.IsPropertyAvailable("Length"))
            {
                result.ContentLength = file.Length;
            }
            else if(file.IsObjectPropertyInstantiated("ListItemAllFields"))
            {
                result.ContentLength = Convert.ToInt64(file.ListItemAllFields["File_x0020_Size"]);
            }

            return result;
        }

        #endregion

        #region Folders

        public Folder RootFolder
        {
            get
            {
                return FolderCache.Get(MakeId(), () => GetFolderById(SpRootFolderId));
            }
        }

        public Folder GetFolderById(object id)
        {   
            return FolderCache.Get(MakeId(id), () =>
                {
                    if ((string)id == "") id = SpRootFolderId;
                    var folder = clientContext.Web.GetFolderByServerRelativeUrl((string)id);
                    clientContext.Load(folder);
                    clientContext.Load(folder.Files, collection => collection.IncludeWithDefaultProperties(r => r.ListItemAllFields));
                    clientContext.Load(folder.Folders);

                    try
                    {
                        clientContext.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        FolderCache.Reset(MakeId(GetParentFolderId(id)));
                        return new SharePointFolderErrorEntry(folder.Context, folder.Path) { Error = e.Message, ID = id };
                    }

                    return folder;
                });
        }

        public Folder GetParentFolder(string serverRelativeUrl)
        {
            return GetFolderById(GetParentFolderId(serverRelativeUrl));
        }

        public IEnumerable<File> GetFolderFiles(object id)
        {
            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return new List<File>();

            return GetFolderById(id).Files;
        }

        public IEnumerable<Folder> GetFolderFolders(object id)
        {
            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return new List<Folder>();

            return folder.Folders.ToList().Where(r => r.ServerRelativeUrl != SpRootFolderId + "/" + "Forms");
        }

        public object RenameFolder(object id, string newTitle)
        {
            FolderCache.Reset(MakeId(id));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return MakeId(id);

            return MakeId(MoveFld(folder, GetParentFolderId(id) + "/" + newTitle).ServerRelativeUrl);
        }

        public object MoveFolder(object id, object toFolderId)
        {
            FolderCache.Reset(MakeId(id));
            FolderCache.Reset(MakeId(GetParentFolderId(id)));
            FolderCache.Reset(MakeId(toFolderId));

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return MakeId(id);

            return MakeId(MoveFld(folder, toFolderId + "/" + GetFolderById(id).Name).ServerRelativeUrl);
        }

        public Folder CopyFolder(object id, object toFolderId)
        {
            FolderCache.Reset(MakeId(toFolderId));

            var folder = GetFolderById(id);
            if (folder is SharePointFolderErrorEntry) return folder;

            return MoveFld(folder, toFolderId + "/" + GetFolderById(id).Name, false);
        }

        private Folder MoveFld(Folder folder, string newUrl, bool delete = true)
        {
            var newFolder = CreateFolder(newUrl);

            if (delete)
            {
                folder.Folders.ToList().ForEach(r => MoveFolder(r.ServerRelativeUrl, newUrl));
                folder.Files.ToList().ForEach(r => MoveFile(r.ServerRelativeUrl, newUrl));

                folder.DeleteObject();
                clientContext.ExecuteQuery();
            }
            else
            {
                folder.Folders.ToList().ForEach(r => CopyFolder(r.ServerRelativeUrl, newUrl));
                folder.Files.ToList().ForEach(r => CopyFile(r.ServerRelativeUrl, newUrl));
            }

            return newFolder;
        }

        public Folder CreateFolder(string id)
        {
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var folder = clientContext.Web.RootFolder.Folders.Add(id);
            clientContext.Load(folder);
            clientContext.ExecuteQuery();

            FolderCache.Add(id, folder);

            return folder;
        }

        public void DeleteFolder(string id)
        {
            FolderCache.Reset(id);
            FolderCache.Reset(MakeId(GetParentFolderId(id)));

            var folder = GetFolderById(id);

            if (folder is SharePointFolderErrorEntry) return;

            folder.DeleteObject();
            clientContext.ExecuteQuery();
        }

        public Core.Folder ToFolder(Folder folder)
        {
            if (folder == null) return null;

            var errorFolder = folder as SharePointFolderErrorEntry;
            if (errorFolder != null)
                return new Core.Folder
                {
                    ID = MakeId(errorFolder.ID),
                    ParentFolderID = null,
                    CreateBy = Owner,
                    CreateOn = DateTime.UtcNow,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = Owner,
                    ModifiedOn = DateTime.UtcNow,
                    ProviderId = ID,
                    ProviderKey = ProviderKey,
                    RootFolderCreator = Owner,
                    RootFolderId = MakeId(SpRootFolderId),
                    RootFolderType = RootFolderType,
                    Shareable = false,
                    Title = MakeTitle(GetTitleById(errorFolder.ID)),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                    Error = errorFolder.Error
                };

            var isRoot = folder.ServerRelativeUrl == SpRootFolderId;
            return new Core.Folder
                {
                    ID = MakeId(isRoot ? "" : folder.ServerRelativeUrl),
                    ParentFolderID = isRoot ? null : MakeId(GetParentFolderId(folder.ServerRelativeUrl)),
                    CreateBy = Owner,
                    CreateOn = CreateOn,
                    FolderType = FolderType.DEFAULT,
                    ModifiedBy = Owner,
                    ModifiedOn = CreateOn,
                    ProviderId = ID,
                    ProviderKey = ProviderKey,
                    RootFolderCreator = Owner,
                    RootFolderId = MakeId(RootFolder.ServerRelativeUrl),
                    RootFolderType = RootFolderType,
                    Shareable = false,
                    Title = isRoot ? CustomerTitle : MakeTitle(folder.Name),
                    TotalFiles = 0,
                    TotalSubFolders = 0,
                };
        }

        #endregion

        public string MakeId(string path = "")
        {
            path = path.Replace(SpRootFolderId, "");
            return string.Format("{0}{1}", "spoint-" + ID, string.IsNullOrEmpty(path) || path == "/" || path == SpRootFolderId ? "" : ("-" + path.Replace('/', '|')));
        }

        private string MakeId(object path)
        {
            return MakeId((string) path);
        }

        protected string MakeTitle(string name)
        {
            return Web.Files.Classes.Global.ReplaceInvalidCharsAndTruncate(name);
        }

        protected string GetParentFolderId(string serverRelativeUrl)
        {
            var path = serverRelativeUrl.Split('/');

            return string.Join("/", path.Take(path.Length - 1));
        }

        protected string GetParentFolderId(object serverRelativeUrl)
        {
            return GetParentFolderId((string)serverRelativeUrl);
        }

        protected string GetTitleById(object serverRelativeUrl)
        {
            return ((string)serverRelativeUrl).Split('/').Last();
        }


        public void Dispose()
        {
            clientContext.Dispose();
        }
    }
}
