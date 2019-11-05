/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalEmulatorEx.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;

using Protocols;
//using TerminalSessions;
//using Poderosa.Forms;
//using Poderosa.Commands;

namespace TerminalEmulator
{
    //AbstractTerminal‚ª•K—v‚È‹@”\‚ðŽó‚¯“n‚µ

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IAbstractTerminalHost {
        ITerminalSettings TerminalSettings { get; }
        TerminalTransmission TerminalTransmission { get; }
        //ISession ISession { get; }
        ITerminalConnection TerminalConnection { get; }
        TerminalOptions TerminalOptions { get; }

        //TerminalControl TerminalControl { get; }
        void NotifyViewsDataArrived();
        void CloseByReceptionThread(string msg);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalControlHost {
        ITerminalSettings TerminalSettings { get; }
        ITerminalConnection TerminalConnection { get; }

        AbstractTerminal Terminal { get; }
        TerminalTransmission TerminalTransmission { get; }
    }

    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^ƒT[ƒrƒX‚ÉƒAƒNƒZƒX‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface to access terminal emulator service
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍATermiunalEmuratorPluginƒvƒ‰ƒOƒCƒ“iƒvƒ‰ƒOƒCƒ“IDuorg.poderosa.terminalemulatorvj‚ª
    /// ’ñ‹Ÿ‚µ‚Ü‚·BŽŸ‚Ì‚æ‚¤‚É‚·‚é‚ÆAITerminalEmulatorService‚ðŽæ“¾‚Å‚«‚Ü‚·B
    /// <code>
    /// ITerminalEmulatorService emuservice = 
    ///   (ITerminalEmulatorService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
    /// </code>
    /// </ja>
    /// <en>
    /// The TermiunalEmuratorPlugin plug-in (plug-in ID[org.poderosa.terminalemulator]) offers this interface. ITerminalEmulatorService can be acquired by doing as follows. 
    /// <code>
    /// ITerminalEmulatorService emuservice = 
    ///   (ITerminalEmulatorService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
    /// </code>
    /// </en>
    /// </remarks>
    public interface ITerminalEmulatorService {
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        void LaterInitialize();

        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^‚ÌƒIƒvƒVƒ‡ƒ“‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// The option of the terminal emulator is shown. 
        /// </en>
        /// </summary>
        //ITerminalEmulatorOptions TerminalEmulatorOptions { get; }
        /// <summary>
        /// <ja>
        /// ƒfƒtƒHƒ‹ƒg‚Ìƒ^[ƒ~ƒiƒ‹Ý’è‚ðì¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create a default terminal setting.
        /// </en>
        /// </summary>
        /// <param name="caption"><ja>ƒ^[ƒ~ƒiƒ‹‚ÌƒLƒƒƒvƒVƒ‡ƒ“‚Å‚·B</ja><en>Caption of terminal.</en></param>
        /// <param name="icon"><ja>ƒ^[ƒ~ƒiƒ‹‚ÌƒAƒCƒRƒ“‚Å‚·Bnull‚ðŽw’è‚·‚é‚ÆƒfƒtƒHƒ‹ƒg‚ÌƒAƒCƒRƒ“‚ªŽg‚í‚ê‚Ü‚·B</ja><en>It is an icon of the terminal. When null is specified, the icon of default is used. </en></param>
        /// <returns><ja>ì¬‚³‚ê‚½ƒ^[ƒ~ƒiƒ‹Ý’èƒIƒuƒWƒFƒNƒg‚ðŽ¦‚·ITerminalSettings‚Å‚·B</ja><en>It is ITerminalSettings that shows the made terminal setting object. </en></returns>
        ITerminalSettings CreateDefaultTerminalSettings(string caption /*, Image icon */);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exclude/>
        ISimpleLogSettings CreateDefaultSimpleLogSettings();
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        //IShellSchemeCollection ShellSchemeCollection { get; }
    }

    //ƒƒOƒtƒ@ƒCƒ‹–¼‚ÌƒJƒXƒ^ƒ}ƒCƒY
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IAutoLogFileFormatter {
        string FormatFileName(string default_directory, ITerminalParameter param, ITerminalSettings settings);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TerminalInitializeInfo {
        private IAbstractTerminalHost _session;
        private ITerminalParameter _param;
        private int _initialWidth;
        private int _initialHeight;

        public TerminalInitializeInfo(IAbstractTerminalHost session, ITerminalParameter param) {
            _session = session;
            _param = param;
            _initialWidth = param.InitialWidth;
            _initialHeight = param.InitialHeight;
        }

        public IAbstractTerminalHost Session {
            get {
                return _session;
            }
        }
        public ITerminalParameter TerminalParameter {
            get {
                return _param;
            }
        }
        public int InitialWidth {
            get {
                return _initialWidth;
            }
        }
        public int InitialHeight {
            get {
                return _initialHeight;
            }
        }
    }
}
