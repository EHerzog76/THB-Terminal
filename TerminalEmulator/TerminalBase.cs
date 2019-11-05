/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalBase.cs,v 1.5 2010/12/29 16:24:57 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Core;
//using TerminalSessions;
//using Poderosa.Document;
using ConnectionParam;
using Protocols;
//using Poderosa.Forms;
//using Poderosa.View;

//namespace Poderosa.Terminal
namespace TerminalEmulator
{
    //TODO –¼‘O‚Æ‚Í— • ‚É‚ ‚ñ‚ÜAbstract‚¶‚á‚Ë[‚È ‚Ü‚½ƒtƒB[ƒ‹ƒh‚ª‘½‚·‚¬‚é‚Ì‚Å®—‚·‚éB
    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^‚Ì’†•‚Æ‚È‚éƒNƒ‰ƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Class that becomes core of terminal emulator.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒNƒ‰ƒX‚Ì‰ðà‚ÍA‚Ü‚¾‚ ‚è‚Ü‚¹‚ñB
    /// </ja>
    /// <en>
    /// This class has not explained yet. 
    /// </en>
    /// </remarks>
	public abstract class AbstractTerminal : ICharProcessor, IByteAsyncInputStream {
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public delegate void AfterExitLockDelegate();

		private ICharDecoder     _decoder;
		private TerminalDocument _document;
		private IAbstractTerminalHost  _session;
        private LogService _logService;
        //private IModalTerminalTask _modalTerminalTask;
        //private PromptRecognizer _promptRecognizer;

        private bool _cleanup = false;

        protected List<AfterExitLockDelegate> _afterExitLockActions;
		protected TerminalMode _terminalMode;
		protected TerminalMode _cursorKeyMode; //_terminalMode‚Í•Ê•¨BAIX‚Å‚Ìvi‚ÅAƒJ[ƒ\ƒ‹ƒL[‚Í•s•Ï‚Æ‚¢‚¤—á‚ªŠm”F‚³‚ê‚Ä‚¢‚é

		protected abstract void ChangeMode(TerminalMode tm);
		protected abstract void ResetInternal();

        protected ProcessCharResult _processCharResult;

        //ICharDecoder
		public abstract void ProcessChar(char ch);

		internal abstract byte[] SequenceKeyData(Keys modifier, Keys body);

		public AbstractTerminal(TerminalInitializeInfo info) {
            //TerminalEmulatorPlugin.Instance.LaterInitialize();
			_session = info.Session;
            
            // //_invalidateParam = new InvalidateParam();
			_document = new TerminalDocument(info.InitialWidth, info.InitialHeight);
            //_document.SetOwner(_session.ISession);
            _afterExitLockActions = new List<AfterExitLockDelegate>();

			_decoder = new ISO2022CharDecoder(this, EncodingProfile.Get(info.Session.TerminalSettings.Encoding));
            _terminalMode = TerminalMode.Normal;
            _logService = new LogService(info.TerminalParameter, _session.TerminalSettings, _session.TerminalOptions);
            //_promptRecognizer = new PromptRecognizer(this);

            //if (info.Session.TerminalSettings.LogSettings != null)
            //{
            //    _logService.ApplyLogSettings(_session.TerminalSettings.LogSettings, false);
            //}

            //event handlers
            ITerminalSettings ts = info.Session.TerminalSettings;
            ts.ChangeEncoding += delegate(EncodingType t) { this.Reset(); };
            _document.DebugFlag = ts.DebugFlag;

            _document.LineFeedRule = GetTerminalSettings().LineFeedRule; //(Telnet.LineFeedRule)
            //ToDo: Set Encoding
            //_document.Encoding = Encoding.GetEncoding("iso-2022-jp")
        }

        //XTERM‚ð•\‚Éo‚³‚È‚¢‚½‚ß‚Ìƒƒ\ƒbƒh
        public static AbstractTerminal Create(TerminalInitializeInfo info) {
            return new XTerm(info);
        }

        public TerminalDocument GetDocument() {
            return _document;
        }
        protected ITerminalSettings GetTerminalSettings() {
            return _session.TerminalSettings;
        }
        protected ITerminalConnection GetConnection() {
            return _session.TerminalConnection;
        }
        protected IAbstractTerminalHost GetTerminalSession() {
            return _session;
        }

        public TerminalMode TerminalMode {
		    get {
			    return _terminalMode;
		    }
		}
		public TerminalMode CursorKeyMode {
			get {
				return _cursorKeyMode;
			}
		}

