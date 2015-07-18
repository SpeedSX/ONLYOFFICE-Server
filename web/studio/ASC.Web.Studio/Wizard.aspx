﻿<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" MasterPageFile="~/Masters/basetemplate.master" CodeBehind="Wizard.aspx.cs" Inherits="ASC.Web.Studio.Wizard" Title="ONLYOFFICE™" %>

<%@ Import Namespace="ASC.Core" %>
<%@ Import Namespace="ASC.Web.Studio.Core" %>
<%@ MasterType TypeName="ASC.Web.Studio.Masters.BaseTemplate" %>
<asp:Content ContentPlaceHolderID="PageContent" runat="server">

    <div class="wizardContent">
        <div class="wizardTitle">
            <%= Resources.Resource.WelcomeTitle %>
        </div>
        <div class="wizardDesc"><%= Resources.Resource.WelcomeDescription %></div>
        <asp:PlaceHolder ID="content" runat="server"></asp:PlaceHolder>
    </div>


 <% if (!CoreContext.Configuration.Standalone && !CoreContext.Configuration.Personal && SetupInfo.CustomScripts.Length != 0)
       { %>

<!-- Google Code for New_1214 Conversion Page -->
<script type="text/javascript">
/* <![CDATA[ */
var google_conversion_id = 1025072253;
var google_conversion_language = "en";
var google_conversion_format = "2";
var google_conversion_color = "ffffff";
var google_conversion_label = "_v6_CNajnlgQ_bjl6AM";
var google_remarketing_only = false;
/* ]]> */
</script>
<script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">
</script>
<noscript>
<div style="display:inline;">
<img height="1" width="1" style="border-style:none;" alt="" src="//www.googleadservices.com/pagead/conversion/1025072253/?label=_v6_CNajnlgQ_bjl6AM&guid=ON&script=0"/>
</div>
</noscript>

<script>(function(w,d,t,r,u){var f,n,i;w=w||[],f=function(){var o={ti:"4024276"};o.q=w,w=new UET(o),w.push("pageLoad")},n=d.createElement(t),n.src=r,n.async=1,n.onload=n.onreadystatechange=function(){var s=this.readyState;s&&s!=="loaded"&&s!=="complete"||(f(),n.onload=n.onreadystatechange=null)},i=d.getElementsByTagName(t)[0],i.parentNode.insertBefore(n,i)})(window,document,"script","//bat.bing.com/bat.js","uetq");</script>

    <% } %>

</asp:Content>
