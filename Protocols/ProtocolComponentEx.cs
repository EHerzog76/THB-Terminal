/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolComponentEx.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

//namespace Poderosa.Protocols
namespace Protocols
{
    //ƒf[ƒ^‘—ŽóM‚Ìˆø”ƒZƒbƒg(byte[], int, int)‚Ì‘©
    /// <summary>
    /// <ja>
    /// ƒf[ƒ^‘—ŽóM‚Ìˆø”ƒZƒbƒg‚ðŽæ‚èˆµ‚¤ƒNƒ‰ƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Class that handles set of argument of data transmitting and receiving.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒNƒ‰ƒX‚ÍAuƒoƒCƒgƒf[ƒ^vuƒIƒtƒZƒbƒgvu’·‚³v‚ðƒZƒbƒg‚Æ‚µ‚Äˆµ‚¤‚à‚Ì‚ÅAƒf[ƒ^‚ð‘—ŽóM‚·‚éÛ‚Ìˆø”‚Æ‚µ‚ÄŽg‚í‚ê‚Ü‚·B
    /// </ja>
    /// <en>
    /// This class is used as an argument when data is transmit and received by the one to treat "Byte data", "Offset", and "Length" as a set. 
    /// </en>
    /// </remarks>
    public class ByteDataFragment {
        private byte[] _buffer;
        private int _offset;
        private int _length;

        /// <summary>
        /// <ja>
        /// ƒf[ƒ^‘—ŽóMƒZƒbƒg‚ðì¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create a set of data transmitting / recieving.
        /// </en>
        /// </summary>
        public ByteDataFragment() {
        }
        /// <summary>
        /// <ja>
        /// uƒf[ƒ^vuƒIƒtƒZƒbƒgvu’·‚³v‚ðŽw’è‚µ‚Äƒf[ƒ^‘—ŽóMƒZƒbƒg‚ðì¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// This class is used as an argument when data is transmit and received by the one to treat "Byte data", "Offset", and "Length" as a set. 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>‘—ŽóM‚³‚ê‚éƒf[ƒ^‚ðŽ¦‚·”z—ñ‚Å‚·B</ja><en>It is an array that shows the transmit and received data. </en></param>
        /// <param name="offset"><ja>‘—ŽóMæ‚ðŽ¦‚·<paramref name="data"/>‚ÌƒIƒtƒZƒbƒgˆÊ’u‚Å‚·B</ja><en>It is an offset position of <paramref name="data"/> that shows the transmitting and receiving destination. </en></param>
        /// <param name="length"><ja>‘—ŽóM‚·‚é’·‚³‚Å‚·B</ja><en>It is sent and received length. </en></param>
        public ByteDataFragment(byte[] data, int offset, int length) {
            Set(data, offset, length);
        }

        /// <summary>
        /// <ja>
        /// uƒf[ƒ^vuƒIƒtƒZƒbƒgvu’·‚³v‚ðÝ’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set "Data", "Offset", "Length".
        /// </en>
        /// </summary>
        /// <param name="buffer"><ja>‘—ŽóM‚³‚ê‚éƒf[ƒ^‚ðŽ¦‚·”z—ñ‚Å‚·B</ja><en>It is an array that shows the tranmit and received data. </en></param>
        /// <param name="offset"><ja>‘—ŽóMæ‚ðŽ¦‚·<paramref name="buffer"/>‚Ö‚ÌƒIƒtƒZƒbƒgˆÊ’u‚Å‚·B</ja><en>It is an offset position to <paramref name="buffer"/> that shows the transmitting and receiving destination. </en></param>
        /// <param name="length"><ja>‘—ŽóM‚·‚é’·‚³‚Å‚·B</ja><en>It is transmit and received length. </en></param>
        /// <returns></returns>
        public ByteDataFragment Set(byte[] buffer, int offset, int length) {
            _buffer = buffer;
            _offset = offset;
            _length = length;
            return this;
        }

