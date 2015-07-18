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
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.MessagingSystem;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core.Users;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Utility;
using System.Web;
using System.Text;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    public class Item
    {
        public bool Disabled { get; set; }
        public bool DisplayedAlways { get; set; }
        public bool HasPermissionSettings { get; set; }
        public bool CanNotBeDisabled { get; set; }
        public string Name { get; set; }
        public string ItemName { get; set; }
        public string IconUrl { get; set; }
        public string DisabledIconUrl { get; set; }
        public string AccessSwitcherLabel { get; set; }
        public string UserOpportunitiesLabel { get; set; }
        public List<string> UserOpportunities { get; set; }
        public Guid ID { get; set; }
        public List<Item> SubItems { get; set; }
        public List<UserInfo> SelectedUsers { get; set; }
        public List<GroupInfo> SelectedGroups { get; set; }
    }

    [ManagementControl(ManagementType.AccessRights, Location)]
    [AjaxNamespace("AccessRightsController")]
    public partial class AccessRights : UserControl
    {
        #region Properies

        public const string Location = "~/UserControls/Management/AccessRights/AccessRights.ascx";

        protected bool CanOwnerEdit;
        protected bool AdvancedRightsEnabled = true;

        protected bool EnableAR = true;

        protected List<IProduct> Products;
        protected List<IProduct> ProductsForAccessSettings;

        protected string[] FullAccessOpportunities { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControl();
            RegisterClientScript();
        }

        protected void RptProductsItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item)
            {
                var phProductItem = (PlaceHolder)e.Item.FindControl("phProductItem");
                var productItem = (AccessRightsProductItem)LoadControl(AccessRightsProductItem.Location);
                productItem.ProductItem = (Item)e.Item.DataItem;
                phProductItem.Controls.Add(productItem);
            }
        }

        #endregion

        #region Methods

        private void InitControl()
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Products = WebItemManager.Instance.GetItemsAll<IProduct>();
            ProductsForAccessSettings = Products.Where(n => String.Compare(n.GetSysName(), "people") != 0).ToList();

            //owner settings
            var curTenant = CoreContext.TenantManager.GetCurrentTenant();
            var currentOwner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);
            CanOwnerEdit = currentOwner.ID.Equals(SecurityContext.CurrentAccount.ID);

            _phOwnerCard.Controls.Add(new EmployeeUserCard
                {
                    EmployeeInfo = currentOwner,
                    EmployeeUrl = currentOwner.GetUserProfilePageURL(),
                });

            var managementPage = Page as Studio.Management;
            var tenantAccess = managementPage != null ? managementPage.TenantAccess : SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID);

            if (tenantAccess.Anyone)
            {
                AdvancedRightsEnabled = false;
            }
            else
            {
                rptProducts.DataSource = GetDataSource();
                rptProducts.ItemDataBound += RptProductsItemDataBound;
                rptProducts.DataBind();
            }

            FullAccessOpportunities = Resource.AccessRightsFullAccessOpportunities.Split('|');
        }

        private void RegisterClientScript()
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/accessrights/js/accessrights.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/accessrights/css/accessrights.less"));

            var curTenant = CoreContext.TenantManager.GetCurrentTenant();
            var currentOwner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);
            var admins = WebItemSecurity.GetProductAdministrators(Guid.Empty).Where(admin => admin.ID != currentOwner.ID).SortByUserName();

            var sb = new StringBuilder();

            sb.AppendFormat("ownerId = {0};", JavaScriptSerializer.Serialize(curTenant.OwnerId));

            sb.AppendFormat("adminList = {0};",
                            JavaScriptSerializer.Serialize(admins.ConvertAll(u => new
                                {
                                    id = u.ID,
                                    smallFotoUrl = u.GetSmallPhotoURL(),
                                    displayName = u.DisplayUserName(),
                                    title = u.Title.HtmlEncode(),
                                    userUrl = CommonLinkUtility.GetUserProfile(u.ID),
                                    accessList = GetAccessList(u.ID)
                                }))
                );
            sb.AppendFormat("ASC.Settings.AccessRights.init({0},\"{1}\");",
                            JavaScriptSerializer.Serialize(Products.Select(p => p.GetSysName()).ToArray()),
                            CustomNamingPeople.Substitute<Resource>("AccessRightsAddGroup").HtmlEncode()
                );

            Page.RegisterInlineScript(sb.ToString());
        }

        private List<Item> GetDataSource()
        {
            var data = new List<Item>();

            foreach (var p in Products)
            {
                var item = new Item
                    {
                        ID = p.ID,
                        Name = p.Name,
                        IconUrl = p.GetIconAbsoluteURL(),
                        DisabledIconUrl = p.GetDisabledIconAbsoluteURL(),
                        SubItems = new List<Item>(),
                        ItemName = p.GetSysName(),
                        UserOpportunitiesLabel = String.Format(Resource.AccessRightsProductUsersCan, p.Name),
                        UserOpportunities = p.GetUserOpportunities(),
                        CanNotBeDisabled = p.CanNotBeDisabled()
                    };

                if (p.HasComplexHierarchyOfAccessRights())
                    item.UserOpportunitiesLabel = String.Format(Resource.AccessRightsProductUsersWithRightsCan, item.Name);

                var productInfo = WebItemSecurity.GetSecurityInfo(item.ID.ToString());
                item.Disabled = !productInfo.Enabled;
                item.SelectedGroups = productInfo.Groups.ToList();
                item.SelectedUsers = productInfo.Users.ToList();

                data.Add(item);
            }

            return data;
        }

        private object GetAccessList(Guid uId)
        {
            var fullAccess = WebItemSecurity.IsProductAdministrator(Guid.Empty, uId);
            var res = new List<object>
                {
                    new
                        {
                            pId = Guid.Empty,
                            pName = "full",
                            pAccess = fullAccess,
                            disabled = uId == SecurityContext.CurrentAccount.ID
                        }
                };

            if (ProductsForAccessSettings == null)
            {
                Products = WebItemManager.Instance.GetItemsAll<IProduct>();
                ProductsForAccessSettings = Products.Where(n => String.Compare(n.GetSysName(), "people") != 0).ToList();
            }

            foreach (var p in ProductsForAccessSettings)
                res.Add(new
                    {
                        pId = p.ID,
                        pName = p.GetSysName(),
                        pAccess = WebItemSecurity.IsProductAdministrator(p.ID, uId),
                        disabled = fullAccess
                    });

            return res;
        }

        #endregion

        #region AjaxMethods

        [AjaxMethod]
        public object ChangeOwner(Guid ownerId)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                var owner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);

                if (owner.IsVisitor())
                    throw new System.Security.SecurityException("Collaborator can not be an owner");

                if (curTenant.OwnerId.Equals(SecurityContext.CurrentAccount.ID) && !Guid.Empty.Equals(ownerId))
                {
                    var confirmLink = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalOwnerChange, ownerId, ownerId);
                    StudioNotifyService.Instance.SendMsgConfirmChangeOwner(curTenant,CoreContext.UserManager.GetUsers(ownerId).DisplayUserName(), confirmLink);

                    MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentChangeOwnerInstructions, owner.DisplayUserName(false));

                    var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email);
                    return new { Status = 1, Message = Resource.ChangePortalOwnerMsg.Replace(":email", emailLink) };
                }

                return new { Status = 0, Message = Resource.ErrorAccessDenied };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        [AjaxMethod]
        public object AddAdmin(Guid id)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var user = CoreContext.UserManager.GetUsers(id);

            if (user.IsVisitor())
                throw new System.Security.SecurityException("Collaborator can not be an administrator");

            WebItemSecurity.SetProductAdministrator(Guid.Empty, id, true);

            var result = new
                {
                    id = user.ID,
                    smallFotoUrl = user.GetSmallPhotoURL(),
                    displayName = user.DisplayUserName(),
                    title = user.Title.HtmlEncode(),
                    userUrl = CommonLinkUtility.GetUserProfile(user.ID),
                    accessList = GetAccessList(user.ID)
                };

            MessageService.Send(HttpContext.Current.Request, MessageAction.AdministratorAdded, user.DisplayUserName(false));

            return result;
        }

        #endregion
    }
}