﻿<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Support.ascx.cs" Inherits="ASC.Web.Studio.UserControls.Common.Support.Support" %>
<%@ Import Namespace="Resources" %>

<% if (!String.IsNullOrEmpty(SupportFeedbackLink))
   { %>
<li class="menu-item none-sub-list support add-block">
    <div class="category-wrapper">
        <a class="menu-item-label outer-text text-overflow support-link" href="<%= SupportFeedbackLink %>" target="_blank" >
            <span class="menu-item-icon support"></span>
            <span class="menu-item-label inner-text">
                <%= Resource.FeedbackSupport %>
            </span>
        </a>
    </div>
</li>
<% } %>