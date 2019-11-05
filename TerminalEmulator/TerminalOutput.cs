/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalOutput.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Resources;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;

using Core;
using Protocols;
//using Poderosa.Forms;

namespace TerminalEmulator
//namespace Poderosa.Terminal
{
    //‚à‚ÆTerminalControl‚ÆAbstractTerminal‚É‚²‚¿‚á‚²‚¿‚á‚µ‚Ä‚¢‚½‘—M‹@”\‚ð”²‚«o‚µ
    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹‚Ö‚Æ‘—M‚·‚é‹@”\‚ð’ñ‹Ÿ‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// Offer the function to transmit to the terminal.
    /// </en>
    /// </summary>
    public class TerminalTransmission {
        private AbstractTerminal _host;
        private ITerminalSettings _settings;
        private ITerminalConnection _connection;
        private ByteDataFragment _dataForLocalEcho;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <exclude/>
        public TerminalTransmission(AbstractTerminal host, ITerminalSettings settings, ITerminalConnection connection) {
            _host = host;
            _settings = settings;
            _connection = connection;
            _dataForLocalEcho = new ByteDataFragment();
        }

        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚ÌƒRƒlƒNƒVƒ‡ƒ“‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Show the connection of terminal.
        /// </en>
        /// </summary>
        public ITerminalConnection Connection {
            get {
                return _connection;
            }
        }

        //‰üs‚Í“ü‚Á‚Ä‚¢‚È‚¢‘O’ñ‚Å
        /// <summary>
        /// <ja>
        /// CharŒ^‚Ì”z—ñ‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send a array of Char type.
        /// </en>
        /// </summary>
        /// <param name="chars"><ja>‘—M‚·‚é•¶Žš”z—ñ</ja><en>String array to send</en></param>
        /// <remarks>
        /// <ja>
        /// •¶Žš‚ÍŒ»Ý‚ÌƒGƒ“ƒR[ƒhÝ’è‚É‚æ‚èƒGƒ“ƒR[ƒh‚³‚ê‚Ä‚©‚ç‘—M‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// After it is encoded by a present encode setting, the character is transmitted. 
        /// </en>
        /// </remarks>
        public void SendString(char[] chars) {
            byte[] data = EncodingProfile.Get(_settings.Encoding).GetBytes(chars);
            Transmit(data);
        }
        /// <summary>
        /// <ja>
        /// ‰üs‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Transmit line feed.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// ŽÀÛ‚É‘—‚éƒf[ƒ^‚Í‰üsÝ’è‚É‚æ‚èAuCRvuLFvuCR+LFv‚Ì‚¢‚¸‚ê‚©‚É‚È‚è‚Ü‚·B
        /// </ja>
        /// <en>
        /// The data actually sent becomes either of "CR" "LF" "CR+LF" by the changing line setting. 
        /// </en>
        /// </remarks>
        public void SendLineBreak() {
            byte[] t = TerminalUtil.NewLineBytes(_settings.TransmitNL);
            Transmit(t);
        }
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚ÌƒTƒCƒY‚ð•ÏX‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Change terminal size.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>ƒ^[ƒ~ƒiƒ‹‚Ì•</ja><en>Width of terminal.</en></param>
        /// <param name="height"><ja>ƒ^[ƒ~ƒiƒ‹‚Ì‚‚³</ja><en>Height of terminal</en></param>
        public void Resize(int width, int height) {
            //TODO Transmit()‚Æ“¯—l‚Ìtry...catch
            if(_connection.TerminalOutput!=null) //keyboard-interactive”FØ’†‚È‚ÇAƒTƒCƒY•ÏX‚Å‚«‚È‚¢‹Ç–Ê‚à‚ ‚é
                _connection.TerminalOutput.Resize(width, height);
        }

        /// <summary>
        /// <ja>
        /// ƒoƒCƒg”z—ñ‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send array of byte.
        /// </en>
        /// </summary>
        /// <param name="data"><ja>‘—M‚·‚éƒf[ƒ^‚ªŠi”[‚³‚ê‚½ƒoƒCƒg”z—ñ</ja><en>Byte array that contains data to send.</en></param>
        public void Transmit(byte[] data) {
            try {
                if(_settings.LocalEcho) {
                    _dataForLocalEcho.Set(data, 0, data.Length);
                    _host.OnReception(_dataForLocalEcho);
                }
                _connection.Socket.Transmit(data, 0, data.Length);
            }
            catch(Exception) {
                try {
                    _connection.Close();
                }
                catch(Exception ex2) { //‚±‚Ì‚Æ‚«‚É‰¼‚ÉƒGƒ‰[‚ª”­¶‚µ‚Ä‚àƒ†[ƒU‚É‚Í’Ê’m‚¹‚¸
                    RuntimeUtil.ReportException(ex2);
                }

                //_host.TerminalHost.OwnerWindow.Warning("Message.TerminalControl.FailedToSend");
                Console.WriteLine("Message.TerminalControl.FailedToSend");
            }
        }

        //Žå‚ÉPaste—p•¡”s‘—MBI—¹ŒãƒNƒ[ƒY
        /// <summary>
        /// <ja>
        /// TextStream‚©‚ç“Ç‚ÝŽæ‚Á‚½ƒf[ƒ^‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Transmit the data read from TextStream.
        /// </en>
        /// </summary>
        /// <param name="reader"><ja>“Ç‚ÝŽæ‚éTextStream</ja><en>Read TextStream</en></param>
        /// <param name="send_linebreak_last"><ja>ÅŒã‚É‰üs‚ð•t‚¯‚é‚©‚Ç‚¤‚©‚ðŽw’è‚·‚éƒtƒ‰ƒOBtrue‚Ì‚Æ‚«AÅŒã‚É‰üs‚ª•t—^‚³‚ê‚Ü‚·B</ja><en>Flag that specifies whether to put changing line at the end. Line feed is given at the end at true. </en></param>
        /// <remarks>
        /// <para>
        /// <ja>ƒf[ƒ^‚ÍŒ»Ý‚ÌƒGƒ“ƒR[ƒhÝ’è‚É‚æ‚èAƒGƒ“ƒR[ƒh‚³‚ê‚Ä‚©‚ç‘—M‚³‚ê‚Ü‚·B</ja><en>After it is encoded by a present encode setting, data is transmitted. </en>
        /// </para>
        /// <para>
        /// <ja><paramref name="reader"/>‚Íƒf[ƒ^‚Ì‘—MŒã‚É•Â‚¶‚ç‚ê‚Ü‚·iCloseƒƒ\ƒbƒh‚ªŒÄ‚Ño‚³‚ê‚Ü‚·jB</ja><en>After data is transmitted, <paramref name="reader"/> is closed (The Close method is called). </en>
        /// </para>
        /// </remarks>
        public void SendTextStream(TextReader reader, bool send_linebreak_last) {
            string line = reader.ReadLine();
            while(line!=null) {
                SendString(line.ToCharArray());

                //‚Â‚Ã‚«‚Ìs‚ª‚ ‚é‚È‚ç‚ÎA‰üs‚Í•K‚¸‘—‚éBÅIs‚Å‚ ‚é‚È‚ç‚ÎA‚»‚ê‚ª‰üs•¶Žš‚ÅI‚í‚Á‚Ä‚¢‚éê‡‚Ì‚Ý‰üs‚ð‘—‚éB
                //‘—‚é‰üs‚ÍƒNƒŠƒbƒvƒ{[ƒh‚Ì“à—e‚ÉŠÖ‚í‚ç‚¸ƒ^[ƒ~ƒiƒ‹‚ÌÝ’è‚ÉŠî‚Ã‚­‚±‚Æ‚É’ˆÓ
                bool last = reader.Peek()==-1;
                bool linebreak = last? send_linebreak_last : true;
                if(linebreak) SendLineBreak();

                line = reader.ReadLine();
            }
            reader.Close();
        }

        //•œŠˆ
        /// <exclude/>
        public void Revive(ITerminalConnection connection, int terminal_width, int terminal_height) {
            _connection = connection;
            Resize(terminal_width, terminal_height);
        }

    }

}