        public ILogService ILogService {
            get {
                return _logService;
            }
        }
        public LogService LogService {
            get {
                return _logService;
            }
        }

        /* internal PromptRecognizer PromptRecognizer {
            get {
                return _promptRecognizer;
            }
        } */
        public IAbstractTerminalHost TerminalHost {
            get {
                return _session;
            }
        }

        public void CloseBySession() {
            CleanupCommon();
        }

		protected virtual void ChangeCursorKeyMode(TerminalMode tm) {
			_cursorKeyMode = tm;
		}

        #region ICharProcessor
        ProcessCharResult ICharProcessor.State {
            get {
                return _processCharResult;
            }
        }
        public void UnsupportedCharSetDetected(char code) {
			string desc;
			if(code=='0')
				desc = "0 (DEC Special Character)"; //‚±‚ê‚Í‚æ‚­‚ ‚é‚Ì‚Å’A‚µ‘‚«‚Â‚«
			else
				desc = new String(code, 1);

			CharDecodeError(String.Format("Message.AbstractTerminal.UnsupportedCharSet", desc));
		}
		public void InvalidCharDetected(byte[] buf) {
			CharDecodeError(String.Format("Message.AbstractTerminal.UnexpectedChar", EncodingProfile.Get(GetTerminalSettings().Encoding).Encoding.WebName));
        }
        #endregion

        //ŽóM‘¤‚©‚ç‚ÌŠÈˆÕŒÄ‚Ño‚µ
        protected void Transmit(byte[] data) {
            _session.TerminalConnection.Socket.Transmit(new ByteDataFragment(data, 0, data.Length));
        }

        //•¶ŽšŒn‚ÌƒGƒ‰[’Ê’m
        protected void CharDecodeError(string msg) {
            /*
            IPoderosaMainWindow window = _session.OwnerWindow;
            if(window==null) return;
            Debug.Assert(window.AsForm().InvokeRequired);

            Monitor.Exit(GetDocument()); //‚±‚ê‚Í–Y‚ê‚é‚È
            switch(GEnv.Options.CharDecodeErrorBehavior) {
                case WarningOption.StatusBar:
                    window.StatusBar.SetMainText(msg);
                    break;
                case WarningOption.MessageBox:
                    window.AsForm().Invoke(new CharDecodeErrorDialogDelegate(CharDecodeErrorDialog), window, msg);
                    break;
            }
            Monitor.Enter(GetDocument());
            */
            Console.WriteLine("CharDecodeError: " + msg);
        }
        private delegate void CharDecodeErrorDialogDelegate(string msg);
        private void CharDecodeErrorDialog(string msg) {
            Console.WriteLine("CharDecodeError: " + msg);
        }

        public void Reset() {
			//Encoding‚ª“¯‚¶Žž‚ÍŠÈ’P‚ÉÏ‚Ü‚¹‚é‚±‚Æ‚ª‚Å‚«‚é
			if(_decoder.CurrentEncoding.Type==GetTerminalSettings().Encoding)
				_decoder.Reset(_decoder.CurrentEncoding);
			else
				_decoder = new ISO2022CharDecoder(this, EncodingProfile.Get(GetTerminalSettings().Encoding));
		}

