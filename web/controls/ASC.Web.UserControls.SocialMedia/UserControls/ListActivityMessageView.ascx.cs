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
using ASC.SocialMedia;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Web.UI;

namespace ASC.Web.UserControls.SocialMedia.UserControls
{
    public partial class ListActivityMessageView : BaseUserControl
    {
        public List<Message> MessageList { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (MessageList != null)
            {
                _ctrlRptrUserActivity.DataSource = MessageList;
                _ctrlRptrUserActivity.DataBind();
            }
        }

        protected void _ctrlRptrUserActivity_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            Message msg = (Message)e.Item.DataItem;
            ((Image)e.Item.FindControl("_ctrlImgSocialMediaIcon")).ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(BaseUserControl), String.Format("ASC.Web.UserControls.SocialMedia.images.{0}.png", msg.Source.ToString().ToLower()));

        }
    }
}