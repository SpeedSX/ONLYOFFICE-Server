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
using ASC.Core;
using ASC.Files.Thirdparty.GoogleDrive;
using ASC.Files.Thirdparty.SharePoint;
using ASC.Files.Thirdparty.Sharpbox;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Files.Core.Security;
using ASC.Web.Files.Utils;

namespace ASC.Files.Thirdparty.ProviderDao
{
    internal class ProviderDaoBase
    {
        private static readonly List<IDaoSelector> Selectors = new List<IDaoSelector>();

        protected static IDaoSelector Default { get; private set; }

        static ProviderDaoBase()
        {
            Default = new DbDaoSelector(
                x => new DaoFactory().GetFileDao(),
                x => new DaoFactory().GetFolderDao(),
                x => new DaoFactory().GetSecurityDao(),
                x => new DaoFactory().GetTagDao());

            //Fill in selectors
            Selectors.Add(Default); //Legacy DB dao
            Selectors.Add(new SharpBoxDaoSelector());
            Selectors.Add(new SharePointDaoSelector());
            Selectors.Add(new GoogleDriveDaoSelector());
        }

        protected bool IsCrossDao(object id1, object id2)
        {
            if (id2 == null || id1 == null)
                return false;
            return !Equals(GetSelector(id1).GetIdCode(id1), GetSelector(id2).GetIdCode(id2));
        }

        protected IDaoSelector GetSelector(object id)
        {
            return Selectors.FirstOrDefault(selector => selector.IsMatch(id)) ?? Default;
        }

        protected void SetSharedByMeProperty(IEnumerable<FileEntry> entries)
        {
            TryGetSecurityDao()
                .GetPureShareRecords(entries.ToArray())
                .Where(x => x.Owner == SecurityContext.CurrentAccount.ID)
                .Select(x => x.EntryId).Distinct().ToList()
                .ForEach(id =>
                             {
                                 var firstEntry = entries.FirstOrDefault(y => y.ID.Equals(id));

                                 if (firstEntry != null)
                                     firstEntry.SharedByMe = true;
                             });
        }

        protected IEnumerable<IDaoSelector> GetSelectors()
        {
            return Selectors;
        }

        //For working with function where no id is availible
        protected IFileDao TryGetFileDao()
        {
            foreach (var daoSelector in Selectors)
            {
                try
                {
                    return daoSelector.GetFileDao(null);
                }
                catch (Exception)
                {

                }
            }
            throw new InvalidOperationException("No DAO can't be instanced without ID");
        }


        //For working with function where no id is availible
        protected ISecurityDao TryGetSecurityDao()
        {
            foreach (var daoSelector in Selectors)
            {
                try
                {
                    return daoSelector.GetSecurityDao(null);
                }
                catch (Exception)
                {

                }
            }
            throw new InvalidOperationException("No DAO can't be instanced without ID");
        }

        //For working with function where no id is availible
        protected ITagDao TryGetTagDao()
        {
            foreach (var daoSelector in Selectors)
            {
                try
                {
                    return daoSelector.GetTagDao(null);
                }
                catch (Exception)
                {

                }
            }
            throw new InvalidOperationException("No DAO can't be instanced without ID");
        }

        //For working with function where no id is availible
        protected IFolderDao TryGetFolderDao()
        {
            foreach (var daoSelector in Selectors)
            {
                try
                {
                    return daoSelector.GetFolderDao(null);
                }
                catch (Exception)
                {

                }
            }
            throw new InvalidOperationException("No DAO can't be instanced without ID");
        }

