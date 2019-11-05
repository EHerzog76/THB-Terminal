/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Util.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections;
//using System.Drawing;
//using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Threading;

//using Poderosa.Preferences;
using Core;

namespace Telnet    //TerminalEmulator
//namespace Poderosa
{
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	public class GUtil {

        public static string FileToDir(string filename) {
			int n = filename.LastIndexOf('\\');
			if(n==-1) throw new FormatException("filename does not include \\");

			return filename.Substring(0, n);
		}

		private static void InternalReportCriticalError(string remark, Exception ex) {
			Debug.WriteLine(remark);
			Debug.WriteLine(ex.Message);
			Debug.WriteLine(ex.StackTrace);

			//ƒGƒ‰[ƒtƒ@ƒCƒ‹‚É’Ç‹L
			string dir = null;
			StreamWriter sw = GetDebugLog(ref dir);
			sw.WriteLine(DateTime.Now.ToString() + remark + ex.Message);
			sw.WriteLine(ex.StackTrace);
			//inner exception‚ð‡ŽŸ
			Exception i = ex.InnerException;
			while(i!=null) {
				sw.WriteLine("[inner] " + i.Message);
				sw.WriteLine(i.StackTrace);
				i = i.InnerException;
			}
			sw.Close();

			//ƒƒbƒZ[ƒWƒ{ƒbƒNƒX‚Å’Ê’mB
			//‚¾‚ª‚±‚Ì’†‚Å—áŠO‚ª”­¶‚·‚é‚±‚Æ‚ªSP1‚Å‚Í‚ ‚é‚ç‚µ‚¢B‚µ‚©‚à‚»‚¤‚È‚é‚ÆƒAƒvƒŠ‚ª‹­§I—¹‚¾B
			//Win32‚ÌƒƒbƒZ[ƒWƒ{ƒbƒNƒX‚ðo‚µ‚Ä‚à“¯‚¶BƒXƒe[ƒ^ƒXƒo[‚È‚ç‘åä•v‚Ì‚æ‚¤‚¾
			//...‚µ‚©‚µA‚»‚ê‚Å‚àNullReferenceException‚ ‚é‚¢‚ÍExecutionEngineException(!)‚ª”­¶‚·‚éê‡‚ª‚ ‚éBWin32ŒÄ‚Ño‚µ‚Å‚à‚¾‚ß‚¾‚Æ‚à‚¤Žèo‚µ‚Å‚«‚ñ‚ÈB‚ ‚«‚ç‚ß‚ÄƒRƒƒ“ƒgƒAƒEƒg
			try {
                Console.WriteLine(dir + " " + ex.Message);
			}
			catch(Exception ex2) {
				Debug.WriteLine(ex2.Message);
				Debug.WriteLine(ex2.StackTrace);
			}
		}
		private static StreamWriter GetDebugLog(ref string dir) {
			try {
				dir = AppDomain.CurrentDomain.BaseDirectory;
				return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
			}
			catch(Exception) {
				dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Poderosa";
				if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				return new StreamWriter(dir + "\\error.log", true, Encoding.Default);
			}
		}

		private static StreamWriter _debugLog = null;
		public static void WriteDebugLog(string data) {
			string dir = null;
			if(_debugLog==null) _debugLog = GetDebugLog(ref dir);
			_debugLog.WriteLine(data);
			_debugLog.Flush();
		}



		//ŽŸ‚ÉÝ’è‚·‚×‚«ƒƒO‚Ìƒtƒ@ƒCƒ‹–¼ host‚ªnull‚¾‚Æ‚»‚±‚Í‹ó”’‚É‚È‚é
		public static string CreateLogFileName(string host) {
			DateTime now = DateTime.Now;
			string date = String.Format("{0}{1,2:D2}{2,2:D2}", now.Year, now.Month, now.Day);

			string basefile;
			if(host==null || host.Length==0)
				basefile = String.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, date);
			else {
				if(host.StartsWith("rsp://"))
					host = host.Substring(6); //rsp://‚Ì‚ ‚Æ‚Ì•¶Žš—ñ
				basefile = String.Format("{0}\\{1}_{2}", Environment.SpecialFolder.ApplicationData + "\\Poderosa", ReplaceBadPathChar(host), date);
			}

			int n = 1;
			do {
				string filename;
				if(n==1)
					filename = String.Format("{0}.log", basefile);
				else
					filename = String.Format("{0}_{1}.log", basefile, n);

				if(!File.Exists(filename))
					return filename;
				else
					n++;
			} while(true);
		}

		public static string ReplaceBadPathChar(string src) {
			char ch = '_';
			return src.Replace('\\', ch).Replace('/', ch).Replace(':', ch).Replace(';', ch).
				Replace(',', ch).Replace('*', ch).Replace('?', ch).Replace('"', ch).
				Replace('<', ch).Replace('>', ch).Replace('|', ch);
		}



		public static void WriteNameValue(TextWriter wr, string name, string value) {
			wr.Write(name);
			wr.Write('=');
			wr.WriteLine(value);
		}

		public static string[] EncodingDescription(Encoding[] src) {
			string[] t = new string[src.Length];
			for(int i=0; i<src.Length; i++)
				t[i] = src[i].WebName;
			return t;
		}

		public static bool IsCursorKey(Keys key) {
			return key==Keys.Left || key==Keys.Right || key==Keys.Up || key==Keys.Down;
		}


		//KeyString‚Ì‹t•ÏŠ·@KeyConverter‚ÌŽÀ‘•‚ÍŽ€‚Ê‚Ù‚Ç’x‚¢
		public static Keys ParseKey(string s) {
			if(s.Length==0)
				return Keys.None;
			else if(s.Length==1) {
				char ch = s[0];
				if('0'<=ch && ch<='9')
					return Keys.D0 + (ch - '0');
				else
					return (Keys)Enum.Parse(typeof(Keys), s);
			}
			else
				return (Keys)Enum.Parse(typeof(Keys), s);
		}
		public static Keys ParseKey(string[] value) { //modifierž‚Ý‚Åƒp[ƒX
			Keys modifier = Keys.None;
			for(int i=0; i<value.Length-1; i++) { //ÅŒãˆÈŠO
				string m = value[i];
				if(m=="Alt") modifier |= Keys.Alt;
				else if(m=="Shift") modifier |= Keys.Shift;
				else if(m=="Ctrl")  modifier |= Keys.Control;
				else throw new Exception(m + " is unknown modifier");
			}
			return modifier | GUtil.ParseKey(value[value.Length-1]);
		}

		//ƒL[‚©‚ç‘Î‰ž‚·‚éƒRƒ“ƒgƒ[ƒ‹ƒR[ƒh(ASCII 0 ‚©‚ç 31‚Ü‚Å)‚É•ÏŠ·‚·‚éB‘Î‰ž‚·‚é‚à‚Ì‚ª‚È‚¯‚ê‚Î-1
		public static int KeyToControlCode(Keys key) {
            Keys modifiers = key & Keys.Modifiers;
			Keys body      = key & Keys.KeyCode;
			if(modifiers == Keys.Control) {
				int ib = (int)body;
				if((int)Keys.A <= ib && ib <= (int)Keys.Z)
					return ib - (int)Keys.A + 1;
				else if(body==Keys.Space)
					return 0;
				else
					return -1;
			}
			else
				return -1;
		}

		public static Thread CreateThread(ThreadStart st) {
			Thread t = new Thread(st);
			//t.ApartmentState = ApartmentState.STA;
			return t;
		}

	}

}
