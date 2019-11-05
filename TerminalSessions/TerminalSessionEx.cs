/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSessionEx.cs,v 1.1 2010/11/19 15:41:20 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

//using Poderosa.Commands;
using Protocols;
using TerminalEmulator;
//using Poderosa.Forms;
using Core;

namespace TerminalSessions
{

    //ƒ^[ƒ~ƒiƒ‹Ú‘±‚ÌƒZƒbƒVƒ‡ƒ“
    //  TerminalEmulatorƒvƒ‰ƒOƒCƒ““à‚ÌƒRƒ}ƒ“ƒh‚ÍACommandTarget->View->Document->Session->TerminalSession->Terminal‚Æ’H‚Á‚ÄƒRƒ}ƒ“ƒhŽÀs‘ÎÛ‚ð“¾‚éB
    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the terminal session.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// Poderosa‚ð•W€‚Ìƒ^[ƒ~ƒiƒ‹ƒGƒ~ƒ…ƒŒ[ƒ^‚Æ‚µ‚Ä—p‚¢‚éê‡‚É‚ÍA<seealso cref="ISession">ISession</seealso>‚ÌŽÀ‘Ô‚ÍA
    /// ‚±‚ÌITerminalSession‚Å‚ ‚èAGetAdapter‚ðŽg‚Á‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <para>
    /// <para>
    /// ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ÍAŽŸ‚Ì•û–@‚ÅŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>ƒAƒNƒeƒBƒu‚ÈƒEƒBƒ“ƒhƒE^ƒrƒ…[‚©‚çŽæ“¾‚·‚éê‡</term>
    /// <description>
    /// <para>
    /// ƒEƒBƒ“ƒhƒEƒ}ƒl[ƒWƒƒ‚ÌActiveWindowƒvƒƒpƒeƒB‚ÍAƒAƒNƒeƒBƒuƒEƒBƒ“ƒhƒE‚ðŽ¦‚µ‚Ü‚·B
    /// ‚±‚ÌƒAƒNƒeƒBƒuƒEƒBƒ“ƒhƒE‚©‚çƒhƒLƒ…ƒƒ“ƒgA‚»‚µ‚ÄAƒZƒbƒVƒ‡ƒ“‚Ö‚Æ‚½‚Ç‚é‚±‚Æ‚Åƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ðŽæ“¾‚Å‚«‚Ü‚·B 
    /// </para>
    /// <code>
    /// // ƒEƒBƒ“ƒhƒEƒ}ƒl[ƒWƒƒ‚ðŽæ“¾
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// IWindowManager wm = cs.WindowManager;
    ///
    /// // ƒAƒNƒeƒBƒuƒEƒBƒ“ƒhƒE‚ðŽæ“¾
    /// IPoderosaMainWindow window = wm.ActiveWindow;
    ///
    /// // ƒrƒ…[‚ðŽæ“¾
    /// IPoderosaView view = window.LastActivatedView;
    /// 
    /// // ƒhƒLƒ…ƒƒ“ƒg‚ðŽæ“¾
    /// IPoderosaDocument doc = view.Document;
    /// 
    /// // ƒZƒbƒVƒ‡ƒ“‚ðŽæ“¾
    /// ISession session = doc.OwnerSession;
    /// 
    /// // ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚Ö‚Æ•ÏŠ·
    /// ITerminalSession termsession = 
    ///   (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// </code>
    /// </description>
    /// </item>
    /// <item><term>ƒƒjƒ…[‚âƒc[ƒ‹ƒo[‚Ìƒ^[ƒQƒbƒg‚©‚çƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ð“¾‚éê‡</term>
    /// <description>
    /// <para>
    /// ƒƒjƒ…[‚âƒc[ƒ‹ƒo[‚©‚çƒRƒ}ƒ“ƒh‚ªˆø‚«“n‚³‚ê‚é‚Æ‚«‚É‚ÍAƒ^[ƒQƒbƒg‚Æ‚µ‚Ä‘€ì‘ÎÛ‚ÌƒEƒBƒ“ƒhƒE‚ª“¾‚ç‚ê‚Ü‚·B
    /// ‚±‚Ìƒ^[ƒQƒbƒg‚ð—˜—p‚µ‚Äƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ð“¾‚é‚±‚Æ‚ª‚Å‚«‚Ü‚·B 
    /// </para>
    /// <para>
    /// <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>‚É‚ÍAƒAƒNƒeƒBƒu‚ÈƒhƒLƒ…ƒƒ“ƒg‚ð“¾‚é‚½‚ß‚ÌAsDocumentOrViewOrLastActivatedDocumentƒƒ\ƒbƒh‚ª‚ ‚è‚Ü‚·B
    /// ‚±‚Ìƒƒ\ƒbƒh‚ðŽg‚Á‚ÄƒhƒLƒ…ƒƒ“ƒg‚ðŽæ“¾‚µA‚»‚±‚©‚çITerminalSession‚Ö‚Æ•ÏŠ·‚·‚é‚±‚Æ‚ÅAƒ^[ƒQƒbƒg‚É‚È‚Á‚Ä‚¢‚éƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ðŽæ“¾‚Å‚«‚Ü‚·B 
    /// </para>
    /// <code>
    /// // target‚ÍƒRƒ}ƒ“ƒh‚É“n‚³‚ê‚½ƒ^[ƒQƒbƒg‚Å‚ ‚é‚Æ‘z’è‚µ‚Ü‚·
    /// // ƒhƒLƒ…ƒƒ“ƒg‚ðŽæ“¾
    /// IPoderosaDocument doc = 
    ///   CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
    /// if (doc != null)
    /// {
    ///   // ƒZƒbƒVƒ‡ƒ“‚ðŽæ“¾
    ///   ISession session = doc.OwnerSession;
    ///   // ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚Ö‚Æ•ÏŠ·
    ///   ITerminalSession termsession = 
    ///     (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The realities of <seealso cref="ISession">ISession</seealso> are these ITerminalSession when Poderosa is used as a standard terminal emulator, and it is possible to acquire it by using GetAdapter. 
    /// </para>
    /// <para>
    /// <para>
    /// The terminal session can be got in the following method. 
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Get from active window or view.</term>
    /// <description>
    /// <para>
    /// Window manager's ActiveWindow property shows the active window. The terminal session can be got by tracing it from this active window to the document and the session. 
    /// </para>
    /// <code>
    /// // Get the window manager.
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// IWindowManager wm = cs.WindowManager;
    ///
    /// // Get the active window.
    /// IPoderosaMainWindow window = wm.ActiveWindow;
    ///
    /// // Get the view.
    /// IPoderosaView view = window.LastActivatedView;
    /// 
    /// // Get the document.
    /// IPoderosaDocument doc = view.Document;
    /// 
    /// // Get the session
    /// ISession session = doc.OwnerSession;
    /// 
    /// // Convert to terminal session.
    /// ITerminalSession termsession = 
    ///   (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// </code>
    /// </description>
    /// </item>
    /// <item><term>Get the terminal session from the target of menu or toolbar.</term>
    /// <description>
    /// <para>
    /// When the command is handed over from the menu and the toolbar, the window to be operated as a target is obtained. 
    /// The terminal session can be obtained by using this target. 
    /// </para>
    /// <para>
    /// In <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>, there is AsDocumentOrViewOrLastActivatedDocument method to obtain an active document. 
    /// </para>
    /// <code>
    /// // It is assumed that target is a target passed to the command. 
    /// // Get the document.
    /// IPoderosaDocument doc = 
    ///   CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
    /// if (doc != null)
    /// {
    ///   // Get the session.
    ///   ISession session = doc.OwnerSession;
    ///   // Convert to terminal session.
    ///   ITerminalSession termsession = 
    ///     (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </en>
    /// </remarks>
    public interface ITerminalSession : ISession {
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹‚ðŠÇ—‚·‚éƒIƒuƒWƒFƒNƒg‚Å‚·B</ja>
        /// <en>Object that manages terminal.</en>
        /// </summary>
        /// <remarks>
        /// <ja>‚±‚ÌƒIƒuƒWƒFƒNƒg‚ÍA‘—ŽóM‚ðƒtƒbƒN‚µ‚½‚¢ê‡‚âƒƒO‚ð‚Æ‚è‚½‚¢ê‡‚È‚Ç‚É—p‚¢‚Ü‚·B</ja><en>This object uses transmitting and receiving to hook and to take the log. </en>
        /// </remarks>
        AbstractTerminal Terminal { get; }
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚Ìƒ†[ƒU[ƒCƒ“ƒ^[ƒtƒFƒCƒX‚ð’ñ‹Ÿ‚·‚éƒRƒ“ƒgƒ[ƒ‹‚Å‚·B
        /// </ja>
        /// <en>
        /// Control that offers user interface of terminal.
        /// </en>
        /// </summary>
        //TerminalControl TerminalControl { get; }
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹Ý’è‚ðŽ¦‚·ƒIƒuƒWƒFƒNƒg‚Å‚·B</ja>
        /// <en>Object that shows terminal setting.</en>
        /// </summary>
        ITerminalSettings TerminalSettings { get; }
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹‚ÌÚ‘±‚ðŽ¦‚·ƒIƒuƒWƒFƒNƒg‚Å‚·B</ja>
        /// <en>Object that shows connection of terminal.</en>
        /// </summary>
        ITerminalConnection TerminalConnection { get; }
        /// <summary>
        /// <ja>Š—L‚·‚éƒEƒBƒ“ƒhƒE‚ðŽ¦‚µ‚Ü‚·B</ja>
        /// <en>The owned window is shown. </en>
        /// </summary>
        //IPoderosaMainWindow OwnerWindow { get; }
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹‚Ö‚Ì‘—M‹@”\‚ð’ñ‹Ÿ‚µ‚Ü‚·B</ja>
        /// <en>The transmission function to the terminal is offered. </en>
        /// </summary>
        TerminalTransmission TerminalTransmission { get; }
    }


    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹ƒT[ƒrƒX‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that provides terminal service.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAV‹KTelnet^SSH^CygwinÚ‘±‚Ì‹@”\‚ð’ñ‹Ÿ‚µ‚Ü‚·B
    /// </para>
    /// <para>
    /// TerminalSessionPluginƒvƒ‰ƒOƒCƒ“iƒvƒ‰ƒOƒCƒ“IDFuorg.poderosa.terminalsessionsvj‚É‚æ‚Á‚Ä
    /// ’ñ‹Ÿ‚³‚ê‚Ä‚¨‚èAŽŸ‚Ì‚æ‚¤‚É‚µ‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <code>
    /// ITerminalSessionsService termservice = 
    ///  (ITerminalSessionsService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
    /// Debug.Assert(termservice != null);
    /// </code>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface offers the function of a new Telnet/SSH/Cygwin connection. 
    /// </para>
    /// <para>
    /// It is offered by the TerminalSessionPlugin plug-in (plugin ID[org.poderosa.terminalsessions]) , and it is possible to get it as follows. 
    /// </para>
    /// <code>
    /// ITerminalSessionsService termservice = 
    ///  (ITerminalSessionsService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
    /// Debug.Assert(termservice != null);
    /// </code>
    /// </en>
    /// </remarks>
    public interface ITerminalSessionsService {
        /// <summary>
        /// <ja>
        /// V‹Kƒ^[ƒ~ƒiƒ‹Ú‘±‚ð‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// The interface to connect a new terminal is shown. 
        /// </en>
        /// </summary>
        ITerminalSessionStartCommand TerminalSessionStartCommand { get; }
        /// <summary>
        /// <ja>
        /// Ú‘±ƒRƒ}ƒ“ƒh‚ÌƒJƒeƒSƒŠ‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// The category of connected command is shown. 
        /// </en>
        /// </summary>
        //ICommandCategory ConnectCommandCategory { get; }
    }

    /// <summary>
    /// <ja>
    /// V‹Kƒ^[ƒ~ƒiƒ‹‚ÌÚ‘±‹@”\‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that offers connected function of new terminal.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="Poderosa.Sessions.ITerminalSessionsService">ITerminalSessionsServicen</seealso>‚Ì
    /// <see cref="Poderosa.Sessions.ITerminalSessionsService.TerminalSessionStartCommand">TerminalSessionStartCommandƒvƒƒpƒeƒB</see>
    /// ‚©‚çŽæ“¾‚Å‚«‚Ü‚·B</ja>
    /// <en>This interface can be got from the <see cref="Poderosa.Sessions.ITerminalSessionsService.TerminalSessionStartCommand">TerminalSessionStartCommand property</see> of ITerminalSessionsServicen. </en>
    /// </remarks>
    public interface ITerminalSessionStartCommand {
        /// <summary>
        /// <ja>Šù‘¶‚ÌÚ‘±‚ð—p‚¢‚ÄV‹Kƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ðŠJŽn‚µ‚Ü‚·B</ja><en>A new terminal session is begun by using an existing connection. </en>
        /// </summary>
        /// <param name="target"><ja>ƒ^[ƒ~ƒiƒ‹‚ÉŒ‹‚Ñ‚Â‚¯‚éƒrƒ…[‚Ü‚½‚ÍƒEƒBƒ“ƒhƒE</ja><en>View or window that ties to terminal</en></param>
        /// <param name="existing_connection"><ja>Šù‘¶‚ÌÚ‘±ƒIƒuƒWƒFƒNƒg</ja><en>Existing connected object</en></param>
        /// <param name="settings"><ja>ƒ^[ƒ~ƒiƒ‹Ý’è‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja><en>Object where terminal setting is stored</en></param>
        /// <returns><ja>ŠJŽn‚³‚ê‚½ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ª•Ô‚³‚ê‚Ü‚·</ja><en>The begun terminal session is returned. </en></returns>
        /// <overloads>
        /// <summary>
        /// <ja>V‹Kƒ^[ƒ~ƒiƒ‹Ú‘±‚ðŠJŽn‚µ‚Ü‚·B</ja><en>Start a new terminal session.</en>
        /// </summary>
        /// </overloads>
        ITerminalSession StartTerminalSession(/* ICommandTarget target, */ ITerminalConnection existing_connection, ITerminalSettings settings);
        //ITerminalParameter‚ÍATelnet/SSH/Cygwin‚Ì‚¢‚¸‚ê‚©‚Å‚ ‚é•K—v‚ª‚ ‚éB
        /// <summary>
        /// <ja>
        /// Ú‘±ƒpƒ‰ƒ[ƒ^‚ð—p‚¢‚ÄV‹KÚ‘±‚ð‚µA‚»‚Ìƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ðŠJŽn‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// It newly connects by using connected parameter, and the terminal session is begun. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>ƒ^[ƒ~ƒiƒ‹‚ÉŒ‹‚Ñ‚Â‚¯‚éƒrƒ…[‚Ü‚½‚ÍƒEƒBƒ“ƒhƒE</ja><en>View or window that ties to terminal</en></param>
        /// <param name="destination">
        /// <ja>Ú‘±Žž‚Ìƒpƒ‰ƒ[ƒ^‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒgB
        /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso>A
        /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>A
        /// <seealso cref="ITCPParameter">ITCPParameter</seealso>‚Ì‚¢‚¸‚ê‚©‚Å‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB</ja>
        /// <en>
        /// Object where parameter when connecting it is stored. 
        /// It should be either <seealso cref="ICygwinParameter">ICygwinParameter</seealso>, <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso> or <seealso cref="ITCPParameter">ITCPParameter</seealso>. </en>
        /// </param>
        /// <param name="settings"><ja>ƒ^[ƒ~ƒiƒ‹Ý’è‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja><en>Object where terminal setting is stored</en></param>
        /// <returns><ja>ŠJŽn‚³‚ê‚½ƒ^[ƒ~ƒiƒ‹ƒZƒbƒVƒ‡ƒ“‚ª•Ô‚³‚ê‚Ü‚·</ja><en>The begun terminal session is returned. </en></returns>
        ITerminalSession StartTerminalSession(/* ICommandTarget target, */ ITerminalParameter destination, ITerminalSettings settings);

        /// <summary>
        /// <ja>ƒZƒbƒVƒ‡ƒ“‚Æ‚Í–³ŠÖŒW‚ÉÚ‘±‚¾‚¯ŠJ‚«‚Ü‚·</ja>
        /// <en>Opens not any session but connection</en>
        /// </summary>
        /// <exclude/>
        ITerminalConnection OpenConnection(ITerminalParameter destination, ITerminalSettings settings);

        //void OpenShortcutFile(ICommandTarget target, string filename);
    }

    //ITerminalParameter‚ðƒCƒ“ƒXƒ^ƒ“ƒVƒG[ƒg‚µ‚ÄITerminalConnection‚É‚·‚éExtensionPoint‚ÌƒCƒ“ƒ^ƒtƒF[ƒX
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalConnectionFactory {
        bool IsSupporting(ITerminalParameter param, ITerminalSettings settings);
        ITerminalConnection EstablishConnection(ITerminalParameter param, ITerminalSettings settings);
    }


    //ƒƒOƒCƒ“ƒ_ƒCƒAƒƒO‚ÌŽg‚¢ŸŽèŒüã—p‚ÌƒTƒ|[ƒg
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITelnetSSHLoginDialogInitializeInfo {
        //Ú‘±æŒó•â
        void AddHost(string value);
        void AddAccount(string value);
        void AddIdentityFile(string value);
        void AddPort(int value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITelnetSSHLoginDialogInitializer {
        void ApplyLoginDialogInfo(ITelnetSSHLoginDialogInitializeInfo info);
    }
   
    //Extension Point‚ª’ñ‹Ÿ
    //Šù‚ÉŠi”[‚³‚ê‚Ä‚¢‚éî•ñ‚Í‰ó‚³‚È‚¢‚æ‚¤‚É‚·‚é‚Ì‚ªƒ‹[ƒ‹
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ILoginDialogUISupport {
        //‚Q‚Â‚Ì–ß‚è’l‚ª‚ ‚é‚Ì‚Åout‚ðŽg‚¤Badapter‚ÍATerminalParameter‚ÌŽí—Þ‚ðŽw’è‚·‚é‚½‚ß‚Ìˆø”B‘Î‰ž‚·‚é‚à‚Ì‚ª‚È‚¢‚Æ‚«‚Ínull‚ð•Ô‚·
        void FillTopDestination(Type adapter, out ITerminalParameter parameter, out ITerminalSettings settings);
        //ƒzƒXƒg–¼‚ÅŽw’è‚·‚é
        void FillCorrespondingDestination(Type adapter, string destination, out ITerminalParameter parameter, out ITerminalSettings settings);
    }

    //Terminal SessionŒÅ—LƒIƒvƒVƒ‡ƒ“
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalSessionOptions {
        bool AskCloseOnExit { get; set; }

        //preference editor only
        int TerminalEstablishTimeout { get; }
        string GetDefaultLoginDialogUISupportTypeName(string logintype);
    }

}
