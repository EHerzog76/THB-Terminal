/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: LoggerEx.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

//using Poderosa.Document;
using Protocols;

namespace TerminalEmulator
{
    /// <summary>
    /// <ja>
    /// ƒƒO‚ÌŠî’êƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Base interface of the log.
    /// </en>
    /// </summary>
    public interface ILoggerBase {
        /// <summary>
        /// <ja>ƒƒO‚ð•Â‚¶‚Ü‚·B</ja>
        /// <en>Close log</en>
        /// </summary>
        void Close();
        /// <summary>
        /// <ja>ƒƒO‚ðƒtƒ‰ƒbƒVƒ…‚µ‚Ü‚·B</ja>
        /// <en>Flush log</en>
        /// </summary>
        void Flush();
    }

    /// <summary>
    /// <ja>
    /// ƒoƒCƒiƒŠ‚ÌƒƒK[‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the logger of binary.
    /// </en>
    /// </summary>
    public interface IBinaryLogger : ILoggerBase {
        /// <summary>
        /// <ja>ƒoƒCƒiƒŠƒƒO‚ð‘‚«ž‚Ý‚Ü‚·B</ja><en>Write a binary log</en>
        /// </summary>
        /// <param name="data"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒf[ƒ^‚Å‚·B</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>ƒoƒCƒiƒŠƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="data"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ð‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja><en>Those who implements about binary logger implement like writing the data passed to <paramref name="data"/>. </en>
        /// </remarks>
        void Write(ByteDataFragment data);
    }

    /// <summary>
    /// <ja>
    /// ƒeƒLƒXƒg‚ÌƒƒK[‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the logger of text.
    /// </en>
    /// </summary>
    public interface ITextLogger : ILoggerBase {
        /// <summary>
        /// <ja>
        /// ƒeƒLƒXƒgƒƒO‚ð‘‚«ž‚Ý‚Ü‚·B
        /// </ja>
        /// <en>Write a text log</en>
        /// </summary>
        /// <param name="line"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒf[ƒ^‚Å‚·B</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>ƒeƒLƒXƒgƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="line"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ð‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja><en>Those who implements about text logger implement like writing the data passed to <paramref name="line"/>. </en>
        /// </remarks>
        void WriteLine(/* GLine */ string line); //ƒeƒLƒXƒgƒx[ƒX‚ÍLine’PˆÊ
        void WriteLine(string line, bool NewLine); //ƒeƒLƒXƒgƒx[ƒX‚ÍLine’PˆÊ
        /// <summary>
        /// <ja>ƒRƒƒ“ƒg‚ð‘‚«ž‚Ý‚Ü‚·B</ja>
        /// <en>Write a comment</en>
        /// </summary>
        /// <param name="comment"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒRƒƒ“ƒg‚Å‚·B</ja><en>Comment to write.</en></param>
        /// <remarks>
        /// <ja>ƒeƒLƒXƒgƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="comment"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ð‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja><en>Those who implements about text logger implement like writing the data passed to <paramref name="comment"/>. </en>
        /// </remarks>
        void Comment(string comment);
    }

    /// <summary>
    /// <ja>
    /// XML‚ÌƒƒK[‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the logger of XML.
    /// </en>
    /// </summary>
    public interface IXmlLogger : ILoggerBase {
        /// <summary>
        /// <ja>
        /// XMLƒƒO‚ð‘‚«ž‚Ý‚Ü‚·B
        /// </ja>
        /// <en>Write a XML log</en>
        /// </summary>
        /// <param name="ch"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒf[ƒ^‚Å‚·B</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>XMLƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="char"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ð‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja><en>Those who implements about XML logger implement like writing the data passed to <paramref name="char"/>. </en>
        /// </remarks>
        void Write(char ch);
        /// <summary>
        /// <ja>
        /// XMLƒƒO‚ðƒGƒXƒP[ƒv‚µ‚Ä‘‚«ž‚Ý‚Ü‚·B
        /// </ja>
        /// <en>
        /// writes log escaping in the XML.
        /// </en>
        /// </summary>
        /// <param name="body"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒf[ƒ^‚Å‚·B</ja><en>Data to write.</en></param>
        /// <remarks>
        /// <ja>XMLƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="body"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ðƒGƒXƒP[ƒv‚µ‚Ä‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja>
        /// <en>Those who implements about XML logger implement like writing the data passed to <paramref name="body"/> with escaping. </en>
        /// </remarks>
        void EscapeSequence(char[] body);
        /// <summary>
        /// <ja>
        /// ƒRƒƒ“ƒg‚ð‘‚«ž‚Ý‚Ü‚·B
        /// </ja>
        /// <en>Write a comment</en>
        /// </summary>
        /// <param name="comment"><ja>‘‚«ž‚Ü‚ê‚æ‚¤‚Æ‚µ‚Ä‚¢‚éƒRƒƒ“ƒg‚Å‚·B</ja><en>Comment to write.</en></param>
        /// <remarks>
        /// <ja>XMLƒƒK[‚ÌŽÀ‘•ŽÒ‚ÍA<paramref name="comment"/>‚É“n‚³‚ê‚½ƒf[ƒ^‚ð‘‚«ž‚Þ‚æ‚¤‚ÉŽÀ‘•‚µ‚Ü‚·B</ja><en>Those who implements about XML logger implement like writing the data passed to <paramref name="comment"/>. </en>
        /// </remarks>
        void Comment(string comment);
    }