        //‚±‚ê‚ÍƒƒCƒ“ƒXƒŒƒbƒh‚©‚çŒÄ‚Ño‚·‚±‚Æ
        public virtual void FullReset() {
            lock(_document) {
                ChangeMode(TerminalMode.Normal);
                _document.ClearScrollingRegion();
                ResetInternal();
				_decoder = new ISO2022CharDecoder(this, EncodingProfile.Get(GetTerminalSettings().Encoding));
            }
        }
/*
        //ModalTerminalTaskŽü•Ó
        public virtual void StartModalTerminalTask(IModalTerminalTask task) {
            _modalTerminalTask = task;
            new ModalTerminalTaskSite(this).Start(task);
        }
        public virtual void EndModalTerminalTask() {
            _modalTerminalTask = null;
        }
        public IModalTerminalTask CurrentModalTerminalTask {
            get {
                return _modalTerminalTask;
            }
        }
*/
        #region IByteAsyncInputStream
        public void OnReception(ByteDataFragment data) {
			try {
                bool pass_to_terminal = true;
                //if(_modalTerminalTask!=null) {
                //    bool show_input = _modalTerminalTask.ShowInputInTerminal;
                //    _modalTerminalTask.OnReception(data);
                //    if(!show_input) pass_to_terminal = false; //“ü—Í‚ðŒ©‚¹‚È‚¢(XMODEM‚Æ‚©)‚Ì‚Æ‚«‚Íƒ^[ƒ~ƒiƒ‹‚É—^‚¦‚È‚¢
                //}

                //ƒoƒCƒiƒŠƒƒO‚Ìo—Í
                _logService.BinaryLogger.Write(data);

                if(pass_to_terminal) {
                    TerminalDocument document = _document;
                    if (document != null)
                    {
                        lock (document)
                        {
                            //_invalidateParam.Reset();
                            //‚±‚±‚©‚ç‹ŒInput()
                            //_manipulator.Load(GetDocument().CurrentLine, 0);
                            //_manipulator.CaretColumn = GetDocument().CaretColumn;

                            //ˆ—–{‘Ì
                            _decoder.OnReception(data);

                            //GetDocument().ReplaceCurrentLine(_manipulator.Export());
                            //GetDocument().CaretColumn = _manipulator.CaretColumn;
                            //‚±‚±‚Ü‚Å

                            //‰E’[‚ÉƒLƒƒƒŒƒbƒg‚ª—ˆ‚½‚Æ‚«‚Í•Ö‹X“I‚ÉŽŸs‚Ì“ª‚É‚à‚Á‚Ä‚¢‚­
                            //if(document.CaretColumn==document.TerminalWidth) {
                            //    document.CurrentLineNumber++; //‚±‚ê‚É‚æ‚Á‚ÄŽŸs‚Ì‘¶Ý‚ð•ÛØ
                            //    document.CaretColumn = 0;
                            //}

                            CheckDiscardDocument();
                            //AdjustTransientScrollBar();

                            //Œ»Ýs‚ª‰º’[‚ÉŒ©‚¦‚é‚æ‚¤‚ÈScrollBarValue‚ðŒvŽZ
                            //int n = document.CurrentLineNumber-document.TerminalHeight+1-document.FirstLineNumber;
                            //if(n < 0) n = 0;

                            //Debug.WriteLine(String.Format("E={0} C={1} T={2} H={3} LC={4} MAX={5} n={6}", _transientScrollBarEnabled, _tag.Document.CurrentLineNumber, _tag.Document.TopLineNumber, _tag.Connection.TerminalHeight, _transientScrollBarLargeChange, _transientScrollBarMaximum, n));
                            /* if(IsAutoScrollMode(n)) {
                                _scrollBarValues.Value = n;
                                document.TopLineNumber = n + document.FirstLineNumber;
                            }
                            else
                                _scrollBarValues.Value = document.TopLineNumber - document.FirstLineNumber;
                            */
                            //Invalidate‚ðlock‚ÌŠO‚Éo‚·B‚±‚Ì‚Ù‚¤‚ªˆÀ‘S‚ÆŽv‚í‚ê‚½

                            //ŽóMƒXƒŒƒbƒh“à‚Å‚Íƒ}[ƒN‚ð‚Â‚¯‚é‚Ì‚ÝBƒ^ƒCƒ}[‚Ås‚¤‚Ì‚ÍIntelliSense‚É•›ì—p‚ ‚é‚Ì‚ÅˆêŽž’âŽ~
                            //_promptRecognizer.SetContentUpdateMark();
                            //_promptRecognizer.Recognize(); 
                        }
                    }

                    if(_afterExitLockActions.Count > 0) {
                        //Control main = _session.OwnerWindow.AsControl();
                        //foreach(AfterExitLockDelegate action in _afterExitLockActions) {
                        //    main.Invoke(action);
                        //}
                        _afterExitLockActions.Clear();
                    }
                }

                //if(_modalTerminalTask!=null) _modalTerminalTask.NotifyEndOfPacket();
                _session.NotifyViewsDataArrived();
			}
			catch(Exception ex) {
                RuntimeUtil.ReportException(ex);
			}
        }

        public void OnAbnormalTermination(string msg) {
            //TODO ƒƒbƒZ[ƒW‚ð GEnv.Strings.GetString("Message.TerminalDataReceiver.GenericError"),_tag.Connection.Param.ShortDescription, msg
            if(!GetConnection().IsClosed) { //•Â‚¶‚éŽw—ß‚ðo‚µ‚½Œã‚ÌƒGƒ‰[‚Í•\Ž¦‚µ‚È‚¢
                GetConnection().Close();
                ShowAbnormalTerminationMessage();
            }
            Cleanup(msg);
        }
        private void ShowAbnormalTerminationMessage() {
                //ITCPParameter tcp = (ITCPParameter)GetConnection().Destination.GetAdapter(typeof(ITCPParameter));
                //if(tcp!=null) {
                //    string msg = String.Format("Message.AbstractTerminal.TCPDisconnected", tcp.Destination);

                //    switch(GEnv.Options.DisconnectNotification) {
                //        case WarningOption.StatusBar:
                //            Console.WriteLine(msg);
                //            break;
                //        case WarningOption.MessageBox:
                //            Console.WriteLine(msg); //TODO DisableƒIƒvƒVƒ‡ƒ“‚Â‚«‚ÌƒTƒ|[ƒg
                //            break;
                //    }
                //}
        }

