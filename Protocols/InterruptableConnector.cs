/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: InterruptableConnector.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using Core;

//namespace Poderosa.Protocols
namespace Protocols
{

	/// <summary>
	/// ƒ\ƒPƒbƒg‚ðŠJ‚«AÚ‘±‚ðŠm—§‚·‚éŽè‡‚ð‚µ‚Â‚Âƒ^ƒCƒ€ƒAƒEƒg‚â“r’†‚Å’†Ž~‚·‚é‚±‚Æ‚à‚Å‚«‚é‚½‚ß‚Ì‹@”\
	/// </summary>
    public abstract class InterruptableConnector : IInterruptable /*, IConnectionResultEventHandler */
    {
        private IPAddressList _addressSet;
        protected IInterruptableConnectorClient _client;
        protected NetUtil _NetUtil;
        protected ITCPParameter _tcpDestination;
		protected IPAddress _connectedAddress;
		protected Socket _socket;
		protected string _host;
		protected int _port;
        protected Socks _socks;

		protected bool _succeeded;
		protected bool _interrupted;
        protected bool _overridingErrorMessage;
		protected bool _tcpConnected;

		protected string _errorMessage;

        public void AsyncConnect(IInterruptableConnectorClient client, ITCPParameter param) {
			_client = client;
            _tcpDestination = param;
            _host = param.Destination;
			_port = param.Port;
            _NetUtil = new NetUtil(_client.ConMain);

            ////AgentForward“™‚Ìƒ`ƒFƒbƒN‚ð‚·‚é
            //foreach(IConnectionResultEventHandler ch in ProtocolsPlugin.Instance.ConnectionResultEventHandler.GetExtensions())
            //    ch.BeforeAsyncConnect((ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter)));

            Thread th = new Thread(new ThreadStart(this.Run) );
            th.Start();
		}


		private void NotifyAsyncClient() {
			if(_succeeded)
				_client.SuccessfullyExit(this.Result);
			else
				_client.ConnectionFailed(_errorMessage);
		}

		public void Interrupt() {
			_interrupted = true;
            //Ú‘±ƒXƒŒƒbƒh‚ªƒuƒƒbƒN‚µ‚Ä‚¢‚½‚è’ÊM’†‚Å‚ ‚Á‚Ä‚àAƒ\ƒPƒbƒg‚ð•Â‚¶‚Ä‚µ‚Ü‚¦‚Î‚·‚®—áŠO‚É‚È‚é‚Í‚¸‚Å‚ ‚èA
            //‰º‚ÌRun()‚Ìcatch‚ÆfinallyƒuƒƒbƒN‚ªŽÀs‚³‚ê‚éB
			if(_socket!=null)
				_socket.Close();
            if (_NetUtil != null)
                _NetUtil = null;
		}

        //Start..EndŠÔ‚Å”­¶‚·‚éException‚É‚Â‚¢‚Ä‚ÍAƒGƒ‰[ƒƒbƒZ[ƒW‚ðã‘‚«‚·‚éB
        //•Ï‚ÈSocketException‚ÌƒGƒ‰[ƒƒbƒZ[ƒW‚ðŽg‚¢‚½‚­‚È‚¢‚Æ‚«—p
        protected void StartOverridingErrorMessage(string message) {
            _errorMessage = message;
            _overridingErrorMessage = true;
        }
        protected void EndOverridingErrorMessage() {
            _overridingErrorMessage = false;
        }

		private void Run() {
			_tcpConnected = false;
			_succeeded = false;
            _socket = null;

			try {
				_addressSet = null;
				_errorMessage = null;
				MakeConnection();
                Debug.Assert(_socket!=null);

				_errorMessage = null;
				Negotiate();
				
				_succeeded = true;
                ////ProtocolUtil.FireConnectionSucceeded((ITerminalParameter)_tcpDestination);
                //_client.SuccessfullyExit(this.Result);
			}
			catch(Exception ex) {
                if(!_interrupted) {
                    RuntimeUtil.DebuggerReportException(ex);
                    if(!_overridingErrorMessage) {
                        _errorMessage = ex.Message;
                    }
                    ////ProtocolUtil.FireConnectionFailure((ITerminalParameter)_tcpDestination, _errorMessage);
                    //_client.ConnectionFailed(_errorMessage);
                }
			}
			finally {
                if(!_interrupted) {
                    if(!_succeeded && _socket!=null && _socket.Connected) {
                        try {
                            _socket.Shutdown(SocketShutdown.Both); //Close()‚¾‚Æ”ñ“¯ŠúŽóM‚µ‚Ä‚éêŠ‚Å‘¦Exception‚É‚È‚Á‚Ä‚µ‚Ü‚¤
                        } catch(Exception ex) { //‚±‚±‚Å‚¿‚á‚ñ‚Æ•Â‚¶‚é‚±‚Æ‚ªo—ˆ‚È‚¢ê‡‚ª‚ ‚Á‚½
                            RuntimeUtil.SilentReportException(ex);
                        }
                    }
                    //‚±‚±‚Å‘Ò‹@‚µ‚Ä‚¢‚½ƒXƒŒƒbƒh‚ª“®‚«o‚·‚Ì‚ÅA‚»‚Ì‘O‚ÉSocket‚ÌDisconnect‚Í‚â‚Á‚Â‚¯‚Ä‚¨‚­B“¯Žž‚É‚Â‚Â‚¢‚½‚¹‚¢‚©ƒ\ƒPƒbƒg‚Ì“®ì‚ª–­‚É‚È‚Á‚½ƒP[ƒX‚ ‚èB
                    NotifyAsyncClient();
                    if (_NetUtil != null)
                        _NetUtil = null;
                }
			}
		}

