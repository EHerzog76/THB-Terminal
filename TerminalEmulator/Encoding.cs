/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Encoding.cs,v 1.3 2010/11/24 16:04:10 kzmi Exp $
 */
using System;
using System.Text;

using ConnectionParam;
using Core;

namespace TerminalEmulator
//namespace Poderosa.Terminal
{
	//encodingŠÖŒW
	public abstract class EncodingProfile {
		
		private Encoding _encoding;
		private EncodingType _type;
		private byte[] _buffer;
		private int _cursor;
		private int _byte_len;
        private char[] _tempOneCharArray;

		protected EncodingProfile(EncodingType t, Encoding enc) {
			_type = t;
			_encoding = enc;
			_buffer = new byte[3]; //¡‚Í‚P•¶Žš‚ÍÅ‘å‚RƒoƒCƒg
			_cursor = 0;
            _tempOneCharArray = new char[1]; //API‚Ì“s‡‚Å’·‚³‚P‚Ìchar[]‚ª•K—v‚È‚Æ‚«Žg‚¤
		}

		public abstract bool IsLeadByte(byte b);

        //æ“ªƒoƒCƒg‚©‚çA•¶Žš‚ª‰½ƒoƒCƒg‚Å\¬‚³‚ê‚Ä‚¢‚é‚©‚ð•Ô‚·
		public abstract int  GetCharLength(byte b);

        //UTF‚ÌBOM‚È‚ÇAƒfƒR[ƒh‚ÌŒ‹‰Êo‚Ä‚«‚Ä‚à–³Ž‹‚·‚×‚«•¶Žš‚©‚ð”»’è
        public abstract bool IsIgnoreableChar(char c);

		public Encoding Encoding {
			get {
				return _encoding;
			}
		}
		public EncodingType Type {
			get {
				return _type;
			}
		}
		internal byte[] Buffer {
			get {
				return _buffer;
			}
		}

		internal byte[] GetBytes(char[] chars) {
			return _encoding.GetBytes(chars);
		}
        //NOTE öÝ“I‚É‚Í_tempOneCharArray‚ÌŽg—p‚Åƒ}ƒ‹ƒ`ƒXƒŒƒbƒh‚Å‚ÌŠëŒ¯‚ª‚ ‚éB
		internal byte[] GetBytes(char ch) {
			_tempOneCharArray[0] = ch;
			return _encoding.GetBytes(_tempOneCharArray);
		}

		internal bool IsInterestingByte(byte b) {
            //"b>=33"‚Ì‚Æ‚±‚ë‚Í‚à‚¤‚¿‚å‚Á‚Æ‚Ü‚¶‚ß‚É”»’è‚·‚é‚×‚«B
            //•¶Žš‚ÌŠÔ‚ÉƒGƒXƒP[ƒvƒV[ƒPƒ“ƒX‚ª“ü‚éƒP[ƒX‚Ö‚Ì‘Î‰žB
			return _cursor==0? IsLeadByte(b) : b>=33; 
		}

        /* REMOVE
		internal int Decode(byte[] data, char[] result) {
			return _encoding.GetChars(data, 0, data.Length, result, 0);
		}
         */

		internal void Reset() {
			_cursor = 0;
			_byte_len = 0;
		}

		//‚PƒoƒCƒg‚ð’Ç‰Á‚·‚éB•¶Žš‚ªŠ®¬‚·‚ê‚ÎƒfƒR[ƒh‚µ‚Ä‚»‚Ì•¶Žš‚ð•Ô‚·B‚Ü‚¾‘±‚«‚ÌƒoƒCƒg‚ª•K—v‚È‚ç\0‚ð•Ô‚·
		internal char PutByte(byte b) {
			if(_cursor==0)
				_byte_len = GetCharLength(b);
			_buffer[_cursor++] = b;
			if(_cursor==_byte_len) {
				_encoding.GetChars(_buffer, 0, _byte_len, _tempOneCharArray, 0);
				_cursor = 0;
                if(IsIgnoreableChar(_tempOneCharArray[0]))
                    return '\0';
                else
    				return _tempOneCharArray[0];
			}
			return '\0';
		}

		public static EncodingProfile Get(EncodingType et) {
			EncodingProfile p = null;
			switch(et) {
				case EncodingType.ISO8859_1:
					p = new ISO8859_1Profile();
					break;
				case EncodingType.EUC_JP:
					p = new EUCJPProfile();
					break;
				case EncodingType.SHIFT_JIS:
					p = new ShiftJISProfile();
					break;
				case EncodingType.UTF8:
					p = new UTF8Profile();
					break;
				case EncodingType.GB2312:
					p = new GB2312Profile();
					break;
				case EncodingType.BIG5:
					p = new Big5Profile();
					break;
				case EncodingType.EUC_CN:
					p = new EUCCNProfile();
					break;
				case EncodingType.EUC_KR:
					p = new EUCKRProfile();
					break;
			}
			return p;
		}

        //NOTE ‚±‚ê‚ç‚Íƒƒ\ƒbƒh‚Ìoverride‚Å‚È‚­delegate‚Å‚Ü‚í‚µ‚½‚Ù‚¤‚ªŒø—¦‚ÍŽáŠ±‚æ‚¢‚Ì‚©‚à
		class ISO8859_1Profile : EncodingProfile {
			public ISO8859_1Profile() : base(EncodingType.ISO8859_1, Encoding.GetEncoding("iso-8859-1")) {
			}
			public override int GetCharLength(byte b) {
				return 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0xA0 && b<=0xFF;
			}
            public override bool IsIgnoreableChar(char c) {
                return false;
            }
		}
		class ShiftJISProfile : EncodingProfile {
			public ShiftJISProfile() : base(EncodingType.SHIFT_JIS, Encoding.GetEncoding("shift_jis")) {
			}
			public override int GetCharLength(byte b) {
				return (b>=0xA1 && b<=0xDF)? 1 : 2;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x81 && b<=0xFC;
			}
            public override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
		class EUCJPProfile : EncodingProfile {
			public EUCJPProfile() : base(EncodingType.EUC_JP, Encoding.GetEncoding("euc-jp")) {
			}
			public override int GetCharLength(byte b) {
				return b==0x8F? 3 : b>=0x8E? 2 : 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x8E && b<=0xFE;
			}
            public override bool IsIgnoreableChar(char c) {
                return false;
            }
        }
		class UTF8Profile : EncodingProfile {
			public UTF8Profile() : base(EncodingType.UTF8, Encoding.UTF8) {
			}
			public override int GetCharLength(byte b) {
				return b>=0xE0? 3 : b>=0x80? 2 : 1;
			}
			public override bool IsLeadByte(byte b) {
				return b>=0x80;
			}
            public override bool IsIgnoreableChar(char c) {
                return c=='\uFFFE' || c=='\uFEFF';
            }
        }
		private class GB2312Profile : EncodingProfile {
			public GB2312Profile()
				: base(EncodingType.GB2312, Encoding.GetEncoding("gb2312")) {
			}
			public override int GetCharLength(byte b) {
				return 2;
			}
			public override bool IsLeadByte(byte b) {
				return b >= 0xA1 && b <= 0xF7;
			}
			public override bool IsIgnoreableChar(char c) {
				return false;
			}
		}
		private class Big5Profile : EncodingProfile {
			public Big5Profile()
				: base(EncodingType.BIG5, Encoding.GetEncoding("big5")) {
			}
			public override int GetCharLength(byte b) {
				return 2;
			}
			public override bool IsLeadByte(byte b) {
				return b >= 0x81 && b <= 0xFE;
			}
			public override bool IsIgnoreableChar(char c) {
				return false;
			}
		}
		private class EUCCNProfile : EncodingProfile {
			public EUCCNProfile()
				: base(EncodingType.EUC_CN, Encoding.GetEncoding("euc-cn")) {
			}
			public override int GetCharLength(byte b) {
				return 2;
			}
			public override bool IsLeadByte(byte b) {
				return b >= 0xA1 && b <= 0xF7;
			}
			public override bool IsIgnoreableChar(char c) {
				return false;
			}
		}
		private class EUCKRProfile : EncodingProfile {
			public EUCKRProfile()
				: base(EncodingType.EUC_KR, Encoding.GetEncoding("euc-kr")) {
			}
			public override int GetCharLength(byte b) {
				return 2;
			}
			public override bool IsLeadByte(byte b) {
				return b >= 0xA1 && b <= 0xFE;
			}
			public override bool IsIgnoreableChar(char c) {
				return false;
			}
		}
	}
}
