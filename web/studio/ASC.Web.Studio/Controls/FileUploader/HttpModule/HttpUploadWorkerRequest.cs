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
using System.Web;

namespace ASC.Web.Studio.Controls.FileUploader.HttpModule
{
    internal class HttpUploadWorkerRequest : HttpWorkerRequest
    {
        private readonly HttpWorkerRequest _httpWorkerRequest;
        private readonly EntityBodyInspector _inspector;

        #region Abstract Method implementation

        public HttpUploadWorkerRequest(HttpWorkerRequest request)
        {
            _httpWorkerRequest = request;
            _inspector = new EntityBodyInspector(this);
        }

        #region Default Implementation

        public override void FlushResponse(bool finalFlush)
        {
            _httpWorkerRequest.FlushResponse(finalFlush);
        }

        public override string GetHttpVerbName()
        {
            return _httpWorkerRequest.GetHttpVerbName();
        }

        public override string GetHttpVersion()
        {
            return _httpWorkerRequest.GetHttpVersion();
        }

        public override string GetLocalAddress()
        {
            return _httpWorkerRequest.GetLocalAddress();
        }

        public override int GetLocalPort()
        {
            return _httpWorkerRequest.GetLocalPort();
        }

        public override string GetQueryString()
        {
            return _httpWorkerRequest.GetQueryString();
        }

        public override string GetRawUrl()
        {
            return _httpWorkerRequest.GetRawUrl();
        }

        public override string GetRemoteAddress()
        {
            return _httpWorkerRequest.GetRemoteAddress();
        }

        public override int GetRemotePort()
        {
            return _httpWorkerRequest.GetRemotePort();
        }

        public override string GetUriPath()
        {
            return _httpWorkerRequest.GetUriPath();
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            _httpWorkerRequest.SendKnownResponseHeader(index, value);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            _httpWorkerRequest.SendResponseFromFile(handle, offset, length);
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            _httpWorkerRequest.SendResponseFromFile(filename, offset, length);
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            _httpWorkerRequest.SendResponseFromMemory(data, length);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            _httpWorkerRequest.SendStatus(statusCode, statusDescription);
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            _httpWorkerRequest.SendUnknownResponseHeader(name, value);
        }

        public override void CloseConnection()
        {
            _httpWorkerRequest.CloseConnection();
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            _httpWorkerRequest.SendCalculatedContentLength(contentLength);
        }

        public override string GetAppPathTranslated()
        {
            return _httpWorkerRequest.GetAppPathTranslated();
        }

        public override string GetAppPath()
        {
            return _httpWorkerRequest.GetAppPath();
        }

        public override string GetAppPoolID()
        {
            return _httpWorkerRequest.GetAppPoolID();
        }

        public override long GetBytesRead()
        {
            return _httpWorkerRequest.GetBytesRead();
        }

        public override byte[] GetClientCertificate()
        {
            return _httpWorkerRequest.GetClientCertificate();
        }

        public override byte[] GetClientCertificateBinaryIssuer()
        {
            return _httpWorkerRequest.GetClientCertificateBinaryIssuer();
        }

        public override int GetClientCertificateEncoding()
        {
            return _httpWorkerRequest.GetClientCertificateEncoding();
        }

        public override byte[] GetClientCertificatePublicKey()
        {
            return _httpWorkerRequest.GetClientCertificatePublicKey();
        }

        public override DateTime GetClientCertificateValidFrom()
        {
            return _httpWorkerRequest.GetClientCertificateValidFrom();
        }

        public override DateTime GetClientCertificateValidUntil()
        {
            return _httpWorkerRequest.GetClientCertificateValidUntil();
        }

        public override long GetConnectionID()
        {
            return _httpWorkerRequest.GetConnectionID();
        }

        public override string GetFilePath()
        {
            return _httpWorkerRequest.GetFilePath();
        }

        public override string GetFilePathTranslated()
        {
            return _httpWorkerRequest.GetFilePathTranslated();
        }

