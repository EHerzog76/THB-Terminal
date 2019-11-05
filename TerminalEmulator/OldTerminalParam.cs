/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: OldTerminalParam.cs,v 1.3 2010/11/24 16:04:10 kzmi Exp $
 */
using System;
using System.Diagnostics;
using System.Collections;
//using System.IO;
using System.Text;
//using System.Windows.Forms;
//using System.Net;
//using System.Net.Sockets;

//using Poderosa.Document;
using TerminalEmulator;
using Core;
//using Poderosa.View;
using Protocols;

namespace ConnectionParam
//namespace TerminalEmulator
{

	/*
	 * TerminalParam‚Íƒ}ƒNƒ‚©‚ç‚àƒtƒ‹‚ÉƒAƒNƒZƒX‰Â”\‚É‚·‚é‚½‚ßpublic‚É‚·‚é
	 * ŒöŠJ‚·‚é•K—v‚Ì‚È‚¢ƒƒ\ƒbƒh‚ðinternal‚É‚·‚é
	 */ 

	//Granados“à‚ÌAuthenticationType‚Æ“¯ˆê‚¾‚ªA‹N“®‚Ì‚‘¬‰»‚Ì‚½‚ßŽg‚í‚È‚¢
	
	/// <summary>
	/// <ja>SSH‚Å‚Ì”FØ•û–@‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies the authemtication method of SSH.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(AuthType))]
	public enum AuthType {
		/// <summary>
		/// <ja>ƒpƒXƒ[ƒh”FØ</ja>
		/// <en>Authentication using password.</en>
		/// </summary>
		//[EnumValue(Description="Enum.AuthType.Password")]
		Password,

		/// <summary>
		/// <ja>ŽèŒ³‚Ì”é–§Œ®‚ÆƒŠƒ‚[ƒgƒzƒXƒg‚É“o˜^‚µ‚½ŒöŠJŒ®‚ðŽg‚Á‚½”FØ</ja>
		/// <en>Authentication using the local private key and the remote public key.</en>
		/// </summary>
		//[EnumValue(Description="Enum.AuthType.PublicKey")]
		PublicKey,

		/// <summary>
		/// <ja>ƒRƒ“ƒ\[ƒ‹ã‚ÅƒpƒXƒ[ƒh‚ð“ü—Í‚·‚é”FØ</ja>
		/// <en>Authentication by sending the password through the console.</en>
		/// </summary>
		//[EnumValue(Description="Enum.AuthType.KeyboardInteractive")]
		KeyboardInteractive
	}

    /// <summary>
    /// <ja>Ú‘±‚ÌŽí—Þ‚ðŽ¦‚µ‚Ü‚·B</ja>
    /// <en>Specifies the type of the connection.</en>
    /// </summary>
    /// <exclude/>
    public enum ConnectionMethod {
        /// <summary>
        /// Telnet
        /// </summary>
        Telnet,
        /// <summary>
        /// SSH1
        /// </summary>
        SSH1,
        /// <summary>
        /// SSH2
        /// </summary>
        SSH2
    }
    
    /// <summary>
	/// <ja>ƒGƒ“ƒR[ƒfƒBƒ“ƒO‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies the encoding of the connection.</en>
    /// <!--
    /// <seealso cref="Poderosa.ConnectionParam.TerminalParam.Encoding"/>
    /// -->
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(EncodingType))]
	public enum EncodingType {
		/// <summary>
		/// <ja>iso-8859-1</ja>
		/// <en>iso-8859-1</en>
		/// </summary>
		//[EnumValue(Description="Enum.EncodingType.ISO8859_1")]
        ISO8859_1,
		/// <summary>
		/// <ja>utf-8</ja>
		/// <en>utf-8</en>
		/// </summary>
		//[EnumValue(Description="Enum.EncodingType.UTF8")]
        UTF8,
		/// <summary>
		/// <ja>euc-jp</ja>
		/// <en>euc-jp (This encoding is primarily used with Japanese characters.)</en>
		/// </summary>
		//[EnumValue(Description="Enum.EncodingType.EUC_JP")]
        EUC_JP,
		/// <summary>
		/// <ja>shift-jis</ja>
		/// <en>shift-jis (This encoding is primarily used with Japanese characters.)</en>
		/// </summary>
		//[EnumValue(Description="Enum.EncodingType.SHIFT_JIS")]
        SHIFT_JIS,
		/// <summary>
		/// <ja>gb2312</ja>
		/// <en>gb2312 (This encoding is primarily used with simplified Chinese characters.)</en>
		/// </summary>
		//[EnumValue(Description = "Enum.EncodingType.GB2312")]
        GB2312,
		/// <summary>
		/// <ja>big5</ja>
		/// <en>big5 (This encoding is primarily used with traditional Chinese characters.)</en>
		/// </summary>
		//[EnumValue(Description = "Enum.EncodingType.BIG5")]
        BIG5,
		/// <summary>
		/// <ja>euc-cn</ja>
		/// <en>euc-cn (This encoding is primarily used with simplified Chinese characters.)</en>
		/// </summary>
		//[EnumValue(Description = "Enum.EncodingType.EUC_CN")]
        EUC_CN,
		/// <summary>
		/// <ja>euc-kr</ja>
		/// <en>euc-kr (This encoding is primarily used with Korean characters.)</en>
		/// </summary>
		//[EnumValue(Description = "Enum.EncodingType.EUC_KR")]
        EUC_KR
	}

	/// <summary>
	/// <ja>ƒƒO‚ÌŽí—Þ‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies the log type.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(LogType))]
	public enum LogType {
		/// <summary>
		/// <ja>ƒƒO‚Í‚Æ‚è‚Ü‚¹‚ñB</ja>
		/// <en>The log is not recorded.</en>
		/// </summary>
		//[EnumValue(Description="Enum.LogType.None")]
        None,
		/// <summary>
		/// <ja>ƒeƒLƒXƒgƒ‚[ƒh‚ÌƒƒO‚Å‚·B‚±‚ê‚ª•W€‚Å‚·B</ja>
		/// <en>The log is a plain text file. This is standard.</en>
		/// </summary>
		//[EnumValue(Description="Enum.LogType.Default")]
        Default,
		/// <summary>
		/// <ja>ƒoƒCƒiƒŠƒ‚[ƒh‚ÌƒƒO‚Å‚·B</ja>
		/// <en>The log is a binary file.</en>
		/// </summary>
		//[EnumValue(Description="Enum.LogType.Binary")]
        Binary,
		/// <summary>
		/// <ja>XML‚Å•Û‘¶‚µ‚Ü‚·B‚Ü‚½“à•”“I‚ÈƒoƒO’ÇÕ‚É‚¨‚¢‚Ä‚±‚Ìƒ‚[ƒh‚Å‚ÌƒƒOÌŽæ‚ð‚¨Šè‚¢‚·‚é‚±‚Æ‚ª‚ ‚è‚Ü‚·B</ja>
		/// <en>The log is an XML file. We may ask you to record the log in this type for debugging.</en>
		/// </summary>
		//[EnumValue(Description="Enum.LogType.Xml")]
        Xml
	}

	/// <summary>
	/// <ja>‘—MŽž‚Ì‰üs‚ÌŽí—Þ‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies the new-line characters for transmission.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(NewLine))]
	public enum NewLine {
		/// <summary>
		/// CR
		/// </summary>
		//[EnumValue(Description="Enum.NewLine.CR")]
        CR,
		/// <summary>
		/// LF
		/// </summary>
		//[EnumValue(Description="Enum.NewLine.LF")]
        LF,
		/// <summary>
		/// CR+LF
		/// </summary>
		//[EnumValue(Description="Enum.NewLine.CRLF")]
        CRLF
	}

	/// <summary>
	/// <ja>ƒ^[ƒ~ƒiƒ‹‚ÌŽí•Ê‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies the type of the terminal.</en>
	/// </summary>
	/// <remarks>
	/// <ja>XTerm‚É‚ÍVT100‚É‚Í‚È‚¢‚¢‚­‚Â‚©‚ÌƒGƒXƒP[ƒvƒV[ƒPƒ“ƒX‚ªŠÜ‚Ü‚ê‚Ä‚¢‚Ü‚·B</ja>
	/// <en>XTerm supports several escape sequences in addition to VT100.</en>
	/// <ja>KTerm‚Í’†g‚ÍXTerm‚Æˆê‚Å‚·‚ªASSH‚âTelnet‚ÌÚ‘±ƒIƒvƒVƒ‡ƒ“‚É‚¨‚¢‚Äƒ^[ƒ~ƒiƒ‹‚ÌŽí—Þ‚ðŽ¦‚·•¶Žš—ñ‚Æ‚µ‚Ä"kterm"‚ªƒZƒbƒg‚³‚ê‚Ü‚·B</ja>
	/// <en>Though the functionality of KTerm is identical to XTerm, the string "kterm" is used for specifying the type of the terminal in the connection of Telnet or SSH.</en>
	/// <ja>‚±‚ÌÝ’è‚ÍA‘½‚­‚Ìê‡TERMŠÂ‹«•Ï”‚Ì’l‚É‰e‹¿‚µ‚Ü‚·B</ja>
	/// <en>In most cases, this setting affects the TERM environment variable.</en>
	/// </remarks>
    /// <exclude/>
	//[EnumDesc(typeof(TerminalType))]
	public enum TerminalType {
		/// <summary>
		/// vt100
		/// </summary>
		//[EnumValue(Description="Enum.TerminalType.VT100")]
        VT100,
		/// <summary>
		/// xterm
		/// </summary>
		//[EnumValue(Description="Enum.TerminalType.XTerm")]
        XTerm,
		/// <summary>
		/// kterm
		/// </summary>
		//[EnumValue(Description="Enum.TerminalType.KTerm")]
        KTerm
    }

	/// <summary>
	/// <ja>ŽóM‚µ‚½•¶Žš‚É‘Î‚·‚é‰üs•û–@‚ðŽ¦‚µ‚Ü‚·B</ja>
	/// <en>Specifies line breaking style.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(LineFeedRule))]
	public enum LineFeedRule {
		/// <summary>
		/// <ja>•W€</ja>
		/// <en>Standard</en>
		/// </summary>
		//[EnumValue(Description="Enum.LineFeedRule.Normal")]
        Normal,
		/// <summary>
		/// <ja>LF‚Å‰üs‚µCR‚ð–³Ž‹</ja>
		/// <en>LF:Line Break, CR:Ignore</en>
		/// </summary>
		//[EnumValue(Description="Enum.LineFeedRule.LFOnly")]
        LFOnly,
		/// <summary>
		/// <ja>CR‚Å‰üs‚µLF‚ð–³Ž‹</ja>
		/// <en>CR:Line Break, LF:Ignore</en>
		/// </summary>
		//[EnumValue(Description="Enum.LineFeedRule.CROnly")]
        CROnly
	}

