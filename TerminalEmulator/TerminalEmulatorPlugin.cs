/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalEmulatorPlugin.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
//using System.Windows.Forms;

//using Poderosa.Plugins;
using Core;
//using TerminalSessions;
//using Poderosa.Preferences;
//using Poderosa.Forms;
//using Poderosa.Commands;

namespace TerminalEmulator
{
    /// <summary>
    /// <ja>
    /// TerminalEmulatorPluginƒvƒ‰ƒOƒCƒ“‚ª’ñ‹Ÿ‚·‚éŠg’£ƒ|ƒCƒ“ƒg‚Å‚·B
    /// </ja>
    /// <en>
    /// Extension point that TerminalEmulatorPlugin plug-in offers.
    /// </en>
    /// </summary>
    /// <exclude/>
    //removed 25.06.2015   => Buildaction = NONE
    public static class TerminalEmulatorConstants {
        public const string TERMINAL_CONTEXT_MENU_EXTENSIONPOINT = "org.poderosa.terminalemulator.contextMenu";
        public const string DOCUMENT_CONTEXT_MENU_EXTENSIONPOINT = "org.poderosa.terminalemulator.documentContextMenu";
        public const string TERMINALSPECIAL_EXTENSIONPOINT = "org.poderosa.terminalemulator.specialCommand";
        public const string INTELLISENSE_CANDIDATE_EXTENSIONPOINT = "org.poderosa.terminalemulator.intellisense";
        public const string LOG_FILENAME_FORMATTER_EXTENSIONPOINT = "org.poderosa.terminalemulator.logFileNameFormatter";
        public const string DYNAMIC_CAPTION_FORMATTER_EXTENSIONPOINT = "org.poderosa.terminalemulator.dynamicCaptionFormatter";
    }

	public class TerminalEmulatorPlugin : ITerminalEmulatorService	{
        public const string PLUGIN_ID = "org.poderosa.terminalemulator";

        private TerminalOptionsSupplier _optionSupplier;
        //private KeepAlive _keepAlive;
        //private CustomKeySettings _customKeySettings; //removed 25.06.2015
        //private ShellSchemeCollection _shellSchemeCollection;
        private PromptCheckerWithTimer _promptCheckerWithTimer;

        private bool _laterInitialized; //’x‰„‰Šú‰»—pƒtƒ‰ƒO

        private static TerminalEmulatorPlugin _instance;
        public static TerminalEmulatorPlugin Instance {
            get {
                return _instance;
            }
        }

        public TerminalEmulatorPlugin() //void InitializePlugin() {
        {
            _instance = this;
            _optionSupplier = new TerminalOptionsSupplier();
            //_keepAlive = new KeepAlive(this);
            //_customKeySettings = new CustomKeySettings(); //removed 25.06.2015
            //_shellSchemeCollection = new ShellSchemeCollection();

            //cs.SerializerExtensionPoint.RegisterExtension(new TerminalSettingsSerializer(pm));

            //PromptChecker
            _promptCheckerWithTimer = new PromptCheckerWithTimer();
        }

        ~TerminalEmulatorPlugin()
        {
            //_shellSchemeCollection.PreClose();
            _promptCheckerWithTimer.Close();
        }
        
        #region ITerminalEmulatorPlugin
        public ITerminalSettings CreateDefaultTerminalSettings(string caption) {
            TerminalSettings t = new TerminalSettings();
            t.BeginUpdate();
            //t.Icon = icon;
            t.Caption = caption;
            //t.EnabledCharTriggerIntelliSense = GEnv.Options.EnableComplementForNewConnections;
            t.EndUpdate();
            return t;
        }
        public ISimpleLogSettings CreateDefaultSimpleLogSettings() {
            return new SimpleLogSettings();
        }
        public IAutoLogFileFormatter[] AutoLogFileFormatter {
            get {
                return null; //(IAutoLogFileFormatter[])_autoLogFileFormatter.GetExtensions();
            }
        }
        public void LaterInitialize() {
            if(!_laterInitialized)
            {
                //_shellSchemeCollection.Load();
            }
            _laterInitialized = true;
        }
        #endregion