		protected virtual void MakeConnection() {
            //‚Ü‚¸SOCKS‚ðŽg‚¤‚×‚«‚©‚Ç‚¤‚©‚ð”»’è‚·‚é
            IProtocolOptions opt = new ProtocolOptions(""); // ProtocolsPlugin.Instance.ProtocolOptions;
            if(opt.UseSocks && SocksApplicapable(opt.SocksNANetworks, IPAddressList.SilentGetAddress(_host))) {
                _socks = new Socks();
                _socks.Account = opt.SocksAccount;
                _socks.Password = opt.SocksPassword;
                _socks.DestName = _host;
                _socks.DestPort = (short)_port;
                _socks.ServerName = opt.SocksServer;
                _socks.ServerPort = (short)opt.SocksPort;
            }

            string dest = _socks==null? _host : _socks.ServerName;
			int    port = _socks==null? _port : _socks.ServerPort;

            IPAddress address = null;
            if(IPAddress.TryParse(dest, out address)) {
				_addressSet = new IPAddressList(address); //Å‰‚©‚çIPƒAƒhƒŒƒXŒ`Ž®‚Ì‚Æ‚«‚ÍŽè‚Å•ÏŠ·B‚»‚¤‚Å‚È‚¢‚ÆDNS‚Ì‹tˆø‚«‚ð‚µ‚Äƒ^ƒCƒ€ƒAƒEƒgA‚Æ‚©‚â‚â‚±‚µ‚¢‚±‚Æ‚ª‹N‚±‚é
            }
            else { //ƒzƒXƒg–¼Œ`Ž®
				StartOverridingErrorMessage(String.Format("Message.AddressNotResolved", dest));
				_addressSet = new IPAddressList(dest);
                EndOverridingErrorMessage();
            }

			StartOverridingErrorMessage(String.Format("Message.FailedToConnectPort", dest, port));
            _socket = _NetUtil.ConnectTCPSocket(_addressSet, port);
            EndOverridingErrorMessage();
			_connectedAddress = ((IPEndPoint)_socket.RemoteEndPoint).Address;

			if(_socks!=null) {
				_errorMessage = "An error occurred in SOCKS negotiation.";
				_socks.Connect(_socket);
                //Ú‘±¬Œ÷‚µ‚½‚ç_host,_port‚ðŒ³‚É–ß‚·
				_host = _socks.DestName;
				_port = _socks.DestPort;
			}

			_tcpConnected = true;
		}


		//...TCP...
        protected abstract void Negotiate();

		//...
		public abstract TerminalConnection Result {
			get;
		}

		public bool Succeeded {
			get {
				return _succeeded;
			}
		}
		public bool Interrupted {
			get {
				return _interrupted;
			}
		}

		public string ErrorMessage {
			get {
				return _errorMessage;
			}
		}
		public IPAddress IPAddress {
			get {
				return _connectedAddress;
			}
		}

        internal IPAddressList IPAddressSet {
            get {
                return _addressSet;
            }
        }

        public Socket RawSocket {
            get {
                return _socket;
            }
        }

		private static bool SocksApplicapable(string nss, IPAddressList address) {
			foreach(string netaddress in nss.Split(';')) {
				if(netaddress.Length==0) continue;

				if(!NetAddressUtil.IsNetworkAddress(netaddress)) {
					throw new FormatException(String.Format("{0} is not suitable as a network address.", netaddress));
				}
                if(address.AvailableAddresses.Length>0 && NetAddressUtil.NetAddressIncludesIPAddress(netaddress, address.AvailableAddresses[0])) //‚PŒÂ‚¾‚¯‚Å”»’fA‚â‚â‚³‚Ú‚è
					return false;
			}
			return true;
		}
	}
}