        public override string GetKnownRequestHeader(int index)
        {
            return _httpWorkerRequest.GetKnownRequestHeader(index);
        }

        public override string GetPathInfo()
        {
            return _httpWorkerRequest.GetPathInfo();
        }

        public override string GetProtocol()
        {
            return _httpWorkerRequest.GetProtocol();
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return _httpWorkerRequest.GetQueryStringRawBytes();
        }

        public override string GetRemoteName()
        {
            return _httpWorkerRequest.GetRemoteName();
        }

        public override int GetRequestReason()
        {
            return _httpWorkerRequest.GetRequestReason();
        }

        public override string GetServerName()
        {
            return _httpWorkerRequest.GetServerName();
        }

        public override string GetServerVariable(string name)
        {
            return _httpWorkerRequest.GetServerVariable(name);
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return _httpWorkerRequest.GetUnknownRequestHeader(name);
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return _httpWorkerRequest.GetUnknownRequestHeaders();
        }

        public override long GetUrlContextID()
        {
            return _httpWorkerRequest.GetUrlContextID();
        }

        public override IntPtr GetUserToken()
        {
            return _httpWorkerRequest.GetUserToken();
        }

        public override bool Equals(object obj)
        {
            return _httpWorkerRequest.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _httpWorkerRequest.GetHashCode();
        }

        public override IntPtr GetVirtualPathToken()
        {
            return _httpWorkerRequest.GetVirtualPathToken();
        }

        public override bool HeadersSent()
        {
            return _httpWorkerRequest.HeadersSent();
        }

        public override bool IsClientConnected()
        {
            return _httpWorkerRequest.IsClientConnected();
        }

        public override bool IsSecure()
        {
            return _httpWorkerRequest.IsSecure();
        }

        public override string MachineConfigPath
        {
            get { return _httpWorkerRequest.MachineConfigPath; }
        }

        public override string MachineInstallDirectory
        {
            get { return _httpWorkerRequest.MachineInstallDirectory; }
        }

        public override string MapPath(string virtualPath)
        {
            return _httpWorkerRequest.MapPath(virtualPath);
        }

        public override Guid RequestTraceIdentifier
        {
            get { return _httpWorkerRequest.RequestTraceIdentifier; }
        }

        public override string RootWebConfigPath
        {
            get { return _httpWorkerRequest.RootWebConfigPath; }
        }

        public override void SendCalculatedContentLength(long contentLength)
        {
            _httpWorkerRequest.SendCalculatedContentLength(contentLength);
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            _httpWorkerRequest.SendResponseFromMemory(data, length);
        }

        public override void SetEndOfSendNotification(EndOfSendNotification callback, object extraData)
        {
            _httpWorkerRequest.SetEndOfSendNotification(callback, extraData);
        }

        public override string ToString()
        {
            return _httpWorkerRequest.ToString();
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return _httpWorkerRequest.GetPreloadedEntityBodyLength();
        }

        public override int GetTotalEntityBodyLength()
        {
            return _httpWorkerRequest.GetTotalEntityBodyLength();
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return _httpWorkerRequest.IsEntireEntityBodyIsPreloaded();
        }

        #endregion

        public void EndOfUploadRequest()
        {
            _inspector.EndRequest();
        }

        public override void EndOfRequest()
        {
            _inspector.EndRequest();
            _httpWorkerRequest.EndOfRequest();
        }

        public override byte[] GetPreloadedEntityBody()
        {
            var buffer = _httpWorkerRequest.GetPreloadedEntityBody();
            _inspector.Inspect(buffer, 0, 0);
            return buffer;
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            _inspector.Inspect(buffer, offset, 0);
            return _httpWorkerRequest.GetPreloadedEntityBody(buffer, offset);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            _inspector.Inspect(buffer, offset, size);
            return _httpWorkerRequest.ReadEntityBody(buffer, offset, size);
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            _inspector.Inspect(buffer, 0, size);
            return _httpWorkerRequest.ReadEntityBody(buffer, size);
        }

        #endregion
    }
}