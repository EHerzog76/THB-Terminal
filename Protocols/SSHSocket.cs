/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SSHSocket.cs,v 1.2 2010/12/02 16:50:48 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;

using Core;

using Granados;
using Granados.SSH2;

//namespace Poderosa.Protocols
namespace Protocols
{
    //SSH‚Ì“üo—ÍŒn
    public abstract class SSHConnectionEventReceiverBase : ISSHConnectionEventReceiver {
        protected SSHTerminalConnection _parent;
        protected SSHConnection _connection;
        protected IByteAsyncInputStream _callback;
        private bool _normalTerminationCalled;

        public SSHConnectionEventReceiverBase(SSHTerminalConnection parent) {
            _parent = parent;
        }
        //SSHConnectionŠm—§Žž‚ÉŒÄ‚Ô
        public void SetSSHConnection(SSHConnection connection) {
            _connection = connection;
            _connection.AutoDisconnect = true; //ÅŒã‚Ìƒ`ƒƒƒlƒ‹Ø’f‚ÅƒRƒlƒNƒVƒ‡ƒ“‚àØ’f
        }
        public SSHConnection Connection {
            get {
                return _connection;
            }
        }
        public virtual void CleanupErrorStatus() {
            if(_connection!=null && _connection.IsOpen) _connection.Close();
        }

        public abstract void Close();

        public virtual void OnAuthenticationPrompt(string[] prompts) {
        }

        public virtual void OnConnectionClosed() {
            OnNormalTerminationCore();
            _connection.Close();
        }

        public virtual void OnError(Exception error) {
            OnAbnormalTerminationCore(error.Message);
        }

        //TODO –Å‘½‚É‚È‚¢‚±‚Æ‚Å‚Í‚ ‚é‚ª‚±‚ê‚ðE‚¤æ‚ðEXTP‚Å
        public virtual void OnDebugMessage(bool always_display, byte[] data) {
            Debug.WriteLine(String.Format("SSH debug {0}[{1}]", data.Length, data[0]));
        }

        public virtual void OnIgnoreMessage(byte[] data) {
            Debug.WriteLine(String.Format("SSH ignore {0}[{1}]", data.Length, data[0]));
        }

        public virtual void OnUnknownMessage(byte type, byte[] data) {
            Debug.WriteLine(String.Format("Unexpected SSH packet type {0}", type));
        }

        //ˆÈ‰º‚ÍŒÄ‚Î‚ê‚é‚±‚Æ‚Í‚È‚¢B‹óŽÀ‘•
        public virtual PortForwardingCheckResult CheckPortForwardingRequest(string remote_host, int remote_port, string originator_ip, int originator_port) {
            return new Granados.PortForwardingCheckResult();
        }
        public virtual void EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel) {
        }

