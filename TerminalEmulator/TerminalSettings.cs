/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSettings.cs,v 1.2 2010/11/27 13:03:20 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
using System.Diagnostics;

using ConnectionParam;
//using Poderosa.View;
using Core;
//using Poderosa.Plugins;

namespace TerminalEmulator
{
    //IShellSchemeDynamicChangeListener‚ÉŠÖ‚µ‚Ä‚ÍAShellScheme‚ðŠm’è‚·‚é‚Æ‚«‚ÉListen‚·‚é
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TerminalSettings : ITerminalSettings {
        private EncodingType _encoding;
        private TerminalType _terminalType;
        private bool _localecho;
        private LineFeedRule _lineFeedRule;
        private NewLine _transmitnl;
        private IMultiLogSettings _multiLogSettings;
        private string _caption;
        //private Image _icon;
        private int _debug;

        private ListenerList<ITerminalSettingsChangeListener> _listeners;
        private bool _updating;

        public event ChangeHandler<string> ChangeCaption;
        public event ChangeHandler<EncodingType> ChangeEncoding;

        public TerminalSettings() {
            //IPoderosaCulture culture = TerminalEmulatorPlugin.Instance.PoderosaWorld.Culture;
            //if (culture.IsJapaneseOS || culture.IsSimplifiedChineseOS || culture.IsTraditionalChineseOS || culture.IsKoreanOS)
            //    _encoding = EncodingType.UTF8;
            //else
                _encoding = EncodingType.ISO8859_1;

            _debug = 0;
            _terminalType = TerminalType.XTerm;
            _localecho = false;
            _lineFeedRule = LineFeedRule.Normal;
            _transmitnl = NewLine.CR;
            _multiLogSettings = new MultiLogSettings();

            _listeners = new ListenerList<ITerminalSettingsChangeListener>();
        }

        //Clone, ‚Å‚àIConeable‚Å‚Í‚È‚¢BListener—Þ‚ÍƒRƒs[‚µ‚È‚¢B
        public virtual ITerminalSettings Clone() {
            TerminalSettings t = new TerminalSettings();
            t.Import(this);
            return t;
        }
        //ListenerˆÈŠO‚ðŽ‚Á‚Ä‚­‚é
        public virtual void Import(ITerminalSettings src) {
            _encoding = src.Encoding;
            _terminalType = src.TerminalType;
            _localecho = src.LocalEcho;
            _lineFeedRule = src.LineFeedRule;
            _transmitnl = src.TransmitNL;
            _caption = src.Caption;
            //_icon = src.Icon;
            _debug = src.DebugFlag;
            TerminalSettings src_r = (TerminalSettings)src;
            _multiLogSettings = src.LogSettings==null? null : (IMultiLogSettings)_multiLogSettings.Clone();
        }

        public EncodingType Encoding {
            get {
                return _encoding;
            }
            set {
                EnsureUpdating();
                _encoding = value;
                if(this.ChangeEncoding!=null) this.ChangeEncoding(value);
            }
        }

        public TerminalType TerminalType {
            get {
                return _terminalType;
            }
            set {
                EnsureUpdating();
                _terminalType = value;
            }
        }

        public IMultiLogSettings LogSettings {
            get {
                return _multiLogSettings;
            }
        }

        public NewLine TransmitNL {
            get {
                return _transmitnl;
            }
            set {
                EnsureUpdating();
                _transmitnl = value;
            }
        }

        public bool LocalEcho {
            get {
                return _localecho;
            }
            set {
                EnsureUpdating();
                _localecho = value;
            }
        }

        public LineFeedRule LineFeedRule {
            get {
                return _lineFeedRule;
            }
            set {
                EnsureUpdating();
                _lineFeedRule = value;
            }
        }

        public string Caption {
            get {
                return _caption;
            }
            set {
                EnsureUpdating();
                _caption = value;
                if(this.ChangeCaption!=null) this.ChangeCaption(value);
            }
        }

        /* public Image Icon {
            get {
                return _icon;
            }
            set {
                EnsureUpdating();
                _icon = value;
            }
        } */

        public int DebugFlag
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }

        //
        public void BeginUpdate() {
            if(_updating) throw new InvalidOperationException("EndUpdate() was missed");
            _updating = true;
            if(!_listeners.IsEmpty) {
                foreach(ITerminalSettingsChangeListener l in _listeners) l.OnBeginUpdate(this);
            }
        }
        public void EndUpdate() {
            if(!_updating) throw new InvalidOperationException("BeginUpdate() was missed");
            _updating = false;
            if(!_listeners.IsEmpty) {
                foreach(ITerminalSettingsChangeListener l in _listeners) l.OnEndUpdate(this);
            }
        }

        public void AddListener(ITerminalSettingsChangeListener l) {
            _listeners.Add(l);
        }
        public void RemoveListener(ITerminalSettingsChangeListener l) {
            _listeners.Remove(l);
        }

        private void EnsureUpdating() {
            if(!_updating) throw new InvalidOperationException("NOT UPDATE STATE");
        }
    }

    public class SimpleLogSettings : ISimpleLogSettings {
        private LogType _logtype;
        private string _logpath;
        private string _logFileName;
        private bool _logappend;

        public SimpleLogSettings() {
            _logtype = LogType.None;
        }
        public SimpleLogSettings(LogType lt, string path) {
            _logtype = lt;
            _logpath = path;
            _logappend = false;
        }

        public ILogSettings Clone() {
            SimpleLogSettings t = new SimpleLogSettings();
            t._logappend = _logappend;
            t._logpath = _logpath;
            t._logtype = _logtype;
            return t;
        }
       
        public LogType LogType {
            get {
                return _logtype;
            }
            set {
                _logtype = value;
            }
        }

        public string LogPath {
            get {
                return _logpath;
            }
            set {
                _logpath = value;
            }
        }

        public bool LogAppend {
            get {
                return _logappend;
            }
            set {
                _logappend = value;
            }
        }
    }
}
