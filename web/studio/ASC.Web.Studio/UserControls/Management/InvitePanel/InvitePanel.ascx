<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InvitePanel.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.InvitePanel" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="Resources" %>

<div id="invitePanelContainer" class="display-none"
    data-header="<%= Resource.InviteLinkTitle %>"
    data-ok="<%= Resource.CloseButton %>">
     <div>
        <% if (EnableInviteLink) { %>
        <p><%= Resource.HelpAnswerLinkInviteSettings %></p>
        <% } else { %>
        <p>
            <%= UserControlsCommonResource.TariffUserLimitReason%>
            <%= Resource.KeepTariffInviteGuests%>
        </p>
        <% if (!ASC.Core.CoreContext.Configuration.Standalone && TenantExtra.EnableTarrifSettings) { %>
        <a href="<%= TenantExtra.GetTariffPageLink() %>">
            <%= Resource.UpgradePlan %>
        </a>
        <% } %>
        <% } %>
    </div>

    <div>
        <div id="linkInviteSettings">
            <div class="share-link-row">
                <div id="shareInviteUserLinkPanel" class="clearFix">

                    <span id="shareInviteUserLinkCopy" class="baseLinkAction text-medium-describe"><%= Resource.CopyToClipboard %></span>

                    <% if (!String.IsNullOrEmpty(ASC.Common.Utils.LinkShorterUtil.BitlyUrl)) { %>
                    <span id="getShortenInviteLink" class="baseLinkAction text-medium-describe"><%= Resource.GetShortenLink %></span>
                    <% } %>

                    <div id="chkVisitorContainer" class="clearFix">
                        <input type="checkbox" id="chkVisitor" <%= EnableInviteLink ? "" : "disabled=\"disabled\" checked=\"checked\"" %> />
                        <label for="chkVisitor"><%= Resource.InviteUsersAsCollaborators%></label>
                        <% if (EnableInviteLink) { %>
                        <input id="hiddenVisitorLink" type="hidden" value="<%= GeneratedVisitorLink%>" />
                        <input id="hiddenUserLink" type="hidden" value="<%= GeneratedUserLink%>" />
                        <% } %>
                    </div>  
                    
                    <textarea id="shareInviteUserLink" class="textEdit" cols="10" rows="2" <% if (!ASC.Web.Core.Mobile.MobileDetector.IsMobile)
                                                                                    { %> readonly="readonly" <%} %>><%= EnableInviteLink ? GeneratedUserLink : GeneratedVisitorLink%></textarea>
                </div>
            </div>
        </div>

        <ul id="shareInviteLinkViaSocPanel" class="clearFix">
            <li><a class="facebook" target="_blank" title="<%= Resource.TitleFacebook %>"></a></li>
            <li><a class="twitter" target="_blank" title="<%= Resource.TitleTwitter %>"></a></li>
            <li><a class="google" target="_blank" title="<%= Resource.TitleGoogle %>"></a></li>
        </ul>

        
    </div>
</div>