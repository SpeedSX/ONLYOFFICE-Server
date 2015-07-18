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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Data.Storage;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using Resources;
using System.Web.UI.HtmlControls;

namespace ASC.Web.Studio.UserControls.Common.ProjectDocumentsPopup
{
    public partial class ProjectDocumentsPopup : System.Web.UI.UserControl
    {
        public string PopupName { get; set; }
        public int ProjectId { get; set; }

        public static string Location { get { return "~/UserControls/Common/ProjectDocumentsPopup/ProjectDocumentsPopup.ascx"; } }

        public ProjectDocumentsPopup()
        {
            PopupName = UserControlsCommonResource.AttachOfProjectDocuments;
            ProjectId = 0;
        }

        private void InitScripts()
        {
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/projectdocumentspopup/css/projectDocumentsPopup.less"));

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/projectdocumentspopup/js/projectDocumentsPopup.js"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _documentUploader.Options.IsPopup = true;
            InitScripts();
            var emptyParticipantScreenControl = new EmptyScreenControl
            {
                ImgSrc = VirtualPathUtility.ToAbsolute("~/UserControls/Common/ProjectDocumentsPopup/Images/project-documents.png"),
                Header = UserControlsCommonResource.ProjectDocuments,
                HeaderDescribe = UserControlsCommonResource.EmptyDocsHeaderDescription,
                Describe = Resources.UserControlsCommonResource.EmptyDocsDescription
            };
            _phEmptyDocView.Controls.Add(emptyParticipantScreenControl);
        }
    }
}