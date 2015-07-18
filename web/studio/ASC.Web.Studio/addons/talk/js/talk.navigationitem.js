﻿/*
    Copyright (c) Ascensio System SIA 2013. All rights reserved.
    http://www.teamlab.com
*/

if (!ASC.Controls.TalkNavigationItem) {
    ASC.Controls.TalkNavigationItem = {
        checkMessagesTimeout: 1000,
        currentValue: -1,
        init: function (timeout) {
            if (isFinite(+timeout)) {
                this.checkMessagesTimeout = +timeout * 1000;
                setTimeout(this.checkNewMessages, 3000);
                if (+timeout > 0) {
                    setInterval(this.checkNewMessages, this.checkMessagesTimeout);
                }
            }
            Teamlab.bind(Teamlab.events.getTalkUnreadMessages, ASC.Controls.TalkNavigationItem.getResponce);
        },
        updateValue: function (value) {
            if (this.currentValue === value) {
                return undefined;
            }
            this.currentValue = value;
            var o = document.getElementById('talkMsgCount');
            if (o) {
                o.innerHTML = value > 100 ? ">100" : value;
                jQuery('#talkMsgLabel')[value > 0 ? 'addClass' : 'removeClass']('has-led');
            }
        },
        getResponce: function (params, count) {
            if (count > 0) {
                ASC.Controls.TalkNavigationItem.updateValue(count);
            }
        },
        checkNewMessages: function () {
            if (jQuery('#talkMsgLabel').length > 0) {
                Teamlab.getTalkUnreadMessages();
            }
        }
    };
}

if (!ASC.Controls.JabberClient) {
    ASC.Controls.JabberClient = {
        username: null,
        jid: null,
        winName: 'ASCJabberClient' + location.hostname,
        helperId: 'JabberClientHelper-' + Math.floor(Math.random() * 1000000),
        params: 'ontouchend' in document ? '' : 'width=900,height=600,status=no,toolbar=no,menubar=no,resizable=yes,scrollbars=no',
        pathCmdHandler: '',
        pathWebTalk: '',
        init: function (name, talkpath, cmdpath) {
            if (typeof name === 'string' && name.length > 0) {
                this.username = name.toLowerCase();
            }
            if (typeof talkpath === 'string' && talkpath.length > 0) {
                this.pathWebTalk = talkpath;
            }
            if (typeof cmdpath === 'string' && cmdpath.length > 0) {
                this.pathCmdHandler = cmdpath;
            }
            var a = this.winName.match(/\w+/g);
            this.winName = a ? a.join('') : this.winName;
        },
        extendChat: function() {
            ASC.Controls.JabberClient.open();
        },
        open: function (jid) {
            var hWnd = null,
                isExist = false,
                name = jid ? jid.substring(0, jid.indexOf('@')) : '';

            name = name || jid || '';

            if (this.username === name.toLowerCase()) {
                jid = null;
            }
            if (typeof jid === 'string') {
                jid = jid.toLowerCase();
            }

            try {
                hWnd = window.open('', this.winName, this.params)
            } catch (err) {
            }

            try {
                isExist = typeof hWnd.ASC === 'undefined' ? false : true;
            } catch (err) {
                isExist = true;
            }

            if (!isExist && typeof this.pathWebTalk) {
                hWnd = window.open(this.pathWebTalk + (jid && typeof jid === 'string' && jid.length > 0 ? '#' + jid : ''), this.winName, this.params);
                isExist = true;
            }

            if (!isExist) {
                return undefined;
            }

            try {
                hWnd.focus()
            } catch (err) {
            }

            try {
                ASC.Controls.TalkNavigationItem.updateValue(0);
                if (ASC.Resources.Master.Hub.Url) {
                    // SendMessagesCount
                    jq.connection.ch.server.smec(0);
                }
            } catch (e) {
                console.error(e.message);
            }

            if (typeof jid === 'string' && jid.length > 0) {
                this.openContact(jid);
            }
            return hWnd;
        },
        openContact: function (jid) {
            if (typeof this.username === 'string' && this.username.length > 0) {
                var name = jid.substring(0, jid.indexOf('@'));
                name = name || jid || '';
                this.jid = jid;

                if (this.username === name.toLowerCase()) {
                    name = null;
                }

                if (name && typeof name === 'string') {
                    jQuery.ajax({
                        async: true,
                        type: 'get',
                        contentType: 'text/xml',
                        cache: false,
                        url: this.pathCmdHandler,
                        data: { from: name.toLowerCase(), to: this.username },
                        complete: null
                    });
                }
            }
        },
        openTenant: function (tenant) {
            if (tenant && typeof tenant === 'string') {
                jQuery.ajax({
                    async: true,
                    type: 'get',
                    contentType: 'text/xml',
                    cache: false,
                    url: this.pathCmdHandler,
                    data: { from: tenant.toLowerCase(), to: this.username, fromtenant: true },
                    complete: null
                });
            }
        },
        terminate: function(data, service) {
            jQuery.ajax({
                async: true,
                type: 'post',
                contentType: 'text/xml',
                cache: false,
                url: service,
                data: data,
                complete: null
            });
        }
    };
}

if (!ASC.Controls.SoundManager) {
    ASC.Controls.SoundManager = {
        isEnabled: null,
        soundContainer: null,
        soundPath: null,
        installPath: null,
        init: function (sound, install) {
            if (typeof sound === 'string') {
                this.soundPath = sound;
            }
            if (typeof install === 'string') {
                this.installPath = install;
            }
        },
        onLoad: function (o) {
            if (typeof o === 'string') {
                o = document.getElementById(o);
            }
            if (!o || typeof o === 'undefined') {
                return null;
            }
            this.soundContainer = o;
            if (this.isEnabled === null) {
                this.isEnabled = true;
            }
            this.play('incmsg');
        },
        alarm: function () {
            if (this.soundContainer === null && this.swfPath !== null) {
                if (this.soundPath !== null) {
                    try {
                        var soundContainerId = 'talkSoundContainer-' + Math.floor(Math.random() * 1000000);
                        var o = document.createElement('div');
                        o.setAttribute('id', soundContainerId);
                        document.body.appendChild(o);
                        swfobject.embedSWF(
                            this.soundPath,
                            soundContainerId,
                            1,
                            1,
                            '9.0.0',
                            this.installPath,
                            {
                                apiInit: function (id) {
                                    ASC.Controls.SoundManager.onLoad(id);
                                },
                                apiId: soundContainerId
                            },
                            { allowScriptAccess: 'always', wmode: 'transparent' },
                            { styleclass: 'sound-container', wmode: 'transparent' }
                        );
                    } catch (err) {
                    }
                }
                this.soundContainer = undefined;
                return undefined;
            }
            this.play('incmsg');
        },
        play: function (soundname) {
            if (this.soundContainer !== null && this.isEnabled === true && typeof soundname === 'string') {
                try {
                    this.soundContainer.playSound(soundname)
                } catch (err) {
                }
            }
        }
    };
}