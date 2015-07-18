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
using System.Data;
using System.Linq;
using System.Text;
using ASC.Data.Storage.S3;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.Mail.Aggregator.Dal.DbSchema;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Exceptions;
using ASC.Web.Core.Files;
using ASC.Web.Files.Api;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using System.IO;
using ASC.Common.Data;
using ASC.Web.Files.Utils;
using ASC.Data.Storage;
using ActiveUp.Net.Mail;
using HtmlAgilityPack;
using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Mail.Aggregator
{
    public partial class MailBoxManager
    {
        #region private defines

// ReSharper disable InconsistentNaming
        private const long ATTACHMENTS_TOTAL_SIZE_LIMIT = 25 * 1024 * 1024; // 25 megabytes
// ReSharper restore InconsistentNaming

        private delegate SqlInsert CreateInsertDelegate();

        #endregion

        #region public methods

        public MailAttachment AttachFileFromDocuments(int tenant, string user, int messageId, string fileId,
                                                          string version, string shareLink, string streamId)
        {
            MailAttachment result;

            using (var fileDao = FilesIntegration.GetFileDao())
            {
                Files.Core.File file;
                var checkLink = FileShareLink.Check(shareLink, true, fileDao, out file);
                if (!checkLink && file == null)
                    file = String.IsNullOrEmpty(version)
                               ? fileDao.GetFile(fileId)
                               : fileDao.GetFile(fileId, Convert.ToInt32(version));

                if (file == null)
                    throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound, "File not found.");

                if (!checkLink && !FilesIntegration.GetFileSecurity().CanRead(file))
                    throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied,
                                                   "Access denied.");

                if (!fileDao.IsExistOnStorage(file))
                {
                    throw new AttachmentsException(AttachmentsException.Types.DocumentNotFound,
                                                   "File not exists on storage.");
                }

                _log.Info("Original file id: {0}", file.ID);
                _log.Info("Original file name: {0}", file.Title);
                var fileExt = FileUtility.GetFileExtension(file.Title);
                var curFileType = FileUtility.GetFileTypeByFileName(file.Title);
                _log.Info("File converted type: {0}", file.ConvertedType);

                if (file.ConvertedType != null)
                {
                    switch (curFileType)
                    {
                        case FileType.Image:
                            fileExt = file.ConvertedType == ".zip" ? ".pptt" : file.ConvertedType;
                            break;
                        case FileType.Spreadsheet:
                            fileExt = file.ConvertedType != ".xlsx" ? ".xlst" : file.ConvertedType;
                            break;
                        default:
                            if (file.ConvertedType == ".doct" || file.ConvertedType == ".xlst" || file.ConvertedType == ".pptt")
                                fileExt = file.ConvertedType;
                            break;
                    }
                }

                var convertToExt = string.Empty;
                switch (curFileType)
                {
                    case FileType.Document:
                        if (fileExt == ".doct")
                            convertToExt = ".docx";
                        break;
                    case FileType.Spreadsheet:
                        if (fileExt == ".xlst")
                            convertToExt = ".xlsx";
                        break;
                    case FileType.Presentation:
                        if (fileExt == ".pptt")
                            convertToExt = ".pptx";
                        break;
                }

                if (!string.IsNullOrEmpty(convertToExt) && fileExt != convertToExt)
                {
                    var fileName = Path.ChangeExtension(file.Title, convertToExt);
                    _log.Info("Changed file name - {0} for file {1}:", fileName, file.ID);

                    using (var readStream = FileConverter.Exec(file, convertToExt))
                    {
                        if (readStream == null)
                            throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied, "Access denied.");

                        using (var memStream = new MemoryStream())
                        {
                            readStream.StreamCopyTo(memStream);
                            result = AttachFile(tenant, user, messageId, fileName, memStream, streamId);
                            _log.Info("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName,result.fileName, result.storedFileUrl);
                        }
                    }
                }
                else
                {
                    using (var readStream = fileDao.GetFileStream(file))
                    {
                        if (readStream == null)
                            throw new AttachmentsException(AttachmentsException.Types.DocumentAccessDenied,"Access denied.");

                        result = AttachFile(tenant, user, messageId, file.Title, readStream, streamId);
                       _log.Info("Attached attachment: ID - {0}, Name - {1}, StoredUrl - {2}", result.fileName, result.fileName, result.storedFileUrl);
                    }
                }
            }

            return result;
        }

        public MailAttachment AttachFile(int tenant, string user, int messageId,
            string name, Stream inputStream, string streamId)
        {
            if (messageId < 0)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_message' must have non-negative value.");

            if (tenant < 0)
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_tenant' must have non-negative value.");

            if (String.IsNullOrEmpty(user))
                throw new AttachmentsException(AttachmentsException.Types.BadParams, "Field 'id_user' is empty.");

            if (inputStream.Length == 0)
                throw new AttachmentsException(AttachmentsException.Types.EmptyFile, "Empty files not supported.");

            if (string.IsNullOrEmpty(streamId))
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            var message = GetMailInfo(tenant, user, messageId, false, false);

            if(message == null || streamId != message.StreamId)
                throw new AttachmentsException(AttachmentsException.Types.MessageNotFound, "Message not found.");

            var totalSize = GetAttachmentsTotalSize(messageId) + inputStream.Length;

            if (totalSize > ATTACHMENTS_TOTAL_SIZE_LIMIT)
                throw new AttachmentsException(AttachmentsException.Types.TotalSizeExceeded, "Total size of all files exceeds limit!");

            var fileNumber = GetMaxAttachmentNumber(messageId, tenant);

            var attachment = new MailAttachment
            {
                fileName = name,
                contentType = MimeMapping.GetMimeMapping(name),
                fileNumber = fileNumber,
                size = inputStream.Length,
                data = inputStream.GetCorrectBuffer(),
                streamId = streamId,
                tenant = tenant,
                user = user,
                mailboxId = message.MailboxId
            };

            QuotaUsedAdd(tenant, inputStream.Length);
            try
            {
                StoreAttachmentWithoutQuota(tenant, user, attachment);
            }
            catch
            {
                QuotaUsedDelete(tenant, inputStream.Length);
                throw;
            }

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    attachment.fileId = SaveAttachment(db, tenant, messageId, attachment);
                    UpdateMessageChainAttachmentsFlag(db, tenant, user, messageId);

                    tx.Commit();
                }
            }

            return attachment;
        }

        public int GetAttachmentId(int messageId, string contentId)
        {
            using (var db = GetDb())
            {
                var query = new SqlQuery(AttachmentTable.name)
                                .Select(AttachmentTable.Columns.id)
                                .Where(AttachmentTable.Columns.id_mail, messageId)
                                .Where(AttachmentTable.Columns.need_remove, false)
                    .Where(AttachmentTable.Columns.content_id, contentId);

                var attachId = db.ExecuteScalar<int>(query);
                return attachId;
            }
        }

        public void StoreAttachmentCopy(int tenant, string user, MailAttachment attachment, string streamId)
        {
            try
            {
                if (attachment.streamId.Equals(streamId)) return;

                var s3Key = MailStoragePathCombiner.GerStoredFilePath(attachment);

                var dataClient = MailDataStore.GetDataStore(tenant);

                if (!dataClient.IsFile(s3Key)) return;

                attachment.fileNumber =
                    !string.IsNullOrEmpty(attachment.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : attachment.fileNumber;

                var newS3Key = MailStoragePathCombiner.GetFileKey(user, streamId, attachment.fileNumber,
                                                                    attachment.storedName);

                var copyS3Url = dataClient.Copy(s3Key, string.Empty, newS3Key);

                attachment.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(copyS3Url);

                attachment.streamId = streamId;

                _log.Debug("StoreAttachmentCopy() tenant='{0}', user_id='{1}', stream_id='{2}', new_s3_key='{3}', copy_s3_url='{4}', storedFileUrl='{5}',  filename='{6}'",
                    tenant, user, streamId, newS3Key, copyS3Url, attachment.storedFileUrl, attachment.fileName);
            }
            catch (Exception ex)
            {
                _log.Error("CopyAttachment(). filename='{0}', ctype='{1}' Exception:\r\n{2}\r\n",
                           attachment.fileName,
                           attachment.contentType,
                    ex.ToString());

                throw;
            }
        }

        public void StoreAttachmentWithoutQuota(int tenant, string user, MailAttachment attachment)
        {
            StoreAttachmentWithoutQuota(tenant, user, attachment, MailDataStore.GetDataStore(tenant));
        }

        public void StoreAttachmentWithoutQuota(int tenant, string user, MailAttachment attachment, IDataStore storage)
        {
            try
            {
                if (attachment.data == null || attachment.data.Length == 0)
                    return;

                if (string.IsNullOrEmpty(attachment.fileName))
                    attachment.fileName = "attachment.ext";

                var contentDisposition = MailStoragePathCombiner.PrepareAttachmentName(attachment.fileName);

                var ext = Path.GetExtension(attachment.fileName);

                attachment.storedName = CreateNewStreamId();

                if (!string.IsNullOrEmpty(ext))
                    attachment.storedName = Path.ChangeExtension(attachment.storedName, ext);

                attachment.fileNumber =
                    !string.IsNullOrEmpty(attachment.contentId) //Upload hack: embedded attachment have to be saved in 0 folder
                        ? 0
                        : attachment.fileNumber;

                var attachmentPath = MailStoragePathCombiner.GerStoredFilePath(attachment);

                using (var reader = new MemoryStream(attachment.data))
                {
                    var uploadUrl = storage.UploadWithoutQuota(string.Empty, attachmentPath, reader, attachment.contentType, contentDisposition);
                    attachment.storedFileUrl = MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }
            }
            catch (Exception e)
            {
                _log.Error("StoreAttachmentWithoutQuota(). filename: {0}, ctype: {1} Exception:\r\n{2}\r\n",
                    attachment.fileName,
                    attachment.contentType,
                    e.ToString());

                throw;
            }
        }

        public string StoreCKeditorImageWithoutQuota(int tenant, string user, int mailboxId, string fileName, byte[] imageData, IDataStore storage)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    throw new ArgumentNullException("imageData");

                var ext = string.IsNullOrEmpty(fileName) ? ".jpg" : Path.GetExtension(fileName);

                if (string.IsNullOrEmpty(ext))
                    ext = ".jpg";

                var storeName = imageData.GetMd5();
                storeName = Path.ChangeExtension(storeName, ext);

                var contentDisposition = ContentDispositionUtil.GetHeaderValue(storeName);
                var contentType = MimeTypesHelper.GetMimeqType(ext);

                var signatureImagePath = MailStoragePathCombiner.GerStoredSignatureImagePath(mailboxId, storeName);

                using (var reader = new MemoryStream(imageData))
                {
                    var uploadUrl = storage.UploadWithoutQuota(user, signatureImagePath, reader, contentType, contentDisposition);
                    return MailStoragePathCombiner.GetStoredUrl(uploadUrl);
                }
            }
            catch (Exception e)
            {
                _log.Error("StoreCKeditorImageWithoutQuota(). filename: {0} Exception:\r\n{1}\r\n", fileName,
                           e.ToString());

                throw;
            }
        }

        public void DeleteMessageAttachments(int tenant, string user, int messageId, List<int> attachmentIds)
        {
            var ids = attachmentIds.ToArray();
            long usedQuota;
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    usedQuota = MarkAttachmetsNeedRemove(db, tenant,
                                                          Exp.And(Exp.Eq(AttachmentTable.Columns.id_mail, messageId),
                                                                  Exp.In(AttachmentTable.Columns.id, ids)));
                    ReCountAttachments(db, messageId);
                    UpdateMessageChainAttachmentsFlag(db, tenant, user, messageId);

                    tx.Commit();
                }
            }

            if (usedQuota > 0)
                QuotaUsedDelete(tenant, usedQuota);
        }

        public MailAttachment GetMessageAttachment(int attachId, int tenant, string user = null)
        {
            using (var db = GetDb())
            {
                var query = GetAttachmentsSelectQuery()
                        .Where(AttachmentTable.Columns.id.Prefix(AttachmentTable.name), attachId)
                    .Where(AttachmentTable.Columns.need_remove.Prefix(AttachmentTable.name), false)
                    .Where(AttachmentTable.Columns.id_tenant.Prefix(AttachmentTable.name), tenant);

                var whereExp = string.IsNullOrEmpty(user)
                                    ? Exp.Eq(MailTable.Columns.id_tenant.Prefix(MailTable.name), tenant)
                                    : GetUserWhere(user, tenant, MailTable.name);

                query.Where(whereExp);

                var attachment = db.ExecuteList(query)
                                   .ConvertAll(ToMailItemAttachment)
                        .FirstOrDefault();

                return attachment;
            }
        }

        public long GetAttachmentsTotalSize(int messageId)
        {
            using (var db = GetDb())
            {
                var totalSize = db.ExecuteList(
                    new SqlQuery(AttachmentTable.name)
                        .SelectSum(AttachmentTable.Columns.size)
                        .Where(AttachmentTable.Columns.id_mail, messageId)
                        .Where(AttachmentTable.Columns.need_remove, false)
                        .Where(AttachmentTable.Columns.content_id, null))
                        .ConvertAll(r => Convert.ToInt64(r[0]))
                        .FirstOrDefault();

                return totalSize;
            }
        }

        public int GetMaxAttachmentNumber(int messageId, int tenant)
        {
            using (var db = GetDb())
            {
                var number = db.ExecuteList(
                    new SqlQuery(AttachmentTable.name)
                        .SelectMax(AttachmentTable.Columns.file_number)
                        .Where(AttachmentTable.Columns.id_mail, messageId)
                        .Where(AttachmentTable.Columns.id_tenant, tenant))
                        .ConvertAll(r => Convert.ToInt32(r[0]))
                        .FirstOrDefault();

                number++;

                return number;
            }
        }

        public int SaveAttachmentInTransaction(int tenant, int messageId, MailAttachment attachment)
        {
            int id;
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    id = SaveAttachment(db, tenant, messageId, attachment);

                    tx.Commit();
                }
            }
            return id;
        }

        public string StoreMailBody(int tenant, string user, MailMessageItem messageItem)
        {
            if (string.IsNullOrEmpty(messageItem.HtmlBody))
            {
                return string.Empty;
            }

            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var savePath = MailStoragePathCombiner.GetBodyKey(user, messageItem.StreamId);
            var storage = MailDataStore.GetDataStore(tenant);

            try
            {
                var safeBody = ReplaceEmbeddedImages(messageItem, _log);

                using (var reader = new MemoryStream(Encoding.UTF8.GetBytes(safeBody)))
                {
                    var res = storage
                        .UploadWithoutQuota(string.Empty, savePath, reader, "text/html", string.Empty)
                        .ToString();

                    _log.Debug("StoreMailBody() tenant='{0}', user_id='{1}', save_body_path='{2}' Result: {3}",
                        tenant, user, savePath, res);

                    return res;
                }
            }
            catch (Exception ex)
            {
                _log.Debug(
                    "StoreMailBody() Problems with message saving in messageId={0}. \r\n Exception: \r\n {0}\r\n",
                    messageItem.MimeMessageId, ex.ToString());

                storage.Delete(string.Empty, savePath);
                throw;
            }
        }

        public string GetMailEmlUrl(int tenant, string user, string streamId)
        {
            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var emlPath = MailStoragePathCombiner.GetEmlKey(user, streamId);
            var dataStore = MailDataStore.GetDataStore(tenant);

            try
            {
                var emlUri = dataStore.GetUri(string.Empty, emlPath);
                var url = MailStoragePathCombiner.GetStoredUrl(emlUri);

                return url;
            }
            catch (Exception ex)
            {
                _log.Error("GetMailEmlUrl() tenant='{0}', user_id='{1}', save_eml_path='{2}' Exception: {3}",
                           tenant, user, emlPath, ex.ToString());
            }

            return "";
        }

        public string StoreMailEml(int tenant, string user, string streamId, Message message)
        {
            // Using id_user as domain in S3 Storage - allows not to add quota to tenant.
            var savePath = MailStoragePathCombiner.GetEmlKey(user, streamId);
            var storage = MailDataStore.GetDataStore(tenant);

            try
            {
                using (var reader = new MemoryStream(message.OriginalData))
                {
                    var res = storage
                        .UploadWithoutQuota(string.Empty, savePath, reader, "message/rfc822", string.Empty)
                        .ToString();

                    _log.Debug("StoreMailEml() tenant='{0}', user_id='{1}', save_eml_path='{2}' Result: {3}",
                        tenant, user, savePath, res);

                    return res;
                }
            }
            catch (Exception)
            {
                storage.Delete(string.Empty, savePath);
                throw;
            }
        }

        public void SaveAttachments(int tenant, int messageId, List<MailAttachment> attachments)
        {
            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    SaveAttachments(db, tenant, messageId, attachments);
                    tx.Commit();
                }
            }
        }

        public void SaveAttachments(DbManager db, int tenant, int messageId, List<MailAttachment> attachments)
        {
            if (!attachments.Any()) return;

            CreateInsertDelegate createInsertQuery = () => new SqlInsert(AttachmentTable.name)
                                                         .InColumns(AttachmentTable.Columns.id_mail,
                                                                    AttachmentTable.Columns.name,
                                                                    AttachmentTable.Columns.stored_name,
                                                                    AttachmentTable.Columns.type,
                                                                    AttachmentTable.Columns.size,
                                                                    AttachmentTable.Columns.file_number,
                                                                    AttachmentTable.Columns.need_remove,
                                                                    AttachmentTable.Columns.content_id,
                                                                    AttachmentTable.Columns.id_tenant,
                                                                    AttachmentTable.Columns.id_mailbox);

            var insertQuery = createInsertQuery();

            int i, len;

            for (i = 0, len = attachments.Count; i < len; i++)
            {
                var attachment = attachments[i];

                insertQuery
                    .Values(messageId, attachment.fileName, attachment.storedName, attachment.contentType, attachment.size,
                            attachment.fileNumber, 0, attachment.contentId, tenant, attachment.mailboxId);

                if ((i%100 != 0 || i == 0) && i + 1 != len) continue;
                db.ExecuteNonQuery(insertQuery);
                insertQuery = createInsertQuery();
            }

            ReCountAttachments(db, messageId);
        }

        public int SaveAttachment(int tenant, int messageId, MailAttachment attachment)
        {
            using (var db = GetDb())
            {
                return SaveAttachment(db, tenant, messageId, attachment);
            }
        }

        public int SaveAttachment(DbManager db, int tenant, int messageId, MailAttachment attachment)
        {
            return SaveAttachment(db, tenant, messageId, attachment, true);
        }

        public int SaveAttachment(DbManager db, int tenant, int messageId, MailAttachment attachment, bool needRecount)
        {
            var id = db.ExecuteScalar<int>(
                        new SqlInsert(AttachmentTable.name)
                            .InColumnValue(AttachmentTable.Columns.id, 0)
                            .InColumnValue(AttachmentTable.Columns.id_mail, messageId)
                            .InColumnValue(AttachmentTable.Columns.name, attachment.fileName)
                            .InColumnValue(AttachmentTable.Columns.stored_name, attachment.storedName)
                            .InColumnValue(AttachmentTable.Columns.type, attachment.contentType)
                            .InColumnValue(AttachmentTable.Columns.size, attachment.size)
                            .InColumnValue(AttachmentTable.Columns.file_number, attachment.fileNumber)
                            .InColumnValue(AttachmentTable.Columns.need_remove, false)
                            .InColumnValue(AttachmentTable.Columns.content_id, attachment.contentId)
                            .InColumnValue(AttachmentTable.Columns.id_tenant, tenant)
                            .InColumnValue(AttachmentTable.Columns.id_mailbox, attachment.mailboxId)
                            .Identity(0, 0, true));

            if (needRecount)
                ReCountAttachments(db, messageId);

            return id;
        }

        #endregion

        #region private methods
        public void StoreAttachments(int tenant, string user, List<MailAttachment> attachments, string streamId)
        {
            StoreAttachments(tenant, user, attachments, streamId, -1);
        }

        public void StoreAttachments(int tenant, string user, List<MailAttachment> attachments, string streamId, int mailboxId)
        {
            if (!attachments.Any() || string.IsNullOrEmpty(streamId)) return;

            var storage = MailDataStore.GetDataStore(tenant);

            if(storage == null)
                throw new NullReferenceException("GetDataStore has returned null reference.");

            var quotaAddSize = attachments.Sum(a => a.data.LongLength);

            QuotaUsedAdd(tenant, quotaAddSize);

            try
            {
                foreach (var attachment in attachments)
                {
                    var isAttachmentNameHasBadName = string.IsNullOrEmpty(attachment.fileName)
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidPathChars()) != -1
                                                         || attachment.fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
                    if (isAttachmentNameHasBadName)
                    {
                        attachment.fileName = string.Format("attacment{0}{1}", attachment.fileNumber,
                                                                               MimeMapping.GetExtention(attachment.contentType));
                    }

                    attachment.streamId = streamId;
                    attachment.tenant = tenant;
                    attachment.user = user;
                    StoreAttachmentWithoutQuota(tenant, user, attachment, storage);

                    //This is dirty hack needed for mailbox updating on attachment processing. If we doesn't update mailbox checktime, it will be reset.
                    if (mailboxId > 0)
                    {
                        LockMailbox(mailboxId);
                    }
                }
            }
            catch
            {
                var storedAttachmentsKeys = attachments
                                            .Where(a => !string.IsNullOrEmpty(a.storedFileUrl))
                                            .Select(MailStoragePathCombiner.GerStoredFilePath)
                                            .ToList();

                //Todo: Rewrite this ugly code
                var isQuotaCleaned = false;
                if (storedAttachmentsKeys.Any())
                {
                    var s3Storage = storage as S3Storage;
                    if (s3Storage != null)
                    {
                        // ToDo: Delete with quota argument needs to be moved to IDataStore!
                        storedAttachmentsKeys
                            .ForEach(key => s3Storage.Delete(string.Empty, key, false));
                    }
                    else
                    {
                        isQuotaCleaned = true;
                        storedAttachmentsKeys.ForEach(key => storage.Delete(string.Empty, key));
                    }
                }

                if(!isQuotaCleaned)
                    QuotaUsedDelete(tenant, quotaAddSize);

                _log.Debug(String.Format("Problems with attachment saving in mailboxId={0}. All message attachments was deleted.", mailboxId));
                throw;
            }
        }

        public AttachmentStream GetAttachmentStream(int attachId, int tenant)
        {
            var attachment = GetMessageAttachment(attachId, tenant);
            return AttachmentManager.GetAttachmentStream(attachment);
        }

        public AttachmentStream GetAttachmentStream(int attachId, int tenant, string user)
        {
            var attachment = GetMessageAttachment(attachId, tenant, user);
            return AttachmentManager.GetAttachmentStream(attachment);
        }

        public static string ReplaceEmbeddedImages(MailMessageItem m, ILogger log = null)
        {
            try
            {
                log = log ?? new NullLogger();

                var doc = new HtmlDocument();

                doc.LoadHtml(m.HtmlBody);

                if (m.Attachments != null && m.Attachments.Count > 0)
                {
                    foreach (var attach in m.Attachments)
                    {
                        if (string.IsNullOrEmpty(attach.storedFileUrl)) continue;
                        if (string.IsNullOrEmpty(attach.contentId) &&
                            string.IsNullOrEmpty(attach.contentLocation)) continue;

                        HtmlNodeCollection oldNodes = null;

                        if (!string.IsNullOrEmpty(attach.contentId))
                        {
                            oldNodes =
                                doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'cid:" +
                                                             attach.contentId.Trim('<').Trim('>') + "'))]");
                        }

                        if (!string.IsNullOrEmpty(attach.contentLocation) && oldNodes == null)
                        {
                            oldNodes =
                               doc.DocumentNode.SelectNodes("//img[@src and (contains(@src,'" +
                                                            attach.contentLocation + "'))]");
                        }

                        if (oldNodes == null)
                        {
                            //This attachment is not embedded;
                            attach.contentId = null;
                            attach.contentLocation = null;
                            continue;
                        }

                        foreach (var node in oldNodes)
                            node.SetAttributeValue("src", attach.storedFileUrl);
                    }
                }

                return doc.DocumentNode.OuterHtml;
            }
            catch (RecursionDepthException ex)
            {
                var data = Encoding.UTF8.GetBytes(m.HtmlBody);

                m.Attachments.Add(new MailAttachment
                {
                    size = data.Length,
                    fileName = "original_message.html",
                    contentType = "text/html; charset=utf-8",
                    data = data
                });

                m.HtmlBody = "<div>The received email size is too big. The complete text was saved into a separate file and attached to this email.</div>";

                log.Error("ReplaceEmbeddedImages() \r\n Exception: \r\n{0}\r\n", ex.ToString());

                return m.HtmlBody;
            }
            catch (Exception)
            {
                return m.HtmlBody;
            }
        }

        #endregion

        #region private methods
        private static int GetCountAttachments(IDbManager db, int messageId)
        {
            var countAttachments = db.ExecuteScalar<int>(
                        new SqlQuery(AttachmentTable.name)
                            .SelectCount(AttachmentTable.Columns.id)
                            .Where(AttachmentTable.Columns.id_mail, messageId)
                            .Where(AttachmentTable.Columns.need_remove, false)
                            .Where(AttachmentTable.Columns.content_id, null));
            return countAttachments;
        }

        private void ReCountAttachments(DbManager db, int messageId)
        {
            var countAttachments = GetCountAttachments(db, messageId);

            db.ExecuteNonQuery(
                  new SqlUpdate(MailTable.name)
                  .Set(MailTable.Columns.attach_count, countAttachments)
                  .Where(MailTable.Columns.id, messageId));
        }

        // Returns size of all tenant attachments. This size used in the quota calculation.
        private static long MarkAttachmetsNeedRemove(IDbManager db, int tenant, Exp condition)
        {
            var queryForAttachmentSizeCalculation = new SqlQuery(AttachmentTable.name)
                                                                .SelectSum(AttachmentTable.Columns.size)
                                                                .Where(AttachmentTable.Columns.id_tenant, tenant)
                                                                .Where(AttachmentTable.Columns.need_remove, false)
                                                                .Where(condition);

            var size = db.ExecuteScalar<long>(queryForAttachmentSizeCalculation);

            db.ExecuteNonQuery(
                new SqlUpdate(AttachmentTable.name)
                    .Set(AttachmentTable.Columns.need_remove, true)
                    .Where(AttachmentTable.Columns.id_tenant, tenant)
                    .Where(condition));

            return size;
        }

        private void QuotaUsedAdd(int tenant, long usedQuota)
        {
            try
            {
                var quotaController = new TennantQuotaController(tenant);
                quotaController.QuotaUsedAdd(Defines.MODULE_NAME, string.Empty, DATA_TAG, usedQuota);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "QuotaUsedAdd with params: tenant={0}, used_quota={1}", tenant, usedQuota);
                throw;
            }
        }

        public void QuotaUsedDelete(int tenant, long usedQuota)
        {
            try
            {
                var quotaController = new TennantQuotaController(tenant);
                quotaController.QuotaUsedDelete(Defines.MODULE_NAME, string.Empty, DATA_TAG, usedQuota);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "QuotaUsedDelete with params: tenant={0}, used_quota={1}", tenant, usedQuota);
                throw;
            }
        }
        #endregion
    }
}
