/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: KeyFunction.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;
//using System.Windows.Forms;
using System.Globalization;

using Core;

namespace TerminalEmulator
{
    //removed 25.06.2015   =>  Buildaction = NONE
    //ŒJ‚èã‚°‚ÄŽÀ‘•‚·‚é‚±‚Æ‚É‚µ‚½AƒL[‚ÌŠ„“–‚Ì‚½‚ß‚ÌƒNƒ‰ƒXB
    //“TŒ^“I‚É‚ÍA—á‚¦‚Î 0x1F‚Ì‘—M‚Í Ctrl+_ ‚¾‚ªA‰pŒêƒL[ƒ{[ƒh‚Å‚ÍŽÀÛ‚É‚Í Ctrl+Shift+- ‚ª•K—v‚Å‚ ‚èA‰Ÿ‚µ‚Ã‚ç‚¢B‚±‚Ì‚ ‚½‚è‚ð‰ðŒˆ‚·‚éB
    //‚Â‚¢‚Å‚ÉA•¶Žš—ñ‚É‘Î‚µ‚ÄƒoƒCƒ“ƒh‚ð‰Â”\‚É‚·‚ê‚ÎA"ls -la"ƒL[‚Ý‚½‚¢‚È‚Ì‚ð’è‹`‚Å‚«‚éB
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class KeyFunction {

        public static string KeyString(Keys key)
        {
            int ik = (int)key;
            if ((int)Keys.D0 <= ik && ik <= (int)Keys.D9)
                return new string((char)('0' + (ik - (int)Keys.D0)), 1);
            else if (key == Keys.None)
                return "";
            else
            {
                return key.ToString();
                /*
                switch(key) {
                    case Keys.None:
                        return "";
                    //“ÁŽêˆµ‚¢ƒOƒ‹[ƒv
                    case Keys.Prior:
                        return "PageUp";
                    case Keys.Next:
                        return "PageDown";
                        //Oem‚Ù‚É‚á‚ç‚ç‚ª‚¤‚´‚Á‚½‚¢
                    case Keys.OemBackslash:
                        return "Backslash";
                    case Keys.OemCloseBrackets:
                        return "CloseBrackets";
                    case Keys.Oemcomma:
                        return "Comma";
                    case Keys.OemMinus:
                        return "Minus";
                    case Keys.OemOpenBrackets:
                        return "OpenBrackets";
                    case Keys.OemPeriod:
                        return "Period";
                    case Keys.OemPipe:
                        return "Pipe";
                    case Keys.Oemplus:
                        return "Plus";
                    case Keys.OemQuestion:
                        return "Question";
                    case Keys.OemQuotes:
                        return "Quotes";
                    case Keys.OemSemicolon:
                        return "Semicolon";
                    case Keys.Oemtilde:
                        return "Tilde";
                    default:
                        return key.ToString();
                }
                 */
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public class Entry {
            private Keys _key;
            private string _data;

            public Keys Key {
                get {
                    return _key;
                }
            }
            public string Data {
                get {
                    return _data;
                }
            }

            public Entry(Keys key, string data) {
                _key = key;
                _data = data;
            }

            //0xŒ`Ž®‚àŠÜ‚ß‚Äˆµ‚¦‚é‚æ‚¤‚É
            public string FormatData() {
                StringBuilder bld = new StringBuilder();
                foreach(char ch in _data) {
                    if(ch < ' ' || (int)ch==0x7F) { //§Œä•¶Žš‚Ædel
                        bld.Append("0x");
                        bld.Append(((int)ch).ToString("X2"));
                    }
                    else
                        bld.Append(ch);
                }
                return bld.ToString();
            }

            public static string ParseData(string s) {
                StringBuilder bld = new StringBuilder();
                int c = 0;
                while(c < s.Length) {
                    char ch = s[c];
                    if(ch=='0' && c+3<=s.Length && s[c+1]=='x') { //0x00Œ`Ž®B
                        int t;
                        if(Int32.TryParse(s.Substring(c+2, 2), NumberStyles.HexNumber, null, out t)) {
                            bld.Append((char)t);
                        }
                        c += 4;
                    }
                    else {
                        bld.Append(ch);
                        c++;
                    }
                }

                return bld.ToString();
            }

        }

        private List<Entry> _elements;

        public KeyFunction() {
            _elements = new List<Entry>();
        }

        internal FixedStyleKeyFunction ToFixedStyle() {
            Keys[] keys = new Keys[_elements.Count];
            char[][] datas = new char[_elements.Count][];
            for(int i=0; i<_elements.Count; i++) {
                keys[i] = _elements[i].Key;
                datas[i] = _elements[i].Data.ToCharArray();
            }

            FixedStyleKeyFunction r = new FixedStyleKeyFunction(keys, datas);
            return r;
        }

        public static string FormatShortcut(Keys key)
        {
            Keys modifiers = key & Keys.Modifiers;
            StringBuilder b = new StringBuilder();
            if ((modifiers & Keys.Control) != Keys.None)
            {
                b.Append("Ctrl");
            }
            if ((modifiers & Keys.Shift) != Keys.None)
            {
                if (b.Length > 0) b.Append('+');
                b.Append("Shift");
            }
            if ((modifiers & Keys.Alt) != Keys.None)
            {
                if (b.Length > 0) b.Append('+');
                b.Append("Alt");
            }
            if (b.Length > 0)
                b.Append('+');

            b.Append(KeyString(key & Keys.KeyCode));
            return b.ToString();
        }

        public string Format() {
            StringBuilder bld = new StringBuilder();
            foreach(Entry e in _elements) {
                if(bld.Length>0) bld.Append(", ");
                bld.Append(FormatShortcut(e.Key));
                bld.Append("=");
                bld.Append(e.FormatData());
            }
            return bld.ToString();
        }

        public static KeyFunction Parse(string format) {
            string[] elements = format.Split(',');
            KeyFunction f = new KeyFunction();
            foreach(string e in elements) {
                int eq = e.IndexOf('=');
                if(eq!=-1) {
                    string keypart = e.Substring(0, eq).Trim();
                    f._elements.Add(new Entry(WinFormsUtil.ParseKey(keypart.Split('+')), Entry.ParseData(e.Substring(eq+1))));
                }
            }
            return f;
        }
    }

    internal class FixedStyleKeyFunction {
        public Keys[] _keys;
        public char[][] _datas;

        public FixedStyleKeyFunction(Keys[] keys, char[][] data) {
            _keys = keys;
            _datas =data;
        }
    }
}
