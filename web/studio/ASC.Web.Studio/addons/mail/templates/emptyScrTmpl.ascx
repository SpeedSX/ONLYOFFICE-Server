﻿<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>

<%-- Empty screen control --%>
<script id="emptyScrTmpl" type="text/x-jquery-tmpl">
    <div class="noContentBlock emptyScrCtrl" >
        {{if typeof(ImgCss)!="undefined" && ImgCss != null && ImgCss != ""}}
        <table>
            <tr>
                <td>
                    <div class="emptyScrImage img ${ImgCss}"></div>
                </td>
                <td>
                    <div class="emptyScrTd">
        {{/if}}
                    {{if typeof(Header)!="undefined" && Header != null && Header != ""}}
                        <div class="header-base-big">${Header}</div>
                    {{/if}}
                    {{if typeof(HeaderDescribe)!="undefined" && HeaderDescribe != null && HeaderDescribe != ""}}
                        <div class="emptyScrHeadDscr">${HeaderDescribe}</div>
                    {{/if}}
                    {{if typeof(Describe)!="undefined" && Describe != null && Describe != ""}}
                        <div class="emptyScrDscr">{{html Describe}}</div>
                    {{/if}}
                    {{if typeof(ButtonHTML)!="undefined" && ButtonHTML != null && ButtonHTML != ""}}
                        <div class="emptyScrBttnPnl">{{html ButtonHTML}}</div>
                    {{/if}}
        {{if typeof(ImgCss)!="undefined" && ImgCss != null && ImgCss != ""}}
                    </div>
                </td>
            </tr>
        </table>
        {{/if}}
    </div>
</script>

<script id="emptyScrButtonTmpl" type="text/x-jquery-tmpl">
    {{each(index, button) buttons}}
        {{if index > 0 }}
        <div style="height:8px;"></div>
        {{/if}}

        {{if button.href != null }}
        <a href="${button.href}" class="${button.cssClass} link dotline">${button.text}</a>
        {{else}}
        <a class="${button.cssClass} link dotline">${button.text}</a>
        {{/if}}
    {{/each}}
</script>