        /// <summary>
        /// <ja>‘—ŽóMƒoƒbƒtƒ@‚Å‚·B</ja>
        /// <en>Tranmit / recieve buffer</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// ‘—ŽóMƒf[ƒ^‚Í‚±‚±‚ÉŠi”[‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Tranmit / Recieved data stored here.
        /// </en>
        /// </remarks>
        public byte[] Buffer {
            get {
                return _buffer;
            }
        }

        /// <summary>
        /// <ja>
        /// ‘—ŽóM‚ÌƒIƒtƒZƒbƒg‚Å‚·B
        /// </ja>
        /// <en>
        /// Offset of tranmitting and receiving. 
        /// </en>
        /// </summary>
        public int Offset {
            get {
                return _offset;
            }
        }

        /// <summary>
        /// <ja>
        /// ‘—ŽóM‚·‚é’·‚³‚Å‚·B
        /// </ja>
        /// <en>
        /// Length of tranmitting and receiving. 
        /// </en>
        /// </summary>
        public int Length {
            get {
                return _length;
            }
        }
    }

    //byte[]ƒx[ƒX‚Ìo—ÍB‹ŒAbstractGuevaraSocket
    /// <summary>
    /// <ja>
    /// ƒf[ƒ^‚ð‘—M‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface to transmit data.
    /// </en>
    /// </summary>
    public interface IByteOutputStream {
        void Transmit(string data);
        /// <summary>
        /// <ja>
        /// ByteDataFragmentƒIƒuƒWƒFƒNƒg‚ðŽw’è‚µ‚Äƒf[ƒ^‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Data is transmitted specifying the ByteDataFragment object. 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>‘—M‚·‚éƒf[ƒ^‚ª“ü‚Á‚Ä‚¢‚éƒIƒuƒWƒFƒNƒg‚Å‚·B</ja><en>Object with transmitted data</en></param>
        /// <overloads>
        /// <summary>
        /// <ja>
        /// ƒf[ƒ^‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Transmitting data.
        /// </en>
        /// </summary>
        /// </overloads>
		void Transmit(ByteDataFragment data);
        /// <summary>
        /// <ja>
        /// uƒoƒCƒg”z—ñvuƒIƒtƒZƒbƒgvu’·‚³v‚ðŽw’è‚µ‚Äƒf[ƒ^‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Data is transmitted specifying "Byte array", "Offset", and "Length". 
        /// </en>
        /// </summary>
        /// <param name="data"><ja>ƒf[ƒ^‚ª“ü‚Á‚Ä‚¢‚éƒoƒCƒg”z—ñ</ja><en>Byte array with data</en></param>
        /// <param name="offset"><ja>ƒf[ƒ^‚ð‘—M‚·‚éˆÊ’u‚ðŽ¦‚µ‚½ƒIƒtƒZƒbƒg</ja><en>Offset that showed position in which data is transmitted</en></param>
        /// <param name="length"><ja>‘—M‚·‚é’·‚³</ja><en>Transmitted length</en></param>
        void Transmit(byte[] data, int offset, int length);
        /// <summary>
        /// <ja>
        /// Ú‘±‚ð•Â‚¶‚Ü‚·B
        /// </ja>
        /// <en>
        /// Close the connection.
        /// </en>
        /// </summary>
		void Close();
    }
    //byte[]ƒx[ƒX‚Ì”ñ“¯Šú“ü—ÍB‹ŒIDataReceiver
    /// <summary>
    /// <ja>
    /// <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>‚ð’Ê‚¶‚Äƒf[ƒ^‚ð”ñ“¯Šú‚Å
    /// ŽóM‚·‚é‚Æ‚«‚É—p‚¢‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that 	used when data is asynchronously received through <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ƒf[ƒ^‚ðŽóM‚µ‚½‚¢ƒvƒ‰ƒOƒCƒ“‚ÍA‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ðŽÀ‘•‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð—pˆÓ‚µA
    /// <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>‚Ì<see cref="IPoderosaSocket.RepeatAsyncRead">
    /// RepeatAsyncReadƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µ‚Ä“o˜^‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// The object that implements this interface is prepared, and the plug-in that wants to receive the data calls and registers the <see cref="IPoderosaSocket.RepeatAsyncRead">
    /// RepeatAsyncRead method</see> of <seealso cref="IPoderosaSocket">IPoderosaSocket</seealso>. 
    /// </en>
    /// </remarks>
    public interface IByteAsyncInputStream {
        /// <summary>
        /// <ja>
        /// ƒf[ƒ^‚ª“Í‚¢‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// Called when data recieves
        /// </en>
        /// </summary>
        /// <param name="data"><ja>ŽóMƒf[ƒ^‚ðŽ¦‚·ƒIƒuƒWƒFƒNƒg</ja><en>Object that show the recieved data.</en></param>
        void OnReception(ByteDataFragment data);
        /// <summary>
        /// <ja>
        /// Ú‘±‚ª’Êí‚ÌØ’fŽè‡‚ÅI—¹‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the connection terminates normally, it is called. 
        /// </en>
        /// </summary>
        void OnNormalTermination();
        /// <summary>
        /// <ja>
        /// Ú‘±‚ªƒGƒ‰[‚È‚Ç‚ÌˆÙí‚É‚æ‚Á‚ÄI—¹‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the connection terminates due to abnormality of the error etc. , it is called. 
        /// </en>
        /// </summary>
        /// <param name="message"><ja>Ø’f‚³‚ê‚½——R‚ðŽ¦‚·•¶Žš—ñ</ja><en>String that shows closed reason</en></param>
        void OnAbnormalTermination(string message);
    }

