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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.UserControls.Common;
using ASC.Web.Studio.UserControls.Common.Banner;
using System.Configuration;
using ASC.Web.Core;
using ASC.Web.Core.WebZones;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Utility;
using ASC.Core.Billing;
using ASC.Web.Studio.UserControls.Management;

namespace ASC.Web.Studio.Masters
{
    public partial class BaseTemplate : MasterPage
    {
        /// <summary>
        /// Block side panel
        /// </summary>
        /// 
        protected string TariffNotify;

        protected string TariffNotifyText;
        public bool DisableTariffNotify { get; set; }

        public bool DisabledSidePanel { get; set; }

        public bool DisabledHelpTour { get; set; }

        public bool DisabledTopStudioPanel { get; set; }

        public bool EnabledWebChat { get; set; }

        public string HubUrl { get; set; }

        public bool IsMobile { get; set; }

        public TopStudioPanel TopStudioPanel;

        private static Regex _browserNotSupported;

        protected bool DisablePartnerPanel { get; set; }
        private bool? IsAuthorizedPartner { get; set; }
        protected Partner Partner { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            TopStudioPanel = (TopStudioPanel)LoadControl(TopStudioPanel.Location);
            MetaKeywords.Content = Resources.Resource.MetaKeywords;
            MetaDescription.Content = Resources.Resource.MetaDescription;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();
            HubUrl = ConfigurationManager.AppSettings["web.hub"] ?? string.Empty;
            EnabledWebChat = Convert.ToBoolean(ConfigurationManager.AppSettings["web.chat"] ?? "false") &&
                             WebItemManager.Instance.GetItems(WebZoneType.CustomProductList, ItemAvailableState.Normal).
                                            Any(id => id.ID == WebItemManager.TalkProductID) &&
                             !(Request.Browser != null && Request.Browser.Browser == "IE" &&
                               (Request.Browser.MajorVersion == 8 || Request.Browser.MajorVersion == 9 || Request.Browser.MajorVersion == 10));

            IsMobile = MobileDetector.IsMobile;

            if (!DisabledSidePanel && EnabledWebChat && !IsMobile)
            {
                SmallChatHolder.Controls.Add(LoadControl(UserControls.Common.SmallChat.SmallChat.Location));
            }

            if (!DisabledSidePanel)
            {
                /** InvitePanel popup **/
                InvitePanelHolder.Controls.Add(LoadControl(InvitePanel.Location));
            }

            if ((!DisabledSidePanel || !DisabledTopStudioPanel)
                && HubUrl != string.Empty && !Request.Path.Equals("/auth.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                AddBodyScripts(ResolveUrl("~/js/third-party/jquery/jquery.signalr.js"));
                AddBodyScripts(ResolveUrl("~/js/third-party/jquery/jquery.hubs.js"));
            }

            if (!DisabledTopStudioPanel)
            {
                TopContent.Controls.Add(TopStudioPanel);
            }

            if (_browserNotSupported == null && !string.IsNullOrEmpty(WebConfigurationManager.AppSettings["web.browser-not-supported"]))
            {
                _browserNotSupported = new Regex(WebConfigurationManager.AppSettings["web.browser-not-supported"], RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            }
            if (_browserNotSupported != null && !string.IsNullOrEmpty(Request.Headers["User-Agent"]) && _browserNotSupported.Match(Request.Headers["User-Agent"]).Success)
            {
                Response.Redirect("~/browser-not-supported.htm");
            }

            if (!EmailActivated && !CoreContext.Configuration.Personal && SecurityContext.IsAuthenticated)
            {
                activateEmailPanel.Controls.Add(LoadControl(ActivateEmailPanel.Location));
            }

            if (AffiliateHelper.BannerAvailable || CoreContext.Configuration.Personal)
            {
                BannerHolder.Controls.Add(LoadControl(Banner.Location));
            }

            DisabledHelpTour = true;
            if (!DisabledHelpTour)
            {
                HeaderContent.Controls.Add(LoadControl(UserControls.Common.HelpTour.HelpTour.Location));
            }

            if (!SecurityContext.IsAuthenticated || !TenantExtra.EnableTarrifSettings || CoreContext.Configuration.Personal
                || CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor())
            {
                DisableTariffNotify = true;
            }
            else
            {
                TariffNotify = TenantExtra.GetTariffNotify();
                TariffNotifyText = TenantExtra.GetTariffNotifyText();
                if (string.IsNullOrEmpty(TariffNotify) ||
                    (TenantExtra.GetCurrentTariff().State == TariffState.Trial && TenantExtra.GetCurrentTariff().DueDate.Subtract(DateTime.Today.Date).Days > 5))
                    DisableTariffNotify = true;
            }

            if (!DisableTariffNotify)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("if (jq('div.mainPageLayout table.mainPageTable').hasClass('with-mainPageTableSidePanel'))jq('#infoBoxTariffNotify').removeClass('display-none');");
                Page.RegisterInlineScript(stringBuilder.ToString());
            }

            if (CoreContext.Configuration.PartnerHosted)
            {
                IsAuthorizedPartner = false;
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner != null)
                {
                    IsAuthorizedPartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                    Partner = partner;
                }
            }
            DisablePartnerPanel = !(IsAuthorizedPartner.HasValue && !IsAuthorizedPartner.Value);

            if (!DisablePartnerPanel)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("if (jq('div.mainPageLayout table.mainPageTable').hasClass('with-mainPageTableSidePanel'))jq('#infoBoxPartnerPanel').removeClass('display-none');");
                Page.RegisterInlineScript(stringBuilder.ToString());
            }
        }

