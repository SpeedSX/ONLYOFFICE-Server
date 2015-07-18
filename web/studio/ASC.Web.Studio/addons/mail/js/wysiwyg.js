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


window.wysiwygEditor = (function($) {
    var editorInstance,
        supportedCustomEvents = { OnChange: "onchange", OnFocus: 'onfocus' },
        eventsHandler = $({}),
        isEditorReady,
        signatureOnload,
        needCkFocus,
        newCkParagraph = '<p>&nbsp;</p>';

    function init() {
        close();

        var config = {
            toolbar: 'Mail',
            removePlugins: 'resize, magicline',
            filebrowserUploadUrl: 'fckuploader.ashx?newEditor=true&esid=mail',
            tabIndex: 5,
            on: {
                instanceReady: function() {
                    isEditorReady = true;
                    if (needCkFocus) {
                        setFocus();
                        needCkFocus = false;
                    }
                    var body = editorInstance.document.getBody().$;
                    var button = $(body).find('.tl-controll-blockquote')[0];
                    if (button) {
                        $(button).unbind('click').bind('click', function() {
                            showQuote(this);
                        });
                        $(button).bind("contextmenu", function(event) {
                            event.stopPropagation ? event.stopPropagation() : (event.cancelBubble = true);
                        });
                    }

                    $(body).on('click', '.delete-btn', function() {
                        var $filelink = $(this).closest('.mailmessage-filelink');
                        var $beforelink = $filelink.prev('p');

                        $filelink.remove();
                        if (!$beforelink.text().trim()) {
                            $beforelink.remove();
                        }
                        
                        eventsHandler.trigger(supportedCustomEvents.OnChange);
                    });

                    $(body).on('click', '.mailmessage-filelink-link', function() {
                        window.open($(this).attr('href'));
                    });
                    
                    $(body).find('.mailmessage-filelink-link').dotdotdot();
                },
                change: onTextChange,
                dataReady: function() {
                    if (signatureOnload) {
                        insertSignature(signatureOnload);
                        signatureOnload = undefined;
                    }
                }
            }
        };

        ckeditorConnector.onReady(function() {
            editorInstance = $('#ckMailEditor').ckeditor(config).editor;
        });
    }

    function showQuote(control) {
        $(control).next('blockquote').show();
        $(control).remove();
    }

    function onTextChange() {
        eventsHandler.trigger(supportedCustomEvents.OnChange);
    }

    function getValue() {
        if (editorInstance) {
            var $html = $('<div/>').append(editorInstance.getData());
            showQuote($html.find('.tl-controll-blockquote'));
            return $html.html() || newCkParagraph;
        }
        return '';
    }

    function setFocus() {
        if (editorInstance) {
            if (isEditorReady) {
                editorInstance.focus();
                eventsHandler.trigger(supportedCustomEvents.OnFocus);
            } else {
                needCkFocus = true;
            }
        }
    }

    function setReply(message) {
        init();
        if (editorInstance) {
            var visibleQoute = false;
            if (TMMail.isIe()) {
                visibleQoute = true;
            }
            var html = $.tmpl('replyMessageHtmlBodyTmpl', { message: message.original, visibleQoute: visibleQoute }).get(0).outerHTML;
            editorInstance.setData(newCkParagraph + html);
        }
    }

    function setForward(message) {
        init();
        if (editorInstance) {
            var html = $.tmpl('forwardMessageHtmlBodyTmpl', message.original).get(0).outerHTML;
            editorInstance.setData(newCkParagraph + html);
        }
    }

    function setDraft(message) {
        init();
        if (editorInstance) {
            if (!TMMail.isIe() && message.htmlBody != '') {
                var $html = $('<div/>').append(message.htmlBody);
                var blockqoute = $html.find('blockquote:first');
                if (blockqoute) {
                    blockqoute.before($.tmpl('blockquoteTmpl', {}).get(0).outerHTML);
                    blockqoute.hide();
                }
                message.htmlBody = $html.html();
            }
            editorInstance.setData(message.htmlBody == '' ? newCkParagraph : message.htmlBody);
        }
    }

    function setSignature(signature) {
        if (signature == undefined || signature.html == undefined) {
            return;
        }
        if (!isEditorReady) {
            signatureOnload = signature;
        } else {
            updateSignature(signature);
        }
    }

    function insertSignature(signature) {
        if (signature == undefined || signature.html == undefined) {
            return;
        }
        if (editorInstance && signature.isActive) {
            var editorBody = $(editorInstance.document.getBody().$);

            var foundSignatures = editorBody.find('> div.tlmail_signature[mailbox_id="' + signature.mailboxId + '"]');

            if (foundSignatures.length == 0) {
                var htmlSignature = $.tmpl("composeSignatureTmpl", signature);
                htmlSignature.data('signature', signature);
                var blockquote = editorBody.find('> .reply-text');
                if (blockquote.length == 0) {
                    blockquote = editorBody.find('> .forward-text');
                }

                if (blockquote.length == 0) {
                    editorBody.append(htmlSignature);
                } else {
                    htmlSignature.insertBefore(blockquote.first());
                }
            }
        }
    }

    function updateSignature(signature) {
        if (signature == undefined || signature.html == undefined) {
            return;
        }
        if (editorInstance) {
            var editorBody = $(editorInstance.document.getBody().$);
            var signatureContainer = editorBody.find('> div.tlmail_signature').last();
            if (signatureContainer.length > 0) {
                if (signature.isActive) {
                    var htmlSignature = $.tmpl("composeSignatureTmpl", signature);
                    htmlSignature.data('signature', signature);
                    signatureContainer.replaceWith(htmlSignature);
                } else {
                    deleteSignature();
                }
            } else {
                if (signature.isActive) {
                    insertSignature(signature);
                }
            }
        }
    }

    function deleteSignature() {
        var editorBody = $(editorInstance.document.getBody().$);
        var signatureContainer = editorBody.find('> div.tlmail_signature').last();
        if (signatureContainer.length > 0) {
            signatureContainer.remove();
        }
    }

    function close() {
        if (editorInstance) {
            editorInstance.removeAllListeners();
            editorInstance = undefined;
        }
        isEditorReady = false;
        needCkFocus = false;
    }

    function bind(eventName, fn) {
        return eventsHandler.bind(eventName, fn);
    }

    function unbind(eventName) {
        return eventsHandler.unbind(eventName);
    }
    
    function insertFileLinks(files) {
        var templates = $.tmpl('messageFileLink', files);
        var $pos = $(editorInstance.getSelection().getStartElement().$);
        templates.insertBefore($pos);
        setFocus();

        var body = editorInstance.document.getBody().$;
        $(body).find('.mailmessage-filelink-link').dotdotdot();
    }

    return {
        init: init,
        getValue: getValue,
        setFocus: setFocus,
        setReply: setReply,
        setForward: setForward,
        setDraft: setDraft,
        close: close,
        events: supportedCustomEvents,
        setSignature: setSignature,
        bind: bind,
        unbind: unbind,
        insertFileLinks: insertFileLinks
    };

})(jQuery);