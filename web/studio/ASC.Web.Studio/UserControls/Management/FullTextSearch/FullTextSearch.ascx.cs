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
using System.Web.UI;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using ASC.FullTextIndex;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.FullTextSearch, "~/UserControls/Management/FullTextSearch/FullTextSearch.ascx")]
    [AjaxNamespace("FullTextSearch")]
    public partial class FullTextSearch : UserControl
    {
        protected FullTextSearchSettings CurrentSettings
        {
            get
            {
                return CoreContext.Configuration.GetSection<FullTextSearchSettings>(Tenant.DEFAULT_TENANT) ?? new FullTextSearchSettings();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!CoreContext.Configuration.Standalone)
                Response.Redirect("~/management.aspx");

            AjaxPro.Utility.RegisterTypeForAjax(GetType(), Page);
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/fulltextsearch/js/fulltextsearch.js"));
            Page.RegisterStyleControl(WebPath.GetPath("usercontrols/management/fulltextsearch/css/fulltextsearch.css"));
        }

        [AjaxMethod]
        public void Save(FullTextSearchSettings settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.Configuration.SaveSection(Tenant.DEFAULT_TENANT, settings);
        }

        [AjaxMethod]
        public object Test()
        {
            return FullTextIndex.FullTextSearch.CheckState() ?
                new { success = true, message = Resources.Resource.FullTextSearchServiceIsRunning } :
                new { success = false, message = Resources.Resource.FullTextSearchServiceIsNotRunning };
        }
    }
}