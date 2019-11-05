/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SessionEx.cs,v 1.1 2010/11/19 15:40:39 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;

//using Poderosa.Forms;
//using Poderosa.Document;
//using Poderosa.Commands;

//namespace Poderosa.Sessions
namespace Core
{
    /// <summary>
    /// <ja>
    /// ƒZƒbƒVƒ‡ƒ“ƒ}ƒl[ƒWƒƒ‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that shows session manager.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍƒZƒbƒVƒ‡ƒ“ƒ}ƒl[ƒWƒƒiSessionManagerPluginƒvƒ‰ƒOƒCƒ“Fƒvƒ‰ƒOƒCƒ“IDuorg.poderosa.core.sessionsvj
    /// ‚É‚æ‚Á‚Ä’ñ‹Ÿ‚³‚ê‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚ ‚èAƒZƒbƒVƒ‡ƒ“î•ñ‚ð‘€ì‚µ‚Ü‚·B
    /// </para>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>‚Ì
    /// <see cref="Poderosa.Plugins.ICoreServices.SessionManager">SessionManagerƒvƒƒpƒeƒB</see>
    /// ‚ðŽg‚Á‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface is an interface offered by the session manager (SessionManagerPlugin plug-in : Plug-inID "org.poderosa.core.sessions") , and session information is operated. 
    /// </para>
    /// <para>
    /// This interface can be acquired by using the <see cref="Poderosa.Plugins.ICoreServices.SessionManager">SessionManager property</see> of <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>. 
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// ISessionManager‚ðŽæ“¾‚µ‚Ü‚·B
    /// <code>
    /// // ICoreServices‚ðŽæ“¾
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // ISessionManager‚ðŽæ“¾
    /// ISessionManager sessionman = cs.SessionManager;
    /// </code>
    /// </ja>
    /// <en>
    /// Get the ISessionManager.
    /// <code>
    /// // Get the ICoreServices.
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get the ISessionManager.
    /// ISessionManager sessionman = cs.SessionManager;
    /// </code>
    /// </en>
    /// </example>
    public interface ISessionManager {
        //Structure
        /// <summary>
        /// <ja>
        /// ‚·‚×‚Ä‚ÌƒZƒbƒVƒ‡ƒ“‚ð—ñ‹“‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Enumerate all sessions.
        /// </en>
        /// </summary>
        IEnumerable<ISession> AllSessions { get; }

        //Start/End
        /// <summary>
        /// <ja>
        /// V‚µ‚¢ƒZƒbƒVƒ‡ƒ“‚ðŠJŽn‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Start a new session.
        /// </en>
        /// </summary>
        /// <param name="session"><ja>ŠJŽn‚·‚éƒZƒbƒVƒ‡ƒ“</ja><en>Session to start.</en></param>
        /// <param name="firstView"><ja>ƒZƒbƒVƒ‡ƒ“‚ÉŠ„‚è“–‚Ä‚éƒrƒ…[</ja><en>View allocated in session</en></param>
        /// <remarks>
        /// <ja>
        /// V‚µ‚­ƒZƒbƒVƒ‡ƒ“‚ðì¬‚·‚é‚½‚ß‚Ìƒrƒ…[‚ÍA<seealso cref="IViewManager">IViewManager</seealso>‚Ì
        /// <see cref="IViewManager.GetCandidateViewForNewDocument">GetCandidateViewForNewDocumentƒƒ\ƒbƒh</see>
        /// ‚Åì‚é‚±‚Æ‚ª‚Å‚«‚Ü‚·B
        /// </ja>
        /// <en>
        /// The view to make the session newly can be made by the <see cref="IViewManager.GetCandidateViewForNewDocument">GetCandidateViewForNewDocument method</see> of IViewManager. 
        /// </en>
        /// </remarks>
        void StartNewSession(ISession session);

        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚Ü‚·B
        /// </ja>
        /// <en>
        /// Close the session.
        /// </en>
        /// </summary>
        /// <param name="session"><ja>•Â‚¶‚½‚¢ƒZƒbƒVƒ‡ƒ“‚Å‚·B</ja><en>Session to close.</en></param>
        /// <returns><ja>ƒZƒbƒVƒ‡ƒ“‚ª•Â‚¶‚ç‚ê‚½‚©‚Ç‚¤‚©‚ðŽ¦‚·’l‚Å‚·B</ja><en>It is a value in which it is shown whether the session was closed. </en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ðŒÄ‚Ño‚·‚ÆAƒZƒbƒVƒ‡ƒ“‚ð\¬‚·‚é<seealso cref="ISession">ISession</seealso>
        /// ‚Ì<see cref="ISession.PrepareCloseSession">PrepareCloseSessionƒƒ\ƒbƒh</see>
        /// ‚ªŒÄ‚Ño‚³‚ê‚Ü‚·B<see cref="ISession.PrepareCloseSession">PrepareCloseSessionƒƒ\ƒbƒh</see>‚ªPrepareCloseResult.Cancel
        /// ‚ð•Ô‚µ‚½‚Æ‚«‚É‚ÍAƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é“®ì‚Í’†Ž~‚³‚ê‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When this method is called, the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> of <seealso cref="ISession">ISession</seealso> that composes the session is called. When the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> returns PrepareCloseResult.Cancel, operation that shuts the session is discontinued. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult TerminateSession(ISession session);

        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ªŠJŽn‚³‚ê‚½‚èØ’f‚³‚ê‚½‚Æ‚«‚Ì’Ê’m‚ðŽó‚¯Žæ‚éƒŠƒXƒi‚ð“o˜^‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// The listener that is begun the session and receives the notification when close is registered. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>“o˜^‚·‚éƒŠƒXƒi</ja><en>Listener to regist</en></param>
        void AddSessionListener(ISessionListener listener);
        /// <summary>
        /// <ja>ƒZƒbƒVƒ‡ƒ“‚ªŠJŽn‚³‚ê‚½‚èØ’f‚³‚ê‚½‚Æ‚«‚Ì’Ê’m‚ðŽó‚¯Žæ‚éƒŠƒXƒi‚ð‰ðœ‚µ‚Ü‚·B</ja>
        /// <en>The listener that is begun the session and receives the notification when close is released.</en>
        /// </summary>
        /// <param name="listener"><ja>‰ðœ‚·‚éƒŠƒXƒi</ja><en>Listener to release.</en></param>
        void RemoveSessionListener(ISessionListener listener);
    }

