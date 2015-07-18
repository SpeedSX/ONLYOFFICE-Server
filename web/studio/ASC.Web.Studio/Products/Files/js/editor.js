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


;
window.ASC.Files.Editor = (function () {
    var isInit = false;

    var docIsChanged = false;
    var fixedVersion = false;
    var canShowHistory = false;

    var docEditor = null;
    var docServiceParams = null;

    var trackEditTimeout = null;
    var shareLinkParam = "";
    var docKeyForTrack = "";
    var tabId = "";
    var serverErrorMessage = null;
    var editByUrl = false;
    var canCreate = true;
    var thirdPartyApp = false;
    var options = null;
    var openinigDate;
    var newScheme = false;
    var doStartEdit = true;

    var init = function () {
        if (isInit === false) {
            isInit = true;
        }

        jq("body").css("overflow-y", "hidden");

        window.onbeforeunload = ASC.Files.Editor.finishEdit;

        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.TrackEditFile, completeTrack);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.CanEditFile, completeCanEdit);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.SaveEditing, completeSave);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.StartEdit, onStartEdit);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetEditHistory, completeGetEditHistory);
        ASC.Files.ServiceManager.bind(ASC.Files.ServiceManager.events.GetDiffUrl, completeGetDiffUrl);
    };

    var createFrameEditor = function (serviceParams) {

        jq("#iframeEditor").parents().css("height", "100%");

        checkDocVersion();

        if (serviceParams) {
            var embedded = (serviceParams.type == "embedded");

            var documentConfig = {
                title: serviceParams.file.title,
                url: serviceParams.url,
                fileType: serviceParams.fileType,
                key: serviceParams.key,
                vkey: serviceParams.vkey,
                options: ASC.Files.Editor.options,
            };

            if (!embedded) {
                documentConfig.info = {
                    author: serviceParams.file.create_by,
                    created: serviceParams.file.create_on
                };
                if (serviceParams.filePath.length) {
                    documentConfig.info.folder = serviceParams.filePath;
                }
                if (serviceParams.sharingSettings.length) {
                    documentConfig.info.sharingSettings = serviceParams.sharingSettings;
                }

                documentConfig.permissions = {
                    edit: serviceParams.canEdit
                };
            }

            var editorConfig = {
                mode: serviceParams.mode,
                lang: serviceParams.lang,
                canAutosave: ASC.Files.Editor.newScheme || !serviceParams.file.provider_key,
                branding: {
                    logoUrl: ASC.Files.Editor.brandingLogoUrl || "",
                    customerLogo: ASC.Files.Editor.brandingCustomerLogo || "",
                },
            };

            if (embedded) {
                editorConfig.embedded = {
                    saveUrl: serviceParams.downloadUrl,
                    embedUrl: serviceParams.embeddedUrl,
                    shareUrl: serviceParams.viewerUrl,
                    toolbarDocked: "top"
                };

                var keyFullscreen = "fullscreen";
                if (location.hash.indexOf(keyFullscreen) < 0) {
                    editorConfig.embedded.fullscreenUrl = serviceParams.embeddedUrl + "#" + keyFullscreen;
                }
            } else {
                editorConfig.canCoAuthoring = true;
                if (ASC.Files.Constants.URL_HANDLER_CREATE) {
                    editorConfig.createUrl = ASC.Files.Constants.URL_HANDLER_CREATE;
                }

                if (serviceParams.sharingSettingsUrl) {
                    editorConfig.sharingSettingsUrl = serviceParams.sharingSettingsUrl;
                }

                if (serviceParams.fileChoiceUrl) {
                    editorConfig.fileChoiceUrl = serviceParams.fileChoiceUrl;
                }

                editorConfig.templates =
                    jq(serviceParams.templates).map(
                        function (i, item) {
                            return {
                                name: item.Key,
                                icon: item.Value
                            };
                        }).toArray();

                editorConfig.user = {
                    id: serviceParams.user.key,
                    name: serviceParams.user.value
                };

                if (serviceParams.type != "embedded") {
                    var listRecent = getRecentList();
                    if (listRecent && listRecent.length) {
                        editorConfig.recent = listRecent.toArray();
                    }
                }
            }

            var typeConfig = serviceParams.type;
            var documentTypeConfig = serviceParams.documentType;
        }

        var eventsConfig = {
            "onReady": ASC.Files.Editor.readyEditor,
            "onDocumentStateChange": ASC.Files.Editor.documentStateChangeEditor,
            "onRequestEditRights": ASC.Files.Editor.requestEditRightsEditor,
            "onSave": ASC.Files.Editor.saveEditor,
            "onError": ASC.Files.Editor.errorEditor,
            "onOutdatedVersion": ASC.Files.Editor.outdatedVersion
        };

        if (serviceParams) {
        if (serviceParams.folderUrl.length > 0) {
            eventsConfig.onBack = ASC.Files.Editor.backEditor;
        }

        if (ASC.Files.Editor.newScheme
            && false
            && !serviceParams.file.provider_key
            && !ASC.Files.Editor.editByUrl
            && !ASC.Files.Editor.thirdPartyApp) {

            ASC.Files.Editor.canShowHistory = true;
            eventsConfig.onRequestHistory = ASC.Files.Editor.requestHistory;
            eventsConfig.onRequestHistoryData = ASC.Files.Editor.getDiffUrl;
        }
        }

        ASC.Files.Editor.docEditor = new DocsAPI.DocEditor("iframeEditor", {
            width: "100%",
            height: "100%",

            type: typeConfig || "desktop",
            documentType: documentTypeConfig,
            document: documentConfig,
            editorConfig: editorConfig,
            events: eventsConfig
        });
    };

    var fixSize = function () {
        var wrapEl = document.getElementById("wrap");
        if (wrapEl) {
            wrapEl.style.height = screen.availHeight + "px";
            window.scrollTo(0, -1);
            wrapEl.style.height = window.innerHeight + "px";
        }
    };

    var checkDocVersion = function () {
        if (!ASC.Files.Editor.newScheme && DocsAPI.DocEditor.version) {
            var version = DocsAPI.DocEditor.version();
            if ((parseFloat(version) || 0) >= 3) {
                ASC.Files.Editor.newScheme = true;
            }
        }
    };

    var readyEditor = function () {
        if (ASC.Files.Editor.serverErrorMessage) {
            docEditorShowError(ASC.Files.Editor.serverErrorMessage);
            return;
        }

        if (checkMessageFromHash()) {
            location.hash = "";
            return;
        }

        if (ASC.Files.Editor.docServiceParams && ASC.Files.Editor.docServiceParams.mode === "edit") {
            ASC.Files.Editor.trackEdit();
        }
    };

    var backEditor = function (event) {
        clearTimeout(trackEditTimeout);
        var href = ASC.Files.Editor.docServiceParams ? ASC.Files.Editor.docServiceParams.folderUrl : ASC.Files.Constants.URL_FILES_START;
        if (event && event.data) {
            window.open(href, "_blank");
        } else {
            location.href = href;
        }
    };

    var documentStateChangeEditor = function (event) {
        if (docIsChanged != event.data) {
            document.title = ASC.Files.Editor.docServiceParams.file.title + (event.data ? " *" : "");
            docIsChanged = event.data;
        }

        if (doStartEdit && event.data) {
            doStartEdit = false;
            if (ASC.Files.Editor.newScheme) {
                ASC.Files.ServiceManager.startEdit(ASC.Files.ServiceManager.events.StartEdit, {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    docKeyForTrack: ASC.Files.Editor.docServiceParams.key,
                    asNew: ASC.Files.Editor.options.asNew,
                    shareLinkKey: ASC.Files.Editor.shareLinkParam,
                });
            }
        }
    };

    var errorEditor = function () {
        ASC.Files.Editor.finishEdit();
    };

    var saveEditor = function (event) {
        if (ASC.Files.Editor.newScheme) {
            setTimeout(function () {
                ASC.Files.Editor.documentStateChangeEditor({ data: false });
                ASC.Files.Editor.docEditor.processSaveResult(true);
            }, 1);
            return true;
        }

        var urlSavedDoc = event.data;

        if (ASC.Files.Editor.editByUrl) {

            ASC.Files.Editor.docEditor.processSaveResult(true);

            var urlRedirect = ASC.Files.Editor.FileWebEditorExternalUrlString
                .format(encodeURIComponent(urlSavedDoc), encodeURIComponent(ASC.Files.Editor.docServiceParams.file.title));

            location.href = urlRedirect + "&openfolder=true";
            return false;

        } else {

            ASC.Files.ServiceManager.saveEditing(ASC.Files.ServiceManager.events.SaveEditing,
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    version: ASC.Files.Editor.docServiceParams.file.version,
                    tabId: ASC.Files.Editor.tabId,
                    fileUri: urlSavedDoc,
                    asNew: ASC.Files.Editor.options.asNew,
                    shareLinkKey: ASC.Files.Editor.shareLinkParam
                });

            return false;
        }
    };

    var requestEditRightsEditor = function () {
        if (ASC.Files.Editor.editByUrl || ASC.Files.Editor.newScheme) {
            location.href = ASC.Files.Editor.docServiceParams.linkToEdit + ASC.Files.Editor.shareLinkParam;
        } else {
            ASC.Files.ServiceManager.canEditFile(ASC.Files.ServiceManager.events.CanEditFile,
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    shareLinkParam: ASC.Files.Editor.shareLinkParam
                });
        }
    };

    var requestHistory = function () {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getEditHistory(ASC.Files.ServiceManager.events.GetEditHistory,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                shareLinkParam: ASC.Files.Editor.shareLinkParam
            });
    };

    var getDiffUrl = function (version) {
        if (!ASC.Files.Editor.canShowHistory) {
            return;
        }

        ASC.Files.ServiceManager.getDiffUrl(ASC.Files.ServiceManager.events.GetDiffUrl,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                version: version | 0,
                shareLinkParam: ASC.Files.Editor.shareLinkParam
            });
    };

    var outdatedVersion = function () {
        location.reload(true);
    };

    var trackEdit = function () {
        clearTimeout(trackEditTimeout);
        if (ASC.Files.Editor.editByUrl || ASC.Files.Editor.thirdPartyApp) {
            return;
        }
        if (ASC.Files.Editor.newScheme && !doStartEdit) {
            return;
        }

        ASC.Files.ServiceManager.trackEditFile(ASC.Files.ServiceManager.events.TrackEditFile,
            {
                fileID: ASC.Files.Editor.docServiceParams.file.id,
                tabId: ASC.Files.Editor.tabId,
                docKeyForTrack: ASC.Files.Editor.docKeyForTrack,
                shareLinkParam: ASC.Files.Editor.shareLinkParam,
                fixedVersion: ASC.Files.Editor.fixedVersion
            });
    };

    var finishEdit = function () {
        if (trackEditTimeout !== null && (!ASC.Files.Editor.newScheme || doStartEdit)) {
            ASC.Files.ServiceManager.trackEditFile("FinishTrackEditFile",
                {
                    fileID: ASC.Files.Editor.docServiceParams.file.id,
                    tabId: ASC.Files.Editor.tabId,
                    docKeyForTrack: ASC.Files.Editor.docKeyForTrack,
                    shareLinkParam: ASC.Files.Editor.shareLinkParam,
                    finish: true,
                    ajaxsync: true
                });
        }
    };

    var completeSave = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            var saveResult = false;
        } else {
            saveResult = true;
            ASC.Files.Editor.fixedVersion = true;
            ASC.Files.Editor.documentStateChangeEditor({ data: false });
            ASC.Files.Editor.trackEdit();
        }

        ASC.Files.Editor.docEditor.processSaveResult(saveResult === true, errorMessage);
    };

    var completeTrack = function (jsonData, params, errorMessage) {
        clearTimeout(trackEditTimeout);
        if (typeof errorMessage != "undefined") {
            if (errorMessage == null) {
                docEditorShowInfo("Connection is lost");
            } else {
                docEditorShowWarning(errorMessage || "Connection is lost");
            }
            return;
        }

        if (jsonData.key == true) {
            trackEditTimeout = setTimeout(ASC.Files.Editor.trackEdit, 5000);
        } else {
            errorMessage = jsonData.value;
            ASC.Files.Editor.docEditor.processRightsChange(false, errorMessage);
        }
    };

    var onStartEdit = function (jsonData, params, errorMessage) {
        if (typeof errorMessage != "undefined") {
            ASC.Files.Editor.docEditor.processRightsChange(false, errorMessage || "Connection is lost");
            return;
        }
    };

    var docEditorShowError = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "error");
    };

    var docEditorShowWarning = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "warning");
    };

    var docEditorShowInfo = function (message) {
        ASC.Files.Editor.docEditor.showMessage("Teamlab Office", message, "info");
    };

    var checkMessageFromHash = function () {
        var regExpError = /^#error\/(\S+)?/;
        if (regExpError.test(location.hash)) {
            var errorMessage = regExpError.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                docEditorShowWarning(errorMessage);
                return true;
            }
        }
        var regExpMessage = /^#message\/(\S+)?/;
        if (regExpMessage.test(location.hash)) {
            errorMessage = regExpMessage.exec(location.hash)[1];
            errorMessage = decodeURIComponent(errorMessage).replace(/\+/g, " ");
            if (errorMessage.length) {
                docEditorShowInfo(errorMessage);
                return true;
            }
        }
        return false;
    };

    var getRecentList = function () {
        if (!localStorageManager.isAvailable) {
            return null;
        }
        var localStorageKey = ASC.Files.Constants.storageKeyRecent;
        var localStorageCount = 50;
        var recentCount = 10;

        var result = new Array();

        try {
            var recordsFromStorage = localStorageManager.getItem(localStorageKey);
            if (!recordsFromStorage) {
                recordsFromStorage = new Array();
            }

            if (recordsFromStorage.length > localStorageCount) {
                recordsFromStorage = recordsFromStorage.pop();
            }

            var currentRecord = {
                url: location.href,
                id: ASC.Files.Editor.docServiceParams.file.id,
                title: ASC.Files.Editor.docServiceParams.file.title,
                folder: ASC.Files.Editor.docServiceParams.filePath,
                fileType: ASC.Files.Editor.docServiceParams.fileTypeNum
            };

            var containRecord = jq(recordsFromStorage).is(function () {
                return this.id == currentRecord.id;
            });

            if (!containRecord) {
                recordsFromStorage.unshift(currentRecord);

                localStorageManager.setItem(localStorageKey, recordsFromStorage);
            }

            result = jq(recordsFromStorage).filter(function () {
                return this.id != currentRecord.id &&
                    this.fileType === currentRecord.fileType;
            });
        } catch (e) {
        }

        return result.slice(0, recentCount);
    };

    var completeCanEdit = function (jsonData, params, errorMessage) {
        var result = typeof jsonData != "undefined";
        // occurs whenever the user tryes to enter edit mode
        ASC.Files.Editor.docEditor.applyEditRights(result, errorMessage);

        if (result) {
            ASC.Files.Editor.tabId = jsonData;
            ASC.Files.Editor.trackEdit();
        }
    };

    var completeGetEditHistory = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.refreshHistory != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
                return;
            }
        } else {
            if (typeof errorMessage != "undefined") {
                var data = {
                    error: errorMessage || "Connection is lost"
                };
            } else {
                data = {
                    currentVersion: ASC.Files.Editor.docServiceParams.file.version,
                    history: jsonData
                };
            }

            ASC.Files.Editor.docEditor.refreshHistory(data);
        }
    };

    var completeGetDiffUrl = function (jsonData, params, errorMessage) {
        if (typeof ASC.Files.Editor.docEditor.setHistoryData != "function") {
            if (typeof errorMessage != "undefined") {
                docEditorShowError(errorMessage || "Connection is lost");
                return;
            }
        } else {
            if (typeof errorMessage != "undefined") {
                var data = {
                    error: errorMessage || "Connection is lost"
                };
            } else {
                data = {
                    version: params.version,
                    url: jsonData.key,
                    urlDiff: jsonData.value
                };
            }

            ASC.Files.Editor.docEditor.setHistoryData(data);
        }
    };

    return {
        init: init,
        createFrameEditor: createFrameEditor,
        fixSize: fixSize,

        docEditor: docEditor,

        //set in .cs
        docServiceParams: docServiceParams,
        shareLinkParam: shareLinkParam,
        docKeyForTrack: docKeyForTrack,
        tabId: tabId,
        serverErrorMessage: serverErrorMessage,
        editByUrl: editByUrl,
        canCreate: canCreate,
        options: options,
        thirdPartyApp: thirdPartyApp,
        openinigDate: openinigDate,
        newScheme: newScheme,

        trackEdit: trackEdit,
        finishEdit: finishEdit,

        //event
        readyEditor: readyEditor,
        backEditor: backEditor,
        documentStateChangeEditor: documentStateChangeEditor,
        requestEditRightsEditor: requestEditRightsEditor,
        errorEditor: errorEditor,
        saveEditor: saveEditor,
        outdatedVersion: outdatedVersion,
        requestHistory: requestHistory,
        getDiffUrl: getDiffUrl,

        fixedVersion: fixedVersion,
        canShowHistory: canShowHistory,
    };
})();