        public void OnNormalTermination() {
            Cleanup(null);
        }
        #endregion

        private void Cleanup(string msg) {
            CleanupCommon();
            //NOTE _session.CloseByReceptionThread()‚ÍA‚»‚Ì‚Ü‚ÜƒAƒvƒŠI—¹‚Æ’¼Œ‹‚·‚éê‡‚ª‚ ‚éB‚·‚é‚ÆA_logService.Close()‚Ìˆ—‚ªI‚í‚ç‚È‚¢‚¤‚¿‚É‹­§I—¹‚É‚È‚Á‚ÄƒƒO‚ª‘‚«‚«‚ê‚È‚¢‰Â”\«‚ª‚ ‚é
            _session.CloseByReceptionThread(msg);
        }

        private void CleanupCommon() {
            if (!_cleanup) {
                _cleanup = true;
                _logService.Close(_document.CurrentLine);
                _document = null;
                _decoder = null;
            }
        }

		private void CheckDiscardDocument() {
			if(_session==null || _terminalMode==TerminalMode.Application) return;

            /*
			TerminalDocument document = _document;
			int del = document.DiscardOldLines(GEnv.Options.TerminalBufferSize+document.TerminalHeight);
			if(del > 0) {
				int newvalue = _scrollBarValues.Value - del;
				if(newvalue<0) newvalue=0;
				_scrollBarValues.Value = newvalue;
				document.InvalidatedRegion.InvalidatedAll = true; //–{“–‚Í‚±‚±‚Ü‚Å‚µ‚È‚­‚Ä‚à—Ç‚³‚»‚¤‚¾‚ª”O‚Ì‚½‚ß
			}
            */
		}

        //ƒhƒLƒ…ƒƒ“ƒgƒƒbƒN’†‚Å‚È‚¢‚ÆŒÄ‚ñ‚Å‚Í‚¾‚ß
		public void IndicateBell() {
 			//if(GEnv.Options.BeepOnBellChar) Win32.MessageBeep(-1);
		}
	}
	
	//Escape Sequence‚ðŽg‚¤ƒ^[ƒ~ƒiƒ‹
	public abstract class EscapeSequenceTerminal : AbstractTerminal {
        private StringBuilder _escapeSequence;
        //private IModalCharacterTask _currentCharacterTask;

