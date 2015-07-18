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


using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Controls;
using ASC.Web.Files.Import;
using ASC.Web.Files.Resources;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.UserControls.Common.LoaderPage;
using System;
using System.Web;

namespace ASC.Web.Files
{
    public partial class _Default : MainPage
    {
        protected bool AddCustomScript;

        protected bool DisplayAppsBanner;

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadScripts();

            LoadControls();

            var mobileAppRegistrator = new CachedMobileAppInstallRegistrator(new MobileAppInstallRegistrator());
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var isRegistered = mobileAppRegistrator.IsInstallRegistered(currentUser.Email, null);

            DisplayAppsBanner =
                SetupInfo.DisplayMobappBanner("files")
                && !CoreContext.Configuration.Standalone
                && !isRegistered;

            if (CoreContext.Configuration.Personal)
            {
                PersonalProcess();
            }
        }

        private void LoadScripts()
        {
            Page.RegisterStyleControl(LoadControl(VirtualPathUtility.ToAbsolute("~/products/files/masters/styles.ascx")));
            Page.RegisterBodyScripts(LoadControl(VirtualPathUtility.ToAbsolute("~/products/files/masters/FilesScripts.ascx")));
        }

        private void LoadControls()
        {
            CommonSideHolder.Controls.Add(LoadControl(MainMenu.Location));

            var mainContent = (MainContent) LoadControl(MainContent.Location);
            mainContent.TitlePage = FilesCommonResource.TitlePage;
            CommonContainerHolder.Controls.Add(mainContent);

            if (CoreContext.Configuration.Personal)
                CommonContainerHolder.Controls.Add(LoadControl(MoreFeatures.Location));

            CommonContainerHolder.Controls.Add(LoadControl(AccessRights.Location));

            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));

            if (ImportConfiguration.SupportInclusion
                && !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                && (Classes.Global.IsAdministrator
                    || CoreContext.Configuration.Personal
                    || FilesSettings.EnableThirdParty))
            {
                SettingPanelHolder.Controls.Add(LoadControl(ThirdParty.Location));
            }
        }

        private void PersonalProcess()
        {
            if (PersonalSettings.IsNewUser)
            {
                PersonalSettings.IsNewUser = false;
                AddCustomScript = SetupInfo.CustomScripts.Length != 0 && !SetupInfo.IsSecretEmail(CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).Email);

                Classes.Global.Logger.Info("New personal user " + SecurityContext.CurrentAccount.ID
                                           + ((string) Session["campaign"] == "personal" ? " with campaign=personal" : ""));
            }

            if (PersonalSettings.IsNotActivated)
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                if (user.ActivationStatus != EmployeeActivationStatus.NotActivated) return;

                try
                {
                    SecurityContext.AuthenticateMe(ASC.Core.Configuration.Constants.CoreSystem);
                    user.ActivationStatus = EmployeeActivationStatus.Activated;
                    CoreContext.UserManager.SaveUserInfo(user);
                }
                finally
                {
                    SecurityContext.Logout();
                }

                SecurityContext.AuthenticateMe(user.ID);

                PersonalSettings.IsNotActivated = false;
                Classes.Global.Logger.InfoFormat("User {0} ActivationStatus - Activated", user.ID);

                StudioNotifyService.Instance.UserHasJoin();
                StudioNotifyService.Instance.SendUserWelcomePersonal(user);
            }
        }
    }
}