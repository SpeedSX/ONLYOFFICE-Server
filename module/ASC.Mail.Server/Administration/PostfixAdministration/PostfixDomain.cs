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


using ASC.Common.Data.Sql;
using ASC.Mail.Server.Administration.Interfaces;
using ASC.Mail.Server.Administration.ServerModel;
using ASC.Mail.Server.Administration.ServerModel.Base;
using ASC.Mail.Server.PostfixAdministration.DbSchema;


namespace ASC.Mail.Server.PostfixAdministration
{
    class PostfixDomain : WebDomainModel
    {
        public PostfixDomain(int id, int tenant, string name, bool isVerified, MailServerBase server)
            : base(id, tenant, name, isVerified, server)
        {
        }

        protected override void _AddDkim(DkimRecordBase dkimToAdd)
        {
            var dbManager = new PostfixAdminDbManager(Server.Id, Server.ConnectionString);
            using (var db = dbManager.GetAdminDb())
            {
                var dkimId = db.ExecuteScalar<int>(
                    new SqlQuery(DkimTable.name)
                        .Select(DkimTable.Columns.id)
                        .Where(DkimTable.Columns.domain_name, Name));

                if (dkimId == 0)
                {
                    var insertDkim = new SqlInsert(DkimTable.name)
                        .InColumnValue(DkimTable.Columns.domain_name, Name)
                        .InColumnValue(DkimTable.Columns.selector, dkimToAdd.Selector)
                        .InColumnValue(DkimTable.Columns.private_key, dkimToAdd.PrivateKey)
                        .InColumnValue(DkimTable.Columns.public_key, dkimToAdd.PublicKey);
                    db.ExecuteNonQuery(insertDkim);
                }
                else
                {
                    var updateDkim = new SqlUpdate(DkimTable.name)
                        .Where(DkimTable.Columns.id, dkimId)
                        .Set(DkimTable.Columns.selector, dkimToAdd.Selector)
                        .Set(DkimTable.Columns.private_key, dkimToAdd.PrivateKey)
                        .Set(DkimTable.Columns.public_key, dkimToAdd.PublicKey);
                    db.ExecuteNonQuery(updateDkim);
                }

            }
        }

    }
}