(function ($) {
    ASC.Files.Editor.init();
    $(function () {
        if (typeof DocsAPI === "undefined") {
            alert("ONLYOFFICE™  is not available. Please contact us at support@onlyoffice.com");
            ASC.Files.Editor.errorEditor();

            return;
        }

        var fixPageCaching = function (delta) {
            if (location.hash.indexOf("reload") == -1) {
                var openingDateParse = Date.parse(ASC.Files.Editor.openinigDate);
                if (!openingDateParse) {
                    return;
                }
                var openinigDate = new Date();
                openinigDate.setTime(openingDateParse);

                var currentTime = new Date();
                var currentUTCTime = new Date(currentTime.getUTCFullYear(), currentTime.getUTCMonth(), currentTime.getUTCDate(), currentTime.getUTCHours(), currentTime.getUTCMinutes());
                if (Math.abs(currentUTCTime - openinigDate) > delta) {
                    location.hash = "reload";
                    location.reload(true);
                }
            } else {
                location.hash = "";
            }
        };
        fixPageCaching(10 * 60 * 1000);

        var $icon = jq("#docsEditorFavicon");
        if ($icon.attr('href').indexOf('logo_favicon_general.ico') !== -1) {//not default
             $icon.attr('href', $icon.attr('href'));
        }

        ASC.Files.Editor.createFrameEditor(ASC.Files.Editor.docServiceParams);

        if (jq("body").hasClass("mobile") || ASC.Files.Editor.docServiceParams && ASC.Files.Editor.docServiceParams.type === "mobile") {
            window.addEventListener("load", ASC.Files.Editor.fixSize);
            window.addEventListener("orientationchange", ASC.Files.Editor.fixSize);
        }
    });
})(jQuery);

String.prototype.format = function () {
    var txt = this,
        i = arguments.length;

    while (i--) {
        txt = txt.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return txt;
};