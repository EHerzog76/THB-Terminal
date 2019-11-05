/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Connector.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

using Granados;
using Core;

//namespace Poderosa.Protocols
namespace Protocols
{
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    class SSHDebugTracer : ISSHEventTracer {
        public void OnTranmission(string type, string detail) {
            Debug.WriteLine("T:"+type+":"+detail);
        }
        public void OnReception(string type, string detail) {
            Debug.WriteLine("R:"+type+":"+detail);
        }
    }

    public class SSHConnector : InterruptableConnector {

        private SSHLoginParameter _destination;
        private HostKeyVerifierBridge _keycheck;
        private TerminalConnection _result;

        public SSHConnector(SSHLoginParameter destination, HostKeyVerifierBridge keycheck) {
            _destination = destination;
            _keycheck = keycheck;
        }

        protected override void Negotiate() {
            //const string defaultCipherAlgoOrder = "AES256CTR;AES256;AES192CTR;AES192;AES128CTR;AES128;Blowfish;TripleDES";
            //string[] cipherAlgorithmOrderArray = defaultCipherAlgoOrder.Split(new Char[] {';'});
            //string[] hostKeyAlgorithmOrderArray = new string[] { "DSA", "RSA" };

            SSHConnectionParameter con = new SSHConnectionParameter();

            con.Protocol = _destination.Method;
            con.CheckMACError = (System.Boolean)true; // PEnv.Options.SSHCheckMAC;
            con.UserName = _destination.Account;
            con.Password = _destination.PasswordOrPassphrase;
            con.AuthenticationType = _destination.AuthenticationType;
            con.IdentityFile = _destination.IdentityFileName;
            con.TerminalWidth = _destination.InitialWidth;
            con.TerminalHeight = _destination.InitialHeight;
            con.TerminalName = _destination.TerminalType;
            con.WindowSize = _client.ConMain.ProtocolOptions.SSHWindowSize;
            //con.PreferableCipherAlgorithms = LocalSSHUtil.ParseCipherAlgorithm(ConsoleMain.Instance.ProtocolOptions.CipherAlgorithmOrder);
            //con.PreferableHostKeyAlgorithms = ConsoleMain.Instance.ProtocolOptions.HostKeyAlgorithmOrder;
            con.AgentForward = _destination.AgentForward;
            if (_client.ConMain.ProtocolOptions.LogSSHEvents)
                con.EventTracer = new SSHEventTracer(_destination.Destination);
            if(_keycheck != null) con.KeyCheck += new HostKeyCheckCallback(this.CheckKey);

            SSHTerminalConnection r = new SSHTerminalConnection((ISSHLoginParameter)_destination);
            SSHConnection ssh = SSHConnection.Connect(con, r.ConnectionEventReceiver, _socket);
            if (ssh != null)
            {
                if (_client.ConMain.ProtocolOptions.RetainsPassphrase && _destination.AuthenticationType != AuthenticationType.KeyboardInteractive)
                    _client.ConMain.PassphraseCache.Add(_destination.Destination, _destination.Account, _destination.PasswordOrPassphrase); //Ú‘±¬Œ÷Žž‚Ì‚ÝƒZƒbƒg
                ////_destination.PasswordOrPassphrase = ""; Ú‘±‚Ì•¡»‚Ì‚½‚ß‚É‚±‚±‚ÅÁ‚³‚¸‚ÉŽc‚µ‚Ä‚¨‚­
                r.AttachTransmissionSide(ssh);
                r.UsingSocks = _socks != null;
                _result = r;
            }
            else
            {
                throw new IOException("Message.SSHConnector.Cancelled");
            }            
        }

        public override TerminalConnection Result
        {
            get
            {
                return _result;
            }
        }

        private bool CheckKey(SSHConnectionInfo ci)
        {
            return _keycheck.Vefiry(_destination, ci);
        }

    }

    public class TelnetConnector : InterruptableConnector {
        private TelnetParameter _destination;
        private TelnetTerminalConnection _result;

        public TelnetConnector(TelnetParameter destination)
        {
            _destination = destination;
        }

        protected override void Negotiate() {
            TelnetNegotiator neg = new TelnetNegotiator(_destination.TerminalType, _destination.InitialWidth, _destination.InitialHeight);
            TelnetTerminalConnection r = new TelnetTerminalConnection((ITCPParameter)_destination, neg, new PlainPoderosaSocket(_socket));
            //BACK-BURNER r.UsingSocks = _socks!=null;
            _result = r;
        }

        public override TerminalConnection Result {
            get {
                return _result;
            }
        }
    }

    public class SilentClient : ISynchronizedConnector, IInterruptableConnectorClient {
        //private IPoderosaForm _form;
        private object _form;
        private AutoResetEvent _event;
        private ITerminalConnection _result;
        private string _errorMessage;
        private bool _timeout;

        public SilentClient(object form) {
            _event = new AutoResetEvent(false);
            _form = form;
        }

        public void SuccessfullyExit(ITerminalConnection result) {
            if(_timeout) return;
            _result = result;
            //_result.SetServerInfo(((TCPTerminalParam)_result.Param).Host, swt.IPAddress);
            _event.Set();
        }
        public void ConnectionFailed(string message) {
            Debug.Assert(message!=null);
            _errorMessage = message;
            if(_timeout) return;
            _event.Set();
        }

        public ConsoleMain ConMain
        {
            get
            {
                return (null);
            }
        }

        public IInterruptableConnectorClient InterruptableConnectorClient {
            get {
                return this;
            }
        }

        public ITerminalConnection WaitConnection(IInterruptable intr, int timeout) {
            //‚¿‚å‚Á‚Æ‹ê‚µ‚¢”»’è
            //if(!(intr is InterruptableConnector) && !(intr is LocalShellUtil.Connector)) throw new ArgumentException("IInterruptable object is not correct");
            if(!(intr is InterruptableConnector) ) throw new ArgumentException("IInterruptable object is not correct");

            if(!_event.WaitOne(timeout, true)) {
                _timeout = true; //TODO Ú‘±‚ð’†Ž~‚·‚×‚«‚©
                _errorMessage = "Message.ConnectionTimedOut";
            }
            _event.Close();

            if(_result==null) {
                //if(_form!=null) _form.Warning(_errorMessage)
                Console.WriteLine(_errorMessage);
                return null;
            }
            else
                return _result;
        }
    }

    public class SSHEventTracer : ISSHEventTracer {
        private /* IPoderosaLog */ object _log;
        private /* PoderosaLogCategoryImpl */ string _category;

        public SSHEventTracer(string destination) {
            //_log = new IPoderosaApplication(); // ((IPoderosaApplication)ProtocolsPlugin.Instance.PoderosaWorld.GetAdapter(typeof(IPoderosaApplication))).PoderosaLog;
            //_category = new PoderosaLogCategoryImpl(String.Format("SSH:{0}", destination));
            _category = String.Format("SSH:{0}", destination);
        }

        public void OnReception(string type, string detail) {
            //_log.AddItem(_category, String.Format("Received: {0}", detail));
            Console.WriteLine(_category + " " + String.Format("Received: {0}", detail));
        }

        public void OnTranmission(string type, string detail) {
            //_log.AddItem(_category, String.Format("Transmitted: {0}", detail));
            Console.WriteLine(_category + " " + String.Format("Transmitted: {0}", detail));
        }
    }
}
