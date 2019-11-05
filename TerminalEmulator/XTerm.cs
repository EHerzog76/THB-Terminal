/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: XTerm.cs,v 1.7 2011/01/04 17:08:35 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;

//using Poderosa.Document;
using ConnectionParam;
using Core;

namespace TerminalEmulator
//namespace Poderosa.Terminal
{
	internal class XTerm : VT100Terminal
	{
		private bool _wrapAroundMode;
		private bool _reverseVideo;
		private bool[] _tabStops;
		//private readonly List<GLine>[] _savedScreen = new List<GLine>[2];	// { main, alternate } •Ê‚Ìƒoƒbƒtƒ@‚ÉˆÚs‚µ‚½‚Æ‚«‚ÉGLine‚ð‘Þ”ð‚µ‚Ä‚¨‚­
        private readonly List<string>[] _savedScreen = new List<string>[2];
		private bool _isAlternateBuffer;
		private bool _savedMode_isAlternateBuffer;
		private readonly int[] _xtermSavedRow = new int[2];	// { main, alternate }
		private readonly int[] _xtermSavedCol = new int[2];	// { main, alternate }

		public XTerm(TerminalInitializeInfo info) : base(info) {
			_wrapAroundMode = true;
			_tabStops = new bool[GetDocument().TerminalWidth];
			_isAlternateBuffer = false;
			_savedMode_isAlternateBuffer = false;
			InitTabStops();
		}

        public bool ReverseVideo {
            get {
                return _reverseVideo;
            }
        }

		protected override ProcessCharResult ProcessNormalChar(char ch) {
			//WrapAround‚ªfalse‚ÅAƒLƒƒƒŒƒbƒg‚ª‰E’[‚Ì‚Æ‚«‚Í‰½‚à‚µ‚È‚¢
            if (!_wrapAroundMode && GetDocument().CaretColumn >= GetDocument().TerminalWidth - 1)
				return ProcessCharResult.Processed;

			//if(_insertMode)
			//	_manipulator.InsertBlanks(_manipulator.CaretColumn, GLine.CalcDisplayLength(ch));
			return base.ProcessNormalChar(ch);
		}
		protected override ProcessCharResult ProcessControlChar(char ch) {
			return base.ProcessControlChar(ch);
			/* •¶ŽšƒR[ƒh‚ªŒë‚Á‚Ä‚¢‚é‚Æ‚±‚Ì‚ ‚½‚è‚ð•sˆÓ‚ÉŽÀs‚µ‚Ä‚µ‚Ü‚¤‚±‚Æ‚ª‚ ‚èA‚æ‚ë‚µ‚­‚È‚¢B
			switch(ch) {
				//’Pƒ‚È•ÏŠ·‚È‚ç‘¼‚É‚à‚Å‚«‚é‚ªAƒTƒ|[ƒg‚µ‚Ä‚¢‚é‚Ì‚Í‚¢‚Ü‚Ì‚Æ‚±‚ë‚±‚ê‚µ‚©‚È‚¢
				case (char)0x8D:
					base.ProcessChar((char)0x1B);
					base.ProcessChar('M');
					return ProcessCharResult.Processed;
				case (char)0x9B:
					base.ProcessChar((char)0x1B);
					base.ProcessChar('[');
					return ProcessCharResult.Processed;
				case (char)0x9D:
					base.ProcessChar((char)0x1B);
					base.ProcessChar(']');
					return ProcessCharResult.Processed;
				default:
					return base.ProcessControlChar(ch);
			}
			*/
		}

		protected override ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset) {
			ProcessCharResult v = base.ProcessEscapeSequence(code, seq, offset);
			if(v!=ProcessCharResult.Unsupported) return v;

			switch(code) {
				case 'F':
					if(seq.Length==offset) { //ƒpƒ‰ƒ[ƒ^‚È‚µ‚Ìê‡
						ProcessCursorPosition(1, 1);
						return ProcessCharResult.Processed;
					}
					else if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //7/8ƒrƒbƒgƒRƒ“ƒgƒ[ƒ‹‚Íí‚É—¼•û‚ðƒTƒ|[ƒg
					break;
				case 'G':
					if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //7/8ƒrƒbƒgƒRƒ“ƒgƒ[ƒ‹‚Íí‚É—¼•û‚ðƒTƒ|[ƒg
					break;
				case 'L':
					if(seq.Length>offset && seq[offset]==' ')
						return ProcessCharResult.Processed; //VT100‚ÍÅ‰‚©‚çOK
					break;
				case 'H':
                    SetTabStop(GetDocument().CaretColumn, true);
					return ProcessCharResult.Processed;
			}

			return ProcessCharResult.Unsupported;
		}
		protected override ProcessCharResult ProcessAfterCSI(string param, char code) {
			ProcessCharResult v = base.ProcessAfterCSI(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;

			switch(code) {
				case 'd':
					ProcessLinePositionAbsolute(param);
					return ProcessCharResult.Processed;
				case 'G':
				case '`': //CSI G‚ÍŽÀÛ‚É—ˆ‚½‚±‚Æ‚ª‚ ‚é‚ªA‚±‚ê‚Í—ˆ‚½‚±‚Æ‚ª‚È‚¢B‚¢‚¢‚Ì‚©H
					ProcessLineColumnAbsolute(param);
					return ProcessCharResult.Processed;
				case 'X':
					ProcessEraseChars(param);
					return ProcessCharResult.Processed;
				case 'P':
                    //_manipulator.DeleteChars(GetDocument().CaretColumn, ParseInt(param, 1));
                    GetDocument().MoveCursor(-1);
                    GetDocument().VirtualScreen.WriteByte(0);
					return ProcessCharResult.Processed;
				case 'p':
					return SoftTerminalReset(param);
				case '@':
                    //ToDo:
                    //_manipulator.InsertBlanks(GetDocument().CaretColumn, ParseInt(param, 1));
                    Console.WriteLine("ToDo: in Modul XTerm InsertBlands");
					return ProcessCharResult.Processed;
				case 'I':
					ProcessForwardTab(param);
					return ProcessCharResult.Processed;
				case 'Z':
					ProcessBackwardTab(param);
					return ProcessCharResult.Processed;
				case 'S':
					ProcessScrollUp(param);
					return ProcessCharResult.Processed;
				case 'T':
					ProcessScrollDown(param);
					return ProcessCharResult.Processed;
				case 'g':
					ProcessTabClear(param);
					return ProcessCharResult.Processed;
				case 't':
					//!!ƒpƒ‰ƒ[ƒ^‚É‚æ‚Á‚Ä–³Ž‹‚µ‚Ä‚æ‚¢ê‡‚ÆA‰ž“š‚ð•Ô‚·‚×‚«ê‡‚ª‚ ‚éB‰ž“š‚Ì•Ô‚µ•û‚ª‚æ‚­‚í‚©‚ç‚È‚¢‚Ì‚Å•Û—¯’†
					return ProcessCharResult.Processed;
				case 'U': //‚±‚ê‚ÍSFU‚Å‚µ‚©Šm”F‚Å‚«‚Ä‚È‚¢
					base.ProcessCursorPosition(GetDocument().TerminalHeight, 1);
					return ProcessCharResult.Processed;
				case 'u': //SFU‚Å‚Ì‚ÝŠm”FB“Á‚Éb‚Í‘±‚­•¶Žš‚ðŒJ‚è•Ô‚·‚ç‚µ‚¢‚ªAˆÓ–¡‚Ì‚ ‚é“®ì‚É‚È‚Á‚Ä‚¢‚é‚Æ‚±‚ë‚ðŒ©‚Ä‚¢‚È‚¢
				case 'b':
					return ProcessCharResult.Processed;
				default:
					return ProcessCharResult.Unsupported;
			}
		}

		protected override void ProcessDeviceAttributes(string param) {
			if(param.StartsWith(">")) {
				byte[] data = Encoding.ASCII.GetBytes(" [>82;1;0c");
				data[0] = 0x1B; //ESC
                Transmit(data);
			}
			else
				base.ProcessDeviceAttributes(param);
		}


		protected override ProcessCharResult ProcessAfterOSC(string param, char code) {
			ProcessCharResult v = base.ProcessAfterOSC(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;

			int semicolon = param.IndexOf(';');
			if(semicolon==-1) return ProcessCharResult.Unsupported;

			string ps = param.Substring(0, semicolon);
			string pt = param.Substring(semicolon+1);
			if(ps=="0" || ps=="2") {
                /* IDynamicCaptionFormatter[] fmts = TerminalEmulatorPlugin.Instance.DynamicCaptionFormatter;
                TerminalDocument doc = GetDocument();

                if(fmts.Length > 0) {
                    ITerminalSettings settings = GetTerminalSettings();
                    string title = fmts[0].FormatCaptionUsingWindowTitle(GetConnection().Destination, settings, pt);
                    _afterExitLockActions.Add(new AfterExitLockDelegate(new CaptionChanger(GetTerminalSettings(), title).Do));
                }
                //Quick Test
                //_afterExitLockActions.Add(new AfterExitLockDelegate(new CaptionChanger(GetTerminalSettings(), pt).Do));
                */
				return ProcessCharResult.Processed;
			}
			else if(ps=="1")
				return ProcessCharResult.Processed; //Set Icon Name‚Æ‚¢‚¤‚â‚Â‚¾‚ª–³Ž‹‚Å‚æ‚³‚»‚¤
			else
				return ProcessCharResult.Unsupported;
		}

		protected override ProcessCharResult ProcessDECSET(string param, char code) {
			ProcessCharResult v = base.ProcessDECSET(param, code);
			if(v!=ProcessCharResult.Unsupported) return v;
            bool set = code=='h';

			switch (param) {
				case "1047":	//Alternate Buffer
					if (set) {
						SwitchBuffer(true);
						// XTerm doesn't clear screen.
					}
					else {
						ClearScreen();
						SwitchBuffer(false);
					}
					return ProcessCharResult.Processed;
				case "1048":	//Save/Restore Cursor
					if (set)
						SaveCursor();
					else
						RestoreCursor();
					return ProcessCharResult.Processed;
				case "1049":	//Save/Restore Cursor and Alternate Buffer
					if (set) {
						SaveCursor();
						SwitchBuffer(true);
						ClearScreen();
					}
					else {
						// XTerm doesn't clear screen for enabling copy/paste from the alternate buffer.
						// But we need ClearScreen for emulating the buffer-switch.
						ClearScreen();
						SwitchBuffer(false);
						RestoreCursor();
					}
					return ProcessCharResult.Processed;
				case "1000":
				case "1001":
				case "1002":
				case "1003":	//ƒ}ƒEƒXŠÖŒW‚Í–³Ž‹
					return ProcessCharResult.Processed;
				case "1034":	// Input 8 bits
					return ProcessCharResult.Processed;
				case "3":	//132 Column Mode
					return ProcessCharResult.Processed;
				case "4":	//Smooth Scroll ‚È‚ñ‚Ì‚±‚Æ‚â‚ç
					return ProcessCharResult.Processed;
				case "5":
					SetReverseVideo(set);
					return ProcessCharResult.Processed;
				case "6":	//Origin Mode
					_scrollRegionRelative = set;
					return ProcessCharResult.Processed;
				case "7":
					_wrapAroundMode = set;
					return ProcessCharResult.Processed;
				case "12":
					//ˆê‰ž•ñ‚ ‚Á‚½‚Ì‚ÅBSETMODE‚Ì12‚È‚çƒ[ƒJƒ‹ƒGƒR[‚È‚ñ‚¾‚ª‚È
					return ProcessCharResult.Processed;
				case "47":
					if (set)
						SwitchBuffer(true);
					else
						SwitchBuffer(false);
					return ProcessCharResult.Processed;
				default:
					return ProcessCharResult.Unsupported;
			}
		}

		protected override ProcessCharResult ProcessSaveDECSET(string param, char code) {
			switch (param) {
				case "1047":
				case "47":
					_savedMode_isAlternateBuffer = _isAlternateBuffer;
					break;
			}
			return ProcessCharResult.Processed;
		}

		protected override ProcessCharResult ProcessRestoreDECSET(string param, char code) {
			switch (param) {
				case "1047":
				case "47":
					SwitchBuffer(_savedMode_isAlternateBuffer);
					break;
			}
			return ProcessCharResult.Processed;
		}

		private void ProcessLinePositionAbsolute(string param) {
			foreach(string p in param.Split(';')) {
				int row = ParseInt(p,1);
				if(row<1) row = 1;
				if(row>GetDocument().TerminalHeight) row = GetDocument().TerminalHeight;

                int col = GetDocument().CaretColumn;

				//ˆÈ‰º‚ÍCSI H‚Æ‚Ù‚Ú“¯‚¶
				GetDocument().CleanLineRange(0, GetDocument().TerminalWidth); // .ReplaceCurrentLine(_manipulator.Export());
				GetDocument().CurrentLineNumber = (GetDocument().TopLineNumber + row - 1);
				//_manipulator.Load(GetDocument().CurrentLine, col);
			}
		}
		private void ProcessLineColumnAbsolute(string param) {
			foreach(string p in param.Split(';')) {
				int n = ParseInt(p,1);
				if(n<1) n = 1;
				if(n>GetDocument().TerminalWidth) n = GetDocument().TerminalWidth;
                GetDocument().CaretColumn = n - 1;
			}
		}
		private void ProcessEraseChars(string param) {
			int n = ParseInt(param, 1);
            int s = GetDocument().CaretColumn;
			for(int i=0; i<n; i++) {
                GetDocument().PutChar(' ');
                //if (GetDocument().CaretColumn >= GetDocument().BufferSize)
				//	break;
			}
            GetDocument().CaretColumn = s;
		}
		private void ProcessScrollUp(string param) {
			int d = ParseInt(param, 1);

			TerminalDocument doc = GetDocument();
			int caret_col = doc.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			//GLine nl = _manipulator.Export();
			doc.CleanLineRange(0, doc.TerminalWidth); // .ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetDocument().TerminalHeight-1);
			for(int i=0; i<d; i++) {
				doc.ScrollDown(doc.ScrollingTop, doc.ScrollingBottom); // TerminalDocument's "Scroll-Down" means XTerm's "Scroll-Up"
				doc.CurrentLineNumber = doc.TopLineNumber + offset; // find correct GLine
			}
			//_manipulator.Load(doc.CurrentLine, caret_col);
		}
		private void ProcessScrollDown(string param) {
			int d = ParseInt(param, 1);

			TerminalDocument doc = GetDocument();
			int caret_col = doc.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			//GLine nl = _manipulator.Export();
			doc.CleanLineRange(0, doc.TerminalWidth); // .ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetDocument().TerminalHeight-1);
			for(int i=0; i<d; i++) {
				doc.ScrollUp(doc.ScrollingTop, doc.ScrollingBottom); // TerminalDocument's "Scroll-Up" means XTerm's "Scroll-Down"
				doc.CurrentLineNumber = doc.TopLineNumber + offset; // find correct GLine
			}
			//_manipulator.Load(doc.CurrentLine, caret_col);
		}
		private void ProcessForwardTab(string param) {
			int n = ParseInt(param, 1);

            int t = GetDocument().CaretColumn;
			for(int i=0; i<n; i++)
				t = GetNextTabStop(t);
			if(t >= GetDocument().TerminalWidth) t = GetDocument().TerminalWidth-1;
            GetDocument().CaretColumn = t;
		}
		private void ProcessBackwardTab(string param) {
			int n = ParseInt(param, 1);

            int t = GetDocument().CaretColumn;
			for(int i=0; i<n; i++)
				t = GetPrevTabStop(t);
			if(t < 0) t = 0;
            GetDocument().CaretColumn = t;
		}
		private void ProcessTabClear(string param) {
			if(param=="0")
                SetTabStop(GetDocument().CaretColumn, false);
			else if(param=="3")
				ClearAllTabStop();
		}

		private void InitTabStops() {
			for(int i=0; i<_tabStops.Length; i++) {
				_tabStops[i] = (i % 8)==0;
			}
		}
		private void EnsureTabStops(int length) {
			if(length>=_tabStops.Length) {
				bool[] newarray = new bool[Math.Max(length, _tabStops.Length*2)];
				Array.Copy(_tabStops, 0, newarray, 0, _tabStops.Length);
				for(int i=_tabStops.Length; i<newarray.Length; i++) {
					newarray[i] = (i % 8)==0;
				}
				_tabStops = newarray;
			}
		}
		private void SetTabStop(int index, bool value) {
			EnsureTabStops(index+1);
			_tabStops[index] = value;
		}
		private void ClearAllTabStop() {
			for(int i=0; i<_tabStops.Length; i++) {
				_tabStops[i] = false;
			}
		}
		protected override int GetNextTabStop(int start) {
			EnsureTabStops(Math.Max(start+1, GetDocument().TerminalWidth));

			int index = start+1;
			while(index<GetDocument().TerminalWidth) {
				if(_tabStops[index]) return index;
				index++;
			}
			return GetDocument().TerminalWidth-1;
		}
		//‚±‚ê‚Ívt100‚É‚Í‚È‚¢‚Ì‚Åoverride‚µ‚È‚¢
		protected int GetPrevTabStop(int start) {
			EnsureTabStops(start+1);

			int index = start-1;
			while(index>0) {
				if(_tabStops[index]) return index;
				index--;
			}
			return 0;
		}

		protected void SwitchBuffer(bool toAlternate) {
			if (_isAlternateBuffer != toAlternate) {
				SaveScreen(toAlternate ? 0 : 1);
				RestoreScreen(toAlternate ? 1 : 0);
				_isAlternateBuffer = toAlternate;
			}
		}

		private void SaveScreen(int sw) {
			List<string> lines = new List<string>();
            int l = GetDocument().TopLineNumber;
            int m = GetDocument().TopLineNumber + GetDocument().TerminalHeight;
            while (l < m)
            {
				lines.Add(GetDocument().VirtualScreen.GetLine(l));
				l++;
			}
			_savedScreen[sw] = lines;
        }

		private void RestoreScreen(int sw) {
			if (_savedScreen[sw] == null) {
				ClearScreen();	// emulate new buffer
				return;
			}
			TerminalDocument doc = GetDocument();
			int w = doc.TerminalWidth;
			int m = doc.TerminalHeight;
            int t = doc.TopLineNumber;

            throw new InvalidOperationException("XTerm: RestoreScreen is not implemented!");
			/* foreach(string l in _savedScreen[sw]) {
				//l.ExpandBuffer(w);
                doc.Replace(t, l);  // doc.AddLine(l);
                t++;
				if(--m == 0) break;
			} */
		}

		protected void ClearScreen() {
			ProcessEraseInDisplay("2");
		}

		protected override void SaveCursor() {
			int sw = _isAlternateBuffer ? 1 : 0;
			_xtermSavedRow[sw] = GetDocument().CurrentLineNumber - GetDocument().TopLineNumber;
            _xtermSavedCol[sw] = GetDocument().CaretColumn;
		}

		protected override void RestoreCursor() {
			int sw = _isAlternateBuffer ? 1 : 0;
			//GLine nl = _manipulator.Export();
			GetDocument().CleanLineRange(0, GetDocument().TerminalWidth); // .ReplaceCurrentLine(nl);
			GetDocument().CurrentLineNumber = GetDocument().TopLineNumber + _xtermSavedRow[sw];
			//_manipulator.Load(GetDocument().CurrentLine, _xtermSavedCol[sw]);
		}

        //‰æ–Ê‚Ì”½“]
        private void SetReverseVideo(bool reverse) {
            if(reverse==_reverseVideo) return;

            _reverseVideo = reverse;
            //GetDocument().InvalidatedRegion.InvalidatedAll = true; //‘S‘ÌÄ•`‰æ‚ð‘£‚·
        }

		private ProcessCharResult SoftTerminalReset(string param){
			if(param=="!") {
				FullReset();
				return ProcessCharResult.Processed;
			}
			else
				return ProcessCharResult.Unsupported;
		}

		internal override byte[] SequenceKeyData(Keys modifier, Keys key) {
			if((int)Keys.F1 <= (int)key && (int)key <= (int)Keys.F12)
				return base.SequenceKeyData(modifier, key);
			else if(RuntimeUtil.IsCursorKey(key))
				return base.SequenceKeyData(modifier, key);
			else {
				byte[] r = new byte[4];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				r[3] = (byte)'~';
				//‚±‚Ì‚ ‚½‚è‚Íxterm‚Å‚ÍŠ„‚Æˆá‚¤‚æ‚¤‚¾
				if(key==Keys.Insert)
					r[2] = (byte)'2';
				else if(key==Keys.Home)
					r[2] = (byte)'7';
				else if(key==Keys.PageUp)
					r[2] = (byte)'5';
				else if(key==Keys.Delete)
					r[2] = (byte)'3';
				else if(key==Keys.End)
					r[2] = (byte)'8';
				else if(key==Keys.PageDown)
					r[2] = (byte)'6';
				else
					throw new ArgumentException("unknown key " + key.ToString());
				return r;
			}
		}

		public override void FullReset() {
			InitTabStops();
			base.FullReset();
		}

        //“®“I•ÏX—p
        private class CaptionChanger {
            private ITerminalSettings _settings;
            private string _title;
            public CaptionChanger(ITerminalSettings settings, string title) {
                _settings = settings;
                _title = title;
            }
            public void Do() {
                _settings.BeginUpdate();
                _settings.Caption = _title;
                _settings.EndUpdate();
            }
        }
	}
}