        protected File PerformCrossDaoFileCopy(object fromFileId, object toFolderId, bool deleteSourceFile)
        {
            var fromSelector = GetSelector(fromFileId);
            var toSelector = GetSelector(toFolderId);
            //Get File from first dao
            var fromFileDao = fromSelector.GetFileDao(fromFileId);
            var toFileDao = toSelector.GetFileDao(toFolderId);
            var fromFile = fromFileDao.GetFile(fromSelector.ConvertId(fromFileId));

            var fromFileShareRecords = TryGetSecurityDao().GetPureShareRecords(fromFile).Where(x => x.EntryType == FileEntryType.File);
            var fromFileNewTags = TryGetTagDao().GetNewTags(Guid.Empty, fromFile);

            var toFile = toFileDao.GetFile(toSelector.ConvertId(toFolderId), fromFile.Title);

            if (toFile == null)
            {
                fromFile.ID = fromSelector.ConvertId(fromFile.ID);

                var mustConvert = !string.IsNullOrEmpty(fromFile.ConvertedType);
                using (var fromFileStream = mustConvert
                                                ? FileConverter.Exec(fromFile)
                                                : fromFileDao.GetFileStream(fromFile))
                {
                    fromFile.ID = null; //Reset id, so it can be created by apropriate provider
                    fromFile.FolderID = toSelector.ConvertId(toFolderId);
                    toFile = toFileDao.SaveFile(fromFile, fromFileStream);
                }
            }

            if (deleteSourceFile)
            {
                if (fromFileShareRecords.Any())
                    fromFileShareRecords.ToList().ForEach(x =>
                                                              {
                                                                  x.EntryId = toFile.ID;
                                                                  TryGetSecurityDao().SetShare(x);
                                                              });

                if (fromFileNewTags.Any())
                {
                    fromFileNewTags.ToList().ForEach(x => x.EntryId = toFile.ID);

                    TryGetTagDao().SaveTags(fromFileNewTags.ToArray());
                }

                //Delete source file if needed
                fromFileDao.DeleteFileStream(fromSelector.ConvertId(fromFileId));
                fromFileDao.DeleteFile(fromSelector.ConvertId(fromFileId));
            }

            return toFile;
        }

        protected Folder PerformCrossDaoFolderCopy(object fromFolderId, object toRootFolderId, bool deleteSourceFolder)
        {
            //Things get more complicated
            var fromSelector = GetSelector(fromFolderId);
            var toSelector = GetSelector(toRootFolderId);

            var fromFolderDao = fromSelector.GetFolderDao(fromFolderId);
            //Create new folder in 'to' folder
            var toFolderDao = toSelector.GetFolderDao(toRootFolderId);
            //Ohh
            var fromFolder = fromFolderDao.GetFolder(fromSelector.ConvertId(fromFolderId));

            var toFolder = toFolderDao.GetFolder(fromFolder.Title, toSelector.ConvertId(toRootFolderId));
            var toFolderId = toFolder != null
                                 ? toFolder.ID
                                 : toFolderDao.SaveFolder(
                                     new Folder
                                         {
                                             Title = fromFolder.Title,
                                             ParentFolderID = toSelector.ConvertId(toRootFolderId)
                                         });

            var foldersToCopy = fromFolderDao.GetFolders(fromSelector.ConvertId(fromFolderId));
            var filesToCopy = fromFolderDao.GetFiles(fromSelector.ConvertId(fromFolderId), false);
            //Copy files first
            foreach (var file in filesToCopy)
            {
                PerformCrossDaoFileCopy(file, toFolderId, deleteSourceFolder);
            }
            foreach (var folder in foldersToCopy)
            {
                PerformCrossDaoFolderCopy(folder.ID, toFolderId, deleteSourceFolder);
            }

            if (deleteSourceFolder)
            {
                var fromFileSecurityDao = TryGetSecurityDao();
                var toFileSecurityDao = TryGetSecurityDao();

                var fromFileShareRecords = fromFileSecurityDao.GetPureShareRecords(fromFolder)
                                                              .Where(x => x.EntryType == FileEntryType.Folder);

                if (fromFileShareRecords.Any())
                    fromFileShareRecords.ToList().ForEach(x =>
                                                              {
                                                                  x.EntryId = toFolderId;
                                                                  toFileSecurityDao.SetShare(x);
                                                              });

                var fromFileTagDao = TryGetTagDao();
                var toFileTagDao = TryGetTagDao();

                var fromFileNewTags = fromFileTagDao.GetNewTags(Guid.Empty, fromFolder);

                if (fromFileNewTags.Any())
                {
                    fromFileNewTags.ToList().ForEach(x => x.EntryId = toFolderId);

                    toFileTagDao.SaveTags(fromFileNewTags.ToArray());
                }

                fromFolderDao.DeleteFolder(fromSelector.ConvertId(fromFolderId));
            }

            return toFolderDao.GetFolder(toFolderId);
        }
    }
}