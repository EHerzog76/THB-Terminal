/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Options.cs,v 1.5 2010/12/29 12:25:30 kzmi Exp $
 */
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Reflection;
//using System.Drawing;
//using System.Windows.Forms;

using Core;
//using Poderosa.Document;
//using Poderosa.View;
using ConnectionParam;
//using Poderosa.Terminal;
//using Poderosa.Preferences;
//using TerminalSessions;

//‹N“®‚Ì‚‘¬‰»‚Ì‚½‚ßA‚±‚±‚Å‚ÍGranados‚ðŒÄ‚Î‚È‚¢‚æ‚¤‚É’ˆÓ‚·‚é

namespace TerminalEmulator
{
    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^‚ÌƒIƒvƒVƒ‡ƒ“‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that shows option of terminal emulator
    /// </en>
    /// </summary>
    /// <exclude/>
    public interface ITerminalEmulatorOptions {
        /// <summary>
        /// <ja>Ø’f‚³‚ê‚½‚Æ‚«‚É•Â‚¶‚é‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO‚Å‚·B</ja>
        /// <en>Flag whether close when closed</en>
        /// </summary>
		bool CloseOnDisconnect {
			get;
			set;
		}

        /// <summary>
        /// <ja>ƒxƒ‹‹L†‚ª“ž—ˆ‚µ‚½‚Æ‚«‚Éƒr[ƒv‚ð–Â‚ç‚·‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO‚Å‚·B</ja>
        /// <en>Flag whether to sound beep when the bell character comes. </en>
        /// </summary>
        bool BeepOnBellChar {
            get;
            set;
        }

        //ƒL[Ý’èŒn
        /// <summary>
        /// <ja>DELƒL[‚Å0x7FƒR[ƒh‚ð‘—M‚·‚é‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO‚Å‚·B</ja>
        /// <en>Flag whether to transmit 0x7F code with DEL key.</en>
        /// </summary>
		bool Send0x7FByDel {
            get;
            set;
		}
        bool Send0x7FByBack {
 			get;
 			set;
 		}

        KeyboardStyle Zone0x1F {
            get;
            set;
        }
        string CustomKeySettings {
            get;
            set;
        }

        WarningOption CharDecodeErrorBehavior {
            get;
            set;
        }
        WarningOption DisconnectNotification {
            get;
            set;
        }

		int TerminalBufferSize {
            get;
            set;
		}

        int KeepAliveInterval {
            get;
            set;
		}

        LogType DefaultLogType {
            get;
            set;
        }
        string DefaultLogDirectory {
            get;
            set;
        }
        /*
        CaretType CaretType {
            get;
            set;
        } */

        // Copy and Paste
        int ShellHistoryLimitCount {
            get;
        }
    }

    public class TerminalOptions : ITerminalEmulatorOptions {
        //•\Ž¦
		private Int32 _lineSpacing;

        //ƒ^[ƒ~ƒiƒ‹
        private Boolean _closeOnDisconnect;
        private Boolean _beepOnBellChar;
        private Boolean _askCloseOnExit;
        private WarningOption _charDecodeErrorBehavior;
        private WarningOption _disconnectNotification;

        //‘€ì
        private Int32 _terminalBufferSize;
        private Boolean _send0x7FByDel;
        private Boolean _send0x7FByBack;
        private KeyboardStyle _zone0x1F;
        private String _customKeySettings;
        private Int32 _keepAliveInterval;
        private String _additionalWordElement;

        //ƒƒO
        private LogType _defaultLogType;
        private String _defaultLogDirectory;

        //PreferenceEditor‚Ì‚Ý
        private Int32 _shellHistoryLimitCount;

        public TerminalOptions(String folder) {
            this.DefineItems();
            if (folder.Length == 0)
                _defaultLogDirectory = AppDomain.CurrentDomain.BaseDirectory;
            else
                _defaultLogDirectory = folder;
        }