        protected string RenderStatRequest()
        {
            if (string.IsNullOrEmpty(SetupInfo.StatisticTrackURL)) return string.Empty;

            var page = HttpUtility.UrlEncode(Page.AppRelativeVirtualPath.Replace("~", ""));
            return String.Format("<img style=\"display:none;\" src=\"{0}\"/>", SetupInfo.StatisticTrackURL + "&page=" + page);
        }

        protected string RenderCustomScript()
        {
            var sb = new StringBuilder();
            //custom scripts
            foreach (var script in SetupInfo.CustomScripts.Where(script => !String.IsNullOrEmpty(script)))
            {
                sb.AppendFormat("<script language=\"javascript\" src=\"{0}\" type=\"text/javascript\"></script>", script);
            }

            return sb.ToString();
        }

        protected bool EmailActivated
        {
            get { return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).ActivationStatus == EmployeeActivationStatus.Activated; }
        }

        protected string ColorThemeClass
        {
            get { return ColorThemesSettings.GetColorThemesSettings(); }
        }

        #region Operations

        private void InitScripts()
        {
            AddStyles("~/skins/<theme_folder>/main.less", true);

            AddClientScript(typeof(MasterResources.MasterSettingsResources));
            AddClientScript(typeof(MasterResources.MasterUserResources));
            AddClientScript(typeof(MasterResources.MasterFileUtilityResources));
            AddClientScript(typeof(MasterResources.MasterCustomResources));

            InitProductSettingsInlineScript();
            InitStudioSettingsInlineScript();

            if (ClientLocalizationScript != null)
            {
                AddClientLocalizationScript(typeof(MasterResources.MasterLocalizationResources));
                AddClientLocalizationScript(typeof(MasterResources.MasterTemplateResources));

                if (clientScriptReference == null)
                {
                    clientScriptReference = new ClientScriptReference();
                }
                if (0 < clientScriptReference.Includes.Count)
                {
                    ClientLocalizationScript.Scripts.Add(clientScriptReference.GetLink(true));
                }
            }
        }

        private void InitStudioSettingsInlineScript()
        {
            var showPromotions = SettingsManager.Instance.LoadSettings<PromotionsSettings>(TenantProvider.CurrentTenantID).Show;
            var showTips = SettingsManager.Instance.LoadSettingsFor<TipsSettings>(SecurityContext.CurrentAccount.ID).Show;

            var script = new StringBuilder();
            script.Append("window.StudioSettings=window.StudioSettings||{};");
            script.AppendFormat("window.StudioSettings.ShowPromotions={0};", showPromotions.ToString().ToLowerInvariant());
            script.AppendFormat("window.StudioSettings.ShowTips={0};", showTips.ToString().ToLowerInvariant());

            Page.RegisterInlineScript(script.ToString(), true, false);
        }

        private void InitProductSettingsInlineScript()
        {
            var isAdmin = WebItemSecurity.IsProductAdministrator(CommonLinkUtility.GetProductID(), SecurityContext.CurrentAccount.ID);

            var script = new StringBuilder();
            script.Append("window.ProductSettings=window.ProductSettings||{};");
            script.AppendFormat("window.ProductSettings.IsProductAdmin={0};", isAdmin.ToString().ToLowerInvariant());

            Page.RegisterInlineScript(script.ToString(), true, false);
        }

        #region Style

        public void AddStyles(Control control, bool theme)
        {
            if (theme)
            {
                if (ThemeStyles == null) return;

                ThemeStyles.Controls.Add(control);
            }
            else
            {
                if (HeadStyles == null) return;

                HeadStyles.Controls.Add(control);
            }
        }

        public void AddStyles(string src, bool theme)
        {
            if (theme)
            {
                if (ThemeStyles == null) return;

                ThemeStyles.Styles.Add(ResolveUrl(ColorThemesSettings.GetThemeFolderName(src)));
            }
            else
            {
                if (HeadStyles == null) return;

                HeadStyles.Styles.Add(src);
            }
        }

        #endregion

        #region Scripts

        public void AddBodyScripts(string src)
        {
            if (BodyScripts == null) return;
            BodyScripts.Scripts.Add(src);
        }

        public void AddLocalizationScripts(string src)
        {
            if (ClientLocalizationScript == null) return;
            ClientLocalizationScript.Scripts.Add(src);
        }

        public void AddBodyScripts(Control control)
        {
            if (BodyScripts == null) return;
            BodyScripts.Controls.Add(control);
        }


        public void RegisterInlineScript(string script, bool beforeBodyScripts, bool onReady)
        {
            if (!beforeBodyScripts)
                InlineScript.Scripts.Add(new Tuple<string, bool>(script, onReady));
            else
                InlineScriptBefore.Scripts.Add(new Tuple<string, bool>(script, onReady));
        }

        #endregion

        #region Content

        public void AddBodyContent(Control control)
        {
            PageContent.Controls.Add(control);
        }

        public void SetBodyContent(Control control)
        {
            PageContent.Controls.Clear();
            AddBodyContent(control);
        }

        #endregion

        #region ClientScript

        public void AddClientScript(Type type)
        {
            baseTemplateMasterScripts.Includes.Add(type);
        }

        private ClientScriptReference clientScriptReference;

        public void AddClientLocalizationScript(Type type)
        {
            if (clientScriptReference == null)
                clientScriptReference = new ClientScriptReference();

            clientScriptReference.Includes.Add(type);
        }

        #endregion

        #endregion
    }
}