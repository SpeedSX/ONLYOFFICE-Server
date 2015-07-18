﻿<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="accountsTmpl" type="text/x-jquery-tmpl">
    <table class="accounts_list" id="common_mailboxes">
        <tbody>
            {{tmpl(common_mailboxes, {showSetDefaultIcon: showSetDefaultIcon}) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="server_mailboxes">
        <tbody>
            {{tmpl(server_mailboxes, {showSetDefaultIcon: showSetDefaultIcon}) "mailboxItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="aliases">
        <tbody>
            {{tmpl(aliases, {showSetDefaultIcon: showSetDefaultIcon}) "aliasItemTmpl"}}
        </tbody>
    </table>
    <table class="accounts_list" id="groups">
        <tbody>
            {{tmpl(groups, {showSetDefaultIcon: showSetDefaultIcon}) "groupItemTmpl"}}
        </tbody>
    </table>
</script>

<script id="mailboxItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover {{if enabled!=true}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
        <td class="default_account_button_column">
            {{if isDefault!=true}}
                <div class="set_as_default_account_icon default_account_icon_block"
                    title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
            {{else}}
                <div class="default_account_icon default_account_icon_block"
                    title="<%: MailScriptResource.DefaultAccountText %>"></div>
            {{/if}}
        </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            {{if !isTeamlabMailbox && oAuthConnection }}
                <span class="notification" title="<%: MailScriptResource.AccountNotificationText %>"><%: MailScriptResource.AccountNotificationText %></span>
            {{/if}}
        </td>
        <td class="manage_signature_column">
            <div class="btn-row __list" title="<%: MailScriptResource.ManageSignatureLabel %>" onclick="accountsModal.showSignatureBox('${email}');">
                <%: MailScriptResource.ManageSignatureLabel %>
            </div>
        </td>
        <td class="menu_column">
            <div class="menu" title="<%: MailScriptResource.Actions %>" data_id="${email}"></div>
        </td>
    </tr>
</script>

<script id="groupItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover inactive {{if enabled!=true}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
            <td class="default_account_button_column">
                <div class="group_default_account_icon_block" title="<%: MailScriptResource.GroupCouldNotSetAsDefault %>"></div>
            </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            <span class="notification" title="<%: MailResource.GroupNotificationText %>"><%: MailResource.GroupNotificationText %></span>
        </td>
    </tr>
</script>

<script id="aliasItemTmpl" type="text/x-jquery-tmpl">
    <tr data_id="${email}" class="row item-row row-hover inactive {{if enabled!=true}}disabled{{/if}}">
        {{if $item.showSetDefaultIcon}}
            <td class="default_account_button_column">
                {{if isDefault!=true}}
                    <div class="set_as_default_account_icon default_account_icon_block"
                         title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
                {{else}}
                    <div class="default_account_icon default_account_icon_block"
                         title="<%: MailScriptResource.DefaultAccountText %>"></div>
                {{/if}}
            </td>
        {{/if}}
        <td class="address">
            <span class="accountname" title="${email}">${email}</span>
        </td>
        <td class="notify_column">
            <span class="notification" title="${"<%: MailScriptResource.AliasNotificationText %>".replace('%mailbox_address%', $data.realEmail)}">
               ${"<%: MailScriptResource.AliasNotificationText %>".replace('%mailbox_address%', $data.realEmail)}</span>
        </td>
    </tr>
</script>

<script id="setDefaultIconItemTmpl" type="text/x-jquery-tmpl">
    <td class="default_account_button_column">
        {{if isDefault!=true}}
            <div class="set_as_default_account_icon default_account_icon_block"
                 title="<%: MailScriptResource.SetAsDefaultAccountText %>"></div>
        {{else}}
            <div class="default_account_icon default_account_icon_block"
                 title="<%: MailScriptResource.DefaultAccountText %>"></div>
        {{/if}}
    </td>
</script>