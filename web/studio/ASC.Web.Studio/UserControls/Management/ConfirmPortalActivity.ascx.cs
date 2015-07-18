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
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using AjaxPro;
using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("AjaxPro.ConfirmPortalActivity")]
    public partial class ConfirmPortalActivity : UserControl
    {
        protected ConfirmType _type;

        protected string _buttonTitle;
        protected string _successMessage;
        protected string _title;

        public static string Location
        {
            get { return "~/UserControls/Management/ConfirmPortalActivity.ascx"; }
        }

        private const string httpPrefix = "http://";

        protected void Page_Load(object sender, EventArgs e)
        {
            var dns = Request["dns"];
            var alias = Request["alias"];

            _type = GetConfirmType();
            switch (_type)
            {
                case ConfirmType.PortalContinue:
                    _buttonTitle = Resources.Resource.ReactivatePortalButton;
                    _title = Resources.Resource.ConfirmReactivatePortalTitle;
                    break;
                case ConfirmType.PortalRemove:
                    _buttonTitle = Resources.Resource.DeletePortalButton;
                    _title = Resources.Resource.ConfirmDeletePortalTitle;
                    AjaxPro.Utility.RegisterTypeForAjax(GetType());
                    break;

                case ConfirmType.PortalSuspend:
                    _buttonTitle = Resources.Resource.DeactivatePortalButton;
                    _title = Resources.Resource.ConfirmDeactivatePortalTitle;
                    break;

                case ConfirmType.DnsChange:
                    _buttonTitle = Resources.Resource.SaveButton;
                    var portalAddress = GenerateLink(GetTenantBasePath(alias));
                    if (!string.IsNullOrEmpty(dns))
                    {
                        portalAddress += string.Format(" ({0})", GenerateLink(dns));
                    }
                    _title = string.Format(Resources.Resource.ConfirmDnsUpdateTitle, portalAddress);
                    break;
            }


            if (IsPostBack && _type != ConfirmType.PortalRemove)
            {
                _successMessage = "";

                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                var updatedFlag = false;

                var messageAction = MessageAction.None;
                switch (_type)
                {
                    case ConfirmType.PortalContinue:
                        curTenant.SetStatus(TenantStatus.Active);
                        _successMessage = string.Format(Resources.Resource.ReactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        break;

                    case ConfirmType.PortalSuspend:
                        curTenant.SetStatus(TenantStatus.Suspended);
                        _successMessage = string.Format(Resources.Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        messageAction = MessageAction.PortalDeactivated;
                        break;

                    case ConfirmType.DnsChange:
                        if (!string.IsNullOrEmpty(dns))
                        {
                            dns = dns.Trim().TrimEnd('/', '\\');
                        }
                        if (curTenant.MappedDomain != dns)
                        {
                            updatedFlag = true;
                        }
                        curTenant.MappedDomain = dns;
                        if (!string.IsNullOrEmpty(alias))
                        {
                            if (curTenant.TenantAlias != alias)
                            {
                                updatedFlag = true;
                            }
                            curTenant.TenantAlias = alias;
                        }
                        _successMessage = string.Format(Resources.Resource.DeactivatePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
                        break;
                }

                bool authed = false;
                try
                {
                    if (!SecurityContext.IsAuthenticated)
                    {
                        SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                        authed = true;
                    }

                    #region Alias or dns update

                    if (IsChangeDnsMode)
                    {
                        if (updatedFlag)
                        {
                            CoreContext.TenantManager.SaveTenant(curTenant);
                        }
                        var redirectUrl = dns;
                        if (string.IsNullOrEmpty(redirectUrl))
                        {
                            redirectUrl = GetTenantBasePath(curTenant);
                        }
                        Response.Redirect(AddHttpToUrl(redirectUrl));
                        return;
                    }

                    #endregion


                    CoreContext.TenantManager.SaveTenant(curTenant);   
                    if (messageAction != MessageAction.None)
                    {
                        MessageService.Send(HttpContext.Current.Request, messageAction);
                    }
                }
                finally
                {
                    if (authed) SecurityContext.Logout();
                }

                var redirectLink = CommonLinkUtility.GetDefault();
                _successMessage = string.Format(_successMessage, redirectLink);

                _messageHolder.Visible = true;
                _confirmContentHolder.Visible = false;
            }
            else
            {
                _messageHolder.Visible = false;
                _confirmContentHolder.Visible = true;

                if (_type == ConfirmType.PortalRemove)
                    _messageHolderPortalRemove.Visible = true;
                else
                    _messageHolderPortalRemove.Visible = false;
            }
        }

        [AjaxMethod]
        public string PortalRemove()
        {
            var curTenant = CoreContext.TenantManager.GetCurrentTenant();

            CoreContext.TenantManager.RemoveTenant(curTenant.TenantId);
           
            var currentUser = CoreContext.UserManager.GetUsers(curTenant.OwnerId);
            var redirectLink = SetupInfo.TeamlabSiteRedirect + "/remove-portal-feedback-form.aspx#" +
                        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"firstname\":\"" + currentUser.FirstName +
                                                                                    "\",\"lastname\":\"" + currentUser.LastName +
                                                                                    "\",\"alias\":\"" + curTenant.TenantAlias +
                                                                                    "\",\"email\":\"" + currentUser.Email + "\"}"));

            bool authed = false;
            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    authed = true;
                }

                MessageService.Send(HttpContext.Current.Request, MessageAction.PortalDeleted);

            }
            finally
            {
                if (authed) SecurityContext.Logout();
            }

            _successMessage = string.Format(Resources.Resource.DeletePortalSuccessMessage, "<br/>", "<a href=\"{0}\">", "</a>");
            _successMessage = string.Format(_successMessage, redirectLink);

            StudioNotifyService.Instance.SendMsgPortalDeletionSuccess(curTenant, redirectLink);

            return JsonConvert.SerializeObject(
                new {
                    successMessage = _successMessage,
                    redirectLink = redirectLink }
                );

        }


        private ConfirmType GetConfirmType()
        {
            return typeof(ConfirmType).TryParseEnum(Request["type"] ?? "", ConfirmType.PortalContinue);
        }

        #region Tenant Base Path

        private static string GetTenantBasePath(string alias)
        {
            return String.Format("http://{0}.{1}", alias, SetupInfo.BaseDomain);
        }

        private static string GetTenantBasePath(Tenant tenant)
        {
            return GetTenantBasePath(tenant.TenantAlias);
        }

        private static string GenerateLink(string url)
        {
            url = AddHttpToUrl(url);
            return string.Format("<a href='{0}' class='link header-base middle bold' target='_blank'>{1}</a>", url, url.Substring(httpPrefix.Length));
        }

        private static string AddHttpToUrl(string url)
        {
            url = url ?? string.Empty;
            if (!url.StartsWith(httpPrefix))
            {
                url = httpPrefix + url;
            }
            return url;
        }

        #endregion

        protected bool IsChangeDnsMode
        {
            get { return GetConfirmType() == ConfirmType.DnsChange; }
        }
    }
}