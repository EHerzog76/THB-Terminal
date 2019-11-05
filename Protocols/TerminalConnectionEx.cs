/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalConnectionEx.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

//namespace Poderosa.Protocols
namespace Protocols
{
    /// <summary>
    /// <ja>
    /// ’ÊM‚·‚é‚½‚ß‚Ìƒ\ƒPƒbƒg‚Æ‚È‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface to became a  socket to connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAÚ‘±‚ðŽ¦‚·<seealso cref="ITerminalConnection">ITerminalConnection</seealso>‚Ì<see cref="ITerminalConnection.Socket">SocketƒvƒƒpƒeƒB</see>‚Æ‚µ‚ÄŽæ“¾‚Å‚«‚Ü‚·B</ja><en>This interface can be got <see cref="ITerminalConnection.Socket">Socket property</see> that show connection on <seealso cref="ITerminalConnection">ITerminalConnection</seealso>.</en>
    /// </remarks>
	public interface IPoderosaSocket : IByteOutputStream {
        /// <summary>
        /// <ja>
        /// ƒf[ƒ^‚ðŽóM‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ð“o˜^‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Regist the interface to recieve data.
        /// </en>
        /// </summary>
        /// <param name="receiver"><ja>ƒf[ƒ^‚ðŽóM‚·‚é‚Æ‚«‚ÉŒÄ‚Ño‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface called when recieve the data.</en></param>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ÍA•¡”‰ñŒÄ‚Ño‚µ‚ÄA‘½”‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ð“o˜^‚·‚é‚±‚Æ‚Í‚Å‚«‚Ü‚¹‚ñB‚Ü‚½“o˜^‚µ‚½ƒCƒ“ƒ^[ƒtƒFƒCƒX‚ð‰ðœ‚·‚é•û–@‚à
        /// —pˆÓ‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB
        /// </ja>
        /// <en>
        /// This method cannot register a lot of interfaces by calling it two or more times. Moreover, the method of releasing the registered interface is not prepared. 
        /// </en>
        /// </remarks>
		void RepeatAsyncRead(IByteAsyncInputStream receiver);
        /// <summary>
        /// <ja>
        /// ƒf[ƒ^‚ðŽóM‚·‚é‚±‚Æ‚ª‚Å‚«‚é‚©‚Ç‚¤‚©‚ðŽ¦‚µ‚Ü‚·Bfalse‚Ì‚Æ‚«‚É‚Íƒf[ƒ^‚ðŽóM‚Å‚«‚Ü‚¹‚ñB
        /// </ja>
        /// <en>
        /// It shows whether to receive the data. At false, it is not possible to receive the data. 
        /// </en>
        /// </summary>
        bool Available { get; }
        /// <summary>
        /// <ja>
        /// ÅI“I‚ÈƒNƒŠ[ƒ“ƒAƒbƒv‚ð‚µ‚Ü‚·Bƒ\ƒPƒbƒgAPI‚É‚ÍDisconnect, Shutdown, Close“™‚ª‚ ‚è‚Ü‚·‚ª‚»‚ê‚É‚æ‚ç‚¸‚ÉŠ®‘S‚È”jŠü‚ðŽÀs‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// A final cleanup is done. A complete annulment is executed without depending on it though socket API includes Disconnect, Shutdown, and Close, etc.
        /// </en>
        /// </summary>
        void ForceDisposed();
    }