    /// <summary>
    /// <ja>
    /// ƒZƒbƒVƒ‡ƒ“ƒzƒXƒgƒIƒuƒWƒFƒNƒg‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that shows session host object.
    /// </en>
    /// </summary>
    public interface ISessionHost {
        //TODO RemoveDocument‚ ‚Á‚Ä‚æ‚¢
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ðƒvƒ‰ƒOƒCƒ“‘¤‚©‚çI—¹‚³‚¹‚Ü‚·B
        /// </ja>
        /// <en>
        /// Terminate the sessoin from plug-in side.
        /// </en>
        /// </summary>
        void TerminateSession();
    }

    //ƒAƒNƒeƒBƒu‚É‚·‚é‘€ì‚ÌŠJŽnðŒ
    /// <summary>
    /// <ja>
    /// ƒhƒLƒ…ƒƒ“ƒg‚ªƒAƒNƒeƒBƒu‚É‚È‚Á‚½‚Æ‚«‚Ì——R‚ðŽ¦‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// The reason when the document becomes active is shown. 
    /// </en>
    /// </summary>
    public enum ActivateReason {
        /// <summary>
        /// <ja>“à•”“®ì‚É‚æ‚èƒAƒNƒeƒBƒu‚É‚È‚Á‚½</ja>
        /// <en>It became active by internal operation. </en>
        /// </summary>
        InternalAction,
        /// <summary>
        /// <ja>ƒ^ƒuƒNƒŠƒbƒN‚É‚æ‚èƒAƒNƒeƒBƒu‚É‚È‚Á‚½</ja>
        /// <en>It became active by the tab click. </en>
        /// </summary>
        TabClick,
        /// <summary>
        /// <ja>ƒrƒ…[‚ªƒtƒH[ƒJƒX‚ðŽó‚¯Žæ‚Á‚½‚½‚ß‚ÉƒAƒNƒeƒBƒu‚É‚È‚Á‚½</ja>
        /// <en>Because the view had got focus, it became active. </en>
        /// </summary>
        ViewGotFocus,
        /// <summary>
        /// <ja>ƒhƒ‰ƒbƒO•ƒhƒƒbƒv‘€ì‚É‚æ‚èƒAƒNƒeƒBƒu‚É‚È‚Á‚½</ja>
        /// <en>It became active by the drag &amp; drop operation. </en>
        /// </summary>
        DragDrop
    }

