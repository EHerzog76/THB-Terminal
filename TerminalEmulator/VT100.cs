/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: VT100.cs,v 1.6 2011/01/04 17:08:35 kzmi Exp $
 */
using System;
using System.IO;
using System.Text;
using System.Diagnostics;

//using Poderosa.Document;
using ConnectionParam;
using Core;

namespace TerminalEmulator
//namespace Poderosa.Terminal
{
	internal class VT100Terminal : EscapeSequenceTerminal {

		private int _savedRow;
		private int _savedCol;
		protected bool _insertMode;
		protected bool _scrollRegionRelative;
		protected bool _inverse;
		protected bool _bgColorHasbeenSet;

		//Ú‘±‚ÌŽí—Þ‚É‚æ‚Á‚ÄƒGƒXƒP[ƒvƒV[ƒPƒ“ƒX‚Ì‰ðŽß‚ð•Ï‚¦‚é•”•ª
		//protected bool _homePositionOnCSIJ2;

		public VT100Terminal(TerminalInitializeInfo info) : base(info) {
			_insertMode = false;
			_scrollRegionRelative = false;
			_inverse = false;
			_bgColorHasbeenSet = false;
			//bool sfu = _terminalSettings is SFUTerminalParam;
			//_homePositionOnCSIJ2 = sfu;
		}
		protected override void ResetInternal() {
			base.ResetInternal();
			_insertMode = false;
			_scrollRegionRelative = false;
		}


		protected override ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset) {
			string param;
			switch(code) {
				case '[':
					if(seq.Length-offset-1>=0) {
						param = new string(seq, offset, seq.Length-offset-1);
						return ProcessAfterCSI(param, seq[seq.Length-1]);
					}
					break;
					//throw new UnknownEscapeSequenceException(String.Format("unknown command after CSI {0}", code));
				case ']':
					if(seq.Length-offset-1>=0) {
						param = new string(seq, offset, seq.Length-offset-1);
						return ProcessAfterOSC(param, seq[seq.Length-1]);
					}
					break;
				case '=':
					ChangeMode(TerminalMode.Application);
					GetDocument().IsApplicationMode = true;
					_bgColorHasbeenSet = false;
					return ProcessCharResult.Processed;
				case '>':
					ChangeMode(TerminalMode.Normal);
					GetDocument().IsApplicationMode = false;
					return ProcessCharResult.Processed;
				case 'E':
					ProcessNextLine();
					return ProcessCharResult.Processed;
				case 'M': 
					ReverseIndex();
					return ProcessCharResult.Processed;
				case 'D': 
					Index();
					return ProcessCharResult.Processed;
				case '7':
					SaveCursor();
					return ProcessCharResult.Processed;
				case '8':
					RestoreCursor();
					return ProcessCharResult.Processed;
				case 'c':
					FullReset();
					return ProcessCharResult.Processed;
			}
			return ProcessCharResult.Unsupported;
		}

		protected virtual ProcessCharResult ProcessAfterCSI(string param, char code) {

			switch(code) {
				case 'c':
					ProcessDeviceAttributes(param);
					break;
				case 'm': //SGR
					ProcessSGR(param);
					break;
				case 'h':
				case 'l':
					return ProcessDECSETMulti(param, code);
				case 'r':
					if(param.Length>0 && param[0]=='?')
						return ProcessRestoreDECSET(param.Substring(1), code);
					else
						ProcessSetScrollingRegion(param);
					break;
				case 's':
					if (param.Length > 0 && param[0] == '?')
						return ProcessSaveDECSET(param.Substring(1), code);
					else
						return ProcessCharResult.Unsupported;
				case 'n':
					ProcessDeviceStatusReport(param);
					break;
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
					ProcessCursorMove(param, code);
					break;
				case 'H':
				case 'f': //f‚Í–{“–‚ÍxtermŒÅ—L
					ProcessCursorPosition(param);
					break;
				case 'J':
					if (!_bgColorHasbeenSet) {
						//GetDocument().BackColor = GetRenderProfile().BackColor;
						_bgColorHasbeenSet = true;
					}
					//ProcessEraseInDisplay(param);
					break;
				case 'K':
					ProcessEraseInLine(param);
					break;
				case 'L':
					ProcessInsertLines(param);
					break;
				case 'M':
					ProcessDeleteLines(param);
					break;
				default:
					return ProcessCharResult.Unsupported; 
			}

			return ProcessCharResult.Processed;
		}
		protected virtual ProcessCharResult ProcessAfterOSC(string param, char code) {
			return ProcessCharResult.Unsupported;
		}

		protected void ProcessSGR(string param) {
            string[] ps = param.Split(';');

            foreach(string cmd in ps) {
                
                //TextDecoration dec = (TextDecoration)_currentdecoration.Clone();
                int code = ParseSGRCode(cmd);
                if(code>=30 && code<=37) {
                    ////‚±‚ê‚¾‚ÆF‚ð•ÏX‚µ‚½‚Æ‚«Šù‚É‰æ–Ê‚É‚ ‚é‚à‚Ì‚Í˜A“®‚µ‚È‚­‚È‚é‚ªA‚»‚±‚ð‚¿‚á‚ñ‚Æ‚·‚é‚Ì‚Í¢“ï‚Å‚ ‚é
                    //dec.TextColor = GetRenderProfile().ESColorSet[code - 30];
                }
                else if(code>=40 && code<=47) {
                    //Color c = GetRenderProfile().ESColorSet[code - 40];
                    //if (_inverse && (dec.TextColor == DrawUtil.DarkColor(c))) {
                    //    dec.Inverse();
                    //}
                    //dec.BackColor = DrawUtil.DarkColor(c); //”wŒiF‚ÍˆÃ‚ß‚É
                    if (_inverse) {
                        //dec.Inverse();
                        _inverse = false;
                    }
                    //if (((GetDocument().BackColor != dec.BackColor) && GetDocument().IsApplicationMode) && !_bgColorHasbeenSet) {
                    //    GetDocument().BackColor = dec.BackColor;
                    //    _bgColorHasbeenSet = true;
                    //}
                }
                else {
                    switch(code) {
                        case 0:
                            //dec = TextDecoration.ClonedDefault();
                            break;
                        case 1:
                        case 5:
                            //dec.Bold = true;
                            break;
                        case 4:
                            //dec.Underline = true;
                            break;
                        case 7:
                            //dec.Inverse();
                            break;
                        case 2:
                            //dec = TextDecoration.ClonedDefault(); //•s–¾‚¾‚ªSGR 2‚ÅI‚í‚Á‚Ä‚¢‚é—á‚ª‚ ‚Á‚½
                            break;
                        case 22:
                        case 25:
                        case 27:
                        case 28:
                            //dec = TextDecoration.ClonedDefault();
                            break;
                        case 24:
                            //dec.Underline = false;
                            break;
                        case 39:
                            //dec.TextColor = Color.Empty;
                            break;
                        case 49:
                            //dec.BackColor = Color.Empty;
                            break;
                        case 10:
                        case 11:
                        case 12:
                            break; //'konsole'‚Æ‚¢‚¤‚â‚Â‚ç‚µ‚¢B–³Ž‹‚Å–â‘è‚È‚³‚»‚¤
                        default:
                            throw new UnknownEscapeSequenceException(String.Format("unknown SGR command {0}", param));
                    }
                }

                //_currentdecoration = dec;
                ////_manipulator.SetDecoration(dec);
            }
        }
		private static int ParseSGRCode(string param) {
			if(param.Length==0)
				return 0;
			else if(param.Length==1)
				return param[0]-'0';
			else if(param.Length==2)
				return (param[0]-'0')*10 + (param[1]-'0');
			else
				throw new UnknownEscapeSequenceException(String.Format("unknown SGR parameter {0}", param));
		}

		protected virtual void ProcessDeviceAttributes(string param) {
			byte[] data = Encoding.ASCII.GetBytes(" [?1;2c"); //‚È‚ñ‚©‚æ‚­‚í‚©‚ç‚È‚¢‚ªMindTerm“™‚ð‚Ý‚é‚Æ‚±‚ê‚Å‚¢‚¢‚ç‚µ‚¢
			data[0] = 0x1B; //ESC
			Transmit(data);
		}
		protected virtual void ProcessDeviceStatusReport(string param) {
			string response;
			if(param=="5")
				response = " [0n"; //‚±‚ê‚ÅOK‚ÌˆÓ–¡‚ç‚µ‚¢
            else if (param == "6")
            {
                //response = String.Format(" [{0};{1}R", GetDocument().CurrentLineNumber - GetDocument().TopLineNumber + 1, _manipulator.CaretColumn + 1);
                response = String.Format(" [{0};{1}R", GetDocument().CurrentLineNumber - GetDocument().TopLineNumber + 1, GetDocument().CaretColumn + 1);
            }
            else
                throw new UnknownEscapeSequenceException("DSR " + param);

			byte[] data = Encoding.ASCII.GetBytes(response);
			data[0] = 0x1B; //ESC
            Transmit(data);
		}

		protected void ProcessCursorMove(string param, char method) {
			int count = ParseInt(param, 1); //ƒpƒ‰ƒ[ƒ^‚ªÈ—ª‚³‚ê‚½‚Æ‚«‚ÌˆÚ“®—Ê‚Í‚P

            int column = GetDocument().CaretColumn;
			switch(method) {
				case 'A':
                    //Move Cursor UP
					//GetDocument().ReplaceCurrentLine(_manipulator.Export());
					//GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber - count);
					//_manipulator.Load(GetDocument().CurrentLine, column);
                    GetDocument().MoveCursorVertical(count * -1);
					break;
				case 'B':
                    //Move Cursor DOWN
					//GetDocument().ReplaceCurrentLine(_manipulator.Export());
					//GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber + count);
					//_manipulator.Load(GetDocument().CurrentLine, column);
                    GetDocument().MoveCursorVertical(count);
					break;
				case 'C': {
                    //Move Cursor Right
					//int newvalue = column + count;
                    int newvalue = count;
					//if(newvalue >= GetDocument().TerminalWidth) newvalue = GetDocument().TerminalWidth-1;
                    GetDocument().MoveCursor(newvalue);
					//_manipulator.ExpandBuffer(newvalue);
					//_manipulator.CaretColumn = newvalue;
				}
					break;
				case 'D': {
                    //Move Cursor Left
					//int newvalue = column - count;
                    int newvalue = count * -1;
					//if(newvalue < 0) newvalue = 0;
                    GetDocument().MoveCursor(newvalue);
					//_manipulator.CaretColumn = newvalue;
				}
					break;
			}
		}

		//CSI H
		protected void ProcessCursorPosition(string param) {
			IntPair t = ParseIntPair(param, 1, 1);
			int row = t.first, col = t.second;
			if(_scrollRegionRelative && GetDocument().ScrollingTop!=-1) {
				row += GetDocument().ScrollingTop;
			}

			if(row<1) row=1;
			else if(row>GetDocument().TerminalHeight) row = GetDocument().TerminalHeight;
			if(col<1) col=1;
			else if(col>GetDocument().TerminalWidth) col = GetDocument().TerminalWidth;
			ProcessCursorPosition(row, col);
		}
		protected void ProcessCursorPosition(int row, int col) {
			//GetDocument().ReplaceCurrentLine(_manipulator.Export());
			//GetDocument().CurrentLineNumber = (GetDocument().TopLineNumber + row - 1);
            GetDocument().MoveCursorTo(col, row);
			////int cc = GetDocument().CurrentLine.DisplayPosToCharPos(col-1);
			////Debug.Assert(cc>=0);
			//_manipulator.Load(GetDocument().CurrentLine, col-1);
		}

		//CSI J
		protected void ProcessEraseInDisplay(string param) {
			int d = ParseInt(param, 0);

			TerminalDocument doc = GetDocument();
            int col = doc.CaretColumn;
			switch(d) {
				case 0: //erase below
					doc.RemoveAfterCaret(); 
					//doc.RemoveAfter(doc.TopLineNumber+doc.TerminalHeight);
					doc.ClearAfter(doc.CurrentLineNumber+1);
					//_manipulator.Load(doc.CurrentLine, col);
					break;
				case 1: //erase above
					//_manipulator.FillSpace(0, _manipulator.CaretColumn);
					//doc.ReplaceCurrentLine(_manipulator.Export());
					doc.ClearRange(doc.TopLineNumber, doc.CurrentLineNumber);
					//_manipulator.Load(doc.CurrentLine, col);
					break;
				case 2: //erase all
					doc.ClearAfter(doc.TopLineNumber);
					break;
				default:
					throw new UnknownEscapeSequenceException(String.Format("unknown ED option {0}", param));
			}

		}

		//CSI K
		private void ProcessEraseInLine(string param) {
			int d = ParseInt(param, 0);

            TerminalDocument doc = GetDocument();
			switch(d) {
				case 0: //erase right
					doc.RemoveAfterCaret();
					break;
				case 1: //erase left
					doc.CleanLineRange(0, doc.CaretColumn);
					break;
				case 2: //erase all
					//doc.Clear(GetDocument().TerminalWidth);
                    doc.CleanLineRange(0, doc.TerminalWidth);
					break;
				default:
					throw new UnknownEscapeSequenceException(String.Format("unknown EL option {0}", param));
			}
		}

		protected virtual void SaveCursor() {
			_savedRow = GetDocument().CurrentLineNumber - GetDocument().TopLineNumber;
            _savedCol = GetDocument().CaretColumn;
		}
		protected virtual void RestoreCursor() {
			GetDocument().CurrentLineNumber = GetDocument().TopLineNumber + _savedRow;

            GetDocument().SetCursorPos(_savedCol, GetDocument().CurrentLineNumber);
		}

		protected void Index() {
			//GLine nl = _manipulator.Export();
			//GetDocument().ReplaceCurrentLine(nl);
			int current = GetDocument().CurrentLineNumber;
			if(current==GetDocument().TopLineNumber+GetDocument().TerminalHeight-1 || current==GetDocument().TopLineNumber+GetDocument().TerminalHeight)
				GetDocument().ScrollDown();
			else
				GetDocument().CurrentLineNumber = current+1;
			//_manipulator.Load(GetDocument().CurrentLine, _manipulator.CaretColumn);
		}
		protected void ReverseIndex() {
			//GLine nl = _manipulator.Export();
			//GetDocument().ReplaceCurrentLine(nl);
			int current = GetDocument().CurrentLineNumber;
			if(current==GetDocument().TopLineNumber || current==GetDocument().ScrollingTop)
				GetDocument().ScrollUp();
			else
				GetDocument().CurrentLineNumber = current-1;
			//_manipulator.Load(GetDocument().CurrentLine, _manipulator.CaretColumn);
		}

		protected void ProcessSetScrollingRegion(string param) {
			int height = GetDocument().TerminalHeight;
			IntPair v = ParseIntPair(param, 1, height);
			
			if(v.first<1) v.first = 1;
			else if(v.first>height) v.first = height;
			if(v.second<1) v.second = 1;
			else if(v.second>height) v.second = height;
			if(v.first>v.second) { //–â“š–³—p‚ÅƒGƒ‰[‚ª—Ç‚¢‚æ‚¤‚É‚àŽv‚¤‚ª
				int t = v.first;
				v.first = v.second;
				v.second = t;
			}

			//Žw’è‚Í1-origin‚¾‚ªˆ—‚Í0-origin
			GetDocument().SetScrollingRegion(v.first-1, v.second-1);
		}

		protected void ProcessNextLine() {
			//GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CurrentLineNumber = (GetDocument().CurrentLineNumber + 1);
			//_manipulator.Load(GetDocument().CurrentLine, 0);
		}

		protected override void ChangeMode(TerminalMode mode) {
			if(_terminalMode==mode) return;

			if(mode==TerminalMode.Normal) {
				GetDocument().ClearScrollingRegion();
				GetConnection().TerminalOutput.Resize(GetDocument().TerminalWidth, GetDocument().TerminalHeight); //‚½‚Æ‚¦‚Îemacs‹N“®’†‚ÉƒŠƒTƒCƒY‚µAƒVƒFƒ‹‚Ö–ß‚é‚ÆƒVƒFƒ‹‚ÍV‚µ‚¢ƒTƒCƒY‚ð”FŽ¯‚µ‚Ä‚¢‚È‚¢
				//RMBox‚ÅŠm”F‚³‚ê‚½‚±‚Æ‚¾‚ªA–³—p‚ÉŒã•û‚ÉƒhƒLƒ…ƒƒ“ƒg‚ðL‚°‚Ä‚­‚é“z‚ª‚¢‚éBƒJ[ƒ\ƒ‹‚ð123‰ñŒã•û‚ÖA‚È‚ÇB
				//ê“–‚½‚è“I‚¾‚ªAƒm[ƒ}ƒ‹ƒ‚[ƒh‚É–ß‚éÛ‚ÉŒã‚ë‚Ì‹ós‚ðíœ‚·‚é‚±‚Æ‚Å‘Î‰ž‚·‚éB
				int l = GetDocument().TopLineNumber + GetDocument().TerminalHeight; // .LastLine;
				while(l > GetDocument().CurrentLineNumber)
					l--;

				l++;
				GetDocument().ClearAfter(l); // .RemoveAfter(l);
			}
			else
				GetDocument().SetScrollingRegion(0, GetDocument().TerminalHeight-1);

			_terminalMode = mode;
		}

		private ProcessCharResult ProcessDECSETMulti(string param, char code) {
			if(param.Length==0) return ProcessCharResult.Processed;
			bool question = param[0]=='?';
			string[] ps = question? param.Substring(1).Split(';') : param.Split(';');
			bool unsupported = false;
			foreach(string p in ps) {
				ProcessCharResult r = question? ProcessDECSET(p, code) : ProcessSetMode(p, code);
				if(r==ProcessCharResult.Unsupported) unsupported = true;
			}
			return unsupported? ProcessCharResult.Unsupported : ProcessCharResult.Processed;
		}

		//CSI ? Pm h, CSI ? Pm l
		protected virtual ProcessCharResult ProcessDECSET(string param, char code) {
			//Debug.WriteLine(String.Format("DECSET {0} {1}", param, code));
			switch (param) {
				case "25":
					return ProcessCharResult.Processed; //!!Show/Hide Cursor‚¾‚ª‚Æ‚è‚ ‚¦‚¸–³Ž‹
				case "1":
					ChangeCursorKeyMode(code == 'h' ? TerminalMode.Application : TerminalMode.Normal);
					return ProcessCharResult.Processed;
				default:
					return ProcessCharResult.Unsupported;
			}
		}
		protected virtual ProcessCharResult ProcessSetMode(string param, char code) {
            bool set = code=='h';
			switch (param) {
				case "4":
					_insertMode = set; //h‚ÅŽn‚Ü‚Á‚Äl‚ÅI‚í‚é
					return ProcessCharResult.Processed;
				case "12":	//local echo
					_afterExitLockActions.Add(new AfterExitLockDelegate(new LocalEchoChanger(GetTerminalSettings(), !set).Do));
					return ProcessCharResult.Processed;
				case "20":
					return ProcessCharResult.Processed; //!!WinXP‚ÌTelnet‚ÅŠm”F‚µ‚½
				case "25":
					return ProcessCharResult.Processed;
				case "34":	//MakeCursorBig, putty‚É‚Í‚ ‚é
					//!set‚ÅƒJ[ƒ\ƒ‹‚ð‹­§“I‚É” Œ^‚É‚µAset‚Å’Êí‚É–ß‚·‚Æ‚¢‚¤‚Ì‚ª³‚µ‚¢“®ì‚¾‚ªŽÀŠQ‚Í‚È‚¢‚Ì‚Å–³Ž‹
					return ProcessCharResult.Processed;
				default:
					return ProcessCharResult.Unsupported;
			}
		}

		//‚±‚ê‚Í‚³‚Ú‚èB‚¿‚á‚ñ‚Æ•Û‘¶‚µ‚È‚¢‚Æ‚¢‚¯‚È‚¢ó‘Ô‚Í‚Ù‚Æ‚ñ‚Ç‚È‚¢‚Ì‚Å
		protected virtual ProcessCharResult ProcessSaveDECSET(string param, char code) {
			//‚±‚Ìparam‚Í•¡”ŒÂƒpƒ‰ƒ[ƒ^
			return ProcessCharResult.Processed;
		}
		protected virtual ProcessCharResult ProcessRestoreDECSET(string param, char code) {
			//‚±‚Ìparam‚Í•¡”ŒÂƒpƒ‰ƒ[ƒ^
			return ProcessCharResult.Processed;
		}

		//‚±‚ê‚ð‘—‚Á‚Ä‚­‚éƒAƒvƒŠƒP[ƒVƒ‡ƒ“‚Í vi‚Åã•ûƒXƒNƒ[ƒ‹
		protected void ProcessInsertLines(string param) {
			int d = ParseInt(param, 1);

			TerminalDocument doc = GetDocument();
			int caret_pos = doc.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			//GLine nl = _manipulator.Export();
			//doc.ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, GetDocument().TerminalHeight-1);

			for(int i=0; i<d; i++) {
				doc.ScrollUp(/* doc.CurrentLineNumber, doc.ScrollingBottom */);
				doc.CurrentLineNumber = doc.TopLineNumber + offset;
			}
			//_manipulator.Load(doc.CurrentLine, caret_pos);
		}

		//‚±‚ê‚ð‘—‚Á‚Ä‚­‚éƒAƒvƒŠƒP[ƒVƒ‡ƒ“‚Í vi‚Å‰º•ûƒXƒNƒ[ƒ‹
		protected void ProcessDeleteLines(string param) {
			int d = ParseInt(param, 1);

			/*
			TerminalDocument doc = GetDocument();
			_manipulator.Clear(GetConnection().TerminalWidth);
			GLine target = doc.CurrentLine;
			for(int i=0; i<d; i++) {
				target.Clear();
				target = target.NextLine;
			}
			*/

			TerminalDocument doc = GetDocument();
			int caret_col = doc.CaretColumn;
			int offset = doc.CurrentLineNumber - doc.TopLineNumber;
			//GLine nl = _manipulator.Export();
			//doc.ReplaceCurrentLine(nl);
			if(doc.ScrollingBottom==-1)
				doc.SetScrollingRegion(0, doc.TerminalHeight-1);

			for(int i=0; i<d; i++) {
				doc.ScrollDown(/* doc.CurrentLineNumber, doc.ScrollingBottom */);
				doc.CurrentLineNumber = doc.TopLineNumber + offset;
			}
			//_manipulator.Load(doc.CurrentLine, caret_col);
		}



		private static string[] FUNCTIONKEY_MAP = { 
		//     F1    F2    F3    F4    F5    F6    F7    F8    F9    F10   F11  F12
			  "11", "12", "13", "14", "15", "17", "18", "19", "20", "21", "23", "24",
	    //     F13   F14   F15   F16   F17  F18   F19   F20   F21   F22
              "25", "26", "28", "29", "31", "32", "33", "34", "23", "24" };
		//“Á’è‚Ìƒf[ƒ^‚ð—¬‚·ƒ^ƒCƒvBŒ»ÝAƒJ[ƒ\ƒ‹ƒL[‚Æƒtƒ@ƒ“ƒNƒVƒ‡ƒ“ƒL[‚ªŠY“–‚·‚é         
		internal override byte[] SequenceKeyData(Keys modifier, Keys body) {
			if((int)Keys.F1 <= (int)body && (int)body <= (int)Keys.F12) {
				byte[] r = new byte[5];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				int n = (int)body - (int)Keys.F1;
				if((modifier & Keys.Shift) != Keys.None) n += 10; //shift‚Í’l‚ð10‚¸‚ç‚·
				char tail;
				if(n>=20)
					tail = (modifier & Keys.Control) != Keys.None? '@' : '$';
				else
					tail = (modifier & Keys.Control) != Keys.None? '^' : '~';
				string f = FUNCTIONKEY_MAP[n];
				r[2] = (byte)f[0];
				r[3] = (byte)f[1];
				r[4] = (byte)tail;
				return r;
			}
			else if(RuntimeUtil.IsCursorKey(body)) {
				byte[] r = new byte[3];
				r[0] = 0x1B;
				if(_cursorKeyMode==TerminalMode.Normal)
					r[1] = (byte)'[';
				else
					r[1] = (byte)'O';

				switch(body) {
					case Keys.Up:
						r[2] = (byte)'A';
						break;
					case Keys.Down:
						r[2] = (byte)'B';
						break;
					case Keys.Right:
						r[2] = (byte)'C';
						break;
					case Keys.Left:
						r[2] = (byte)'D';
						break;
					default:
						throw new ArgumentException("unknown cursor key code", "key");
				}
				return r;
			}
			else {
				byte[] r = new byte[4];
				r[0] = 0x1B;
				r[1] = (byte)'[';
				r[3] = (byte)'~';
				if(body==Keys.Insert)
					r[2] = (byte)'1';
				else if(body==Keys.Home)
					r[2] = (byte)'2';
				else if(body==Keys.PageUp)
					r[2] = (byte)'3';
				else if(body==Keys.Delete)
					r[2] = (byte)'4';
				else if(body==Keys.End)
					r[2] = (byte)'5';
				else if(body==Keys.PageDown)
					r[2] = (byte)'6';
				else
					throw new ArgumentException("unknown key " + body.ToString());
				return r;
			}
		}

        private class LocalEchoChanger {
            private ITerminalSettings _settings;
            private bool _value;
            public LocalEchoChanger(ITerminalSettings settings, bool value) {
                _settings = settings;
                _value = value;
            }
            public void Do() {
                _settings.BeginUpdate();
                _settings.LocalEcho = _value;
                _settings.EndUpdate();
            }
        }
	}
}