    /// <summary>
    /// <ja>
    /// ƒƒOƒT[ƒrƒX‚ÉƒAƒNƒZƒX‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface accessed log service.
    /// </en>
    /// </summary>
    public interface ILogService {
        /// <summary>
        /// <ja>
        /// ƒoƒCƒiƒŠ‚ÌƒƒK[‚ð“o˜^‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Regist the logger of binary
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>“o˜^‚·‚éƒƒK[</ja><en>Logger to regist.</en></param>
        void AddBinaryLogger(IBinaryLogger logger);
        /// <summary>
        /// <ja>
        /// ƒoƒCƒiƒŠ‚ÌƒƒK[‚ð‰ðœ‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Remove the logger of binary
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>‰ðœ‚·‚éƒƒK[</ja><en>Logger to remove.</en></param>
        void RemoveBinaryLogger(IBinaryLogger logger);
        /// <summary>
        /// <ja>
        /// ƒeƒLƒXƒg‚ÌƒƒK[‚ð“o˜^‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Regist the logger of text
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>“o˜^‚·‚éƒƒK[</ja><en>Logger to regist.</en></param>
        void AddTextLogger(ITextLogger logger);
        /// <summary>
        /// <ja>
        /// ƒeƒLƒXƒg‚ÌƒƒK[‚ð‰ðœ‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Remove the logger of text
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>‰ðœ‚·‚éƒƒK[</ja><en>Logger to remove.</en></param>
        void RemoveTextLogger(ITextLogger logger);
        /// <summary>
        /// <ja>
        /// XML‚ÌƒƒK[‚ð“o˜^‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Regist the logger of XML
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>“o˜^‚·‚éƒƒK[</ja><en>Logger to regist.</en></param>
        void AddXmlLogger(IXmlLogger logger);
        /// <summary>
        /// <ja>
        /// XML‚ÌƒƒK[‚ð‰ðœ‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Remove the logger of XML
        /// </en>
        /// </summary>
        /// <param name="logger"><ja>‰ðœ‚·‚éƒƒK[</ja><en>Logger to remove.</en></param>
        void RemoveXmlLogger(IXmlLogger logger);

        /// <summary>
        /// <ja>
        /// ƒƒOÝ’è‚ð”½‰f‚³‚¹‚Ü‚·B
        /// </ja>
        /// <en>
        /// Apply the log setting.
        /// </en>
        /// </summary>
        /// <param name="settings"><ja>ƒƒO‚ÌÝ’è</ja><en>Set of log.</en></param>
        /// <param name="clear_previous"><ja>Ý’è‘O‚ÉƒNƒŠƒA‚·‚é‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO</ja><en>Flag whether clear before it sets it</en></param>
        /// <exclude/>
        void ApplyLogSettings(ILogSettings settings, bool clear_previous);
        /// <summary>
        /// <ja>
        /// ƒƒO‚ÌƒRƒƒ“ƒg‚ðÝ’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set the comment on the log.
        /// </en>
        /// </summary>
        /// <param name="comment"><ja>Ý’è‚·‚éƒRƒƒ“ƒg</ja><en>Comment to set.</en></param>
        /// <exclude/>
        void Comment(string comment);
    }

}