        public EscapeSequenceTerminal(TerminalInitializeInfo info) : base(info) {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		protected override void ResetInternal() {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		public override void ProcessChar(char ch) {
			if(_processCharResult != ProcessCharResult.Escaping) {
				if(ch==0x1B) {
					_processCharResult = ProcessCharResult.Escaping;
				} else {
                    //if(_currentCharacterTask!=null) { //ƒ}ƒNƒ‚È‚ÇAchar‚ðŽæ‚éƒ^ƒCƒv
                    //    _currentCharacterTask.ProcessChar(ch);
                    //}

                    this.LogService.XmlLogger.Write(ch);

					if(ch < 0x20 || (ch>=0x80 && ch<0xA0))
						_processCharResult = ProcessControlChar(ch);
					else
						_processCharResult = ProcessNormalChar(ch);
				}
			}
			else {
				if(ch=='\0') return; //ƒV[ƒPƒ“ƒX’†‚ÉNULL•¶Žš‚ª“ü‚Á‚Ä‚¢‚éƒP[ƒX‚ªŠm”F‚³‚ê‚½ ‚È‚¨¡‚ÍXmlLogger‚É‚à‚±‚Ìƒf[ƒ^‚Ís‚©‚È‚¢B
				_escapeSequence.Append(ch);
				bool end_flag = false; //escape sequence‚ÌI‚í‚è‚©‚Ç‚¤‚©‚ðŽ¦‚·ƒtƒ‰ƒO
				if(_escapeSequence.Length==1) { //ESC+‚P•¶Žš‚Å‚ ‚éê‡
					end_flag = ('0'<=ch && ch<='9') || ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='>' || ch=='=' || ch=='|' || ch=='}' || ch=='~';
				}
				else if(_escapeSequence[0]==']') { //OSC‚ÌI’[‚ÍBEL‚©ST(String Terminator)
					end_flag = ch==0x07 || ch==0x9c; 
				}
				else if (this._escapeSequence[0] == '@') {
					end_flag = (ch == '0') || (ch == '1');
				}
				else {
					end_flag = ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='@' || ch=='~' || ch=='|' || ch=='{';
				}
				
				if(end_flag) { //ƒV[ƒPƒ“ƒX‚Ì‚¨‚í‚è
					char[] seq = _escapeSequence.ToString().ToCharArray();

                    this.LogService.XmlLogger.EscapeSequence(seq);
					
                    try {
						char code = seq[0];
						_processCharResult = ProcessCharResult.Unsupported; //ProcessEscapeSequence‚Å—áŠO‚ª—ˆ‚½Œã‚Åó‘Ô‚ªEscaping‚Í‚Ð‚Ç‚¢Œ‹‰Ê‚ðµ‚­‚Ì‚Å
						_processCharResult = ProcessEscapeSequence(code, seq, 1);
						if(_processCharResult==ProcessCharResult.Unsupported)
							throw new UnknownEscapeSequenceException(String.Format("ESC {0}", new string(seq)));
					}
					catch(UnknownEscapeSequenceException ex) {
    					CharDecodeError("Message.EscapesequenceTerminal.UnsupportedSequence"+ex.Message);
                        RuntimeUtil.SilentReportException(ex);
                    }
					finally {
						_escapeSequence.Remove(0, _escapeSequence.Length);
					}
				}
				else
					_processCharResult = ProcessCharResult.Escaping;
			}
		}

		protected virtual ProcessCharResult ProcessControlChar(char ch) {
			if(ch=='\n' || ch==0xB) { //Vertical Tab‚ÍLF‚Æ“™‚µ‚¢
				LineFeedRule rule = GetTerminalSettings().LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.LFOnly) {
					if(rule==LineFeedRule.LFOnly) //LF‚Ì‚Ý‚Ì“®ì‚Å‚ ‚é‚Æ‚«
						DoCarriageReturn();
					DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch=='\r') {
				LineFeedRule rule = GetTerminalSettings().LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.CROnly) {
					DoCarriageReturn();
					if(rule==LineFeedRule.CROnly)
						DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch==0x07) {
				this.IndicateBell();
				return ProcessCharResult.Processed;
			}
			else if(ch==0x08) { // Backspace
				//s“ª‚ÅA’¼‘Os‚Ì––”ö‚ªŒp‘±‚Å‚ ‚Á‚½ê‡s‚ð–ß‚·
                GetDocument().PutChar(ch);
				return ProcessCharResult.Processed;
			}
			else if(ch==0x09) { //Tabulator
				//_manipulator.CaretColumn = GetNextTabStop(_manipulator.CaretColumn);
                GetDocument().TabStop();
				return ProcessCharResult.Processed;
			}
			else if(ch==0x0E) {
				return ProcessCharResult.Processed; //ˆÈ‰º‚Q‚Â‚ÍCharDecoder‚Ì’†‚Åˆ—‚³‚ê‚Ä‚¢‚é‚Í‚¸‚È‚Ì‚Å–³Ž‹
			}
			else if(ch==0x0F) {
				return ProcessCharResult.Processed;
			}
			else if(ch==0x00) {
				return ProcessCharResult.Processed; //null char‚Í–³Ž‹ !!CR NUL‚ðCR LF‚Æ‚Ý‚È‚·Žd—l‚ª‚ ‚é‚ªACR LF CR NUL‚Æ‚­‚é‚±‚Æ‚à‚ ‚Á‚Ä“ï‚µ‚¢
			}
			else {
				Debug.WriteLine("Unknown char " + (int)ch);
				//“K“–‚ÈƒOƒ‰ƒtƒBƒbƒN•\Ž¦‚Ù‚µ‚¢
				return ProcessCharResult.Unsupported;
			}
		}
		private void DoLineFeed() {
			//nl.EOLType = (nl.EOLType==EOLType.CR || nl.EOLType==EOLType.CRLF)? EOLType.CRLF : EOLType.LF;
            this.LogService.TextLogger.WriteLine("", true); //ƒƒO‚És‚ðcommit
			GetDocument().LineFeed();
				
			//ƒJƒ‰ƒ€•ÛŽ‚Í•K—vBƒTƒ“ƒvƒ‹:linuxconf.log
			//int col = _manipulator.CaretColumn;
			//_manipulator.Load(GetDocument().CurrentLine, col);
		}
		private void DoCarriageReturn() {
            GetDocument().CarriageReturn();
		}

		protected virtual int GetNextTabStop(int start) {
			int t = start;
			//t‚æ‚è‚ÅÅ¬‚Ì‚W‚Ì”{”‚Ö‚à‚Á‚Ä‚¢‚­
			t += (8 - t % 8);
			if(t >= GetDocument().TerminalWidth) t = GetDocument().TerminalWidth-1;
			return t;
		}
		
		protected virtual ProcessCharResult ProcessNormalChar(char ch) {
			//int tw = GetDocument().TerminalWidth;
            //if (GetDocument().CaretColumn + 1 > tw)
            //{
                //this.LogService.TextLogger.WriteLine("", true); //ƒƒO‚És‚ðcommit
                ////GetDocument().ReplaceCurrentLine(l);
				//GetDocument().LineFeed();
				////_manipulator.Load(GetDocument().CurrentLine, 0);
			//}
            this.LogService.TextLogger.WriteLine(ch.ToString(), false);
            GetDocument().PutChar(ch);
			
			return ProcessCharResult.Processed;
		}

		protected abstract ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset);

		//FormatException‚Ì‚Ù‚©‚ÉOverflowException‚Ì‰Â”\«‚à‚ ‚é‚Ì‚Å
		protected static int ParseInt(string param, int default_value) {
			try {
				if(param.Length>0)
					return Int32.Parse(param);
				else
					return default_value;
			}
			catch(Exception ex) {
				throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", param, ex.Message));
			}
		}

		protected static IntPair ParseIntPair(string param, int default_first, int default_second) {
			IntPair ret = new IntPair(default_first, default_second);

			string[] s = param.Split(';');
			
			if(s.Length >= 1 && s[0].Length>0) {
				try {
					ret.first = Int32.Parse(s[0]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[0], ex.Message));
				}
			}

			if(s.Length >= 2 && s[1].Length>0) {
				try {
					ret.second = Int32.Parse(s[1]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[1], ex.Message));
				}
			}

