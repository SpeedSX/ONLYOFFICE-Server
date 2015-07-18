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
using System.IO;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.MailServer.DataContracts;
using ASC.Api.MailServer.Extensions;
using ASC.Core;
using ASC.Core.Users;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Server.Utils;
using System.Security;
using ASC.Web.Studio.Core;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.MailServer
{
    public partial class MailServerApi
    {
        /// <summary>
        ///    Create mailbox
        /// </summary>
        /// <param name="name"></param>
        /// <param name="domain_id"></param>
        /// <param name="user_id"></param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Create mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/add")]
        public MailboxData CreateMailbox(string name, int domain_id, string user_id)
        {
            var domain = MailServer.GetWebDomain(domain_id, MailServerFactory);
            var isSharedDomain = domain.Tenant == Defines.SHARED_TENANT_ID;

            if (!IsAdmin && !isSharedDomain)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid mailbox name.", "name");

            if (domain_id < 0)
                throw new ArgumentException(@"Invalid domain id.", "domain_id");

            Guid user;

            if (!Guid.TryParse(user_id, out user))
                throw new ArgumentException(@"Invalid user id.", "user_id");

            if (isSharedDomain && !IsAdmin && user != SecurityContext.CurrentAccount.ID)
                throw new SecurityException("Creation of a shared mailbox is allowed only for the current account if user is not admin.");

            var teamlabAccount = CoreContext.Authentication.GetAccountByID(user);

            if (teamlabAccount == null)
                throw new InvalidDataException("Unknown user.");

            var userInfo = CoreContext.UserManager.GetUsers(user);

            if(userInfo.IsVisitor())
                throw new InvalidDataException("User is visitor.");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of mailbox localpart exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException("Incorrect mailbox name.");

            var mailboxName = name.ToLowerInvariant();

            var login = string.Format("{0}@{1}", mailboxName, domain.Name);

            var password = PasswordGenerator.GenerateNewPassword(12);

            var account = MailServerFactory.CreateMailAccount(teamlabAccount, login);

            var mailbox = MailServer.CreateMailbox(mailboxName, password, domain, account, MailServerFactory);

            return mailbox.ToMailboxData();

        }

        /// <summary>
        ///    Create my mailbox
        /// </summary>
        /// <param name="name"></param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Create mailbox</short> 
        /// <category>Mailboxes</category>
        [Create(@"mailboxes/addmy")]
        public MailboxData CreateMyMailbox(string name)
        {
            if (!SetupInfo.IsVisibleSettings("AdministrationPage") || !SetupInfo.IsVisibleSettings("MailCommonDomain") || CoreContext.Configuration.Standalone)
                throw new Exception("Common domain is not available");

            var domain = MailServer.GetWebDomains(MailServerFactory).FirstOrDefault(x => x.Tenant == Defines.SHARED_TENANT_ID);

            if (domain == null)
                throw new SecurityException("Domain not found.");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"Invalid mailbox name.", "name");

            var teamlabAccount = CoreContext.Authentication.GetAccountByID(SecurityContext.CurrentAccount.ID);

            if (teamlabAccount == null)
                throw new InvalidDataException("Unknown user.");

            var userInfo = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (userInfo.IsVisitor())
                throw new InvalidDataException("User is visitor.");

            if (name.Length > 64)
                throw new ArgumentException(@"Local part of mailbox localpart exceed limitation of 64 characters.", "name");

            if (!Parser.IsEmailLocalPartValid(name))
                throw new ArgumentException("Incorrect mailbox name.");

            var mailboxName = name.ToLowerInvariant();

            var login = string.Format("{0}@{1}", mailboxName, domain.Name);

            var password = PasswordGenerator.GenerateNewPassword(12);

            var account = MailServerFactory.CreateMailAccount(teamlabAccount, login);

            var mailbox = MailServer.CreateMailbox(mailboxName, password, domain, account, MailServerFactory);

            return mailbox.ToMailboxData();

        }

        /// <summary>
        ///    Returns list of the mailboxes associated with tenant
        /// </summary>
        /// <returns>List of MailboxData for current tenant</returns>
        /// <short>Get mailboxes list</short> 
        /// <category>Mailboxes</category>
        [Read(@"mailboxes/get")]
        public List<MailboxData> GetMailboxes()
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            var mailboxes = MailServer.GetMailboxes(MailServerFactory);
            return mailboxes
                .Select(mailbox => mailbox.ToMailboxData())
                .ToList();
        }

        /// <summary>
        ///    Deletes the selected mailbox
        /// </summary>
        /// <param name="id">id of mailbox</param>
        /// <returns>id of mailbox</returns>
        /// <short>Remove mailbox from mail server</short> 
        /// <category>Mailboxes</category>
        [Delete(@"mailboxes/remove/{id}")]
        public int RemoveMailbox(int id)
        {
            if (id < 0)
                throw new ArgumentException(@"Invalid domain id.", "id");

            var mailbox = MailServer.GetMailbox(id, MailServerFactory);

            if(mailbox == null)
                throw new ItemNotFoundException("Account not found.");

            var isSharedDomain = mailbox.Address.Domain.Tenant == Defines.SHARED_TENANT_ID;

            if (!IsAdmin && !isSharedDomain)
                throw new SecurityException("Need admin privileges.");

            if (isSharedDomain && !IsAdmin && mailbox.Account.TeamlabAccount.ID != SecurityContext.CurrentAccount.ID)
                throw new SecurityException("Removing of a shared mailbox is allowed only for the current account if user is not admin.");
            
            var groups = MailServer.GetMailGroups(MailServerFactory);

            var groupsContainsMailbox = groups.Where(g => g.InAddresses.Contains(mailbox.Address))
                  .Select(g => g);

            foreach (var mailGroup in groupsContainsMailbox)
            {
                if (mailGroup.InAddresses.Count == 1)
                {
                    MailServer.DeleteMailGroup(mailGroup.Id, MailServerFactory);
                }
                else
                {
                    mailGroup.RemoveMember(mailbox.Address.Id);
                }
            }

            MailServer.DeleteMailbox(mailbox);

            return id;
        }

        /// <summary>
        ///    Add alias to mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="alias_name">name of alias</param>
        /// <returns>MailboxData associated with tenant</returns>
        /// <short>Add mailbox's aliases</short>
        /// <category>AddressData</category>
        [Update(@"mailboxes/alias/add")]
        public AddressData AddMailboxAlias(int mailbox_id, string alias_name)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (string.IsNullOrEmpty(alias_name))
                throw new ArgumentException(@"Invalid alias name.", "alias_name");

            if (mailbox_id < 0)
                throw new ArgumentException(@"Invalid mailbox id.", "mailbox_id");

            if (alias_name.Length > 64)
                throw new ArgumentException(@"Local part of mailbox alias exceed limitation of 64 characters.", "alias_name");

            if (!Parser.IsEmailLocalPartValid(alias_name))
                throw new ArgumentException("Incorrect mailbox alias.");

            var mailbox = MailServer.GetMailbox(mailbox_id, MailServerFactory);

            if (mailbox == null)
                throw new ArgumentException("Mailbox not exists");

            if (mailbox.Address.Domain.Tenant == Defines.SHARED_TENANT_ID)
                throw new InvalidOperationException("Adding mailbox alias is not allowed for shared domain.");

            var mailboxAliasName = alias_name.ToLowerInvariant();

            var alias = mailbox.AddAlias(mailboxAliasName, mailbox.Address.Domain, MailServerFactory);

            return alias.ToAddressData();
        }

        /// <summary>
        ///    Remove alias from mailbox
        /// </summary>
        /// <param name="mailbox_id">id of mailbox</param>
        /// <param name="address_id"></param>
        /// <returns>id of mailbox</returns>
        /// <short>Remove mailbox's aliases</short>
        /// <category>Mailboxes</category>
        [Update(@"mailboxes/alias/remove")]
        public int RemoveMailboxAlias(int mailbox_id, int address_id)
        {
            if (!IsAdmin)
                throw new SecurityException("Need admin privileges.");

            if (address_id < 0)
                throw new ArgumentException(@"Invalid address id.", "address_id");

            if (mailbox_id < 0)
                throw new ArgumentException(@"Invalid mailbox id.", "mailbox_id");

            var mailbox = MailServer.GetMailbox(mailbox_id, MailServerFactory);

            if (mailbox == null)
                throw new ArgumentException("Mailbox not exists");

            mailbox.RemoveAlias(address_id);

            return mailbox_id;
        }
    }
}
