/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSession.cs,v 1.2 2010/12/29 16:25:02 kzmi Exp $
 */
using System;
//using System.Windows.Forms;
using System.Collections;
//using System.Drawing;
using System.Diagnostics;

using TerminalEmulator;
using Protocols;
using ConnectionParam;
using Core;

namespace TerminalSessions
{
	//NOTE Invalidate‚É•K—v‚Èƒpƒ‰ƒ[ƒ^ ‚±‚ê‚àˆÓ}‚ª‚¢‚Ü‚¢‚¿‚¾‚È‚ 
	public class InvalidateParam {
		private Delegate _delegate;
		private object[] _param;
		private bool _set;
		public void Set(Delegate d, object[] p) {
			_delegate = d;
			_param = p;
			_set = true;
		}
		public void Reset() {
			_set = false;
		}
        public void InvokeFor(/* Control c */)
        {
            throw new Exception("TerminalSession: InvokeFor not Implemented.");
			//if(_set) c.Invoke(_delegate, _param);
		}
	}

	//Ú‘±‚É‘Î‚µ‚ÄŠÖ˜A•t‚¯‚éƒf[ƒ^
	public class TerminalSession : ITerminalSession, IAbstractTerminalHost, ITerminalControlHost, IDisposable {
        private delegate void HostCauseCloseDelagate(string msg);

        public TelnetSSHLogin _parent;
        private ISessionHost _sessionHost;
		private TerminalTransmission _output;
        private AbstractTerminal _terminal;
        private ITerminalSettings _terminalSettings;
        private TerminalOptions _terminalOptions;
        //private TerminalControl _terminalControl;
        private KeepAlive _keepAlive;
        private bool _terminated;
        private bool disposed = false;

		public TerminalSession(ITerminalConnection connection, ITerminalSettings terminalSettings, TerminalOptions terminalOptions) {
            _terminalSettings = terminalSettings;
            _terminalOptions = terminalOptions;
			//VT100Žw’è‚Å‚àxtermƒV[ƒPƒ“ƒX‚ð‘—‚Á‚Ä‚­‚éƒAƒvƒŠƒP[ƒVƒ‡ƒ“‚ªŒã‚ð‚½‚½‚È‚¢‚Ì‚Å
			_terminal = AbstractTerminal.Create(new TerminalInitializeInfo(this, connection.Destination));
            _output = new TerminalTransmission(_terminal, _terminalSettings, connection);
            _keepAlive = new KeepAlive(this);
		}
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //if (_parent != null)
                    //    _parent.IsRunning = false;
                    if(!_terminated)
                        InternalTerminate();
                }

                //free unmanaged objects
                //AdditionalCleanup();

                this.disposed = true;
            }
        }
        ~TerminalSession()
        {
            Dispose(false);
        }

        public void Revive(ITerminalConnection connection) {
            TerminalDocument doc = _terminal.GetDocument();
            _output.Revive(connection, doc.TerminalWidth ,doc.TerminalHeight);
            _output.Connection.Socket.RepeatAsyncRead(_terminal); //ÄŽóM
        }

        #region ITerminalSession
        public AbstractTerminal Terminal {
			get {
				return _terminal;
			}
		}
        public ITerminalConnection TerminalConnection {
            get {
                return _output.Connection;
            }
        }
        public ITerminalSettings TerminalSettings {
            get {
                return _terminalSettings;
            }
        }
        public TerminalTransmission TerminalTransmission {
            get {
                return _output;
            }
        }
        public ISession ISession {
            get {
                return this;
            }
        }
        public TerminalOptions TerminalOptions
        {
            get
            {
                return _terminalOptions;
            }
        }
        public ILogService LogService {
            get {
                return _terminal.ILogService;
            }
        }
        #endregion

        //ŽóMƒXƒŒƒbƒh‚©‚çŒÄ‚ÔADocumentXV‚Ì’Ê’m
        public void NotifyViewsDataArrived() {
            //if(_terminalControl!=null) 
            //    _terminalControl.DataArrived();
            
            
            //Console.WriteLine("TerminalSession: NotifyViewsDataArrived");
            //Console.Write(_terminal.GetDocument().ShowScreen());
        }
        //³íEˆÙí‚Æ‚àŒÄ‚Î‚ê‚é
        public void CloseByReceptionThread(string msg) {
            Console.WriteLine("TerminalSession: CloseByReceptionThread - " + msg);
            if (_parent != null)
                _parent.IsRunning = false;
            if(_terminated) return;

            InternalTerminate();
            
            //new HostCauseCloseDelagate(HostCauseClose); 
        }
        private void HostCauseClose(string msg) {
            Console.WriteLine("TerminalSession: HostCauseClose - " + msg);
            //if(TerminalSessionsPlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CloseOnDisconnect)
                _sessionHost.TerminateSession();
            if (_parent != null)
                _parent.IsRunning = false;
        }
        
        //ISession
        public string Caption {
            get {
                string s = _terminalSettings.Caption;
                if(_output.Connection.IsClosed) s += "Caption.Disconnected";
                return s;
            }
        }
        //TerminalSession‚ÌŠJŽn
        public void InternalStart(/* ISessionHost host */) {
            //_sessionHost = host;
            //host.RegisterDocument(_terminal.IDocument);
            _output.Connection.Socket.RepeatAsyncRead(_terminal);
        }
        public void InternalTerminate() {
            Console.WriteLine("TerminalSession: InternalTerminate");
            _terminated = true;
            try {
                _output.Connection.Close();
                _output.Connection.Socket.ForceDisposed();
            }
            catch(Exception ex) { //‚±‚±‚Å‚Ì—áŠO‚Í–³Ž‹
                Console.WriteLine("TerminalSession: InternalTerminate / " + ex.Message);
            }
            if (_terminal != null)
            {
                _terminal.CloseBySession();
                _terminal = null;
            }
        }
        public PrepareCloseResult PrepareCloseSession() {
            return PrepareCloseResult.TerminateSession;
        }
	}

}
