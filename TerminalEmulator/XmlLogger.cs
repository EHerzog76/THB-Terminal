/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: XmlLogger.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;

//using Poderosa.Document;
using Protocols;
//using Poderosa.Util;
using Core;
using ConnectionParam;

//‚¤‚ëŠo‚¦‚¾‚ªAŽQÆæƒAƒZƒ“ƒuƒŠ‚ÌŽQÆ‚Í‚P‚Â‚Ìƒ\[ƒXƒtƒ@ƒCƒ‹’PˆÊ‚¾‚Á‚½‚æ‚¤‚È‹C‚ª‚·‚é‚Ì‚ÅA
//System.Xml.dll‚Ì“Ç‚Ýž‚Ý‚ð‹É—Í’x‚ç‚¹‚é‚½‚ß‚Éƒtƒ@ƒCƒ‹‚ð•ª—£

namespace TerminalEmulator
{
    public class XmlLogger : LoggerBase, IXmlLogger {

        private XmlWriter _writer;
        private char[] _buffer;

        public XmlLogger(ISimpleLogSettings log, StreamWriter w)
            : base(log) {
            _writer = new XmlTextWriter(w);
            _writer.WriteStartDocument();
            _writer.WriteStartElement("terminal-log");

            //Ú‘±Žž‚ÌƒAƒgƒŠƒrƒ…[ƒg‚ð‘‚«ž‚Þ
            _writer.WriteAttributeString("time", DateTime.Now.ToString());
            _buffer = new char[1];

        }

        public void Write(char ch) {
            switch(ch) {
                case (char)0:
                    WriteSPChar("NUL");
                    break;
                case (char)1:
                    WriteSPChar("SOH");
                    break;
                case (char)2:
                    WriteSPChar("STX");
                    break;
                case (char)3:
                    WriteSPChar("ETX");
                    break;
                case (char)4:
                    WriteSPChar("EOT");
                    break;
                case (char)5:
                    WriteSPChar("ENQ");
                    break;
                case (char)6:
                    WriteSPChar("ACK");
                    break;
                case (char)7:
                    WriteSPChar("BEL");
                    break;
                case (char)8:
                    WriteSPChar("BS");
                    break;
                case (char)11:
                    WriteSPChar("VT");
                    break;
                case (char)12:
                    WriteSPChar("FF");
                    break;
                case (char)14:
                    WriteSPChar("SO");
                    break;
                case (char)15:
                    WriteSPChar("SI");
                    break;
                case (char)16:
                    WriteSPChar("DLE");
                    break;
                case (char)17:
                    WriteSPChar("DC1");
                    break;
                case (char)18:
                    WriteSPChar("DC2");
                    break;
                case (char)19:
                    WriteSPChar("DC3");
                    break;
                case (char)20:
                    WriteSPChar("DC4");
                    break;
                case (char)21:
                    WriteSPChar("NAK");
                    break;
                case (char)22:
                    WriteSPChar("SYN");
                    break;
                case (char)23:
                    WriteSPChar("ETB");
                    break;
                case (char)24:
                    WriteSPChar("CAN");
                    break;
                case (char)25:
                    WriteSPChar("EM");
                    break;
                case (char)26:
                    WriteSPChar("SUB");
                    break;
                case (char)27:
                    WriteSPChar("ESC");
                    break;
                case (char)28:
                    WriteSPChar("FS");
                    break;
                case (char)29:
                    WriteSPChar("GS");
                    break;
                case (char)30:
                    WriteSPChar("RS");
                    break;
                case (char)31:
                    WriteSPChar("US");
                    break;
                default:
                    _buffer[0] = ch;
                    _writer.WriteChars(_buffer, 0, 1);
                    break;
            }

        }
        public void EscapeSequence(char[] body) {
            _writer.WriteStartElement("ESC");
            _writer.WriteAttributeString("seq", new string(body));
            _writer.WriteEndElement();
        }
        public void Flush() {
            _writer.Flush();
        }
        public void Close() {
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
            _writer.Close();
        }

        public void Comment(string comment) {
            _writer.WriteElementString("comment", comment);
        }

        private void WriteSPChar(string name) {
            _writer.WriteElementString(name, "");
        }

    }
}
