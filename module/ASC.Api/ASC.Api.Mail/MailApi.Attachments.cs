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
using ASC.Api.Attributes;
using ASC.Mail.Aggregator.Dal;

namespace ASC.Api.Mail
{
    public partial class MailApi
    {
        /// <summary>
        /// Export all message's attachments to MyDocuments
        /// </summary>
        /// <param name="id_message">Id of any message</param>
        /// <returns>Count of exported attachments</returns>
        /// <category>Messages</category>
        [Update(@"attachments/mydocuments/export")]
        public int ExportAttachmentsToMyDocuments(int id_message)
        {
            if (id_message < 0)
                throw new ArgumentException(@"Invalid message id", "id_message");

            var documentsDal = new DocumentsDal(MailBoxManager, TenantId, Username);
            var savedAttachmentsList = documentsDal.StoreAttachmentsToMyDocuments(id_message);

            return savedAttachmentsList.Count;
        }

        /// <summary>
        /// Export attachment to MyDocuments
        /// </summary>
        /// <param name="id_attachment">Id of any attachment from the message</param>
        /// <returns>Id document in My Documents</returns>
        /// <category>Messages</category>
        [Update(@"attachment/mydocuments/export")]
        public int ExportAttachmentToMyDocuments(int id_attachment)
        {
            if (id_attachment < 0)
                throw new ArgumentException(@"Invalid attachment id", "id_attachment");

            var documentsDal = new DocumentsDal(MailBoxManager, TenantId, Username);
            var documentId = documentsDal.StoreAttachmentToMyDocuments(id_attachment);
            return documentId;
        }
    }
}
