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
using System.Configuration;
using System.Linq;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Studio.Core.Backup;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Migration, Location)]
    [AjaxNamespace("TransferPortal")]
    public partial class TransferPortal : UserControl
    {
        public const string Location = "~/UserControls/Management/TransferPortal/TransferPortal.ascx";

        protected class TransferRegionWithName : TransferRegion
        {
            public string FullName { get; set; }
        }

        private List<TransferRegionWithName> _transferRegions;

        protected string CurrentRegion
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.Name).FirstOrDefault() ?? string.Empty; }
        }

        protected string BaseDomain
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.BaseDomain).FirstOrDefault() ?? string.Empty; }
        }

        protected List<TransferRegionWithName> TransferRegions
        {
            get { return _transferRegions ?? (_transferRegions = GetRegions()); }
        }

        protected bool IsVisibleMigration
        {
            get
            {
                return (ConfigurationManager.AppSettings["web.migration.status"] == "true") && TransferRegions.Count > 1;
            }
        }

        protected bool EnableMigration
        {
            get
            {
                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var quota = TenantExtra.GetTenantQuota();
                return SetupInfo.IsSecretEmail(currentUser.Email) || currentUser.IsOwner() && !quota.Trial && !quota.Free;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/TransferPortal/js/transferportal.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/transferportal/css/transferportal.less"));

            popupTransferStart.Options.IsPopup = true;
        }

        private static List<TransferRegionWithName> GetRegions()
        {
            try
            {
                using (var backupClient = new BackupServiceClient())
                {
                    return backupClient.GetTransferRegions()
                                       .Select(x => new TransferRegionWithName
                                           {
                                               Name = x.Name,
                                               IsCurrentRegion = x.IsCurrentRegion,
                                               BaseDomain = x.BaseDomain,
                                               FullName = TransferResourceHelper.GetRegionDescription(x.Name)
                                           })
                                       .ToList();
                }
            }
            catch
            {
                return new List<TransferRegionWithName>();
            }
        }
    }
}