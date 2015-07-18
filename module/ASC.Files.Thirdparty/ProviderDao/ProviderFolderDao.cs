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
using System.Linq;
using ASC.Files.Core;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderFolderDao : ProviderDaoBase, IFolderDao
    {
        public void Dispose()
        {
        }

        public Folder GetFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            var result = selector.GetFolderDao(folderId).GetFolder(selector.ConvertId(folderId));

            if (result != null && !Default.IsMatch(folderId))
            {
                SetSharedByMeProperty(new[] {result});
            }

            return result;
        }

        public Folder GetFolder(string title, object parentId)
        {
            var selector = GetSelector(parentId);
            return selector.GetFolderDao(parentId).GetFolder(title, selector.ConvertId(parentId));
        }

        public Folder GetRootFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            return selector.GetFolderDao(folderId).GetRootFolder(selector.ConvertId(folderId));
        }

        public Folder GetRootFolderByFile(object fileId)
        {
            var selector = GetSelector(fileId);
            return selector.GetFolderDao(fileId).GetRootFolderByFile(selector.ConvertId(fileId));
        }

        public List<Folder> GetFolders(object parentId)
        {
            var selector = GetSelector(parentId);
            return selector.GetFolderDao(parentId).GetFolders(selector.ConvertId(parentId));
        }

        public List<Folder> GetFolders(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            var selector = GetSelector(parentId);
            var result = selector.GetFolderDao(parentId).GetFolders(selector.ConvertId(parentId), orderBy, filterType, subjectID, searchText);

            if (!result.Any()) return new List<Folder>();

            if (!Default.IsMatch(parentId))
            {
                SetSharedByMeProperty(result);
            }

            return result;
        }

        public List<Folder> GetFolders(object[] folderIds)
        {
            var result = Enumerable.Empty<Folder>();

            foreach (var selector in GetSelectors())
            {
                var selectorLocal = selector;
                var mathedIds = folderIds.Where(selectorLocal.IsMatch);

                if (!mathedIds.Any()) continue;

                result = result.Concat(mathedIds.GroupBy(selectorLocal.GetIdCode)
                                                .SelectMany(y => selectorLocal.GetFolderDao(y.FirstOrDefault())
                                                                              .GetFolders(y.Select(selectorLocal.ConvertId).ToArray())));
            }

            return result.ToList();
        }

        public List<Folder> GetParentFolders(object folderId)
        {
            var selector = GetSelector(folderId);
            return selector.GetFolderDao(folderId).GetParentFolders(selector.ConvertId(folderId));
        }

        public object SaveFolder(Folder folder)
        {
            if (folder == null) throw new ArgumentNullException("folder");

            if (folder.ID != null)
            {
                var folderId = folder.ID;
                var selector = GetSelector(folderId);
                folder.ID = selector.ConvertId(folderId);
                var newFolderId = selector.GetFolderDao(folderId).SaveFolder(folder);
                folder.ID = folderId;
                return newFolderId;
            }
            if (folder.ParentFolderID != null)
            {
                var folderId = folder.ParentFolderID;
                var selector = GetSelector(folderId);
                folder.ParentFolderID = selector.ConvertId(folderId);
                var newFolderId = selector.GetFolderDao(folderId).SaveFolder(folder);
                folder.ParentFolderID = folderId;
                return newFolderId;

            }
            throw new ArgumentException("No folder id or parent folder id to determine provider");
        }

        public void DeleteFolder(object folderId)
        {
            var selector = GetSelector(folderId);
            selector.GetFolderDao(folderId).DeleteFolder(selector.ConvertId(folderId));
        }

        public object MoveFolder(object folderId, object toRootFolderId)
        {
            var selector = GetSelector(folderId);
            if (IsCrossDao(folderId, toRootFolderId))
            {
                return PerformCrossDaoFolderCopy(folderId, toRootFolderId, true).ID;
            }
            else
            {
                return selector.GetFolderDao(folderId).MoveFolder(selector.ConvertId(folderId), selector.ConvertId(toRootFolderId));
            }
        }

        public Folder CopyFolder(object folderId, object toRootFolderId)
        {
            var selector = GetSelector(folderId);
            return IsCrossDao(folderId, toRootFolderId)
                ? PerformCrossDaoFolderCopy(folderId, toRootFolderId, false)
                : selector.GetFolderDao(folderId).CopyFolder(selector.ConvertId(folderId), selector.ConvertId(toRootFolderId));
        }

        public IDictionary<object, string> CanMoveOrCopy(object[] folderIds, object to)
        {
            if (!folderIds.Any()) return new Dictionary<object, string>();

            var selector = GetSelectors().FirstOrDefault(x => folderIds.All(x.IsMatch));
            return selector.GetFolderDao(folderIds.FirstOrDefault()).CanMoveOrCopy(folderIds, to);
        }

        public object RenameFolder(object folderId, string newTitle)
        {
            var selector = GetSelector(folderId);
            return selector.GetFolderDao(folderId).RenameFolder(selector.ConvertId(folderId), newTitle);
        }

        public List<File> GetFiles(object parentId, OrderBy orderBy, FilterType filterType, Guid subjectID, string searchText)
        {
            var selector = GetSelector(parentId);
            var result = selector.GetFolderDao(parentId).GetFiles(selector.ConvertId(parentId), orderBy, filterType, subjectID, searchText);

            if (!result.Any()) return new List<File>();

            if (!Default.IsMatch(parentId))
            {
                SetSharedByMeProperty(result);
            }

            return result;
        }

        public List<object> GetFiles(object parentId, bool withSubfolders)
        {
            var selector = GetSelector(parentId);
            return selector.GetFolderDao(parentId).GetFiles(selector.ConvertId(parentId), withSubfolders);
        }

        public int GetItemsCount(object folderId, bool withSubfoldes)
        {
            var selector = GetSelector(folderId);
            return selector.GetFolderDao(folderId).GetItemsCount(selector.ConvertId(folderId), withSubfoldes);
        }

        public bool UseTrashForRemove(Folder folder)
        {
            var selector = GetSelector(folder.ID);
            return selector.GetFolderDao(folder.ID).UseTrashForRemove(folder);
        }

        public bool UseRecursiveOperation(object folderId, object toRootFolderId)
        {
            var selector = GetSelector(folderId);
            var useRecursive = selector.GetFolderDao(folderId).UseRecursiveOperation(folderId, null);
            if (toRootFolderId != null)
            {
                var toFolderSelector = GetSelector(toRootFolderId);
                useRecursive = useRecursive &&
                               toFolderSelector.GetFolderDao(toRootFolderId).
                                                UseRecursiveOperation(folderId, toFolderSelector.ConvertId(toRootFolderId));
            }
            return useRecursive;
        }

        public long GetMaxUploadSize(object folderId, bool chunkedUpload)
        {
            var selector = GetSelector(folderId);
            var storageMaxUploadSize = selector.GetFolderDao(folderId).GetMaxUploadSize(selector.ConvertId(folderId), chunkedUpload);

            if (storageMaxUploadSize == -1 || storageMaxUploadSize == long.MaxValue)
                storageMaxUploadSize = 1024L*1024L*1024L;

            return storageMaxUploadSize;
        }

        #region Only for TMFolderDao

        public IEnumerable<Folder> Search(string text, FolderType folderType)
        {
            return TryGetFolderDao().Search(text, folderType);
        }

        public object GetFolderID(string module, string bunch, string data, bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderID(module, bunch, data, createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public IEnumerable<object> GetFolderIDs(string module, string bunch, IEnumerable<string> data, bool createIfNotExists)
        {
            return TryGetFolderDao().GetFolderIDs(module, bunch, data, createIfNotExists);
        }

        public object GetFolderIDCommon(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDCommon(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public object GetFolderIDProjects(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDProjects(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public object GetFolderIDPhotos(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDProjects(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public string GetBunchObjectID(object folderID)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetBunchObjectID(folderID)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public object GetFolderIDUser(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDUser(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public object GetFolderIDShare(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDShare(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        public object GetFolderIDTrash(bool createIfNotExists)
        {
            return (from selector in GetSelectors()
                    let folderId = selector.GetFolderDao(null).GetFolderIDTrash(createIfNotExists)
                    where folderId != null
                    select folderId).FirstOrDefault();
        }

        #endregion
    }
}