        public TerminalOptionsSupplier OptionSupplier {
            get {
                return _optionSupplier;
            }
        }
        // public KeepAlive KeepAlive {
        //    get {
        //        return _keepAlive;
        //    }
        //}
        //removed 25.06.2015
        //public CustomKeySettings CustomKeySettings {
        //    get {
        //        return _customKeySettings;
        //    }
        //}
    }

    /*  //removed 25.06.2015
    public class CustomKeySettings {
        private FixedStyleKeyFunction _keyFunction;

        public void Reset(ITerminalEmulatorOptions opt) {
            //TODO ‚±‚±‚ÍPeripheralPanel‚Æ‚©‚Ô‚Á‚Ä‚¢‚éB‚È‚ñ‚Æ‚©‚µ‚½‚¢
            StringBuilder bld = new StringBuilder();
            if(opt.Send0x7FByDel) bld.Append("Delete=0x7F");
            if(opt.Send0x7FByBack) {
                if(bld.Length>0) bld.Append(", ");
                bld.Append("Back=0x7F");
            }

            KeyboardStyle ks = opt.Zone0x1F;
            if(ks!=KeyboardStyle.None) {
                string s;
                if(ks==KeyboardStyle.Default)
                    s = "Ctrl+D6=0x1E, Ctrl+Minus=0x1F";
                else //Japanese
                    s = "Ctrl+BackSlash=0x1F";
                if(bld.Length>0) bld.Append(", ");
                bld.Append(s);
            }

            if(opt.CustomKeySettings.Length>0) {
                if(bld.Length>0) bld.Append(", ");
                bld.Append(opt.CustomKeySettings);
            }

            //Ždã‚°Bƒp[ƒXƒGƒ‰[‚ª‚¿‚å‚Á‚ÆƒAƒŒ‚¾
            _keyFunction = KeyFunction.Parse(bld.ToString()).ToFixedStyle();
        }

        public char[] Scan(Keys key) {
            //TODO ‚±‚ÌŽÀ‘•‚¾‚ÆAƒp[ƒXƒGƒ‰[‚Ì‚ ‚é‚Æ‚«AƒL[‚ð‰Ÿ‚µ‚½Žž“_‚ÅƒGƒ‰[‚É‚È‚éB‚±‚ê‚Í‚Ü‚¸‚¢BPreferenceListener‚Éƒ[ƒhŠ®—¹’Ê’m‚ª—~‚µ‚¢
            if(_keyFunction==null) {
                //Reset(GEnv.Options);
            }

            //ŽÀs•p“x‚‚¢‚Ì‚ÅA‚¢‚¿‚¨‚¤AIEnumeratorŽg‚í‚È‚¢B
            for(int i=0; i<_keyFunction._keys.Length; i++) {
                if(_keyFunction._keys[i]==key) return _keyFunction._datas[i];
            }
            return null;
        }
    }
    */

    //ƒ^ƒCƒ}[‚Åƒvƒƒ“ƒvƒg”FŽ¯
    public class PromptCheckerWithTimer {
        //private ITimerSite _timerSite;
        public PromptCheckerWithTimer() {
            //IntelliSense‚É•›ì—p‚ ‚é‚Ì‚ÅˆêŽž’âŽ~’†
            //_timerSite = TerminalEmulatorPlugin.Instance.GetWinFormsService().CreateTimer(1000, new TimerDelegate(OnTimer));
        }
        public void Close() {
            //_timerSite.Close();
        }

        private void OnTimer() {
            //‘Sƒ^[ƒ~ƒiƒ‹‚ÉˆêÄˆ’u
            //ISessionManager sm = TerminalEmulatorPlugin.Instance.GetSessionManager();
            //foreach(ISession s in sm.AllSessions) {
            //    //‚¿‚å‚Á‚Æ— ‹Z“I‚¾‚ª
            //    ITerminalControlHost tc = (ITerminalControlHost)s.GetAdapter(typeof(ITerminalControlHost));
            //    if(tc!=null) {
            //        tc.Terminal.PromptRecognizer.CheckIfUpdated();
            //    }
            //}
        }
    }
}