        public void DefineItems(/* IPreferenceBuilder builder */) {
            //•\Ž¦
            _lineSpacing      = 0;
            
            //ƒ^[ƒ~ƒiƒ‹
            _closeOnDisconnect = true;
            _beepOnBellChar    = false;
            _askCloseOnExit    = false;
            _charDecodeErrorBehavior = WarningOption.MessageBox;
            _disconnectNotification = WarningOption.StatusBar;

            //‘€ì
            _terminalBufferSize    = 1000;
            _send0x7FByDel         = false;
            _send0x7FByBack        = false;
            _zone0x1F              = KeyboardStyle.None;
            _customKeySettings     = "";
            _keepAliveInterval = 60000;

            //ƒƒO
            _defaultLogType        = LogType.None;
            _defaultLogDirectory   = "";

            //PreferenceEditor‚Ì‚Ý
            _shellHistoryLimitCount = 100;
        }
        public TerminalOptions Import(TerminalOptions src) {
            //•\Ž¦
            _lineSpacing = src._lineSpacing;

            //ƒ^[ƒ~ƒiƒ‹
            _closeOnDisconnect = src._closeOnDisconnect;
            _beepOnBellChar = src._beepOnBellChar;
            _askCloseOnExit = src._askCloseOnExit;
            _charDecodeErrorBehavior = src._charDecodeErrorBehavior;
            _disconnectNotification = src._disconnectNotification;

            //‘€ì
            _terminalBufferSize = src._terminalBufferSize;
            _send0x7FByDel = src._send0x7FByDel;
            _send0x7FByBack = src._send0x7FByBack;
            _zone0x1F = src._zone0x1F;
            _customKeySettings = src._customKeySettings;
            _keepAliveInterval = src._keepAliveInterval;

            //ƒƒO
            _defaultLogType = src._defaultLogType;
            _defaultLogDirectory = src._defaultLogDirectory;

            //PreferenceEditor‚Ì‚Ý
            _shellHistoryLimitCount = src._shellHistoryLimitCount;

            return this;
        }

        public bool CloseOnDisconnect {
            get {
                return _closeOnDisconnect;
            }
            set {
                _closeOnDisconnect = value;
            }
        }
        public bool BeepOnBellChar {
            get {
                return _beepOnBellChar;
            }
            set {
                _beepOnBellChar = value;
            }
        }

        public bool Send0x7FByDel {
            get {
                return _send0x7FByDel;
            }
            set {
                _send0x7FByDel = value;
            }
        }
        public bool Send0x7FByBack {
 			get {
 				return _send0x7FByBack;
 			}
 			set {
 				_send0x7FByBack = value;
            }
 		}

        public KeyboardStyle Zone0x1F {
            get {
                return _zone0x1F;
            }
            set {
                _zone0x1F = value;
            }
        }
        public string CustomKeySettings {
            get {
                return _customKeySettings;
            }
            set {
                _customKeySettings = value;
            }
        }

        public int TerminalBufferSize {
            get {
                return _terminalBufferSize;
            }
            set {
                _terminalBufferSize = value;
            }
        }

        public int LineSpacing {
            get {
                return _lineSpacing;
            }
            set {
                _lineSpacing = value;
            }
        }

        public int KeepAliveInterval {
            get {
                return _keepAliveInterval;
            }
            set {
                _keepAliveInterval = value;
            }
        }

        public LogType DefaultLogType {
            get {
                return _defaultLogType;
            }
            set {
                _defaultLogType = value;
            }
        }
        public string DefaultLogDirectory {
            get {
                return _defaultLogDirectory;
            }
            set {
                _defaultLogDirectory = value;
            }
        }

        public WarningOption CharDecodeErrorBehavior {
            get {
                return _charDecodeErrorBehavior;
            }
            set {
                _charDecodeErrorBehavior = value;
            }
        }
        public WarningOption DisconnectNotification {
            get {
                return _disconnectNotification;
            }
            set {
                _disconnectNotification = value;
            }
        }

        public int ShellHistoryLimitCount {
            get {
                return _shellHistoryLimitCount;
            }
        }
    }

	public class TerminalOptionsSupplier {

        private String _originalFolder;
        private TerminalOptions _originalOptions;

        //TerminalOptions‚ª•p”É‚ÉƒAƒNƒZƒX‚·‚é‚Ì‚Åinternal‚É

