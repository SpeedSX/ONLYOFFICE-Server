﻿<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NavigationSidePanel.ascx.cs" Inherits="ASC.Web.CRM.Controls.Common.NavigationSidePanel" %>
<%@ Import Namespace="ASC.CRM.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="ASC.Web.Studio.Controls.Common" %>
<%@ Import Namespace="ASC.Web.Studio.Core.Voip" %>
<%@ Import Namespace="ASC.Web.Studio.Utility" %>
<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Core.Users" %>


<div class="page-menu">
    <ul class="menu-actions">
        <li id="menuCreateNewButton" class="menu-main-button without-separator <%= MobileVer ? "big" : "middle" %>" title="<%= CRMCommonResource.CreateNew %>">
            <span class="main-button-text"><%= CRMCommonResource.CreateNew %></span>
            <span class="white-combobox"></span>
        </li>
        <% if (!MobileVer) %>
        <%
           { %>
            <li id="menuOtherActionsButton" class="menu-gray-button" title="<%= Resources.Resource.MoreActions %>">
                <span class="btn_other-actions">...</span>
            </li>
        <% } %>
    </ul>

    <%-- popup windows --%>
    <div id="createNewButton" class="studio-action-panel">
        <ul class="dropdown-content">
            <li><a class="dropdown-item" href="default.aspx?action=manage"><%= CRMContactResource.Company %></a></li>
            <li><a class="dropdown-item" href="default.aspx?action=manage&type=people"><%= CRMContactResource.Person %></a></li>
            <li><a id="menuCreateNewTask" class="dropdown-item" href="javascript:void(0);"><%= CRMTaskResource.Task %></a></li>
            <li><a id="menuCreateNewDeal" class="dropdown-item" href="deals.aspx?action=manage"><%= CRMDealResource.Deal %></a></li>
            <li><a id="menuCreateNewInvoice" class="dropdown-item" href="invoices.aspx?action=create"><%= CRMInvoiceResource.Invoice %></a></li>
            <li><a class="dropdown-item" href="cases.aspx?action=manage"><%= CRMCasesResource.Case %></a></li>
        </ul>
    </div>

    <% if (!MobileVer) %>
    <%
       { %>
        <div id="otherActions" class="studio-action-panel">
            <ul class="dropdown-content">
                <li><a id="importContactLink" class="dropdown-item" href="default.aspx?action=import"><%= CRMContactResource.ImportContacts %></a></li>
                <li><a id="importTasksLink" class="dropdown-item" href="tasks.aspx?action=import"><%= CRMTaskResource.ImportTasks %></a></li>
                <li><a id="importDealsLink" class="dropdown-item" href="deals.aspx?action=import"><%= CRMDealResource.ImportDeals %></a></li>
                <li><a id="importCasesLink" class="dropdown-item" href="cases.aspx?action=import"><%= CRMCasesResource.ImportCases %></a></li>

                <% if (CRMSecurity.IsAdmin)
                   { %>
                    <li><a id="exportListToCSV" class="dropdown-item"><%= CRMCommonResource.ExportCurrentListToCsvFile %></a></li>
                <% if (FileUtility.CanWebView(".csv") || FileUtility.CanWebEdit(".csv"))
                   { %>
                    <li><a id="openListInEditor" class="dropdown-item"><%= CRMCommonResource.OpenCurrentListInTheEditor %></a></li>
                <% } %>
                <% } %>
            </ul>
        </div>
    <% } %>

    <ul class="menu-list">
        <li id="nav-menu-contacts" class="menu-item sub-list<% if (CurrentPage == "contacts")
                                                               { %> active currentCategory<% }
                                                               else if (CurrentPage == "companies" || CurrentPage == "persons")
                                                               { %> currentCategory<% } %>">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href=".#">
                    <span class="menu-item-icon group"></span><span class="menu-item-label inner-text"><%= CRMContactResource.Contacts %></span>
                </a>
                <span id="feed-new-contacts-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item<% if (CurrentPage == "companies")
                                           { %> active<% } %>">
                    <a class="menu-item-label outer-text text-overflow companies-menu-item">
                        <span class="menu-item-label inner-text"><%= CRMContactResource.Companies %></span>
                    </a>
                </li>
                <li class="menu-sub-item<% if (CurrentPage == "persons")
                                           { %> active<% } %>">
                    <a class="menu-item-label outer-text text-overflow persons-menu-item">
                        <span class="menu-item-label inner-text"><%= CRMContactResource.Persons %></span>
                    </a>
                </li>
            </ul>
        </li>
        <li id="nav-menu-tasks" class="menu-item none-sub-list<% if (CurrentPage == "tasks")
                                                                 { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="tasks.aspx#">
                <span class="menu-item-icon tasks"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.TaskModuleName %></span>
            </a>
            <span id="feed-new-crmTasks-count" class="feed-new-count"></span>
        </li>
        <li id="nav-menu-deals" class="menu-item  none-sub-list<% if (CurrentPage == "deals")
                                                                  { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="deals.aspx#">
                <span class="menu-item-icon opportunities"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.DealModuleName %></span>
            </a>
            <span id="feed-new-deals-count" class="feed-new-count"></span>
        </li>

        <li id="nav-menu-invoices" class="menu-item  sub-list<% if (CurrentPage == "invoices")
                                                                { %> active currentCategory<% } %>">
            <div class="category-wrapper">
                <span class="expander"></span>
                <a class="menu-item-label outer-text text-overflow" href="invoices.aspx">
                    <span class="menu-item-icon documents"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.InvoiceModuleName %></span>
                </a>
                <span id="feed-new-invoices-count" class="feed-new-count"></span>
            </div>
            <ul class="menu-sub-list">
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow drafts-menu-item" href="invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJakVpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCRWNtRm1kQ0FnSUNBZ0lDSXNJbDlmYVdRaU9qRXdNelV3TTMwPSJ9">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Draft %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow sent-menu-item" href="invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJaklpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCVFpXNTBJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Sent %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow paid-menu-item" href="invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJalFpTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCUVlXbGtJQ0FnSUNBZ0lpd2lYMTlwWkNJNk1UQXpOVEF6ZlE9PSJ9">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Paid %></span>
                    </a>
                </li>
                <li class="menu-sub-item">
                    <a class="menu-item-label outer-text text-overflow rejected-menu-item" href="invoices.aspx#eyJpZCI6InNvcnRlciIsInR5cGUiOiJzb3J0ZXIiLCJwYXJhbXMiOiJleUpwWkNJNkltNTFiV0psY2lJc0ltUmxaaUk2ZEhKMVpTd2laSE5qSWpwMGNuVmxMQ0p6YjNKMFQzSmtaWElpT2lKa1pYTmpaVzVrYVc1bkluMD0ifTt7ImlkIjoiZHJhZnQiLCJ0eXBlIjoiY29tYm9ib3giLCJwYXJhbXMiOiJleUoyWVd4MVpTSTZJak1pTENKMGFYUnNaU0k2SWlBZ0lDQWdJQ0FnSUNCU1pXcGxZM1JsWkNBZ0lDQWdJQ0lzSWw5ZmFXUWlPakV3TXpVd00zMD0ifQ==">
                        <span class="menu-item-label inner-text"><%= CRMEnumResource.InvoiceStatus_Rejected %></span>
                    </a>
                </li>
            </ul>
        </li>

        <li id="nav-menu-cases" class="menu-item  none-sub-list<% if (CurrentPage == "cases")
                                                                  { %> active<% } %>">
            <a class="menu-item-label outer-text text-overflow" href="cases.aspx#">
                <span class="menu-item-icon cases"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.CasesModuleName %></span>
            </a>
            <span id="feed-new-cases-count" class="feed-new-count"></span>
        </li>
        <% if (CRMSecurity.IsAdmin && VoipPaymentSettings.IsEnabled) %>
        <% { %>
            <li id="nav-menu-voip-calls" class="menu-item  none-sub-list<% if (CurrentPage == "settings_voip.calls")
                                                                            { %> active<% } %>">
                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=voip.calls">
                    <span class="menu-item-icon cases"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPCallsSettings %></span>
                </a>
                <span id="feed-new-voip-calls-count" class="feed-new-count"></span>
            </li>
        <% } %>

        <asp:PlaceHolder ID="InviteUserHolder" runat="server"></asp:PlaceHolder>

        <% if (CRMSecurity.IsAdmin) %>
        <% { %>
            <li id="menuSettings" class="menu-item add-block sub-list<% if (CurrentPage.IndexOf("settings_", StringComparison.Ordinal) > -1)
                                                                        { %> currentCategory<% } %>">
                <div class="category-wrapper">
                    <span class="expander"></span>
                    <a class="menu-item-label outer-text text-overflow<% if (CurrentPage.IndexOf("settings_", StringComparison.Ordinal) == -1)
                                                                         { %> gray-text<% } %>" href="settings.aspx">
                        <span class="menu-item-icon settings"></span><span class="menu-item-label inner-text"><%= CRMCommonResource.SettingModuleName %></span>
                    </a>
                </div>
                <ul class="menu-sub-list">
                    <li class="menu-sub-item<% if (CurrentPage == "settings_common")
                                               { %> active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=common">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.CommonSettings %></span>
                        </a>
                    </li>


                    <li id="contactSettingsConteiner" class="menu-sub-item menu-item<% if (CurrentPage == "settings_contact_stage" || CurrentPage == "settings_contact_type")
                                                                                       { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander" id="contactSettingsExpander"></span>
                            <a href="settings.aspx?type=contact_stage" class="menu-item-label outer-text text-overflow" id="menuMyProjects"><%= CRMCommonResource.ContactSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_contact_stage")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=contact_stage">
                                    <span class="menu-item-label inner-text"><%= CRMContactResource.ContactStages %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_contact_type")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=contact_type">
                                    <span class="menu-item-label inner-text"><%= CRMContactResource.ContactType %></span>
                                </a>
                            </li>
                        </ul>
                    </li>

                    <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_invoice_items" || CurrentPage == "settings_invoice_tax" || CurrentPage == "settings_organisation_profile")
                                                         { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander"></span>
                            <a href="settings.aspx?type=invoice_items" class="menu-item-label outer-text text-overflow"><%= CRMCommonResource.InvoiceSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_invoice_items")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=invoice_items">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.ProductsAndServices %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_invoice_tax")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=invoice_tax">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.InvoiceTaxes %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_organisation_profile")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=organisation_profile">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.OrganisationProfile %></span>
                                </a>
                            </li>

                        </ul>
                    </li>

                    <% if (VoipPaymentSettings.IsEnabled)
                       { %>

                        <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_voip.common" || CurrentPage == "settings_voip.numbers")
                                                             { %> open<% } %>">
                            <div class="sub-list">
                                <span class="expander"></span>
                                <a href="settings.aspx?type=voip.numbers" class="menu-item-label outer-text text-overflow"><%= CRMCommonResource.VoIPSettings %></a>
                            </div>
                            <ul class="menu-sub-list">
                                <li class="menu-sub-item<% if (CurrentPage == "settings_voip.common")
                                                           { %> active<% } %>">
                                    <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=voip.common">
                                        <span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPCommonSettings %></span>
                                    </a>
                                </li>
                                <li class="menu-sub-item<% if (CurrentPage == "settings_voip.numbers")
                                                           { %> active<% } %>">
                                    <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=voip.numbers">
                                        <span class="menu-item-label inner-text"><%= CRMCommonResource.VoIPNumbersSettings %></span>
                                    </a>
                                </li>
                            </ul>
                        </li>
                
                    <% } %>

                    <li class="menu-sub-item menu-item<% if (CurrentPage == "settings_custom_field" || CurrentPage == "settings_history_category" || CurrentPage == "settings_task_category" || CurrentPage == "settings_deal_milestone" || CurrentPage == "settings_tag")
                                                         { %> open<% } %>">
                        <div class="sub-list">
                            <span class="expander"></span>
                            <a href="settings.aspx?type=custom_field" class="menu-item-label outer-text text-overflow"><%= CRMCommonResource.OtherSettings %></a>
                        </div>
                        <ul class="menu-sub-list">
                            <li class="menu-sub-item<% if (CurrentPage == "settings_custom_field")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=custom_field">
                                    <span class="menu-item-label inner-text"><%= CRMSettingResource.CustomFields %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_history_category")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=history_category">
                                    <span class="menu-item-label inner-text"><%= CRMSettingResource.HistoryCategories %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_task_category")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=task_category">
                                    <span class="menu-item-label inner-text"><%= CRMTaskResource.TaskCategories %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_deal_milestone")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=deal_milestone">
                                    <span class="menu-item-label inner-text"><%= CRMDealResource.DealMilestone %></span>
                                </a>
                            </li>
                            <li class="menu-sub-item<% if (CurrentPage == "settings_tag")
                                                       { %> active<% } %>">
                                <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=tag">
                                    <span class="menu-item-label inner-text"><%= CRMCommonResource.Tags %></span>
                                </a>
                            </li>

                        </ul>
                    </li>

                    <li id="menuCreateWebsite" class="menu-sub-item<% if (CurrentPage == "settings_web_to_lead_form")
                                                                      { %> active<% } %>">
                        <a class="menu-item-label outer-text text-overflow" href="settings.aspx?type=web_to_lead_form">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.WebToLeadsForm %></span>
                        </a>
                    </li>

                    <% if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) { %>
                    <li id="menuAccessRights" class="menu-sub-item">
                        <a class="menu-item-label outer-text text-overflow" href="<%= CommonLinkUtility.GetAdministration(ManagementType.AccessRights) + "#crm" %>">
                            <span class="menu-item-label inner-text"><%= CRMSettingResource.AccessRightsSettings %></span>
                        </a>
                    </li>
                    <% } %>   

                </ul>
            </li>
        <% } %> 
        <asp:PlaceHolder ID="HelpHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="SupportHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="UserForumHolder" runat="server"></asp:PlaceHolder>
        <asp:PlaceHolder ID="VideoGuides" runat="server"></asp:PlaceHolder>
    </ul>
</div>