    /// <summary>
    /// <ja>
    /// ƒhƒLƒ…ƒƒ“ƒg‚âƒZƒbƒVƒ‡ƒ“‚ª•Â‚¶‚ç‚ê‚æ‚¤‚Æ‚·‚é‚Æ‚«‚Ì–ß‚è’l‚ðŽ¦‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// The return value when the document and the session start being shut is shown. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ‚±‚Ì—ñ‹“‘Ì‚ÍA<seealso cref="ISession">ISession</seealso>‚Ì<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentƒƒ\ƒbƒh</see>
    /// ‚â<see cref="ISession.PrepareCloseSession">PrepareCloseSessionƒƒ\ƒbƒh</see>‚Ì–ß‚è’l‚Æ‚µ‚ÄŽg‚í‚ê‚Ü‚·B
    /// </para>
    /// <para>
    /// PrepareCloseResult.ContinueSession‚ªŽg‚í‚ê‚é‚Ì‚ÍA<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentƒƒ\ƒbƒh</see>‚Ì‚Æ‚«‚¾‚¯‚Å‚·B
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This enumeration is used as a return value of the <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see> and the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> of <seealso cref="ISession">ISession</seealso>. 
    /// </para>
    /// <para>
    /// Only PrepareCloseResult.ContinueSession is used on <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see>
    /// </para>
    /// </en>
    /// </remarks>
    public enum PrepareCloseResult {
        /// <summary>
        /// <ja>
        /// ƒhƒLƒ…ƒƒ“ƒg‚Í•Â‚¶‚Ü‚·‚ªAƒZƒbƒVƒ‡ƒ“‚Í•Â‚¶‚Ü‚¹‚ñB
        /// </ja>
        /// <en>
        /// Close the document, but session is not close.
        /// </en>
        /// </summary>
        ContinueSession,
        /// <summary>
        /// <ja>
        /// ƒhƒLƒ…ƒƒ“ƒg‚âƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚Ü‚·B
        /// </ja>
        /// <en>
        /// Close the document and the session.
        /// </en>
        /// </summary>
        TerminateSession,
        /// <summary>
        /// <ja>
        /// ƒhƒLƒ…ƒƒ“ƒg‚âƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é“®ì‚ðƒLƒƒƒ“ƒZƒ‹‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Cancel closing the document and the session.
        /// </en>
        /// </summary>
        Cancel
    }

    /// <summary>
    /// <ja>
    /// ƒZƒbƒVƒ‡ƒ“‚ðŽ¦‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// The session is shown. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// •W€‚Ìƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^‚Æ‚µ‚Ä—p‚¢‚éê‡AISession‚ÌŽÀ‘Ô‚ÍAISession‚©‚çŒp³‚µ‚Ä‚¢‚é
    /// <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>‚Å‚ ‚èAGetAdapterƒƒ\ƒbƒh‚Å•ÏŠ·‚Å‚«‚Ü‚·B
    /// </ja>
    /// <en>
    /// The realities of ISession are <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso> that has been succeeded to, 
    /// and can be converted from ISession by the GetAdapter method when using it as a standard terminal emulator. 
    /// </en>
    /// </remarks>
    public interface ISession {
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ÌƒLƒƒƒvƒVƒ‡ƒ“‚Å‚·B
        /// </ja>
        /// <en>
        /// Caption of the session.
        /// </en>
        /// </summary>
        string Caption { get; }
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ÌƒAƒCƒRƒ“‚Å‚·B
        /// </ja>
        /// <en>
        /// Icon of the session.
        /// </en>
        /// </summary>
        //Image Icon { get; } //16*16