    //Ú‘±‚Ì¬Œ÷EŽ¸”s‚Ì’Ê’mB‚½‚Æ‚¦‚ÎMRUƒRƒ“ƒ|[ƒlƒ“ƒg‚ª‚±‚ê‚ðŽóM‚µ‚ÄŽ©g‚Ìî•ñ‚ðXV‚·‚é
    //Interrupt‚³‚ê‚½ê‡‚Í’Ê’m‚È‚µ
    /// <summary>
    /// <ja>
    /// Ú‘±‚Ì¬Œ÷EŽ¸”s‚Ì’Ê’m‚ðŽó‚¯Žæ‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that receives notification of success and failure of connection
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAProtocolsƒvƒ‰ƒOƒCƒ“‚ÌŠg’£ƒ|ƒCƒ“ƒguorg.poderosa.protocols.resultEventHandlerv‚É‚æ‚Á‚Ä’ñ‹Ÿ‚³‚ê‚Ü‚·B
    /// </ja>
    /// <en>
    /// This interface is offered with the extension point (org.poderosa.protocols.resultEventHandler) of the Protocols plug-in. 
    /// </en>
    /// </remarks>
    public interface IConnectionResultEventHandler {
        /// <summary>
        /// <ja>
        /// Ú‘±‚ª¬Œ÷‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the connection succeeds, it is called. 
        /// </en>
        /// </summary>
        /// <param name="param"><ja>Ú‘±ƒpƒ‰ƒ[ƒ^‚Å‚·</ja><en>Connection parameter.</en></param>
        void OnSucceeded(ITerminalParameter param);
        /// <summary>
        /// <ja>
        /// Ú‘±‚ªŽ¸”s‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the connection fails, it is called. 
        /// </en>
        /// </summary>
        /// <param name="param"><ja>Ú‘±ƒpƒ‰ƒ[ƒ^‚Å‚·</ja><en>Connection parameter.</en></param>
        /// <param name="msg"><ja>Ž¸”s‚µ‚½——R‚ªŠÜ‚Ü‚ê‚éƒƒbƒZ[ƒW‚Å‚·</ja><en>Message being included for failing reason</en></param>
        void OnFailed(ITerminalParameter param, string msg);

        /// <summary>
        /// <ja>
        /// ”ñ“¯ŠúÚ‘±ŠJŽn‘O‚ÉŒÄ‚Î‚ê‚Ü‚·
        /// </ja>
        /// <en>
        /// Called before the asynchronous connection starts
        /// </en>
        /// </summary>
        /// <param name="param"></param>
        void BeforeAsyncConnect(ITerminalParameter param);
    }

}
