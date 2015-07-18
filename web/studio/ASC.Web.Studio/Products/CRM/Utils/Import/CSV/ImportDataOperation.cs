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


using ASC.Common.Security.Authentication;
using ASC.Common.Threading.Progress;
using ASC.Common.Web;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Dao;
using ASC.Data.Storage;
using ASC.Web.CRM.Resources;
using ASC.Web.CRM.Services.NotifyService;
using ASC.Web.Studio.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ASC.Web.CRM.Classes
{
    public partial class ImportDataOperation : IProgressItem
    {
        #region Constructor

        public ImportDataOperation()
            : this(EntityType.Contact, String.Empty, String.Empty)
        {
        }

        public ImportDataOperation(EntityType entityType, String CSVFileURI, String importSettingsJSON)
        {
            _CSVFileURI = CSVFileURI;
            _dataStore = Global.GetStore();
            _tenantID = TenantProvider.CurrentTenantID;
            _daoFactory = Global.DaoFactory;
            _entityType = entityType;
            _author = SecurityContext.CurrentAccount;

            _notifyClient = NotifyClient.Instance;

            Id = String.Format("{0}_{1}", TenantProvider.CurrentTenantID, (int)_entityType);

            _log = LogManager.GetLogger("ASC.CRM");

            if (!String.IsNullOrEmpty(importSettingsJSON))
                _importSettings = new ImportCSVSettings(importSettingsJSON);
        }

        #endregion

        #region Members

        private readonly ILog _log;

        private readonly IDataStore _dataStore;

        private readonly IAccount _author;

        private readonly NotifyClient _notifyClient;

        private readonly int _tenantID;

        private readonly DaoFactory _daoFactory;

        private readonly String _CSVFileURI;

        private readonly ImportCSVSettings _importSettings;

        private readonly EntityType _entityType;

        private String[] _columns;

        #endregion

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is ImportDataOperation)) return false;

            var dataOperation = (ImportDataOperation)obj;

            if (_tenantID == dataOperation._tenantID && _entityType == dataOperation._entityType) return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _tenantID.GetHashCode() + _entityType.GetHashCode();
        }

        public object Clone()
        {
            var cloneObj = new ImportDataOperation
            {
                Error = Error,
                Id = Id,
                IsCompleted = IsCompleted,
                Percentage = Percentage,
                Status = Status
            };

            return cloneObj;
        }

        #region Property

        public object Id { get; set; }

        public object Status { get; set; }

        public object Error { get; set; }

        public double Percentage { get; set; }

        public bool IsCompleted { get; set; }

        #endregion

        private String GetPropertyValue(String propertyName)
        {
            if (_importSettings.ColumnMapping[propertyName] == null) return String.Empty;

            var values =
                _importSettings.ColumnMapping[propertyName].Values<int>().ToList().ConvertAll(columnIndex => _columns[columnIndex]);

            values.RemoveAll(item => item == String.Empty);

            return String.Join(",", values.ToArray());
        }

        private void Complete()
        {
            IsCompleted = true;

            Percentage = 100;

            _log.Debug("Import is completed");

            _notifyClient.SendAboutImportCompleted(_author.ID, _entityType);
        }

        public void RunJob()
        {
            CoreContext.TenantManager.SetCurrentTenant(_tenantID);

            SecurityContext.AuthenticateMe(_author);

            var userCulture = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).GetCulture();

            System.Threading.Thread.CurrentThread.CurrentCulture = userCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = userCulture;

            //Fake http context allows fearlessly use shared DbManager.
            bool fakeContext = HttpContext.Current == null;

            try
            {
                if (fakeContext)
                {
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("fake", CommonLinkUtility.GetFullAbsolutePath(PathProvider.BaseAbsolutePath), string.Empty),
                        new HttpResponse(new StringWriter()));
                }

                switch (_entityType)
                {
                    case EntityType.Contact:
                        ImportContactsData();
                        break;
                    case EntityType.Opportunity:
                        ImportOpportunityData();
                        break;
                    case EntityType.Case:
                        ImportCaseData();
                        break;
                    case EntityType.Task:
                        ImportTaskData();
                        break;
                    default:
                        throw new ArgumentException(CRMErrorsResource.EntityTypeUnknown);
                }
            }
            finally
            {
                if (fakeContext && HttpContext.Current != null)
                {
                    new DisposableHttpContext(HttpContext.Current).Dispose();
                    HttpContext.Current = null;
                }
            }
        }
    }

}