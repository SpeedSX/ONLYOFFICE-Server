﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocService.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Management.DocService" %>
<%@ Import Namespace="ASC.Web.Core.Files" %>
<%@ Import Namespace="Resources" %>

<div id="docServiceBlock" class="settings-block">
    <div class="header-base"><%= Resource.DocServiceUrl %></div>

    <p>
        <%= Resource.DocServiceText %>
    </p>

    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlApi %></div>
        <input id="docServiceUrlApi" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceApiUrl %>" placeholder="https://<editors-dns-name>/OfficeWeb/apps/api/documents/api.js" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/OfficeWeb/apps/api/documents/api.js") %> </div>
    </div>
    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlCommand %></div>
        <input id="docServiceUrlCommand" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceCommandUrl %>" placeholder="https://<editors-dns-name>/coauthoring/CommandService.ashx" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/coauthoring/CommandService.ashx") %> </div>
    </div>
    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlStorage %></div>
        <input id="docServiceUrlStorage" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceStorageUrl %>" placeholder="https://<editors-dns-name>/FileUploader.ashx" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/FileUploader.ashx") %> </div>
    </div>
    <div class="doc-service-item">
        <div class="header-base-small"><%= Resource.DocServiceUrlConverter %></div>
        <input id="docServiceUrlConverter" type="text" class="doc-service-value textEdit"
            value="<%= FilesLinkUtility.DocServiceConverterUrl %>" placeholder="https://<editors-dns-name>/ConvertService.ashx" />
        <div class="gray-text"><%= string.Format(Resource.DocServiceUrlExample, "https://&lt;editors-dns-name&gt;/ConvertService.ashx") %> </div>
    </div>

    <div class="middle-button-container">
        <span id="docServiceButtonSave" class="button blue"><%= Resource.SaveButton %></span>
        <span class="splitter-buttons"></span>
        <span id="docServiceButtonTest" class="button gray"><%= Resource.TestButton %></span>
    </div>
</div>
<div class="settings-help-block">
    <p><%=String.Format(Resource.DocServiceUrlHelp, "<br />","<b>","</b>")%></p>
</div>
