/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalParameter.cs,v 1.2 2010/12/10 22:29:02 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;

using Granados;

//namespace Poderosa.Protocols
namespace Protocols
{
    /** TerminalParameter‘°‚Ì’ñ‹Ÿ
     *    ‚±‚±‚Å‚Í]—ˆ‚Ì‚æ‚¤‚ÈƒNƒ‰ƒXŒp³ŠÖŒW‚ª‚ ‚é‚ªAGetAdapter‚ÅŽæ‚é‚×‚«‚à‚Ì‚Å‚ ‚éB
     *    •ïŠÜŠÖŒW‚ðŒp³‚Å•\Œ»‚µ‚Ä‚¢‚½‚Æ‚±‚ë‚Éƒ€ƒŠ‚ª¶‚¶‚Ä‚¢‚½B•¡”ƒCƒ“ƒ^ƒtƒF[ƒX‚ÌŽÀ‘•‚Ì‚½‚ß‚É•Ö‹X“I‚ÉŒp³‚ðŽg‚Á‚Ä‚¢‚é‚É‚·‚¬‚È‚¢
     */

    public abstract class TerminalParameter : ITerminalParameter, ICloneable {

        public const string DEFAULT_TERMINAL_TYPE = "xterm";

        private int _initialWidth;  //ƒVƒFƒ‹‚Ì•
        private int _initialHeight; //ƒVƒFƒ‹‚Ì‚‚³
        private string _terminalType;

        public TerminalParameter() {
            SetTerminalName(DEFAULT_TERMINAL_TYPE);
            SetTerminalSize(80, 25); //‰½‚àÝ’è‚µ‚È‚­‚Ä‚à
        }
        public TerminalParameter(TerminalParameter src) {
            _terminalType = src._terminalType;
            _initialWidth = src._initialWidth;
            _initialHeight = src._initialHeight;
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
        public string TerminalType {
            get {
                return _terminalType;
            }
        }
        public void SetTerminalSize(int width, int height) {
            _initialWidth = width;
            _initialHeight = height;
        }
        public void SetTerminalName(string terminaltype) {
            _terminalType = terminaltype;
        }

        public abstract bool UIEquals(ITerminalParameter param);

        #region ICloneable
        public abstract object Clone();
        #endregion
    }

    //Telnet, SSH‚Å‚ÌÚ‘±æî•ñ
    public abstract class TCPParameter : TerminalParameter, ITCPParameter {
        private string _destination;
        //private IPAddress _address;
        private int _port;

        public TCPParameter() {
            _destination = "";
        }
        public TCPParameter(string destination, int port) {
            _destination = destination;
            _port = port;
        }
        public TCPParameter(TCPParameter src)
            : base(src) {
            _destination = src._destination;
            _port = src._port;
        }

        public string Destination {
            get {
                return _destination;
            }
            set {
                _destination = value;
            }
        }
        public int Port {
            get {
                return _port;
            }
            set {
                _port = value;
            }
        }

        public override bool UIEquals(ITerminalParameter param) {
            ITCPParameter tcp = (ITCPParameter)param; //.GetAdapter(typeof(ITCPParameter));
            return tcp!=null && _destination==tcp.Destination && _port==tcp.Port;
        }
    }

    public class TelnetParameter : TCPParameter {
        public TelnetParameter() {
            this.Port = 23;
        }
        public TelnetParameter(string dest, int port)
            : base(dest, port) {
        }
        public TelnetParameter(TelnetParameter src)
            : base(src) {
        }
        #region ICloneable
        public override object Clone() {
            return new TelnetParameter(this);
        }
        #endregion
    }

    public class SSHLoginParameter : TCPParameter, ISSHLoginParameter {
        private SSHProtocol _method;
        private AuthenticationType _authType;
        private string _account;
        private string _identityFile;
        private string _passwordOrPassphrase;
        private bool _letUserInputPassword;
        private IAgentForward _agentForward;

        public SSHLoginParameter() {
            _method = SSHProtocol.SSH2;
            _authType = AuthenticationType.Password;
            _passwordOrPassphrase = "";
            _identityFile = "";
            _letUserInputPassword = true;
            this.Port = 22;
        }
        public SSHLoginParameter(SSHLoginParameter src)
            : base(src) {
            _method = src._method;
            _authType = src._authType;
            _account = src._account;
            _identityFile = src._identityFile;
            _passwordOrPassphrase = src._passwordOrPassphrase;
            _letUserInputPassword = src._letUserInputPassword;
            _agentForward = src._agentForward;
        }

