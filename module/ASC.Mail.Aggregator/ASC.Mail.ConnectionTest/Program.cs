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
using System.Web.Configuration;
using System.Net.Mail;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ActiveUp.Net.Mail;


namespace ASC.Mail.ConnectionTest
{
    partial class Program
    {
        static readonly ILogger Logger = LoggerFactory.GetLogger(LoggerFactory.LoggerType.Nlog, "ConnectionTest");

        private static void Main(string[] args)
        {
            var options = new Options();

            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                                                                () => Logger.Info("Bad command line parameters.")))
            {
                try
                {
                    if (Boolean.Parse(WebConfigurationManager.AppSettings["mailbox.settigs"]))
                    {
                        var mbox = new MailBox
                            {
                                EMail = new MailAddress(options.Email),
                                Password = WebConfigurationManager.AppSettings["mailbox.password"],
                                Account = WebConfigurationManager.AppSettings["mailbox.account"],
                                Port = int.Parse(WebConfigurationManager.AppSettings["mailbox.port"]),
                                Server = WebConfigurationManager.AppSettings["mailbox.server"],
                                SmtpAccount = WebConfigurationManager.AppSettings["mailbox.smtp_account"],
                                SmtpPassword = WebConfigurationManager.AppSettings["mailbox.smtp_password"],
                                SmtpPort = int.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_port"]),
                                SmtpServer = WebConfigurationManager.AppSettings["mailbox.smtp_server"],
                                SmtpAuth = Boolean.Parse(WebConfigurationManager.AppSettings["mailbox.smtp_auth"]),
                                Imap = Boolean.Parse(WebConfigurationManager.AppSettings["mailbox.imap"]),
                                IncomingEncryptionType = (EncryptionType)int.Parse(WebConfigurationManager.AppSettings["mailbox.incoming_encryption_type"]),
                                OutcomingEncryptionType = (EncryptionType)int.Parse(WebConfigurationManager.AppSettings["mailbox.outcoming_encryption_type"]),
                                AuthenticationTypeIn = (SaslMechanism) int.Parse(WebConfigurationManager.AppSettings["mailbox.auth_type_in"]),
                                AuthenticationTypeSmtp = (SaslMechanism) int.Parse(WebConfigurationManager.AppSettings["mailbox.auth_type_smtp"]),
                            };

                        Logger.Info("Create account");
                        CreateAccount(mbox);
                    }
                    else
                    {
                        Logger.Info("Create simple account");
                        CreateAccountSimple(options.Email, options.Password);
                    }

                    Logger.Info("[SUCCESS] test passed");

                }
                catch (Exception)
                {
                    Logger.Info("[Error] test not passed");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Any key to exit...");
            Console.ReadKey();
        }

        private static MailBox CreateAccountSimple(string email, string password)
        {
            var begin_date = DateTime.Now.Subtract(new TimeSpan(MailBox.DefaultMailLimitedTimeDelta));

            var mail_box_manager = new MailBoxManager(30);
            try
            {
                Logger.Info("Search mailbox settings:");
                var mbox = mail_box_manager.SearchMailboxSettings(email, password, "", 0);

                Logger.Info("Account: " + mbox.Account + "; Password: " + mbox.Password);
                Logger.Info("Imap: " + mbox.Imap + "; Server: " + mbox.Server + "; Port: " + mbox.Port +
                            "; AuthenticationType: " + mbox.AuthenticationTypeIn + "; EncryptionType: " + mbox.IncomingEncryptionType);
                Logger.Info("SmtpServer: " + mbox.SmtpServer + "; SmtpPort: " + mbox.SmtpPort + "; AuthenticationType: " 
                    + mbox.AuthenticationTypeSmtp + "; OutcomingEncryptionType: " + mbox.OutcomingEncryptionType);

                mbox.BeginDate = begin_date; // Apply restrict for download

                MailServerHelper.Test(mbox);

                return mbox;
            }
            catch (ImapConnectionException ex_imap)
            {
                if (ex_imap is ImapConnectionTimeoutException)
                    Logger.Info("ImapConnectionTimeoutException: " + ex_imap.Message);
                else Logger.Info("ImapConnectionException: " + ex_imap.Message);
            }
            catch (Pop3ConnectionException ex_pop)
            {
                if (ex_pop is Pop3ConnectionTimeoutException)
                    Logger.Info("Pop3ConnectionTimeoutException: " + ex_pop.Message);
                else Logger.Info("Pop3ConnectionException: " + ex_pop.Message);
            }
            catch (SmtpConnectionException ex_smtp)
            {
                if (ex_smtp is SmtpConnectionTimeoutException)
                    Logger.Info("SmtpConnectionTimeoutException: " + ex_smtp.Message);
                else Logger.Info("SmtpConnectionException: " + ex_smtp.Message);
            }
            catch (Exception ex)
            {
                Logger.Info("Exception: " + ex.Message);
            }
            throw new Exception();
        }

        public static MailBox CreateAccount( MailBox mbox)
        {
            Logger.Info("Account: " + mbox.Account + "; Password: " + mbox.Password);
            Logger.Info("Imap: " + mbox.Imap + "; Server: " + mbox.Server + "; Port: " + mbox.Port +
                        "; AuthenticationType: " + mbox.AuthenticationTypeIn + "; EncryptionType: " + mbox.IncomingEncryptionType);
            Logger.Info("SmtpServer: " + mbox.SmtpServer + "; SmtpPort: " + mbox.SmtpPort + "; AuthenticationType: "
                + mbox.AuthenticationTypeSmtp + "; OutcomingEncryptionType: " + mbox.OutcomingEncryptionType);

            try
            {
                MailServerHelper.Test(mbox);
                return mbox;
            }
            catch (ImapConnectionException ex_imap)
            {
                if (ex_imap is ImapConnectionTimeoutException)
                    Logger.Info("ImapConnectionTimeoutException: " + ex_imap.Message);
                else Logger.Info("ImapConnectionException: " + ex_imap.Message);
            }
            catch (Pop3ConnectionException ex_pop)
            {
                if (ex_pop is Pop3ConnectionTimeoutException)
                    Logger.Info("Pop3ConnectionTimeoutException: " + ex_pop.Message);
                else Logger.Info("Pop3ConnectionException: " + ex_pop.Message);
            }
            catch (SmtpConnectionException ex_smtp)
            {
                if (ex_smtp is SmtpConnectionTimeoutException)
                    Logger.Info("SmtpConnectionTimeoutException: " + ex_smtp.Message);
                else Logger.Info("SmtpConnectionException: " + ex_smtp.Message);
            }
            catch (Exception ex)
            {
                Logger.Info("Exception: " + ex.Message);
            }

            throw new Exception();
        }
    }
}
