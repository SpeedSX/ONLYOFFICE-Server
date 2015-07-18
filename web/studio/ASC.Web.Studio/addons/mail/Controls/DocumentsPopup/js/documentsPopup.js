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


window.DocumentsPopup = (function($) {
    var isInit = false;
    var lastId = -1;
    var supportedCustomEvents = { SelectFiles: "on_documents_selected" };
    var eventsHandler = jq({});
    var documentSelectorTree;
    var folderFiles = [];
    var isShareableFolder = false;

    var $attachFilesAsLinksSelector;

    function init() {
        if (!isInit) {
            isInit = true;
            folderFiles = [];

            initElements();
            bindEvents();

            documentSelectorTree = new ASC.Files.TreePrototype("#documentSelectorTree");
            documentSelectorTree.clickOnFolder = getListFolderFiles;

            var treeNode = jq("#documentSelectorTree .tree-node")[0];
            lastId = jq(treeNode).attr('data-id');

            jq("#popupDocumentUploader .buttonContainer #attach_btn").unbind('click').bind('click', function() {
                if (!jq(this).hasClass('disable')) {
                    attachSelectedFiles();
                }
                return false;
            });

            jq("#popupDocumentUploader .buttonContainer #cancel_btn").unbind('click').bind('click', function() {
                if (!jq(this).hasClass('disable')) {
                    DocumentsPopup.EnableEsc = true;
                    jq.unblockUI();
                }
                return false;
            });

            jq("#popupDocumentUploader .fileList input").unbind('click').bind('click', function() {
                if (!jq(this).is(":checked")) {
                    jq("#checkAll").prop("checked", false);
                    return;
                }
                var checkedAll = true;
                jq(".fileList input").each(function() {
                    if (!jq(this).is(":checked")) {
                        checkedAll = false;
                        return;
                    }
                });
                if (checkedAll) {
                    jq("#checkAll").prop("checked", true);
                }
            });

            toggleButtons(true, false);
        }
    }

    function initElements() {
        $attachFilesAsLinksSelector = $('#attachFilesAsLinksSelector');
    }

    function bindEvents() {

    }

    function getListFolderFiles(id) {
        if (id == undefined || id == '') {
            return;
        }

        lastId = id;
        toggleButtons(true, false);
        jq(".fileList").empty();
        jq(".loader").show();
        hideEmptyScreen();
        Teamlab.getDocFolder(null, id, function() { onGetFolderFiles(arguments); });
    }

    function showEmptyScreen() {
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").show();
        toggleButtons(true, false);
    }

    function hideEmptyScreen() {
        jq("#popupDocumentUploader .fileContainer").find("#emptyFileList").hide();
    }

    function toggleButtons(hide, all) {
        if (hide) {
            jq("#popupDocumentUploader .buttonContainer #attach_btn").removeClass("disable").addClass("disable");
            if (all) {
                jq("#popupDocumentUploader .buttonContainer #cancel_btn").removeClass("disable").addClass("disable");
            }
        } else {
            jq("#popupDocumentUploader .buttonContainer #attach_btn").removeClass("disable");
            if (all) {
                jq("#popupDocumentUploader .buttonContainer #cancel_btn").removeClass("disable");
            }
        }
    }

    function onGetFolderFiles(args) {
        var content = new Array();
        var folders = args[1].folders;
        for (var i = 0; i < folders.length; i++) {
            var folderName = folders[i].title;
            var folId = folders[i].id;
            var folder = { title: folderName, exttype: "", id: folId, type: "folder" };
            content.push(folder);
        }
        
        folderFiles = args[1].files;
        isShareableFolder = args[1].isShareable === undefined ? args[1].current.isShareable : args[1].isShareable;
        
        for (i = 0; i < folderFiles.length; i++) {
            var fileName = decodeURIComponent(folderFiles[i].title);

            var exttype = ASC.Files.Utility.getCssClassByFileTitle(fileName, true);

            var fileId = folderFiles[i].id;
            var access = folderFiles[i].access;
            var viewUrl = folderFiles[i].viewUrl;
            var version = folderFiles[i].version;
            var contentLength = folderFiles[i].contentLength;
            var pureContentLength = folderFiles[i].pureContentLength;
            var file = { title: fileName, access: access, exttype: exttype, version: version, id: fileId, type: "file", ViewUrl: viewUrl, size_string: contentLength, size: pureContentLength, original: folderFiles[i] };
            content.push(file);
        }

        jq(".fileList").empty();
        if (content.length == 0) {
            showEmptyScreen();
            jq(".loader").hide();
            return;
        }
        hideEmptyScreen();
        jq(".fileContainer").find("#emptyFileList").hide();
        jq.tmpl("docAttachTmpl", content).appendTo(jq(".fileList"));
        jq(".loader").hide();
        toggleButtons(false, true);
        toggleButtons(true, false);

        jq("#filesViewContainer :checkbox").change(testCheckboxesState);
    }

    function selectFile(id) {
        var checkbox = jq(".fileList").find('input[id="' + id + '"]');
        if (checkbox.prop("checked")) {
            checkbox.removeAttr("checked");
        } else {
            checkbox.prop("checked", true);
        }

        testCheckboxesState();
    }

    function testCheckboxesState() {
        var files = jq('#filesViewContainer  :checkbox');
        var hasSelected = false;
        for (var i = 0; i < files.length; i++) {
            if (jq(files[i]).is(':checked')) {
                hasSelected = true;
                break;
            }
        }
        toggleButtons(!hasSelected, false);
    }

    function openFolder(id) {
        getListFolderFiles(id);
        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(id);
    }

    function showPortalDocUploader() {
        if (!isInit) {
            init();
        }

        documentSelectorTree.rollUp();
        documentSelectorTree.setCurrent(lastId);
        getListFolderFiles(lastId);

        jq(".fileList li input").removeAttr("checked");

        var margintop = jq(window).scrollTop() - 135;
        margintop = margintop + 'px';

        toggleButtons(true, false);

        PopupKeyUpActionProvider.EnableEsc = false;
        jq.blockUI({
            message: jq("#popupDocumentUploader"),
            css: {
                left: '50%',
                top: '25%',
                opacity: '1',
                border: 'none',
                padding: '0px',
                width: '650px',

                cursor: 'default',
                textAlign: 'left',
                position: 'absolute',
                'margin-left': '-300px',
                'margin-top': margintop,
                'background-color': 'White'
            },

            overlayCSS: {
                backgroundColor: '#AAA',
                cursor: 'default',
                opacity: '0.3'
            },
            focusInput: false,
            baseZ: 666,

            fadeIn: 0,
            fadeOut: 0,

            onBlock: function() {
            }
        });
    }

    function attachSelectedFiles() {
        try {
            toggleButtons(true, true);

            var listfiles = [];

            var fileIds = jq.map(jq(".fileList li input:checked"), function(el) {
                var id = jq(el).attr('id');
                return $.isNumeric(id) ? parseInt(id) : id;
            });
            
            var selectedFiles = jq.grep(folderFiles, function(f) {
                return jq.inArray(f.id, fileIds) != -1;
            });

            for (var i = 0; i < selectedFiles.length; i++) {
                var file = selectedFiles[i];

                var type;
                if (ASC.Files.Utility.CanImageView(file.title)) {
                    type = "image";
                } else {
                    if (ASC.Files.Utility.CanWebEdit(file.title) && ASC.Resources.Master.TenantTariffDocsEdition) {
                        type = "editedFile";
                    } else {
                        if (ASC.Files.Utility.CanWebView(file.title) && ASC.Resources.Master.TenantTariffDocsEdition) {
                            type = "viewedFile";
                        } else {
                            type = "noViewedFile";
                        }
                    }
                }

                var downloadUrl = ASC.Files.Utility.GetFileDownloadUrl(file.id);
                var viewUrl = ASC.Files.Utility.GetFileViewUrl(file.id);
                var docViewUrl = ASC.Files.Utility.GetFileWebViewerUrl(file.id);
                var editUrl = ASC.Files.Utility.GetFileWebEditorUrl(file.id);

                var fileTmpl = {
                    title: file.title,
                    access: file.access,
                    type: type,
                    exttype: ASC.Files.Utility.getCssClassByFileTitle(file.title),
                    id: file.id,
                    version: file.version,
                    ViewUrl: viewUrl,
                    downloadUrl: downloadUrl,
                    editUrl: editUrl,
                    docViewUrl: docViewUrl,
                    webUrl: file.webUrl,
                    size: file.pureContentLength,
                    inShareableFolder: isShareableFolder
                };

                listfiles.push(fileTmpl);
            }

            PopupKeyUpActionProvider.EnableEsc = true;

            eventsHandler.trigger(supportedCustomEvents.SelectFiles, { data: listfiles, asLinks: $attachFilesAsLinksSelector.is(':checked') });

            jq.unblockUI();
        } catch(e) {
            toggleButtons(false, true);
        }
    }

    function bind(eventName, fn) {
        eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        eventsHandler.unbind(eventName);
    }

    return {
        init: init,
        bind: bind,
        unbind: unbind,
        events: supportedCustomEvents,
        onGetFolderFiles: onGetFolderFiles,
        showPortalDocUploader: showPortalDocUploader,
        openFolder: openFolder,
        attachSelectedFiles: attachSelectedFiles,
        selectFile: selectFile
    };
})(jQuery);