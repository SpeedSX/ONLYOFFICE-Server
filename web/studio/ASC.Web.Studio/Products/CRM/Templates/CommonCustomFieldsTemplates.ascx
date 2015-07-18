﻿<%@ Control Language="C#" AutoEventWireup="false" EnableViewState="false" %>
<%@ Assembly Name="ASC.Web.CRM" %>
<%@ Assembly Name="ASC.Web.Core" %>
<%@ Import Namespace="ASC.Web.CRM.Resources" %>

<%--Custom fields for details pages--%>

<script id="customFieldListTmpl" type="text/x-jquery-tmpl">
    {{if label==""}}
        <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
            <colgroup>
                <col style="width: 50px;"/>
                <col style="width: 22px;"/>
                <col/>
            </colgroup>
            <tbody>
                <tr class="headerToggleBlock" data-toggleId="0">
                    <td colspan="3" style="white-space:nowrap;">
                        <span class="headerToggle header-base"><%= CRMCommonResource.AdditionalInformation %></span>
                        <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                        <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                    </td>
                </tr>
                {{tmpl(list) "fieldTmpl"}}
            </tbody>
        </table>
    {{else}}
        <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
            <colgroup>
                <col style="width: 50px;"/>
                <col style="width: 22px;"/>
                <col/>
            </colgroup>
            <tbody>
                <tr class="headerToggleBlock" data-toggleId="${labelid}">
                    <td colspan="3" style="white-space:nowrap;">
                        <span class="headerToggle header-base">${label}</span>
                        <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                        <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                    </td>
                </tr>
                {{tmpl(list) "fieldTmpl"}}
            </tbody>
        </table>
    {{/if}}
</script>

<script id="fieldTmpl" type="text/x-jquery-tmpl">
    {{if fieldType ==  0 || fieldType ==  2 || fieldType ==  5}}
        <tr>
            <td class="describe-text" style="white-space:nowrap;" title="${label}">${label}:</td>
            <td></td>
            <td id="custom_field_${id}">${value.replace(/  /g, " &nbsp;")}</td>
        </tr>
    {{else fieldType ==  1}}
        <tr>
            <td class="describe-text" style="white-space:nowrap;" title="${label}">${label}:</td>
            <td></td>
            <td id="custom_field_${id}">{{html jq.htmlEncodeLight(value).replace(/&#10;/g, "<br/>").replace(/  /g, " &nbsp;") }}</td>
        </tr>
    {{else fieldType ==  3}}
        <tr>
            <td class="describe-text" style="white-space:nowrap" title="${label}">${label}:</td>
            <td></td>
            <td>
                {{if value == "true"}}
                  <input id="custom_field_${id}" type="checkbox" style="vertical-align:middle;" checked="checked" disabled="disabled"/>
               {{else}}
                  <input id="custom_field_${id}" type="checkbox" style="vertical-align:middle;" disabled="disabled"/>
               {{/if}}
            </td>
        </tr>
    {{/if}}
</script>

<script id="customFieldListWithoutLabelTmpl" type="text/x-jquery-tmpl">
    <table border="0" cellpadding="0" cellspacing="0" class="crm-detailsTable">
        <colgroup>
            <col style="width: 50px;"/>
            <col style="width: 22px;"/>
            <col/>
        </colgroup>
        <tbody>
            <tr class="headerToggleBlock" data-toggleId="0">
                <td colspan="3" style="white-space:nowrap;">
                    <span class="headerToggle header-base"><%= CRMCommonResource.AdditionalInformation %></span>
                    <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
                    <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
                </td>
            </tr>
            {{tmpl(list) "fieldTmpl"}}
        </tbody>
    </table>
</script>

<script id="customFieldListWithoutLabelWithGroupTmpl" type="text/x-jquery-tmpl">
    {{tmpl(list) "fieldTmpl"}}
</script>


<%--Custom fields for actions pages--%>

<script id="customFieldRowTmpl" type="text/x-jquery-tmpl">
    {{if fieldType ==  3}}
        <dt class="header-base-small">
             <label>
               {{tmpl "fieldForActionsTmpl"}}
                ${label}
             </label>
        </dt>
        <dd><input type="hidden" name="customField_${id}" /></dd>
    {{else fieldType ==  4}}
        <dt class="headerToggleBlock clearFix">
            {{tmpl "fieldForActionsTmpl"}}
            <span class="openBlockLink"><%= CRMCommonResource.Show %></span>
            <span class="closeBlockLink"><%= CRMCommonResource.Hide %></span>
        </dt>
        <dd class="underHeaderBase clearFix"></dd>
    {{else}}
        <dt class="header-base-small">${label}</dt>
        <dd>
            {{tmpl "fieldForActionsTmpl"}}
        </dd>
    {{/if}}
</script>

<script id="fieldForActionsTmpl" type="text/x-jquery-tmpl">
    {{if fieldType ==  0}}
        <input id="custom_field_${id}" name="customField_${id}" size="${(mask.size > 150 ? 150 : mask.size)}"
                type="text" class="textEdit" maxlength="255" value="${value}"/>
    {{else fieldType ==  1}}
        <textarea rows="${(mask.rows > 25 ? 25 : mask.rows)}" cols="${(mask.cols > 150 ? 150 : mask.cols)}" name="customField_${id}"
                id="custom_field_${id}">${value}</textarea>
    {{else fieldType ==  2}}
        <select class="comboBox" name="customField_${id}" id="custom_field_${id}">
             <option value="">&nbsp;</option>
          {{each mask}}
             <option value="${$value}">${$value}</option>
          {{/each}}
        </select>
    {{else fieldType ==  3}}
        {{if value == "true"}}
          <input id="custom_field_${id}" type="checkbox" checked="checked"/>
        {{else}}
          <input id="custom_field_${id}" type="checkbox"/>
        {{/if}}
    {{else fieldType ==  4}}
        <span class="headerToggle header-base">${label}</span>
    {{else fieldType ==  5}}
        <input type="text" id="custom_field_${id}" name="customField_${id}" class="textEdit textEditCalendar" value="${value}"/>
    {{/if}}
</script>