        public AuthenticationType AuthenticationType {
            get {
                return _authType;
            }
            set {
                _authType = value;
            }
        }
        public string Account {
            get {
                return _account;
            }
            set {
                _account = value;
            }
        }
        public string IdentityFileName {
            get {
                return _identityFile;
            }
            set {
                Debug.Assert(value!=null);
                _identityFile = value;
            }
        }
        public SSHProtocol Method {
            get {
                return _method;
            }
            set {
                _method = value;
            }
        }
        public string PasswordOrPassphrase {
            get {
                return _passwordOrPassphrase;
            }
            set {
                _passwordOrPassphrase = value;
            }
        }
        public bool LetUserInputPassword {
            get {
                return _letUserInputPassword;
            }
            set {
                _letUserInputPassword = value;
            }
        }
        public IAgentForward AgentForward {
            get {
                return _agentForward;
            }
            set {
                _agentForward = value;
            }
        }
        public override bool UIEquals(ITerminalParameter param) {
            /*
            ISSHLoginParameter ssh = (ISSHLoginParameter)param.GetAdapter(typeof(ISSHLoginParameter));
            return ssh!=null && base.UIEquals(param) && _account==ssh.Account; //ƒvƒƒgƒRƒ‹‚ªˆá‚¤‚¾‚¯‚Å‚Í“¯ˆêŽ‹‚µ‚Ä‚µ‚Ü‚¤
            */
            return false;
        }
        #region ICloneable
        public override object Clone() {
            return new SSHLoginParameter(this);
        }
        #endregion
    }

    public class SSHSubsystemParameter : SSHLoginParameter, ISSHSubsystemParameter {
        private string _subsystemName;

        public SSHSubsystemParameter() {
            _subsystemName = "";
        }
        public SSHSubsystemParameter(SSHSubsystemParameter src)
            : base(src) {
            _subsystemName = src._subsystemName;
        }

        public string SubsystemName {
            get {
                return _subsystemName;
            }
            set {
                _subsystemName = value;
            }
        }

        #region ICloneable
        public override object Clone() {
            return new SSHSubsystemParameter(this);
        }
        #endregion
    }
    /*
    public class LocalShellParameter : TerminalParameter, ICygwinParameter {
        private string _home;
        private string _shellName;
        private string _cygwinDir;

        public LocalShellParameter() {
            _home      = CygwinUtil.DefaultHome;
            _shellName = CygwinUtil.DefaultShell;
            _cygwinDir = CygwinUtil.DefaultCygwinDir;
        }
        public LocalShellParameter(LocalShellParameter src)
            : base(src) {
            _home = src._home;
            _shellName = src._shellName;
            _cygwinDir = src._cygwinDir;
        }

        public string Home {
            get {
                return _home;
            }
            set {
                _home = value;
            }
        }
        public string ShellName {
            get {
                return _shellName;
            }
            set {
                _shellName = value;
            }
        }
        //ˆø”‚È‚µ‚ÌƒVƒFƒ‹–¼
        public string ShellBody {
            get {
                int c = _shellName.IndexOf(' ');
                if(c!=-1)
                    return _shellName.Substring(0, c); //Å‰‚ÌƒXƒy[ƒX‚ÌŽè‘O‚Ü‚Å: ‚Ó‚Â‚¤/bin/bash
                else
                    return _shellName;
            }
        }
        public string CygwinDir {
            get {
                return _cygwinDir;
            }
            set {
                _cygwinDir = value;
            }
        }
            

        public override bool UIEquals(ITerminalParameter param) {
            return param is LocalShellParameter; //Cygwin‚Í‘S•”“¯ˆêŽ‹
        }

        #region ICloneable
        public override object Clone() {
            return new LocalShellParameter(this);
        }
        #endregion
    } */

    public class EmptyTerminalParameter : TerminalParameter {
        public override bool UIEquals(ITerminalParameter t) {
            return t is EmptyTerminalParameter;
        }

        public override object Clone() {
            return new EmptyTerminalParameter();
        }


    }
}
