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


jq(function () {
    jq('#savePasswordSettingsBtn').click(PasswordSettingsManager.SaveSettings);
});

jq(document).ready(function () {
    PasswordSettingsManager.LoadSettings();
});

PasswordSettingsManager = new function () {

    this.LoadSettings = function () {
        PasswordSettingsController.LoadPasswordSettings(function (result) {

            var res = result.value;

            if (res == null)
                return;

            var jsonObj = JSON.parse(res);

            jq('#slider').slider({
                range: "max",
                min: 6,
                max: 16,
                value: jsonObj.MinLength,
                slide: function (event, ui) {
                    jq("#count").html(ui.value);
                }
            });

            jq("#count").html(jsonObj.MinLength);
            jq("input#chkUpperCase").prop("checked", jsonObj.UpperCase);
            jq("input#chkDigits").prop("checked", jsonObj.Digits);
            jq("input#chkSpecSymbols").prop("checked", jsonObj.SpecSymbols);
        });
    };

    this.SaveSettings = function () {

        AjaxPro.onLoading = function (b) {
            if (b)
                LoadingBanner.showLoaderBtn("#studio_passwordSettings");
            else
                LoadingBanner.hideLoaderBtn("#studio_passwordSettings");
        };

        var jsonObj = {
            "MinLength": jq('#count').html(),
            "UpperCase": jq("input#chkUpperCase").is(":checked"),
            "Digits": jq("input#chkDigits").is(":checked"),
            "SpecSymbols": jq("input#chkSpecSymbols").is(":checked")
        };

        PasswordSettingsController.SavePasswordSettings(JSON.stringify(jsonObj), function (result) {
            var res = result.value;
            LoadingBanner.showMesInfoBtn("#studio_passwordSettings", res.Message, res.Status == 1 ? "success" : "error");
        });
    };
};