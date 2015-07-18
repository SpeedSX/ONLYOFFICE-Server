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
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin.Profile;
using ASC.Geolocation;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

using AjaxPro;
using log4net;

namespace ASC.Web.Studio
{
    /// <summary>
    /// Base page for all pages in projects
    /// </summary>
    public class MainPage : Page
    {
        protected virtual bool MayNotAuth { get; set; }

        protected virtual bool MayNotPaid { get; set; }

        protected virtual bool CheckWizardCompleted { get { return !CoreContext.Configuration.Standalone || WarmUp.Instance.Completed; } }

        protected virtual bool RedirectToStartup
        {
            get
            {
                if(!CoreContext.Configuration.Standalone) return false;
                return !WarmUp.Instance.Completed && Request.QueryString["warmup"] != "true";
            }
        }

        protected virtual bool MayPhoneNotActivate { get; set; }

        protected static ILog Log
        {
            get { return LogManager.GetLogger("ASC.Web"); }
        }

        protected void Page_PreInit(object sender, EventArgs e)
        {
            if(RedirectToStartup)
            {
                Response.Redirect("~/Startup.aspx");
            }

            if (CheckWizardCompleted)
            {
                var s = SettingsManager.Instance.LoadSettings<WizardSettings>(TenantProvider.CurrentTenantID);
                if (!s.Completed)
                {
                    Response.Redirect("~/wizard.aspx");
                }
            }

            //check auth
            if (!SecurityContext.IsAuthenticated
                && !AuthByCookies()
                && !MayNotAuth)
            {
                if (SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone)
                {
                    OutsideAuth();
                }
                else
                {
                    var refererURL = GetRefererUrl();
                    Session["refererURL"] = refererURL;
                    Response.Redirect("~/auth.aspx", true);
                }
            }

            if (!MayNotPaid && TenantStatisticsProvider.IsNotPaid())
            {
                Response.Redirect(TenantExtra.GetTariffPageLink(), true);
            }

            if (SecurityContext.IsAuthenticated
                && StudioSmsNotificationSettings.IsVisibleSettings
                && StudioSmsNotificationSettings.Enable
                && !MayPhoneNotActivate)
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                if (!CoreContext.UserManager.IsUserInGroup(SecurityContext.CurrentAccount.ID, Constants.GroupAdmin.ID)
                    && (string.IsNullOrEmpty(user.MobilePhone)
                        || user.MobilePhoneActivationStatus == MobilePhoneActivationStatus.NotActivated))
                {
                    Response.Redirect(CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PhoneActivation), true);
                }
            }

            //check disable and public 
            var webitem = CommonLinkUtility.GetWebItemByUrl(Request.Url.ToString());
            var parentIsDisabled = false;
            if (webitem != null && webitem.IsSubItem())
            {
                var parentItemID = WebItemManager.Instance.GetParentItemID(webitem.ID);
                parentIsDisabled = WebItemManager.Instance[parentItemID].IsDisabled();
            }

            if (webitem != null && (webitem.IsDisabled() || parentIsDisabled) && !MayNotAuth)
            {
                if (webitem.ID == WebItemManager.PeopleProductID
                    && string.Equals(GetType().BaseType.FullName, "ASC.Web.People.Profile"))
                {
                    Response.Redirect("~/my.aspx", true);
                }

                Response.Redirect("~/", true);
            }

            if (SecurityContext.IsAuthenticated)
            {
                try
                {
                    StatisticManager.SaveUserVisit(TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, CommonLinkUtility.GetProductID());
                }
                catch (Exception exc)
                {
                    Log.Error("failed save user visit", exc);
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            InitInlineScript();
        }

        private static bool AuthByCookies()
        {
            var cookiesKey = CookiesManager.GetCookies(CookiesType.AuthKey);
            if (string.IsNullOrEmpty(cookiesKey)) return false;

            try
            {
                if (SecurityContext.AuthenticateMe(cookiesKey)) return true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("AutoAuthByCookies Error {0}", ex);
            }

            return false;
        }

        private string GetRefererUrl()
        {
            var refererURL = Request.GetUrlRewriter().AbsoluteUri;
            if (String.IsNullOrEmpty(refererURL)
                || refererURL.IndexOf("Subgurim_FileUploader", StringComparison.InvariantCultureIgnoreCase) != -1
                || (this is _Default)
                || (this is ServerError)
                )
                refererURL = (string)Session["refererURL"];

            return refererURL;
        }

        private void InitInlineScript()
        {
            var scripts = HttpContext.Current.Items[Constant.AjaxID + ".pagescripts"] as ListDictionary;

            if (scripts == null) return;

            var sb = new StringBuilder();

            foreach (var key in scripts.Keys)
            {
                sb.Append(scripts[key]);
            }

            this.RegisterInlineScript(sb.ToString(), onReady: false);
        }

        protected void SetLanguage(bool checkIp = true, bool abTesting = false)
        {
            var abTestingQuery = string.Empty;
            if (abTesting) abTesting = AbTestingQuery(out abTestingQuery);

            if (Request.QueryString.Count == 0)
            {
                var ipGeolocationInfo = new GeolocationHelper("teamlabsite").GetIPGeolocationFromHttpContext();
                if (checkIp && ipGeolocationInfo != null && !string.IsNullOrEmpty(ipGeolocationInfo.Key))
                {
                    var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, ipGeolocationInfo.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (cultureInfo != null)
                    {

                        var redirectUrl = String.Format("/{0}/{1}", cultureInfo.TwoLetterISOLanguageName, Request.Path);

                        if (redirectUrl.EndsWith("auth.aspx", StringComparison.InvariantCulture))
                            redirectUrl = redirectUrl.Remove(redirectUrl.IndexOf("auth.aspx", StringComparison.Ordinal));

                        if (abTesting)
                            redirectUrl += (redirectUrl.Contains("?") ? "&" : "?") + abTestingQuery;

                        Response.Redirect(redirectUrl, true);

                    }
                }
            }
            else
            {
                var lang = Request["lang"];

                if (!string.IsNullOrEmpty(lang))
                {
                    lang = lang.Split(',')[0];
                    var cultureInfo = SetupInfo.EnabledCultures.Find(c => String.Equals(c.TwoLetterISOLanguageName, lang, StringComparison.InvariantCultureIgnoreCase));
                    if (cultureInfo != null)
                    {
                        Thread.CurrentThread.CurrentUICulture = cultureInfo;
                    }
                    else
                    {
                        Log.WarnFormat("Lang {0} not supported", lang);
                    }
                }
            }

            if (abTesting)
            {
                var redirectUrl = Request.Path;

                redirectUrl += (redirectUrl.Contains("?") ? "&" : "?") + abTestingQuery;

                Response.Redirect(redirectUrl, true);
            }
        }

        protected bool AbTestingQuery(out string query)
        {
            query = string.Empty;
            
            const string q = "ab";

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[q])
                || !string.IsNullOrEmpty((string) Session[q])
                || !string.IsNullOrEmpty(Request[q])
                || Request.Url.HasProfile())
                return false;

            Session[q] = "1";
            if (new Random((int)DateTime.Now.Ticks & 0x0000FFFF).Next(2) == 0) return false;

            query = q + "=1";
            return true;
        }

        private void OutsideAuth()
        {
            var cookie = SecurityContext.AuthenticateMe(Constants.OutsideUser.ID);
            if (HttpContext.Current != null)
            {
                CookiesManager.SetCookies(CookiesType.AuthKey, cookie);
            }
            else
            {
                SecurityContext.AuthenticateMe(cookie);
            }
        }
    }
}