        //ˆÈ‰º‚ÍSessionManager‚ªŒÄ‚ÔB‚±‚êˆÈŠO‚Å‚ÍŒÄ‚ñ‚Å‚Í‚¢‚¯‚È‚¢
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“ƒ}ƒl[ƒWƒƒ‚©‚çŒÄ‚Ño‚³‚ê‚é‰Šú‰»‚Ìƒƒ\ƒbƒh‚Å‚·B
        /// </ja>
        /// <en>
        /// Initialization called from session manager method.
        /// </en>
        /// </summary>
        /// <param name="host"><ja>ƒZƒbƒVƒ‡ƒ“‚ð‘€ì‚·‚é‚½‚ß‚ÌƒZƒbƒVƒ‡ƒ“ƒzƒXƒgƒIƒuƒWƒFƒNƒg‚Å‚·B</ja>
        /// <en>Session host object to operate session.</en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ÍA<seealso cref="ISessionManager">ISessionManager</seealso>‚Ì<see cref="ISessionManager.StartNewSession">StartNewSessionƒƒ\ƒbƒh</see>
        /// ‚ªŒÄ‚Ño‚³‚ê‚½‚Æ‚«‚ÉAƒZƒbƒVƒ‡ƒ“ƒ}ƒl[ƒWƒƒ‚É‚æ‚Á‚ÄŠÔÚ“I‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// ŠJ”­ŽÒ‚ÍA‚±‚Ìƒƒ\ƒbƒh‚ð’¼ÚŒÄ‚Ño‚µ‚Ä‚Í‚¢‚¯‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// ŠJ”­ŽÒ‚Íˆê”Ê‚ÉA‚±‚Ìƒƒ\ƒbƒh‚Ìˆ—‚É‚¨‚¢‚ÄƒhƒLƒ…ƒƒ“ƒg‚ðì¬‚µA<paramref name="host">host</paramref>‚Æ‚µ‚Ä“n‚³‚ê‚½<seealso cref="ISessionHost">ISessionHost</seealso>
        /// ‚Ì<see cref="ISessionHost.RegisterDocument">RegisterDocumentƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µ‚ÄƒhƒLƒ…ƒƒ“ƒg‚ð“o˜^‚µ‚Ü‚·B
        /// </para>
        /// <para>
        /// ƒZƒbƒVƒ‡ƒ“‚ÌÚ×‚É‚Â‚¢‚Ä‚ÍA<see href="chap04_02_04.html">ƒZƒbƒVƒ‡ƒ“‚Ì‘€ì</see>‚ðŽQÆ‚µ‚Ä‚­‚¾‚³‚¢B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="ISessionManager.StartNewSession">StartNewSession method</see> of 
        /// <seealso cref="ISessionManager">ISessionManager</seealso> is called, this method is 
        /// indirectly called by the session manager. The developer must not call this method directly. 
        /// </para>
        /// <para>
        /// The developer makes the document in general in the processing of this method, calls the <see cref="ISessionHost.RegisterDocument">RegisterDocument method</see> of <seealso cref="ISessionHost">ISessionHost</seealso> passed as <paramref name="host">host</paramref>, and registers the document. 
        /// </para>
        /// <para>
        /// Please refer to <see href="chap04_02_04.html">Operation of session</see> for details of the session. 
        /// </para>
        /// </en>
        /// </remarks>
        void InternalStart(/* ISessionHost host */);

        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ªI—¹‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚éƒƒ\ƒbƒh‚Å‚·B
        /// </ja>
        /// <en>
        /// It is a method of the call when the session ends. 
        /// </en>
        /// </summary>
        void InternalTerminate();

        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚Ä‚à‚æ‚¢‚©‚ðŒˆ’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// It is decided whether I may close the session. 
        /// </en>
        /// </summary>
        /// <returns><ja>ƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é‚©‚Ç‚¤‚©‚ðŒˆ’è‚·‚é’l‚Å‚·B</ja><en>It is a value in which it is decided whether to close the session. </en></returns>
        /// <remarks>
        /// <ja>
        ///    <para>
        ///    ‚±‚Ìƒƒ\ƒbƒh‚ÍAƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é‘€ì‚ªs‚í‚ê‚é‚ÆŒÄ‚Ño‚³‚ê‚Ü‚·B
        ///    </para>
        ///    <para>
        ///    ŠJ”­ŽÒ‚ÍƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é‚©‚Ç‚¤‚©‚ð<seealso cref="T:Poderosa.Sessions.PrepareCloseResult">PrepareCloseResult—ñ‹“‘Ì</seealso>
        ///    ‚Æ‚µ‚Ä•Ô‚µ‚Ä‚­‚¾‚³‚¢BPrepareCloseResult.Cancel‚ð•Ô‚µ‚½ê‡AƒZƒbƒVƒ‡ƒ“‚ð•Â‚¶‚é“®ì‚ÍŽæ‚èÁ‚³‚ê‚Ü‚·B
        ///    </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the operation that close the session is done, this method is called. 
        /// </para>
        /// <para>
        /// The developer must return whether to close the document as 
        /// <seealso cref="PrepareCloseResult">PrepareCloseResult enumeration</seealso>. 
        /// When PrepareCloseResult.Cancel is returned, operation that closed the document is canceled. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult PrepareCloseSession();
    }

    /// <summary>
    /// <ja>
    /// ƒZƒbƒVƒ‡ƒ“‚ªŠJŽn^Ø’f‚³‚ê‚½‚Æ‚«‚Ì’Ê’m‚ðŽó‚¯Žæ‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that receives notification when session begin/finish
    /// </en>
    /// </summary>
    public interface ISessionListener {
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ªŠJŽn‚³‚ê‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the session is started, it is called. 
        /// </en>
        /// </summary>
        /// <param name="session"><ja>ŠJŽn‚³‚ê‚½ƒZƒbƒVƒ‡ƒ“</ja><en>Started session.</en></param>
        void OnSessionStart(ISession session);
        /// <summary>
        /// <ja>
        /// ƒZƒbƒVƒ‡ƒ“‚ªI—¹‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// When the session ends, it is called. 
        /// </en>
        /// </summary>
        /// <param name="session"><ja>I—¹‚µ‚½ƒZƒbƒVƒ‡ƒ“</ja><en>Ended session</en></param>
        void OnSessionEnd(ISession session);
    }

}