#if !MACRODOC
	/// <summary>
	/// <ja>ƒtƒ[ƒRƒ“ƒgƒ[ƒ‹‚ÌÝ’è</ja>
	/// <en>Specifies the flow control.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(FlowControl))]
	public enum FlowControl {
		/// <summary>
		/// <ja>‚È‚µ</ja>
		/// <en>None</en>
		/// </summary>
		//[EnumValue(Description="Enum.FlowControl.None")]
        None,
		/// <summary>
		/// X ON / X OFf
		/// </summary>
		//[EnumValue(Description="Enum.FlowControl.Xon_Xoff")]
        Xon_Xoff,
		/// <summary>
		/// <ja>ƒn[ƒhƒEƒFƒA</ja>
		/// <en>Hardware</en>
		/// </summary>
		//[EnumValue(Description="Enum.FlowControl.Hardware")]
        Hardware
	}

	/// <summary>
	/// <ja>ƒpƒŠƒeƒB‚ÌÝ’è</ja>
	/// <en>Specifies the parity.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(Parity))]
	public enum Parity {
		/// <summary>
		/// <ja>‚È‚µ</ja>
		/// <en>None</en>
		/// </summary>
		//[EnumValue(Description="Enum.Parity.NOPARITY")]
        NOPARITY = 0,
		/// <summary>
		/// <ja>Šï”</ja>
		/// <en>Odd</en>
		/// </summary>
		//[EnumValue(Description="Enum.Parity.ODDPARITY")]
        ODDPARITY   =        1,
		/// <summary>
		/// <ja>‹ô”</ja>
		/// <en>Even</en>
		/// </summary>
		//[EnumValue(Description="Enum.Parity.EVENPARITY")]
        EVENPARITY  =        2
		//MARKPARITY  =        3,
		//SPACEPARITY =        4
	}

	/// <summary>
	/// <ja>ƒXƒgƒbƒvƒrƒbƒg‚ÌÝ’è</ja>
	/// <en>Specifies the stop bits.</en>
	/// </summary>
    /// <exclude/>
	//[EnumDesc(typeof(StopBits))]
	public enum StopBits {
		/// <summary>
		/// <ja>1ƒrƒbƒg</ja>
		/// <en>1 bit</en>
		/// </summary>
		//[EnumValue(Description="Enum.StopBits.ONESTOPBIT")]
        ONESTOPBIT  =        0,
		/// <summary>
		/// <ja>1.5ƒrƒbƒg</ja>
		/// <en>1.5 bits</en>
		/// </summary>
		//[EnumValue(Description="Enum.StopBits.ONE5STOPBITS")]
        ONE5STOPBITS=        1,
		/// <summary>
		/// <ja>2ƒrƒbƒg</ja>
		/// <en>2 bits</en>
		/// </summary>
		//[EnumValue(Description="Enum.StopBits.TWOSTOPBITS")]
        TWOSTOPBITS =        2
	}
#endif

}