﻿<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.Mail" %>
<%@ Import Namespace="ASC.Web.Mail.Resources" %>

<script id="message-print-tmpl" type="text/x-jquery-tmpl">
    <div id="message-print-view">
        {{if messages.length > 0}}
            <div class="head-subject">
                <div class="viewTitle">${messages[0].subject}</div>
            </div>
        {{/if}}

        {{each(index, message) messages}}
        <div class="message-print-box" data-messageid="${id}">
            <div class="message-wrap">
                <div class="full-view">
                    <div class="head">
                        <div class="row">
                            <label><%: MailScriptResource.FromLabel %>:</label>
                            <div class="value">${from}</div>
                        </div>
                        <div class="row">
                            <label><%: MailScriptResource.ToLabel %>:</label>
                            <div class="value">${to}</div>
                        </div>
                        {{if cc }}
                        <div class="row">
                            <label><%: MailResource.CopyLabel %>:</label>
                            <div class="value">${cc}</div>
                        </div>
                        {{/if}}
                        {{if bcc }}
                        <div class="row">
                            <label><%: MailResource.BCCLabel %>:</label>
                            <div class="value">${bcc}</div>
                        </div>
                        {{/if}}
                        <div class="row">
                            <label><%: MailScriptResource.DateLabel %>:</label>
                            <div class="value">${date}</div>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="display-none">
                {{if contentIsBlocked == true}}
                    {{tmpl(message) "messageBlockContent"}}
                {{/if}}
            </div>

            <div class="body"></div>
            
            {{if hasAttachments == true}}
                <div class="attachments">
                    {{if attachments.length > 0}}
                        <div class="title-attachments">
                            <div class="icon"><i class="icon-attachment"></i></div>
                            <div class="attachment-message has-attachment"></div><%: MailResource.Attachments %> (${attachments.length}):
                            <span class="fullSizeLabel">
                                <%: MailResource.FullSize %>: ${$item.fileSizeToStr(full_size)}
                            </span>
                        </div>
                    {{/if}}
                    <table class="attachments_list">
                        <tbody>
                            {{each attachments}}
                                <tr class="row">
                                    <td class="file_icon">
                                        <div class="attachmentImage ${$value.iconCls}"/>
                                    </td>
                                    <td class="file_info">
                                        <span title="${$value.fileName}">
                                            <span class="file-name">
                                                ${$item.cutFileName($item.getFileNameWithoutExt($value.fileName))}
                                                <span class="file-extension">${$item.getFileExtension($value.fileName)}</span>
                                            </span>
                                        </span>
                                        <span class="fullSizeLabel">(${$item.fileSizeToStr($value.size)})</span>
                                    </td>
                                </tr>
                            {{/each}}
                        </tbody>
                    </table>
                </div>
            {{/if}}
        </div>
        {{/each}}
    </div>
</script>