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
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage.S3;
using ASC.FullTextIndex;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Core;
using FileShare = System.IO.FileShare;

namespace ASC.Files.Core.Data
{
    internal class FileDao : AbstractDao, IFileDao
    {
        private static readonly object syncRoot = new object();

        public FileDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {

        }

        private static Exp BuildLike(string[] columns, string[] keywords)
        {
            var like = Exp.Empty;
            foreach (var keyword in keywords)
            {
                var keywordLike = Exp.Empty;
                foreach (var column in columns)
                {
                    keywordLike |= Exp.Like(column, keyword, SqlLike.StartWith) | Exp.Like(column, ' ' + keyword);
                }
                like &= keywordLike;
            }
            return like;
        }

        public void InvalidateCache(object fileId)
        {

        }

        public File GetFile(object fileId)
        {
            using (var dbManager = GetDb())
            {
                return dbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("current_version", true)))
                    .ConvertAll(ToFile)
                    .SingleOrDefault();
            }
        }

        public File GetFile(object fileId, int fileVersion)
        {
            using (var dbManager = GetDb())
            {
                return dbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId) & Exp.Eq("version", fileVersion)))
                    .ConvertAll(ToFile)
                    .SingleOrDefault();
            }
        }

        public File GetFile(object parentId, String title)
        {
            if (String.IsNullOrEmpty(title)) throw new ArgumentNullException(title);
            using (var dbManager = GetDb())
            {
                var sqlQueryResult = dbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("title", title) & Exp.Eq("current_version", true) & Exp.Eq("folder_id", parentId)))
                    .ConvertAll(ToFile);

                return sqlQueryResult.Count > 0 ? sqlQueryResult[0] : null;
            }
        }

        public List<File> GetFileHistory(object fileId)
        {
            using (var dbManager = GetDb())
            {
                var files = dbManager
                    .ExecuteList(GetFileQuery(Exp.Eq("id", fileId)).OrderBy("version", false))
                    .ConvertAll(ToFile);

                return files;
            }
        }

        public List<File> GetFiles(object[] fileIds)
        {
            if (fileIds == null || fileIds.Length == 0) return new List<File>();

            using (var dbManager = GetDb())
            {
                return dbManager
                    .ExecuteList(GetFileQuery(Exp.In("id", fileIds) & Exp.Eq("current_version", true)))
                    .ConvertAll(ToFile);
            }
        }

        public Stream GetFileStream(File file, long offset)
        {
            return Global.GetStore().GetReadStream(string.Empty, GetUniqFilePath(file), (int)offset);
        }

        public Uri GetPreSignedUri(File file, TimeSpan expires)
        {
            return Global.GetStore().GetPreSignedUri(string.Empty, GetUniqFilePath(file), expires,
                                                     new List<String>
                                                         {
                                                             String.Concat("Content-Disposition:", ContentDispositionUtil.GetHeaderValue(file.Title, withoutBase: true))
                                                         });
        }

        public bool IsSupportedPreSignedUri(File file)
        {
            return Global.GetStore() is S3Storage;
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file, 0);
        }

        public File SaveFile(File file, Stream fileStream)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            if (SetupInfo.MaxChunkedUploadSize < file.ContentLength)
            {
                throw FileSizeComment.FileSizeException;
            }

            var isNew = false;

            lock (syncRoot)
            {
                using (var db = GetDb())
                using (var tx = db.BeginTransaction())
                {
                    if (file.ID == null)
                    {
                        file.ID = db.ExecuteScalar<int>(new SqlQuery("files_file").SelectMax("id")) + 1;
                        file.Version = 1;
                        file.VersionGroup = 1;
                        isNew = true;
                    }

                    file.Title = Global.ReplaceInvalidCharsAndTruncate(file.Title);

                    file.ModifiedBy = SecurityContext.CurrentAccount.ID;
                    file.ModifiedOn = TenantUtil.DateTimeNow();
                    if (file.CreateBy == default(Guid)) file.CreateBy = SecurityContext.CurrentAccount.ID;
                    if (file.CreateOn == default(DateTime)) file.CreateOn = TenantUtil.DateTimeNow();

                    db.ExecuteNonQuery(
                        Update("files_file")
                            .Set("current_version", false)
                            .Where("id", file.ID)
                            .Where("current_version", true));

                    var sql = Insert("files_file")
                                .InColumnValue("id", file.ID)
                                .InColumnValue("version", file.Version)
                                .InColumnValue("version_group", file.VersionGroup)
                                .InColumnValue("current_version", true)
                                .InColumnValue("folder_id", file.FolderID)
                                .InColumnValue("title", file.Title)
                                .InColumnValue("content_length", file.ContentLength)
                                .InColumnValue("category", (int)file.FilterType)
                                .InColumnValue("create_by", file.CreateBy.ToString())
                                .InColumnValue("create_on", TenantUtil.DateTimeToUtc(file.CreateOn))
                                .InColumnValue("modified_by", file.ModifiedBy.ToString())
                                .InColumnValue("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .InColumnValue("converted_type", file.ConvertedType)
                                .InColumnValue("comment", file.Comment);
                    db.ExecuteNonQuery(sql);
                    tx.Commit();

                    file.PureTitle = file.Title;

                    var parentFoldersIds = db.ExecuteList(
                        new SqlQuery("files_folder_tree")
                            .Select("parent_id")
                            .Where(Exp.Eq("folder_id", file.FolderID))
                            .OrderBy("level", false)
                        ).ConvertAll(row => row[0]);

                    if (parentFoldersIds.Count > 0)
                        db.ExecuteNonQuery(
                            Update("files_folder")
                                .Set("modified_on", TenantUtil.DateTimeToUtc(file.ModifiedOn))
                                .Set("modified_by", file.ModifiedBy.ToString())
                                .Where(Exp.In("id", parentFoldersIds)));

                    if (isNew)
                    {
                        RecalculateFilesCount(db, file.FolderID);
                    }
                }
            }

            if (fileStream != null)
            {
                try
                {
                    SaveFileStream(file, fileStream);
                }
                catch
                {
                    if (isNew)
                    {
                        DeleteFile(file.ID);
                    }
                    else if (!IsExistOnStorage(file))
                    {
                        DeleteVersion(file);
                    }
                    throw;
                }
            }
            return GetFile(file.ID);
        }

        private void DeleteVersion(File file)
        {
            if (file == null
                || file.ID == null
                || file.Version <= 1) return;

            using (var dbManager = GetDb())
            {
                dbManager.ExecuteNonQuery(
                    Delete("files_file")
                        .Where("id", file.ID)
                        .Where("version", file.Version));

                dbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("current_version", true)
                        .Where("id", file.ID)
                        .Where("version", file.Version - 1));
            }
        }

        private static void SaveFileStream(File file, Stream stream)
        {
            Global.GetStore().Save(string.Empty, GetUniqFilePath(file), stream, file.Title);
        }

        public void DeleteFile(object fileId)
        {
            if (fileId == null) return;
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    var fromFolders = db
                        .ExecuteList(Query("files_file").Select("folder_id").Where("id", fileId).GroupBy("id"))
                        .ConvertAll(r => r[0]);

                    db.ExecuteNonQuery(Delete("files_file").Where("id", fileId));
                    db.ExecuteNonQuery(Delete("files_tag_link").Where("entry_id", fileId).Where("entry_type", FileEntryType.File));
                    db.ExecuteNonQuery(Delete("files_tag").Where(Exp.EqColumns("0", Query("files_tag_link l").SelectCount().Where(Exp.EqColumns("tag_id", "id")))));
                    db.ExecuteNonQuery(Delete("files_security").Where("entry_id", fileId).Where("entry_type", FileEntryType.File));

                    tx.Commit();

                    fromFolders.ForEach(folderId => RecalculateFilesCount(db, folderId));
                }
            }
        }

        public bool IsExist(String title, object folderId)
        {
            using (var dbManager = GetDb())
            {
                var fileCount = dbManager.ExecuteScalar<int>(
                    Query("files_file")
                        .SelectCount()
                        .Where("title", title)
                        .Where("folder_id", folderId));

                return fileCount != 0;
            }
        }

        public object MoveFile(object id, object toFolderId)
        {
            if (id == null) return null;
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    var fromFolders = db
                        .ExecuteList(Query("files_file").Select("folder_id").Where("id", id).GroupBy("id"))
                        .ConvertAll(r => r[0]);

                    var sql = Update("files_file")
                        .Set("folder_id", toFolderId)
                        .Where("id", id);

                    if (Global.FolderTrash.Equals(toFolderId))
                    {
                        sql
                            .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                            .Set("modified_on", DateTime.UtcNow);
                    }

                    db.ExecuteNonQuery(sql);
                    tx.Commit();

                    fromFolders.ForEach(folderId => RecalculateFilesCount(db, folderId));
                    RecalculateFilesCount(db, toFolderId);
                }
            }
            return id;
        }

        public File CopyFile(object id, object toFolderId)
        {
            var file = GetFile(id);
            if (file != null)
            {
                var copy = new File
                    {
                        ContentLength = file.ContentLength,
                        FileStatus = file.FileStatus,
                        FolderID = toFolderId,
                        Title = file.Title,
                        Version = file.Version,
                        VersionGroup = file.VersionGroup,
                        ConvertedType = file.ConvertedType,
                    };

                using (var stream = GetFileStream(file))
                {
                    copy = SaveFile(copy, stream);
                }
                return copy;
            }
            return null;
        }

        public object FileRename(object fileId, String newTitle)
        {
            newTitle = Global.ReplaceInvalidCharsAndTruncate(newTitle);
            using (var dbManager = GetDb())
            {
                dbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("title", newTitle)
                        .Set("modified_on", DateTime.UtcNow)
                        .Set("modified_by", SecurityContext.CurrentAccount.ID.ToString())
                        .Where("id", fileId)
                        .Where("current_version", true));
            }
            return fileId;
        }

        public string UpdateComment(object fileId, int fileVersion, string comment)
        {
            using (var dbManager = GetDb())
            {
                comment = comment ?? string.Empty;
                comment = comment.Substring(0, Math.Min(comment.Length, 255));
                dbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("comment", comment)
                        .Where("id", fileId)
                        .Where("version", fileVersion));
            }
            return comment;
        }

        public void CompleteVersion(object fileId, int fileVersion)
        {
            using (var dbManager = GetDb())
            {
                dbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("version_group = version_group + 1")
                        .Where("id", fileId)
                        .Where(Exp.Gt("version", fileVersion)));
            }
        }

        public void ContinueVersion(object fileId, int fileVersion)
        {
            using (var dbManager = GetDb())
            {
                using (var tx = dbManager.BeginTransaction())
                {
                    var versionGroup =
                        dbManager.ExecuteScalar<int>(
                            Query("files_file")
                                .Select("version_group")
                                .Where("id", fileId)
                                .Where("version", fileVersion)
                                .GroupBy("id"));

                    dbManager.ExecuteNonQuery(
                        Update("files_file")
                            .Set("version_group = version_group - 1")
                            .Where("id", fileId)
                            .Where(Exp.Gt("version", fileVersion))
                            .Where(Exp.Gt("version_group", versionGroup)));

                    tx.Commit();
                }
            }
        }

        public bool UseTrashForRemove(File file)
        {
            return file.RootFolderType != FolderType.TRASH;
        }

        public bool IsExist(object fileId)
        {
            using (var dbManager = GetDb())
            {
                var count = dbManager.ExecuteScalar<int>(Query("files_file").SelectCount().Where("id", fileId));
                return count != 0;
            }
        }

        private static String GetUniqFileDirectory(object fileIdObject)
        {
            if (fileIdObject == null) throw new ArgumentNullException("fileIdObject");
            var fileIdInt = Convert.ToInt32(Convert.ToString(fileIdObject));
            return string.Format("folder_{0}/file_{1}", (fileIdInt / 1000 + 1) * 1000, fileIdInt);
        }

        private static String GetUniqFilePath(File file)
        {
            return file != null
                       ? GetUniqFilePath(file, "content" + FileUtility.GetFileExtension(file.PureTitle))
                       : null;
        }

        private static String GetUniqFilePath(File file, string fileTitle)
        {
            return file != null
                       ? string.Format("{0}/v{1}/{2}", GetUniqFileDirectory(file.ID), file.Version, fileTitle)
                       : null;
        }

        private void RecalculateFilesCount(IDbManager db, object folderId)
        {
            db.ExecuteNonQuery(GetRecalculateFilesCountUpdate(folderId));
        }

        #region chunking

        public ChunkedUploadSession CreateUploadSession(File file, long contentLength)
        {
            if (SetupInfo.ChunkUploadSize > contentLength)
                return new ChunkedUploadSession(file, contentLength) { UseChunks = false };

            var tempPath = Guid.NewGuid().ToString();
            var uploadId = Global.GetStore().InitiateChunkedUpload(FileConstant.StorageDomainTmp, tempPath);

            var uploadSession = new ChunkedUploadSession(file, contentLength);
            uploadSession.Items["TempPath"] = tempPath;
            uploadSession.Items["UploadId"] = uploadId;

            return uploadSession;
        }

        public void UploadChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (!uploadSession.UseChunks)
            {
                UploadSingleChunk(uploadSession, stream, chunkLength);
                return;
            }

            var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
            var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
            var chunkNumber = uploadSession.GetItemOrDefault<int>("ChunksUploaded") + 1;

            var eTag = Global.GetStore().UploadChunk(FileConstant.StorageDomainTmp, tempPath, uploadId, stream, chunkNumber, chunkLength);

            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag") ?? new List<string>();
            eTags.Add(eTag);

            uploadSession.Items["ChunksUploaded"] = chunkNumber;
            uploadSession.Items["ETag"] = eTags;
            uploadSession.BytesUploaded += chunkLength;

            if (uploadSession.BytesUploaded == uploadSession.BytesTotal)
            {
                uploadSession.File = FinalizeUploadSession(uploadSession);
            }
        }

        private void UploadSingleChunk(ChunkedUploadSession uploadSession, Stream stream, long chunkLength)
        {
            if (uploadSession.BytesTotal == 0)
                uploadSession.BytesTotal = chunkLength;

            if (uploadSession.BytesTotal == chunkLength)
            {
                uploadSession.File = SaveFile(GetFileForCommit(uploadSession), stream);
                uploadSession.BytesUploaded = chunkLength;
            }
            else if (uploadSession.BytesTotal > chunkLength)
            {
                //This is hack fixing strange behaviour of plupload in flash mode.

                if (!uploadSession.Items.ContainsKey("ChunksBuffer"))
                    uploadSession.Items["ChunksBuffer"] = Path.GetTempFileName();

                using (var bufferStream = new FileStream(uploadSession.GetItemOrDefault<string>("ChunksBuffer"), FileMode.Append))
                {
                    stream.StreamCopyTo(bufferStream);
                }

                uploadSession.BytesUploaded += chunkLength;

                if (uploadSession.BytesTotal == uploadSession.BytesUploaded)
                {
                    using (var bufferStream = new FileStream(uploadSession.GetItemOrDefault<string>("ChunksBuffer"), FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
                    {
                        uploadSession.File = SaveFile(GetFileForCommit(uploadSession), bufferStream);
                    }
                }
            }
        }

        private File FinalizeUploadSession(ChunkedUploadSession uploadSession)
        {
            var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
            var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
            var eTags = uploadSession.GetItemOrDefault<List<string>>("ETag")
                                     .Select((x, i) => new KeyValuePair<int, string>(i + 1, x))
                                     .ToDictionary(x => x.Key, x => x.Value);

            Global.GetStore().FinalizeChunkedUpload(FileConstant.StorageDomainTmp, tempPath, uploadId, eTags);

            var file = GetFileForCommit(uploadSession);
            SaveFile(file, null);
            Global.GetStore().Move(FileConstant.StorageDomainTmp, tempPath, string.Empty, GetUniqFilePath(file));

            return file;
        }

        public void AbortUploadSession(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.UseChunks)
            {
                var tempPath = uploadSession.GetItemOrDefault<string>("TempPath");
                var uploadId = uploadSession.GetItemOrDefault<string>("UploadId");
                Global.GetStore().AbortChunkedUpload(FileConstant.StorageDomainTmp, tempPath, uploadId);
            }
            else if (uploadSession.Items.ContainsKey("ChunksBuffer"))
            {
                System.IO.File.Delete(uploadSession.GetItemOrDefault<string>("ChunksBuffer"));
            }
        }

        private File GetFileForCommit(ChunkedUploadSession uploadSession)
        {
            if (uploadSession.File.ID != null)
            {
                var file = GetFile(uploadSession.File.ID);
                file.Version++;
                file.ContentLength = uploadSession.BytesTotal;
                file.Comment = string.Empty;
                return file;
            }

            return new File { FolderID = uploadSession.File.FolderID, Title = uploadSession.File.Title, ContentLength = uploadSession.BytesTotal };
        }

        #endregion

        #region Only in TMFileDao

        public IEnumerable<File> Search(String searchText, FolderType folderType)
        {
            if (FullTextSearch.SupportModule(FullTextSearch.FileModule))
            {
                var ids = FullTextSearch.Search(FullTextSearch.FileModule.Match(searchText));

                using (var dbManager = GetDb())
                {
                    return dbManager
                        .ExecuteList(GetFileQuery(Exp.In("id", ids) & Exp.Eq("current_version", true)))
                        .ConvertAll(ToFile)
                        .Where(
                            f =>
                            folderType == FolderType.BUNCH
                                ? f.RootFolderType == FolderType.BUNCH
                                : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                        .ToList();
                }
            }
            else
            {
                var keywords = searchText
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => 3 <= k.Trim().Length)
                    .ToArray();

                if (keywords.Length == 0) return Enumerable.Empty<File>();

                var q = GetFileQuery(Exp.Eq("f.current_version", true) & BuildLike(new[] { "f.title" }, keywords));
                using (var dbManager = GetDb())
                {
                    return dbManager
                        .ExecuteList(q)
                        .ConvertAll(ToFile)
                        .Where(f =>
                               folderType == FolderType.BUNCH
                                   ? f.RootFolderType == FolderType.BUNCH
                                   : f.RootFolderType == FolderType.USER | f.RootFolderType == FolderType.COMMON)
                        .ToList();
                }
            }
        }

        public void DeleteFileStream(object fileId)
        {
            Global.GetStore().Delete(GetUniqFilePath(GetFile(fileId)));
        }

        public void DeleteFolder(object fileId)
        {
            Global.GetStore().DeleteDirectory(GetUniqFileDirectory(fileId));
        }

        public bool IsExistOnStorage(File file)
        {
            return Global.GetStore().IsFile(GetUniqFilePath(file));
        }

        private const string DiffTitle = "diff.zip";
        public void SaveEditHistory(File file, string changes, Stream differenceStream)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (string.IsNullOrEmpty(changes)) throw new ArgumentNullException("changes");
            if (differenceStream == null) throw new ArgumentNullException("differenceStream");

            changes = changes.Trim();
            using (var dbManager = GetDb())
            {
                dbManager.ExecuteNonQuery(
                    Update("files_file")
                        .Set("changes", changes)
                        .Where("id", file.ID)
                        .Where("version", file.Version));
            }

            Global.GetStore().Save(string.Empty, GetUniqFilePath(file, DiffTitle), differenceStream, DiffTitle);
        }

        public List<EditHistory> GetEditHistory(object fileId, int fileVersion = 0)
        {
            using (var dbManager = GetDb())
            {
                var query = Query("files_file")
                    .Select("id")
                    .Select("version")
                    .Select("version_group")
                    .Select("modified_on")
                    .Select("modified_by")
                    .Select("changes")
                    .Where(Exp.Eq("id", fileId))
                    .OrderBy("version", true);

                if (fileVersion > 0)
                {
                    query.Where(Exp.Eq("version", fileVersion));
                }

                return
                    dbManager
                        .ExecuteList(query)
                        .ConvertAll(r => new EditHistory
                            {
                                ID = Convert.ToInt32(r[0]),
                                Version = Convert.ToInt32(r[1]),
                                VersionGroup = Convert.ToInt32(r[2]),
                                ModifiedOn = TenantUtil.DateTimeFromUtc(Convert.ToDateTime(r[3])),
                                ModifiedBy = new Guid((string) r[4]),
                                Changes = (string) (r[5]),
                            });
            }
        }

        public string GetDifferenceUrl(File file)
        {
            return Global.GetStore().GetPreSignedUri(string.Empty, GetUniqFilePath(file, DiffTitle), TimeSpan.FromHours(1), null).ToString();
        }

        #endregion
    }
}