			return ret;
		}

        /*
        //ModalTask‚ÌƒZƒbƒg‚ðŒ©‚é
        public override void StartModalTerminalTask(IModalTerminalTask task) {
            base.StartModalTerminalTask(task);
            _currentCharacterTask = (IModalCharacterTask)task.GetAdapter(typeof(IModalCharacterTask));
        }
        public override void EndModalTerminalTask() {
            base.EndModalTerminalTask();
            _currentCharacterTask = null;
        }
        */
	}

    //ŽóMƒXƒŒƒbƒh‚©‚çŽŸ‚ÉÝ’è‚·‚×‚«ScrollBar‚Ì’l‚ð”z’u‚·‚éB
    internal class ScrollBarValues {
		//ŽóMƒXƒŒƒbƒh‚Å‚±‚ê‚ç‚Ì’l‚ðÝ’è‚µAŽŸ‚ÌOnPaint“™ƒƒCƒ“ƒXƒŒƒbƒh‚Å‚ÌŽÀs‚ÅCommit‚·‚é
		private bool _dirty; //‚±‚ê‚ª—§‚Á‚Ä‚¢‚é‚Æ—vÝ’è
		private bool _enabled;
		private int  _value;
		private int  _largeChange;
		private int  _maximum;

        public bool Dirty {
            get {
                return _dirty;
            }
            set {
                _dirty = value;
            }
        }
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;
            }
        }
        public int Value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        }
        public int LargeChange {
            get {
                return _largeChange;
            }
            set {
                _largeChange= value;
            }
        }
        public int Maximum {
            get {
                return _maximum;
            }
            set {
                _maximum = value;
            }
        }
    }

    public interface ICharProcessor {
        void ProcessChar(char ch);
        ProcessCharResult State { get; }
        void UnsupportedCharSetDetected(char code);
        void InvalidCharDetected(byte[] data);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public enum ProcessCharResult {
        Processed,
        Unsupported,
        Escaping
    }



    public class UnknownEscapeSequenceException : Exception {
        public UnknownEscapeSequenceException(string msg) : base(msg) { }
    }

    public struct IntPair {
        public int first;
        public int second;

        public IntPair(int f, int s) {
            first = f;
            second = s;
        }
    }
}