    //’[––‚Æ‚µ‚Ä‚Ìo—ÍB‹ŒTerminalConnection‚Ì‚¢‚­‚Â‚©‚Ìƒƒ\ƒbƒh‚ð”²‚«o‚µ‚½
    /// <summary>
    /// <ja>
    /// ’[––ŒÅ—L‚Ìƒf[ƒ^‚ðo—Í‚·‚é‹@”\‚ð’ñ‹Ÿ‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// Offer the function to output peculiar data to the terminal.
    /// </en>
    /// </summary>
    public interface ITerminalOutput {
        /// <summary>
        /// <ja>
        /// ƒuƒŒ[ƒNM†‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send break.
        /// </en>
        /// </summary>
		void SendBreak();
        /// <summary>
        /// <ja>
        /// ƒL[ƒvƒAƒ‰ƒCƒuƒf[ƒ^‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send keep alive data.
        /// </en>
        /// </summary>
		void SendKeepAliveData();
        /// <summary>
        /// <ja>
        /// AreYouThere‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send AreYouThere.
        /// </en>
        /// </summary>
		void AreYouThere(); //Telnet only‚©‚à‚æ
        /// <summary>
        /// <ja>
        /// ’[––‚ÌƒTƒCƒY‚ð•ÏX‚·‚éƒRƒ}ƒ“ƒh‚ð‘—M‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Send the command to which the size of the terminal is changed.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>•ÏXŒã‚Ì•i•¶Žš’PˆÊj</ja><en>Width after it changes(unit of character)</en></param>
        /// <param name="height"><ja>•ÏXŒã‚Ì‚‚³i•¶Žš’PˆÊj</ja><en>Height after it changes(unit of character)</en></param>
        void Resize(int width, int height);
    }

    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒRƒlƒNƒVƒ‡ƒ“‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the terminal connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>‚Ì
    /// <see cref="Poderosa.Sessions.ITerminalSession.TerminalConnection">TerminalConnectionƒvƒƒpƒeƒB‚Å</see>
    /// Žæ“¾‚Å‚«‚Ü‚·B
    /// </ja>
    /// <en>
    /// This interface can be got in the <see cref="Poderosa.Sessions.ITerminalSession.TerminalConnection">TerminalConnection property</see> of <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>. 
    /// </en>
    /// </remarks>
    public interface ITerminalConnection {
        /// <summary>
        /// <ja>
        /// Ú‘±æî•ñ‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
        /// </ja>
        /// <en>
        /// Interface that show the connection information.
        /// </en>
        /// </summary>
        ITerminalParameter Destination {
            get;
        }
        /// <summary>
        /// <ja>
        /// ƒuƒŒ[ƒNM†‚Ì‘—M‚âAreYouThereA
        /// ƒ^[ƒ~ƒiƒ‹ƒTƒCƒY•ÏX‚Ì’Ê’m‚È‚ÇAƒ^[ƒ~ƒiƒ‹‚É“ÁŽê§Œä‚·‚éƒƒ\ƒbƒh‚ð‚à‚ÂITerminalOutput‚Å‚·B
        /// </ja>
        /// <en>
        /// It is ITerminalOutput with the method of the special control in terminals of the transmission of the break, AreYouThere, and the notification of the change of the size of the terminal, etc.
        /// </en>
        /// </summary>
        ITerminalOutput TerminalOutput {
            get;
        }
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚Ö‚Ì‘—ŽóM‹@”\‚ð‚à‚ÂIPoderosaSocket‚Å‚·B
        /// </ja>
        /// <en>
        /// IPoderosaSocket with transmitting and receiving function to terminal.
        /// </en>
        /// </summary>
        IPoderosaSocket Socket {
            get;
        }
        /// <summary>
        /// <ja>
        /// Ú‘±‚ª•Â‚¶‚Ä‚¢‚é‚©‚Ç‚¤‚©‚ðŽ¦‚µ‚Ü‚·Btrue‚Ì‚Æ‚«Ú‘±‚Í•Â‚¶‚Ä‚¢‚Ü‚·B
        /// </ja>
        /// <en>
        /// It is shown whether the connection closes. The connection close at true. 
        /// </en>
        /// </summary>
        bool IsClosed {
            get;
        }

        /// <summary>
        /// <ja>Ú‘±‚ð•Â‚¶‚Ü‚·B</ja>
        /// <en>Close the connection.</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// ‚±‚ÌƒRƒlƒNƒVƒ‡ƒ“‚ªƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚Æ‚µ‚ÄŽg‚í‚ê‚Ä‚¢‚éê‡‚É‚ÍA’¼Ú‚±‚Ìƒƒ\ƒbƒh‚ðŒÄ‚Ño‚³‚¸A
        /// ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‘¤‚©‚çØ’f‚µ‚Ä‚­‚¾‚³‚¢B
        /// </ja>
        /// <en>
        /// Please do not call this method directly when this connection is used as a terminal session, and cut it from the terminal session side. 
        /// </en>
        /// </remarks>
        void Close();

    }
}
