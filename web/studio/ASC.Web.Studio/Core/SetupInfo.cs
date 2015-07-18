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


using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ASC.Web.Studio.Core
{
    public class SetupInfo
    {
        public static string StatisticTrackURL
        {
            get { return GetAppSettings("web.track-url", string.Empty); }
        }

        public static string UserVoiceURL
        {
            get { return GetAppSettings("web.uservoice", string.Empty); }
        }

        public static string MainLogoURL
        {
            get { return GetAppSettings("web.logo.main", string.Empty); }
        }

        public static string MainLogoMailTmplURL
        {
            get { return GetAppSettings("web.logo.mail.tmpl", string.Empty); }
        }

        public static List<CultureInfo> EnabledCultures
        {
            get
            {
                return GetAppSettings("web.cultures", "en-US")
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => CultureInfo.GetCultureInfo(l.Trim()))
                    .OrderBy(l => l.Name)
                    .ToList();
            }
        }

        public static decimal ExchangeRateRuble
        {
            get { return GetAppSettings("exchange-rate.ruble", 40); }
        }

        public static long MaxImageUploadSize
        {
            get { return GetAppSettings<long>("web.max-upload-size", 1024 * 1024); }
        }

        /// <summary>
        /// Max possible file size for not chunked upload. Less or equal than 100 mb.
        /// </summary>
        public static long MaxUploadSize
        {
            get { return Math.Min(AvailableFileSize, MaxChunkedUploadSize); }
        }

        public static long AvailableFileSize
        {
            get { return 100L*1024L*1024L; }
        }

        /// <summary>
        /// Max possible file size for chunked upload.
        /// </summary>
        public static long MaxChunkedUploadSize
        {
            get
            {
                var diskQuota = TenantExtra.GetTenantQuota();
                var usedSize = UserControls.Statistics.TenantStatisticsProvider.GetUsedSize();

                if (diskQuota != null)
                {
                    var freeSize = diskQuota.MaxTotalSize - usedSize;
                    if (freeSize < diskQuota.MaxFileSize)
                        return freeSize < 0 ? 0 : freeSize;

                    return diskQuota.MaxFileSize;
                }

                return ChunkUploadSize;
            }
        }

        public static string TeamlabSiteRedirect
        {
            get
            {
                return GetAppSettings("web.teamlab-site", string.Empty);
                ;
            }
        }

        public static long ChunkUploadSize
        {
            get { return GetAppSettings("files.uploader.chunk-size", 5*1024*1024); }
        }

        public static bool ThirdPartyAuthEnabled
        {
            get { return String.Equals(GetAppSettings("web.thirdparty-auth", "true"), "true"); }
        }

        public static string NoTenantRedirectURL
        {
            get { return GetAppSettings("web.notenant-url", "http://www.onlyoffice.com/wrongportalname.aspx"); }
        }

        public static string[] CustomScripts
        {
            get { return GetAppSettings("web.custom-scripts", string.Empty).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public static string NotifyAddress
        {
            get { return GetAppSettings("web.promo-url", string.Empty); }
        }

        public static string TipsAddress
        {
            get { return GetAppSettings("web.promo-tips-url", string.Empty); }
        }

        public static string UserForum
        {
            get { return GetAppSettings("web.user-forum", string.Empty); }
        }

        public static string SupportFeedback
        {
            get { return GetAppSettings("web.support-feedback", string.Empty); }
        }

        public static string GetImportServiceUrl()
        {
            var url = GetAppSettings("web.import-contacts-url", string.Empty);
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var urlSeparatorChar = "?";
            if (url.Contains(urlSeparatorChar))
            {
                urlSeparatorChar = "&";
            }
            var cultureName = HttpUtility.HtmlEncode(System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
            return UrlSwitcher.SelectCurrentUriScheme(string.Format("{0}{2}culture={1}&mobile={3}", url, cultureName, urlSeparatorChar, MobileDetector.IsMobile));
        }

        public static string BaseDomain
        {
            get { return GetAppSettings("core.base-domain", string.Empty); }
        }

        public static string WebApiBaseUrl
        {
            get { return VirtualPathUtility.ToAbsolute(GetAppSettings("api.url", "~/api/2.0/")); }
        }

        public static TimeSpan ValidEamilKeyInterval
        {
            get { return GetAppSettings("email.validinterval", TimeSpan.FromDays(7)); }
        }

        public static bool IsSecretEmail(string email)
        {
            var s = (ConfigurationManager.AppSettings["web.autotest.secret-email"] ?? "").Trim();

            //the point is not needed in gmail.com
            email = Regex.Replace(email ?? "", "\\.*(?=\\S*(@gmail.com$))", "");

            return !string.IsNullOrEmpty(s) &&
                   s.Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries).Contains(email, StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool DisplayMobappBanner(string product)
        {
            var s = (ConfigurationManager.AppSettings["web.display.mobapps.banner"] ?? "").Trim();

            return s.Split(new char[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries).Contains(product, StringComparer.InvariantCultureIgnoreCase);
        }



        public static string ShareGooglePlusUrl
        {
            get { return GetAppSettings("web.share.google-plus", "https://plus.google.com/share?url={0}"); }
        }

        public static string ShareTwitterUrl
        {
            get { return GetAppSettings("web.share.twitter", "https://twitter.com/intent/tweet?text={0}"); }
        }

        public static string ShareFacebookUrl
        {
            get { return GetAppSettings("web.share.facebook", "http://www.facebook.com/sharer.php?s=100&p[url]={0}&p[title]={1}&p[images][0]={2}&p[summary]={3}"); }
        }
        

        public static bool IsVisibleSettings<TSettings>()
        {
            return IsVisibleSettings(typeof(TSettings).Name);
        }

        public static bool IsVisibleSettings(string settings)
        {
            var s = GetAppSettings("web.hide-settings", null);
            if (string.IsNullOrEmpty(s)) return true;

            var hideSettings = s.Split(new[] { ',', ';', ' ' });
            return !hideSettings.Contains(settings, StringComparer.CurrentCultureIgnoreCase);
        }

        private static string GetAppSettings(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        private static T GetAppSettings<T>(string key, T defaultValue)
        {
            var configSetting = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configSetting))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return (T)converter.ConvertFromString(configSetting);
                }
            }
            return defaultValue;
        }
    }
}