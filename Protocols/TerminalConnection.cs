/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalConnection.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.IO;

using Core;
using Granados;
using Granados.SSH2;

//namespace Poderosa.Protocols
namespace Protocols
{
	public class PlainPoderosaSocket : IPoderosaSocket {
		private IByteAsyncInputStream _callback;
		private Socket _socket;
		private byte[] _buf;
        private ByteDataFragment _dataFragment;
        private AsyncCallback _callbackRoot;
        private TerminalConnection _ownerConnection;

	    public PlainPoderosaSocket(Socket s) {
			_socket = s;
            _buf = new byte[4096];  //ProtocolsPlugin.Instance.ProtocolOptions.SocketBufferSize
            _dataFragment = new ByteDataFragment(_buf, 0, 0);
            _callbackRoot = new AsyncCallback(RepeatCallback);
		}

        public void SetOwnerConnection(TerminalConnection con) {
            _ownerConnection = con;
        }

        public void Transmit(string data)
        {
            if (_socket.Connected)
                _socket.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length, SocketFlags.None);
        }
		public void Transmit(ByteDataFragment data) {
            if (_socket.Connected)
    			_socket.Send(data.Buffer, data.Offset, data.Length, SocketFlags.None);
		}
		public void Transmit(byte[] data, int offset, int length) {
            if(_socket.Connected)
    			_socket.Send(data, offset, length, SocketFlags.None);
		}
		public void Close() {
            try {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Disconnect(false);
                Debug.WriteLineIf(DebugOpt.Socket, "PlainSocket close");
            }
            catch(Exception ex) {
                RuntimeUtil.SilentReportException(ex);
            }
		}
        public void ForceDisposed() {
            _socket.Close();
        }

		public void RepeatAsyncRead(IByteAsyncInputStream receiver) {
			_callback = receiver;
            BeginReceive();
		}

		private void RepeatCallback(IAsyncResult result) {
			try {
				int n = _socket.EndReceive(result);
                _dataFragment.Set(_buf, 0, n);
                Debug.Assert(_ownerConnection!=null); //‚±‚ê‚ðŒÄ‚Ño‚·‚æ‚¤‚É‚È‚é‚Ü‚Å‚É‚ÍƒZƒbƒg‚³‚ê‚Ä‚¢‚é‚±‚ÆI

                if (n > 0) {
                    if(OnReceptionCore(_dataFragment)==GenericResult.Succeeded)
                        BeginReceive();
                }
                else if (n < 0) {
                    //WindowsME‚É‚¨‚¢‚Ä‚ÍA‚Æ‚«‚Ç‚«‚±‚±‚Å-1‚ª•Ô‚Á‚Ä‚«‚Ä‚¢‚é‚±‚Æ‚ª”­Šo‚µ‚½B‰º‚ÌErrorCode 995‚Ìê‡‚à“¯—l
                    BeginReceive();
                }
                else
                    OnNormalTerminationCore();
			}
			catch(Exception ex) {
                if(!_ownerConnection.IsClosed) {
                    RuntimeUtil.SilentReportException(ex);
                    if((ex is SocketException) && ((SocketException)ex).ErrorCode==995) {
                        BeginReceive();
                    }
                    else
                        OnAbnormalTerminationCore(ex.Message);
                }
			}
		}

        //IByteAsyncInputStream‚Ìƒnƒ“ƒhƒ‰‚Å—áŠO‚ª—ˆ‚é‚Æ‚¯‚Á‚±‚¤ŽSŽ–‚È‚Ì‚Å‚±‚Ì’†‚Å‚µ‚Á‚©‚èƒK[ƒh

        private GenericResult OnReceptionCore(ByteDataFragment data) {
            try {
                _callback.OnReception(_dataFragment);
                return GenericResult.Succeeded;
            }
            catch(Exception ex) {
                RuntimeUtil.ReportException(ex);
                Close();
                return GenericResult.Failed;
            }
        }

        private GenericResult OnNormalTerminationCore() {
            try {
                _ownerConnection.CloseBySocket();
                _callback.OnNormalTermination();
                return GenericResult.Succeeded;
            }
            catch(Exception ex) {
                RuntimeUtil.ReportException(ex);
                _socket.Disconnect(false);
                return GenericResult.Failed;
            }
        }

        private GenericResult OnAbnormalTerminationCore(string msg) {
            try {
                _ownerConnection.CloseBySocket();
                _callback.OnAbnormalTermination(msg);
                return GenericResult.Succeeded;
            }
            catch(Exception ex) {
                RuntimeUtil.ReportException(ex);
                _socket.Disconnect(false);
                return GenericResult.Failed;
            }
        }

