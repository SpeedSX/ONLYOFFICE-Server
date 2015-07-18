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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;

namespace ASC.Web.Studio.UserControls.Management
{
    [Serializable]
    [DataContract]
    public class TariffSettings : ISettings
    {
        [DataMember(Name = "HideRecommendation")]
        public bool HideBuyRecommendationSetting { get; set; }

        [DataMember(Name = "HideAnnualRecomendation")]
        public bool HideAnnualRecomendationSetting { get; set; }

        public ISettings GetDefault()
        {
            return new TariffSettings
                {
                    HideBuyRecommendationSetting = false,
                    HideAnnualRecomendationSetting = false,
                };
        }

        public Guid ID
        {
            get { return new Guid("{07956D46-86F7-433b-A657-226768EF9B0D}"); }
        }

        public static bool HideRecommendation
        {
            get { return SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID).HideBuyRecommendationSetting; }
            set
            {
                var tariffSettings = SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID);
                tariffSettings.HideBuyRecommendationSetting = value;
                SettingsManager.Instance.SaveSettingsFor(tariffSettings, SecurityContext.CurrentAccount.ID);
            }
        }

        public static bool HideAnnualRecomendation
        {
            get { return SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID).HideAnnualRecomendationSetting; }
            set
            {
                var tariffSettings = SettingsManager.Instance.LoadSettingsFor<TariffSettings>(SecurityContext.CurrentAccount.ID);
                tariffSettings.HideAnnualRecomendationSetting = value;
                SettingsManager.Instance.SaveSettingsFor(tariffSettings, SecurityContext.CurrentAccount.ID);
            }
        }
    }
}