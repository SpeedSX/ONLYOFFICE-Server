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


/*
    Copyright (c) Ascensio System SIA 2015. All rights reserved.
    https://www.onlyoffice.com
*/
window.accountsManager = (function($) {
    var isInit = false,
        accountList = [],
        getAccountsHandler;

    var init = function() {
        if (isInit === false) {
            var accounts;
            isInit = true;

            getAccountsHandler = serviceManager.bind(window.Teamlab.events.getAccounts, onGetMailAccounts);
            serviceManager.bind(window.Teamlab.events.removeMailMailbox, onRemoveMailbox);
            serviceManager.bind(window.Teamlab.events.updateMailMailbox, onUpdateMailMailbox);
            serviceManager.bind(window.Teamlab.events.setMailMailboxState, onSetMailboxState);
            serviceManager.bind(window.Teamlab.events.updateMailboxSignature, onUpdateMailboxSignature);
            serviceManager.bind(window.Teamlab.events.setEMailInFolder, onSetEMailInFolder);

            accountsModal.init();
            accountsPage.init();
            accounts = $.map(window.MailAccounts, function (el) { return el; });
            window.MailAccounts = null;
            initAccounts(accounts);
            accountsPage.loadAccounts(accounts);
            accountsPanel.init();
            accountsPanel.update();
        }
    };

    var onGetMailAccounts = function(params, accounts) {
        accountsPage.clear();
        initAccounts(accounts);
        accountsPage.loadAccounts(accounts);
    };

    var initAccounts = function(accounts) {
        accountList = [];
        $.each(accounts, function(index, value) {
            var account = {
                name: TMMail.ltgt(value.name),
                email: TMMail.ltgt(value.email),
                enabled: value.enabled,
                signature: value.signature,
                is_alias: value.isAlias,
                is_group: value.isGroup,
                oauth: value.oAuthConnection,
                emailInFolder: value.eMailInFolder,
                is_teamlab: value.isTeamlabMailbox,
                mailbox_id: value.mailboxId,
                is_default: value.isDefault,
                is_shared_domain: value.isSharedDomainMailbox,
                authError: value.authError,
                quotaError: value.quotaError
            };
            addAccount(account);
        });
    };

    var onUpdateMailMailbox = function(params) {
        accountsModal.hide();
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == params.email.toLowerCase()) {
                accountList[i].name = params.name;
                break;
            }
        }

        if (params.activateOnSuccess) {
            accountsModal.activateAccountWithoutQuestion(params.email.toLowerCase());
        }
    };

    var onRemoveMailbox = function(params, email) {
        accountsPage.deleteAccount(email);
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                accountList.splice(i, 1);
                break;
            }
        }
        mailBox.markFolderAsChanged(TMMail.sysfolders.inbox.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.sent.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.drafts.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.trash.id);
        mailBox.markFolderAsChanged(TMMail.sysfolders.spam.id);
    };

    var onSetMailboxState = function (params, mailboxId) {
        var aliases = getAliasesByMailboxId(mailboxId);
        for (var i = 0; i < aliases.length; i++) {
            enableMailbox(aliases[i].email, params.enabled);
        }

        var accountMailbox = getAccountById(mailboxId);
        if (!accountMailbox)
            return;

        enableMailbox(accountMailbox.email, params.enabled);

        if (TMMail.pageIs('writemessage')) {
            var account = getAccountByAddress(params.email);
            messagePage.updateFromAccountField(account);
        }
    };

    function enableMailbox(email, enabled) {
        accountsPage.activateAccount(email, enabled);
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                accountList[i].enabled = enabled;
                break;
            }
        }
    }

    var onUpdateMailboxSignature = function(params, signature) {
        accountsModal.hide();
        var account = window.accountsManager.getAccountById(params.id);
        if (account) {
            account.signature = signature;
        }
        var aliases = getAliasesByMailboxId(params.id);
        for (var i = 0; i < aliases.length; i++) {
            aliases[i].signature = signature;
        }
    };

    var onSetEMailInFolder = function(params) {
        accountsModal.hide();
        var account = getAccountById(params.id);
        account.emailInFolder = params.emailInFolder;

        if (params.resetFolder) {
            window.toastr.success(window.MailScriptResource.ResetAccountEMailInFolderSuccess);
        } else {
            window.toastr.success(window.MailScriptResource.SetAccountEMailInFolderSuccess);
        }
    };

    var getAccountList = function() {
        return accountList;
    };

    var setDefaultAccount = function(email, setDefault) {
        var emailToLowerCase = email.toLowerCase(),
            currentAccount;

        for (var i = 0; i < accountList.length; i++) {
            currentAccount = accountList[i];
            currentAccount.is_default = false;
            if (currentAccount.email == emailToLowerCase) {
                currentAccount.is_default = setDefault;
            }
        }

        accountsPanel.update();
    };

    var getAccountByAddress = function(email) {
        var mailBox = undefined;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == email.toLowerCase()) {
                mailBox = accountList[i];
                break;
            }
        }
        return mailBox;
    };

    var getAccountById = function(id) {
        var mailBox = undefined;
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && !accountList[i].is_group && !accountList[i].is_alias) {
                mailBox = accountList[i];
                break;
            }
        }
        return mailBox;
    };

    var getAliasesByMailboxId = function(id) {
        var aliases = [];
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].mailbox_id == id && accountList[i].is_alias) {
                aliases.push(accountList[i]);
            }
        }
        return aliases;
    };

    var addAccount = function(account) {
        account.email = account.email.toLowerCase();
        for (var i = 0; i < accountList.length; i++) {
            if (accountList[i].email == account.email) {
                return;
            }
        }
        accountList.push(account);
    };

    function any() {
        return accountList.length > 0;
    }

    return {
        init: init,
        getAccountList: getAccountList,
        setDefaultAccount: setDefaultAccount,
        getAccountByAddress: getAccountByAddress,
        getAccountById: getAccountById,
        addAccount: addAccount,
        any: any,
        enableMailbox: enableMailbox
    };

})(jQuery);