        protected void OnNormalTerminationCore() {
            if(_normalTerminationCalled) return;

            /* NOTE
             *  ³íI—¹‚Ìê‡‚Å‚àASSHƒpƒPƒbƒgƒŒƒxƒ‹‚Å‚ÍChannelEOF, ChannelClose, ConnectionClose‚ª‚ ‚èAê‡‚É‚æ‚Á‚Ä‚Í•¡”ŒÂ‚ª‘g‚Ý‡‚í‚³‚ê‚é‚±‚Æ‚à‚ ‚éB
             *  ‘g‚Ý‡‚í‚¹‚ÌÚ×‚ÍƒT[ƒo‚ÌŽÀ‘•ˆË‘¶‚Å‚à‚ ‚é‚Ì‚ÅA‚±‚±‚Å‚Í‚P‰ñ‚¾‚¯•K‚¸ŒÄ‚Ô‚Æ‚¢‚¤‚±‚Æ‚É‚·‚éB
             */
            _normalTerminationCalled = true;
            EnsureCallbackHandler();
            _parent.CloseBySocket();

            try {
                _callback.OnNormalTermination();
            }
            catch(Exception ex) {
                CloseError(ex);
            }
        }
        protected void OnAbnormalTerminationCore(string msg) {
            EnsureCallbackHandler();
            _parent.CloseBySocket();

            try {
                _callback.OnAbnormalTermination(msg);
            }
            catch(Exception ex) {
                CloseError(ex);
            }
        }
        protected void EnsureCallbackHandler() {
            int n = 0;
            //TODO ‚«‚ê‚¢‚Å‚È‚¢‚ªAÚ‘±`StartRepeat‚Ü‚Å‚ÌŠÔ‚ÉƒGƒ‰[‚ªƒT[ƒo‚©‚ç’Ê’m‚³‚ê‚½‚Æ‚«‚ÉB
            while(_callback==null && n++<100) //‚í‚¸‚©‚ÈŽžŠÔ·‚Åƒnƒ“ƒhƒ‰‚ªƒZƒbƒg‚³‚ê‚È‚¢‚±‚Æ‚à‚ ‚é
                Thread.Sleep(100);
        }
        //Terminationˆ—‚ÌŽ¸”sŽž‚Ìˆ—
        private void CloseError(Exception ex) {
            try {
                RuntimeUtil.ReportException(ex);
                CleanupErrorStatus();
            }
            catch(Exception ex2) {
                RuntimeUtil.ReportException(ex2);
            }
        }
    }

    public class SSHSocket : SSHConnectionEventReceiverBase, IPoderosaSocket, ITerminalOutput, ISSHChannelEventReceiver {
        private SSHChannel _channel;
        private ByteDataFragment _data;
        private bool _waitingSendBreakReply;
        //”ñ“¯Šú‚ÉŽóM‚·‚éB
        private MemoryStream _buffer; //RepeatAsyncRead‚ªŒÄ‚Î‚ê‚é‘O‚ÉŽóM‚µ‚Ä‚µ‚Ü‚Á‚½ƒf[ƒ^‚ðˆêŽž•ÛŠÇ‚·‚éƒoƒbƒtƒ@

        public SSHSocket(SSHTerminalConnection parent) : base(parent) {
            _data = new ByteDataFragment();
        }

        public SSHChannel Channel {
            get {
                return _channel;
            }
        }

        public void RepeatAsyncRead(IByteAsyncInputStream cb) {
            _callback = cb;
            //ƒoƒbƒtƒ@‚É‰½‚ª‚µ‚©—­‚Ü‚Á‚Ä‚¢‚éê‡F
            //NOTE ‚±‚ê‚ÍAIPoderosaSocket#StartAsyncRead‚ðŒÄ‚ÔƒV[ƒPƒ“ƒX‚ð‚È‚­‚µAÚ‘±‚ðŠJŽn‚·‚éuŠÔ(IProtocolService‚Ìƒƒ\ƒbƒhŒn)‚©‚ç
            //ƒf[ƒ^–{‘Ì‚ðŽóM‚·‚éŒû‚ð’ñ‹Ÿ‚³‚¹‚é‚æ‚¤‚É‚·‚ê‚Îœ‹Ž‚Å‚«‚éB‚µ‚©‚µƒvƒƒOƒ‰ƒ}‚Ì‘¤‚Æ‚µ‚Ä‚ÍAÚ‘±¬Œ÷‚ðŠm”F‚µ‚Ä‚©‚çƒf[ƒ^ŽóMŒû‚ð—pˆÓ‚µ‚½‚¢‚Ì‚ÅA
            //iPoderosa‚Å‚¢‚¦‚ÎAƒƒOƒCƒ“ƒ{ƒ^ƒ“‚ÌOK‚ð‰Ÿ‚·Žž“_‚ÅAbstractTerminal‚Ü‚Å€”õ‚¹‚Ë‚Î‚È‚ç‚È‚¢‚Æ‚¢‚¤‚±‚ÆjA‚»‚ê‚æ‚è‚Íƒf[ƒ^‚ð•Û—¯‚µ‚Ä‚¢‚é‚Ù‚¤‚ª‚¢‚¢‚¾‚ë‚¤
            if(_buffer!=null) {
                lock(this) {
                    _buffer.Close();
                    byte[] t = _buffer.ToArray();
                    _data.Set(t, 0, t.Length);
                    if(t.Length>0) _callback.OnReception(_data);
                    _buffer = null;
                }
            }
        }

        public override void CleanupErrorStatus() {
            if(_channel!=null) _channel.Close();
            base.CleanupErrorStatus();
        }

        public void OpenShell() {
            _channel = _connection.OpenShell(this);
        }
        public void OpenSubsystem(string subsystem) {
            SSH2Connection ssh2 = _connection as SSH2Connection;
            if(ssh2==null) throw new SSHException("OpenSubsystem() can be applied to only SSH2 connection");
            _channel = ssh2.OpenSubsystem(this, subsystem);
        }

        public override void Close() {
            if(_channel != null) _channel.Close();
        }
        public void ForceDisposed() {
            _connection.Close(); //ƒ}ƒ‹ƒ`ƒ`ƒƒƒlƒ‹‚¾‚ÆƒAƒEƒg‚©‚à
        }

        public void Transmit(string data)
        {
            _channel.Transmit(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length);
        }

        public void Transmit(ByteDataFragment data) {
            _channel.Transmit(data.Buffer, data.Offset, data.Length);
        }

        public void Transmit(byte[] buf, int offset, int length) {
            _channel.Transmit(buf, offset, length);
        }

        //ˆÈ‰ºAITerminalOutput
        public void Resize(int width, int height) {
            if(!_parent.IsClosed)
                _channel.ResizeTerminal(width, height, 0, 0);
        }
        public void SendBreak() {
            if(_parent.SSHLoginParameter.Method==SSHProtocol.SSH1)
                throw new NotSupportedException();
            else {
                _waitingSendBreakReply = true;
                ((Granados.SSH2.SSH2Channel)_channel).SendBreak(500);
            }
        }
        public void SendKeepAliveData() {
            if(!_parent.IsClosed)
                _connection.SendIgnorableData("keep alive");
        }
        public void AreYouThere() {
            throw new NotSupportedException();
        }

        public void OnChannelClosed() {
            OnNormalTerminationCore();
        }
        public void OnChannelEOF() {
            OnNormalTerminationCore();
        }
        public void OnData(byte[] data, int offset, int length) {
            if(_callback == null) { //RepeatAsyncRead‚ªŒÄ‚Î‚ê‚é‘O‚Ìƒf[ƒ^‚ðW‚ß‚Ä‚¨‚­
                lock(this) {
                    if(_buffer == null) _buffer = new MemoryStream(0x1000);
                    _buffer.Write(data, offset, length);
                }
            }
            else {
                _data.Set(data, offset, length);
                _callback.OnReception(_data);
            }
        }
        public void OnExtendedData(int type, byte[] data) {
        }
        public void OnMiscPacket(byte type, byte[] data, int offset, int length) {
            if(_waitingSendBreakReply) {
                _waitingSendBreakReply = false;
				if(type==(byte)Granados.SSH2.PacketType.SSH_MSG_CHANNEL_FAILURE)
                    Console.WriteLine("Message.SSHTerminalconnection.BreakError");
            }
        }

        public void OnChannelReady() { //!!Transmit‚ð‹–‰Â‚·‚é’Ê’m‚ª•K—vH
        }

        public void OnChannelError(Exception ex) {
            OnAbnormalTerminationCore(ex.Message);
        }


        public SSHConnectionInfo ConnectionInfo {
            get {
                return _connection.ConnectionInfo;
            }
        }

        public bool Available {
            get {
                return _connection.Available;
            }
        }
    }

    //Keyboard Interactive”FØ’†
    public class KeyboardInteractiveAuthHanlder : SSHConnectionEventReceiverBase, IPoderosaSocket {
        private MemoryStream _passwordBuffer;
        private string[] _prompts;

        public KeyboardInteractiveAuthHanlder(SSHTerminalConnection parent)
            : base(parent) {
        }

        public override void OnAuthenticationPrompt(string[] prompts) {
            //‚±‚±‚É—ˆ‚éƒP[ƒX‚Í‚Q‚ÂB

            if(_callback==null) //1. Å‰‚Ì”FØ’†
                _prompts = prompts;
            else { //2. ƒpƒXƒ[ƒh“ü—Í‚Ü‚¿‚ª‚¢‚È‚Ç‚Å‚à‚¤ˆê‰ñ‚Æ‚¢‚¤ê‡
                EnsureCallbackHandler();
                ShowPrompt(prompts);
            }
        }

        public void RepeatAsyncRead(IByteAsyncInputStream receiver) {
            _callback = receiver;
            if(_prompts!=null) ShowPrompt(_prompts);
        }
        private void ShowPrompt(string[] prompts) {
            bool hasPassword = _parent.SSHLoginParameter.PasswordOrPassphrase != null
                            && !_parent.SSHLoginParameter.LetUserInputPassword;
            bool sendPassword = false;
            for (int i = 0; i < prompts.Length; i++) {
                if (hasPassword && prompts[i].Contains("assword")) {
                    sendPassword = true;
                    break;
                }
                if (i != 0) prompts[i] += "\r\n";
                byte[] buf = Encoding.Default.GetBytes(prompts[i]);
                _callback.OnReception(new ByteDataFragment(buf, 0, buf.Length));
            }

            if (sendPassword) {
                SendPassword(_parent.SSHLoginParameter.PasswordOrPassphrase);
            }
        }

        public bool Available {
            get {
                return _connection.Available;
            }
        }

        public void Transmit(string data)
        {
            Transmit(new ByteDataFragment(System.Text.ASCIIEncoding.ASCII.GetBytes(data), 0, data.Length));
        }

        public void Transmit(ByteDataFragment data) {
            Transmit(data.Buffer, data.Offset, data.Length);
        }

        public void Transmit(byte[] data, int offset, int length) {
            if(_passwordBuffer==null) _passwordBuffer = new MemoryStream();

            for(int i=offset; i<offset+length; i++) {
                byte b = data[i];
                if(b==13 || b==10) { //CR/LF
                    SendPassword(null);
                }
                else
                    _passwordBuffer.WriteByte(b);
            }
        }
        private void SendPassword(string password) {
            string[] response;
            if (password != null) {
                response = new string[] { password };
            } else {
                byte[] pwd = _passwordBuffer.ToArray();
                if (pwd.Length > 0) {
                    _passwordBuffer.Close();
                    _passwordBuffer.Dispose();
                    _passwordBuffer = null;
                    response = new string[] { Encoding.ASCII.GetString(pwd) };
                } else {
                    response = null;
                }
            }
            
            if (response != null) {
                _callback.OnReception(new ByteDataFragment(new byte[] { 13, 10 }, 0, 2)); //•\Ž¦ãCR+LF‚Å‰üs‚µ‚È‚¢‚ÆŠiDˆ«‚¢
                if(((Granados.SSH2.SSH2Connection)_connection).DoKeyboardInteractiveAuth(response)==AuthenticationResult.Success) {
                    _parent.SSHLoginParameter.PasswordOrPassphrase = response[0];
                    SuccessfullyExit();
                    return;
                }
            }
            _connection.Disconnect("");
            throw new IOException("Message.SSHConnector.Cancelled");
        }
        //ƒVƒFƒ‹‚ðŠJ‚«AƒCƒxƒ“ƒgƒŒƒV[ƒo‚ð‘‚«Š·‚¦‚é
        private void SuccessfullyExit() {
            SSHSocket sshsocket = new SSHSocket(_parent);
            sshsocket.SetSSHConnection(_connection);
            sshsocket.RepeatAsyncRead(_callback); //_callback‚©‚çæ‚Ìˆ—‚Í“¯‚¶
            _connection.EventReceiver = sshsocket;
            _parent.ReplaceSSHSocket(sshsocket);
            sshsocket.OpenShell();
        }

        public override void Close() {
            _connection.Close();
        }
        public void ForceDisposed() {
            _connection.Close();
        }

    }
}