        public bool Available {
            get {
                return _socket.Available>0;
            }
        }

        private void BeginReceive() {
            _socket.BeginReceive(_buf, 0, _buf.Length, SocketFlags.None, _callbackRoot, null);
        }
	}

    //‘—M‚µ‚½‚à‚Ì‚ð‚»‚Ì‚Ü‚Ü–ß‚·
    public class LoopbackSocket : IPoderosaSocket {
        private IByteAsyncInputStream _receiver;

        public void RepeatAsyncRead(IByteAsyncInputStream receiver) {
            _receiver = receiver;
        }

        public bool Available {
            get {
                return false;
            }
        }

        public void ForceDisposed() {
        }

        public void Transmit(string data)
        {
            Transmit(new ByteDataFragment(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length));
        }

        public void Transmit(ByteDataFragment data) {
            _receiver.OnReception(data);
        }

        public void Transmit(byte[] data, int offset, int length) {
            Transmit(new ByteDataFragment(data,offset,length));
        }

        public void Close() {
        }
    }


    public class ConnectionStats
    {
		private int _sentDataAmount;
		private int _receivedDataAmount;

        public int SentDataAmount {
            get {
                return _sentDataAmount;
            }
        }
        public int ReceivedDataAmount {
            get {
                return _receivedDataAmount;
            }
        }
		public void AddSentDataStats(int bytecount) {
			//_sentPacketCount++;
			_sentDataAmount += bytecount;
		}
		public void AddReceivedDataStats(int bytecount) {
			//_receivedPacketCount++;
			_receivedDataAmount += bytecount;
		}
    }

    public abstract class TerminalConnection : ITerminalConnection
    {
		protected ITerminalParameter _destination;
        protected ConnectionStats _stats;
        protected ITerminalOutput _terminalOutput; //”h¶ƒNƒ‰ƒX‚Å‚Í‚±‚ê‚ðƒZƒbƒg‚·‚é
        protected IPoderosaSocket _socket;

		//‚·‚Å‚ÉƒNƒ[ƒY‚³‚ê‚½‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO
		protected bool _closed;

		protected TerminalConnection(ITerminalParameter p) {
			_destination = p;
            _stats = new ConnectionStats();
		}

        //By Erwin
        public abstract void Transmit(string data);

		public ITerminalParameter Destination {
			get {
				return _destination;
			}
            set {
                _destination = value;
            }
		}
        public ITerminalOutput TerminalOutput {
            get {
                return _terminalOutput;
            }
        }
        public IPoderosaSocket Socket {
            get {
                return _socket;
            }
        }
		public bool IsClosed {
			get {
				return _closed;
			}
		}

        //ƒ\ƒPƒbƒg‘¤‚ÅƒGƒ‰[‚ª‹N‚«‚½‚Æ‚«‚Ìˆ’u
        public void CloseBySocket() {
            if(!_closed) CloseCore();
        }

		//I—¹ˆ—
		public virtual void Close() {
            if(!_closed) CloseCore();
		}

        private void CloseCore() {
            _closed = true;
        }
	}

    public abstract class TCPTerminalConnection : TerminalConnection
    {

		protected bool _usingSocks;

		protected TCPTerminalConnection(ITCPParameter p)
            : base((ITerminalParameter)p) {
			_usingSocks = false;
		}

		//Ý’è‚ÍÅ‰‚¾‚¯s‚¤
		public bool UsingSocks {
			get {
				return _usingSocks;
			}
			set {
				_usingSocks = value;
			}
		}
	}


    public class SSHTerminalConnection : TCPTerminalConnection
    {

        private SSHConnectionEventReceiverBase _sshSocket; //Keyboard-interactive‚Ì‚Æ‚«‚Ì”FØ’†‚Ì‚Ý_sshSocket‚ÍKeyboardInteractiveAuthHanlder
        private ISSHLoginParameter _sshLoginParameter;

		public SSHTerminalConnection(ISSHLoginParameter ssh)
              : base((ITCPParameter)ssh) {
            _sshLoginParameter = ssh;
            if(ssh.AuthenticationType!=AuthenticationType.KeyboardInteractive) {
                SSHSocket s = new SSHSocket(this);
                _sshSocket = s;
                _socket = s;
                _terminalOutput = s;
            }
            else {
                KeyboardInteractiveAuthHanlder s = new KeyboardInteractiveAuthHanlder(this);
                _sshSocket = s;
                _socket = s;
                _terminalOutput = null; //‚Ü‚¾—˜—p‰Â”\‚Å‚È‚¢
            }
		}
        //Keyboard-interactive‚Ìê‡A”FØ¬Œ÷Œã‚É‚±‚ê‚ðŽÀs
        internal void ReplaceSSHSocket(SSHSocket sshsocket) {
            _sshSocket = sshsocket;
            _socket = sshsocket;
            _terminalOutput = sshsocket;
        }
        public ISSHConnectionEventReceiver ConnectionEventReceiver {
            get {
                return _sshSocket;
            }
        }
        public ISSHLoginParameter SSHLoginParameter {
            get {
                return _sshLoginParameter;
            }
        }
        public override void Transmit(string data)
        {
            ((SSHSocket)_sshSocket).Transmit(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
        }

       
		public void AttachTransmissionSide(SSHConnection con) {
            _sshSocket.SetSSHConnection(con);
            if(con.AuthenticationResult==AuthenticationResult.Success) {
                SSHSocket ss = (SSHSocket)_sshSocket; //Keyboard-Interactive‚ª‚ç‚Ý‚Å‚¿‚å‚Á‚Æ•sŽ©‘R‚É‚È‚Á‚Ä‚é‚È
                //ISSHSubsystemParameter subsystem = (ISSHSubsystemParameter)_sshLoginParameter;  //.GetAdapter(typeof(ISSHSubsystemParameter));
                //if(subsystem!=null)
                //    ss.OpenSubsystem(subsystem.SubsystemName);
                //else //‚Ó‚Â‚¤‚ÌƒVƒFƒ‹
                    ss.OpenShell();
            }
		}

		public override void Close() {
			if(_closed) return; //‚Q“xˆÈãƒNƒ[ƒY‚µ‚Ä‚à•›ì—p‚È‚µ 
			base.Close();
            _sshSocket.Close();
		}
	}

    public class TelnetReceiver : IByteAsyncInputStream
    {
        private IByteAsyncInputStream _callback;
		private TelnetNegotiator _negotiator;
        private TelnetTerminalConnection _parent;
        private ByteDataFragment _localdata;

        public TelnetReceiver(TelnetTerminalConnection parent, TelnetNegotiator negotiator) {
            _parent = parent;
            _negotiator = negotiator;
            _localdata = new ByteDataFragment();
        }

        public void SetReceiver(IByteAsyncInputStream receiver) {
            _callback = receiver;
        }

		public void OnReception(ByteDataFragment data) {
			ProcessBuffer(data);
            if(!_parent.IsClosed)
    			_negotiator.Flush(_parent.RawSocket);
		}

		public void OnNormalTermination() {
    		_callback.OnNormalTermination();
		}

		public void OnAbnormalTermination(string msg) {
			_callback.OnAbnormalTermination(msg);
		}

		//CR NUL -> CR •ÏŠ·‚¨‚æ‚Ñ IAC‚©‚ç‚Í‚¶‚Ü‚éƒV[ƒPƒ“ƒX‚Ìˆ—
		private void ProcessBuffer(ByteDataFragment data) {
            int limit = data.Offset + data.Length;
            int offset = data.Offset;
            byte[] buf = data.Buffer;
            //Debug.WriteLine(String.Format("Telnet len={0}, proc={1}", data.Length, _negotiator.InProcessing));

			while(offset < limit) {
				while(offset < limit && _negotiator.InProcessing) {
					if(_negotiator.Process(buf[offset++])==TelnetNegotiator.ProcessResult.REAL_0xFF)
						_callback.OnReception(_localdata.Set(buf, offset-1, 1));
				}

				int delim = offset;
				while(delim < limit) {
					byte b = buf[delim];
					if(b==0xFF) {
						_negotiator.StartNegotiate();
						break;
					}
					if(b==0 && delim-1>=0 && buf[delim-1]==0x0D) break; //CR NULƒTƒ|[ƒg
					delim++;
				}

				if(delim>offset) _callback.OnReception(_localdata.Set(buf, offset, delim-offset)); //delim‚ÌŽè‘O‚Ü‚Åˆ—
				offset = delim+1;
			}

		}
    }

    public class TelnetSocket : IPoderosaSocket, ITerminalOutput
    {
        private IPoderosaSocket _socket;
        private TelnetReceiver _callback;
        private TelnetTerminalConnection _parent;

        public TelnetSocket(TelnetTerminalConnection parent, IPoderosaSocket socket, TelnetReceiver receiver) {
            _parent = parent;
            _callback = receiver;
            _socket = socket;
        }

		public void RepeatAsyncRead(IByteAsyncInputStream callback) {
            _callback.SetReceiver(callback);
			_socket.RepeatAsyncRead(_callback);
		}

        public void Close() {
            _socket.Close();
        }
        public void ForceDisposed() {
            _socket.Close();
        }

    	public void Resize(int width, int height) {
			if(!_parent.IsClosed) {
				TelnetOptionWriter wr = new TelnetOptionWriter();
				wr.WriteTerminalSize(width, height);
				wr.WriteTo(_socket);
			}
		}

        public void Transmit(string data)
        {
            Transmit(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
        }

        public void Transmit(ByteDataFragment data) {
            Transmit(data.Buffer, data.Offset, data.Length);
        }

		public void Transmit(byte[] buf, int offset, int length) {
			for(int i=0; i<length; i++) {
				byte t = buf[offset+i];
				if(t==0xFF || t==0x0D) { //0xFF‚Ü‚½‚ÍCRLFˆÈŠO‚ÌCR‚ðŒ©‚Â‚¯‚½‚ç
					WriteEscaping(buf, offset, length);
					return;
				}
			}
			_socket.Transmit(buf, offset, length); //‘å’ï‚Ìê‡‚Í‚±‚¤‚¢‚¤ƒf[ƒ^‚Í“ü‚Á‚Ä‚¢‚È‚¢‚Ì‚ÅA‚‘¬‰»‚Ì‚½‚ß‚»‚Ì‚Ü‚Ü‘—‚èo‚·
		}
		private void WriteEscaping(byte[] buf, int offset, int length) {
			byte[] newbuf = new byte[length*2];
			int newoffset = 0;
			for(int i=0; i<length; i++) {
				byte t = buf[offset+i];
				if(t==0xFF) {
					newbuf[newoffset++] = 0xFF;
					newbuf[newoffset++] = 0xFF; //‚QŒÂ
				}
				else if(t==0x0D/* && (i==length-1 || buf[offset+i+1]!=0x0A)*/) {
					newbuf[newoffset++] = 0x0D;
					newbuf[newoffset++] = 0x00;
				}
				else
					newbuf[newoffset++] = t;
			}
			_socket.Transmit(newbuf, 0, newoffset);
		}

        public bool Available {
            get {
                return _socket.Available;
            }
        }

		public void AreYouThere() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.AreYouThere;
			_socket.Transmit(data, 0, data.Length);
		}
		public void SendBreak() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.Break;
			_socket.Transmit(data, 0, data.Length);
		}
		public void SendKeepAliveData() {
			byte[] data = new byte[2];
			data[0] = (byte)TelnetCode.IAC;
			data[1] = (byte)TelnetCode.NOP;
			_socket.Transmit(data, 0, data.Length);
		}
    }

    public class TelnetTerminalConnection : TCPTerminalConnection
    {
        private TelnetReceiver _telnetReceiver;
        private TelnetSocket _telnetSocket;
        private IPoderosaSocket _rawSocket;

		public TelnetTerminalConnection(ITCPParameter p, TelnetNegotiator neg, PlainPoderosaSocket s)
            : base(p) {
            s.SetOwnerConnection(this);
            _telnetReceiver = new TelnetReceiver(this, neg);
            _telnetSocket = new TelnetSocket(this, s, _telnetReceiver);
            _rawSocket = s;
            _socket = _telnetSocket;
            _terminalOutput = _telnetSocket;
		}
        //Telnet‚ÌƒGƒXƒP[ƒv‹@”\‚Â‚«
        public TelnetSocket TelnetSocket {
            get {
                return _telnetSocket;
            }
        }
        //TelnetSocket‚ª“à•ï‚·‚é¶ƒ\ƒPƒbƒg
        public IPoderosaSocket RawSocket {
            get {
                return _rawSocket;
            }
        }

        public override void Transmit(string data)
        {
            _telnetSocket.Transmit(data);
        }

		public override void Close() {
			if(_closed) return; //‚Q“xˆÈãƒNƒ[ƒY‚µ‚Ä‚à•›ì—p‚È‚µ 
            _telnetSocket.Close();
			base.Close();
		}
	}

    public class RawTerminalConnection : ITerminalConnection, ITerminalOutput
    {
        private IPoderosaSocket _socket;
        private ITerminalParameter _terminalParameter;

        public RawTerminalConnection(IPoderosaSocket socket, ITerminalParameter tp) {
            _socket = socket;
            _terminalParameter = tp;
        }

        
        public ITerminalParameter Destination {
            get {
                return _terminalParameter;
            }
        }

        public ITerminalOutput TerminalOutput {
            get {
                return this;
            }
        }

        public IPoderosaSocket Socket {
            get {
                return _socket;
            }
        }

        public bool IsClosed {
            get {
                return false;
            }
        }

        public void Close() {
            _socket.Close();
        }

        //ITerminalOutput‚ÍƒVƒJƒg
        public void SendBreak() {
        }

        public void SendKeepAliveData() {
        }

        public void AreYouThere() {
        }

        public void Resize(int width, int height) {
        }
    }
}
