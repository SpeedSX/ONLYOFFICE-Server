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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Web.Core.Utility.Settings;
using AjaxPro;

namespace ASC.Web.Core.Users
{
    [Serializable]
    [DataContract]
    public class UserHelpTourSettings : ISettings
    {
        public Guid ID
        {
            get { return new Guid("{DF4B94B7-42C8-4fce-AAE2-D479F3B39BDD}"); }
        }

        [DataMember(Name = "ModuleHelpTour")]
        public Dictionary<Guid, int> ModuleHelpTour { get; set; }

        [DataMember(Name = "IsNewUser")]
        public bool IsNewUser { get; set; }

        public ISettings GetDefault()
        {
            return new UserHelpTourSettings
                       {
                           ModuleHelpTour = new Dictionary<Guid, int>(),
                           IsNewUser = false
                       };
        }
    }
        
    [AjaxNamespace("UserHelpTourUsage")]
    public class UserHelpTourHelper
    {
        private static UserHelpTourSettings Settings
        {
            get { return SettingsManager.Instance.LoadSettingsFor<UserHelpTourSettings>(SecurityContext.CurrentAccount.ID); }
            set { SettingsManager.Instance.SaveSettingsFor(value, SecurityContext.CurrentAccount.ID); }
        }

        public static bool IsNewUser
        {
            get { return Settings.IsNewUser; }
            set
            {
                var settings = Settings;
                settings.IsNewUser = value;
                Settings = settings;
            }
        }

        public static int GetStep(Guid module)
        {
            var setting = Settings;

            if (setting.IsNewUser)
                return setting.ModuleHelpTour.ContainsKey(module) ? setting.ModuleHelpTour[module] : 0;

            return -1;
        }

        [AjaxMethod]
        public void SetStep(Guid module, int step)
        {
            var settings = Settings;

            if (settings.ModuleHelpTour.ContainsKey(module))
            {
                settings.ModuleHelpTour[module] = step;
            }
            else
            {
                settings.ModuleHelpTour.Add(module, step);
            }

            Settings = settings;
        }
    }
}