        //ƒ^[ƒ~ƒiƒ‹
		//[ConfigFlagElement(typeof(CaretType), Initial=(int)(CaretType.Blink|CaretType.Box), Max=(int)CaretType.Max)]
		  //                                  private CaretType _caretType;
		
		//[ConfigEnumElement(typeof(DisconnectNotification), InitialAsInt=(int)DisconnectNotification.StatusBar)]
		//                                    private DisconnectNotification _disconnectNotification;
		//[ConfigEnumElement(typeof(LogType), InitialAsInt=(int)LogType.None)]
		//                                    private LogType _defaultLogType;
		//[ConfigStringElement(Initial="")]   private string  _defaultLogDirectory;
		//[ConfigEnumElement(typeof(WarningOption), InitialAsInt=(int)WarningOption.MessageBox)]
		//                                    private WarningOption _warningOption;
		//[ConfigEnumElement(typeof(Keys), InitialAsInt=(int)Keys.None)]
		//                                    private Keys _localBufferScrollModifier;

		//ƒ}ƒEƒX‚ÆƒL[ƒ{[ƒh
		//[ConfigEnumElement(typeof(AltKeyAction), InitialAsInt=(int)AltKeyAction.Menu)]
        //                                    private AltKeyAction _leftAltKey;
		//[ConfigEnumElement(typeof(AltKeyAction), InitialAsInt=(int)AltKeyAction.Menu)]
		//                                    private AltKeyAction _rightAltKey;
		//[ConfigEnumElement(typeof(RightButtonAction), InitialAsInt=(int)RightButtonAction.ContextMenu)]
		//                                    private RightButtonAction _rightButtonAction;

        public TerminalOptionsSupplier() {
        }

        //IPreferencesupplier
        public TerminalOptionsSupplier(String folder) {
            _originalFolder = folder;
            _originalOptions = new TerminalOptions(folder);
        }

        public ITerminalEmulatorOptions OriginalOptions {
            get {
                return _originalOptions;
            }
        }
    }

	//‚¨‚©‚µ‚È•¶Žš‚ª—ˆ‚½‚Æ‚«‚Ç‚¤‚·‚é‚©
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(WarningOption))]
	public enum WarningOption {
		/* [EnumValue(Description="Enum.WarningOption.Ignore")] */ Ignore,
		/* [EnumValue(Description="Enum.WarningOption.StatusBar")] */ StatusBar,
		/* [EnumValue(Description="Enum.WarningOption.MessageBox")] */ MessageBox
	}

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(AltKeyAction))]
	public enum AltKeyAction {
		/* [EnumValue(Description="Enum.AltKeyAction.Menu")] */ Menu,
		/* [EnumValue(Description="Enum.AltKeyAction.ESC")] */ ESC,
		/* [EnumValue(Description="Enum.AltKeyAction.Meta")] */ Meta
	}

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    //[EnumDesc(typeof(KeyboardStyle))]
    public enum KeyboardStyle {
        /* [EnumValue(Description="Enum.KeyboardStyle.None")] */ None,
        /* [EnumValue(Description="Enum.KeyboardStyle.Default")] */ Default,
        /* [EnumValue(Description="Enum.KeyboardStyle.Japanese")] */ Japanese
    }

    /// <summary>
    /// <ja>
    /// ƒIƒvƒVƒ‡ƒ“‚ª•s³‚È‚Æ‚«‚É”­¶‚·‚é—áŠO‚Å‚·B
    /// </ja>
    /// <en>
    /// Exception generated when option is illegal.
    /// </en>
    /// </summary>
	public class InvalidOptionException : Exception {
        /// <summary>
        /// <ja>
        /// ƒIƒvƒVƒ‡ƒ“‚ª•s³‚È‚Æ‚«‚É”­¶‚·‚é—áŠO‚ðì¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Generate Exception when option is illegal
        /// </en>
        /// </summary>
        /// <param name="msg"><ja>—áŠO‚ÌƒƒbƒZ[ƒW‚Å‚·B</ja>
        /// <en>Message of exception.</en>
        /// </param>
		public InvalidOptionException(string msg) : base(msg) {}
	}
}
