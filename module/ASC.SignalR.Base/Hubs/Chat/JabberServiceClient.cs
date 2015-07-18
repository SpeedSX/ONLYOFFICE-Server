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


using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Jabber;
using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace ASC.SignalR.Base.Hubs.Chat
{
    public class JabberServiceClient
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);
        private static DateTime lastErrorTime;
        private static ILog log = LogManager.GetLogger(typeof(JabberServiceClient));

        public byte AddXmppConnection(string connectionId, string userName, byte state, int tenantId)
        {
            byte result = Chat.UserOffline;
            if (!IsAvailable()) throw new Exception();

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    result = service.AddXmppConnection(connectionId, userName, state, tenantId);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
                return result;
            }
        }

        public byte RemoveXmppConnection(string connectionId, string userName, int tenantId)
        {
            byte result = Chat.UserOffline;
            if (!IsAvailable()) throw new Exception();

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    result = service.RemoveXmppConnection(connectionId, userName, tenantId);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
                return result;
            }
        }

        public bool IsAvailable()
        {
            return lastErrorTime + Timeout < DateTime.Now;
        }
        
        public int GetNewMessagesCount(int tenantId, string userName)
        {
            var result = 0;
            if (string.IsNullOrEmpty(userName) || !IsAvailable())
            {
                return result;
            }

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    result = service.GetNewMessagesCount(tenantId, userName);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
            return result;
        }

        public string GetAuthToken(int tenantId)
        {
            var result = string.Empty;
            if (!IsAvailable())
            {
                return result;
            }

            var userName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName ?? string.Empty;
            if (string.IsNullOrEmpty(userName))
            {
                return result;
            }

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    result = Attempt(() => service.GetUserToken(tenantId, userName), 3);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
            return result;
        }

        public void SendCommand(int tenantId, string from, string to, string command, bool fromTenant)
        {
            if (!IsAvailable()) return;

            using (var service = new JabberServiceClientWcf())
            {
                try
                {
                    service.SendCommand(tenantId, from, to, command, fromTenant);
                }
                catch (Exception error)
                {
                    ProcessError(error);
                }
            }
        }

        public void SendMessage(int tenantId, string from, string to, string text)
        {
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    service.SendMessage(tenantId, from, to, text, null);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        public byte SendState(int tenantId, string userName, byte state)
        {
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    return service.SendState(tenantId, userName, state);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return Chat.UserOffline;
        }

        public MessageClass[] GetRecentMessages(int tenantId, string from, string to, int id)
        {
            MessageClass[] messages = null;
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    messages = service.GetRecentMessages(tenantId, from, to, id);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return messages;
        }

        public Dictionary<string, byte> GetAllStates(int tenantId, string userName)
        {
            Dictionary<string, byte> states = null;
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    states = service.GetAllStates(tenantId, userName);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return states;
        }

        public byte GetState(int tenantId, string userName)
        {
            byte state = 0;
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    state = service.GetState(tenantId, userName);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
            return state;
        }

        public void Ping(string userId, int tenantId, string userName, byte state)
        {
            try
            {
                if (!IsAvailable()) throw new Exception();
                using (var service = new JabberServiceClientWcf())
                {
                    service.Ping(userId, tenantId, userName, state);
                }
            }
            catch (Exception error)
            {
                ProcessError(error);
            }
        }

        private void ProcessError(Exception error)
        {
            log.ErrorFormat("Service Error: {0}, {1}, {2}", error.Message, error.StackTrace,
                (error.InnerException != null) ? error.InnerException.Message : string.Empty);
            if (error is FaultException)
            {
                throw error;
            }
            if (error is CommunicationException || error is TimeoutException)
            {
                lastErrorTime = DateTime.Now;
            }
            throw error;
        }

        private T Attempt<T>(Func<T> f, int count)
        {
            var i = 0;
            while (true)
            {
                try
                {
                    return f();
                }
                catch
                {
                    if (count < ++i)
                    {
                        throw;
                    }
                }
            }
        }
    }
}