/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolsEx.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

//using Poderosa.Forms;
using Granados;

namespace Protocols
{
    public interface IConsoleMain
    {
        void NetLog(string text);
        IProtocolOptions ProtocolOptions { get; }
    }

    /// <summary>
    /// <ja>
    /// V‹K‚Éƒ^[ƒ~ƒiƒ‹Ú‘±‚ð‚µ‚½‚Æ‚«A‚»‚ê‚ðƒLƒƒƒ“ƒZƒ‹‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface to cancel it when terminal was newly connected.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
    /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectƒƒ\ƒbƒh</see>A
    /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectƒƒ\ƒbƒh</see>A
    /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectƒƒ\ƒbƒh</see>‚Ì–ß‚è’l‚Æ‚µ‚ÄŽg‚í‚ê‚Ü‚·B
    /// </ja>
    /// <en>
    /// This interface is used as a return value of the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see> and the method of <seealso cref="IProtocolService">IProtocolService</seealso> of <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see>. 
    /// </en>
    /// </remarks>
    public interface IInterruptable {
        /// <summary>
        /// <ja>
        /// Ú‘±‚ð’†Ž~‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Interrupt the connection.
        /// </en>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ðŒÄ‚Ño‚·‚ÆA<seealso cref="IInterruptableConnectorClient">IInterruptableConnectorClient</seealso>‚É
        /// ŽÀ‘•‚µ‚½ƒƒ\ƒbƒh‚ÍŒÄ‚Ño‚³‚ê‚¸‚ÉAÚ‘±‚ª’†Ž~‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// The connection is discontinued without calling the method of implementing on <seealso cref="IInterruptableConnectorClient">IInterruptableConnectorClient</seealso> when this method is called. 
        /// </en>
        /// </remarks>
        /// </summary>
        void Interrupt();
    }

    /// <summary>
    /// <ja>
    /// V‹K‚Éƒ^[ƒ~ƒiƒ‹ƒRƒlƒNƒVƒ‡ƒ“‚ð”ñ“¯Šú‚Åì¬‚·‚é‚Æ‚«AÚ‘±‚Ì¬Œ÷‚âŽ¸”s‚Ìó‘Ô‚ðŽó‚¯Žæ‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface to receive state of success and failure of connection when terminal connection is asynchronously newly made
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
    /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectƒƒ\ƒbƒh</see>A
    /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectƒƒ\ƒbƒh</see>A
    /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µ‚ÄA”ñ“¯Šú‚ÌÚ‘±‚ð‚·‚éÛA
    /// ¬Œ÷‚âŽ¸”s‚Ìó‘Ô‚ðŽó‚¯Žæ‚é‚½‚ß‚É—p‚¢‚Ü‚·B
    /// </para>
    /// <para>
    /// ŠÈˆÕ“I‚È“¯ŠúÚ‘±‚ð‚·‚é‚Ì‚Å‚ ‚ê‚ÎA‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ðŽÀ‘•‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð—pˆÓ‚·‚é‘ã‚í‚è‚ÉA
    /// <seealso cref="IProtocolService">IProtocolService</seealso>‚Ì<see cref="IProtocolService.CreateSynchronizedConnector">CreateSynchronizedConnectorƒƒ\ƒbƒh</see>
    /// ‚ðŒÄ‚Ño‚µA‚»‚Ì<see cref="ISynchronizedConnector.InterruptableConnectorClient">InterruptableConnectorClientƒvƒƒpƒeƒB</see>‚Ì
    /// ’l‚ðŽg‚¤‚±‚Æ‚à‚Å‚«‚Ü‚·B
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface is used to receive the state of the success and the failure when the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see>, the <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, and the <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> are called, and the asynchronous system is connected. 
    /// </para>
    /// <para>
    /// The <see cref="IProtocolService.CreateSynchronizedConnector">CreateSynchronizedConnector method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> can be called instead of preparing the object that mounts this interface, and the value of the <see cref="ISynchronizedConnector.InterruptableConnectorClient">InterruptableConnectorClient property </see>be used if it simplicity and synchronous connects it. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IInterruptableConnectorClient {
        /// <summary>
        /// <ja>
        /// Ú‘±‚ª¬Œ÷‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// Called when the connection is succeeded.
        /// </en>
        /// </summary>
        /// <param name="result"><ja>Ú‘±‚ªŠ®—¹‚µ‚½ƒRƒlƒNƒVƒ‡ƒ“‚Å‚·B</ja><en>Connection that connection is completed.</en></param>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ªŒÄ‚Ño‚³‚ê‚½‚çÚ‘±‚ÍŠ®—¹‚µ‚Ä‚¢‚Ü‚·BˆÈ~A<paramref name="result"/>‚ð’Ê‚¶‚Äƒf[ƒ^‚ð‘—ŽóM‚Å‚«‚Ü‚·B
        /// </ja>
        /// <en>
        /// If this method is called, the connection is completed. Data can be sent and received at the following through <paramref name="result"/>. 
        /// </en>
        /// </remarks>
        void SuccessfullyExit(ITerminalConnection result);
        /// <summary>
        /// <ja>
        /// Ú‘±‚ªŽ¸”s‚µ‚½‚Æ‚«‚ÉŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// Called when the connection is failed.
        /// </en>
        /// </summary>
        /// <param name="message"><ja>Ž¸”s‚ð‚°‚éƒƒbƒZ[ƒW‚Å‚·B</ja><en>Message to report failure</en></param>
        void ConnectionFailed(string message);

        ConsoleMain ConMain { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <exclude/>
    public delegate void SuccessfullyExitDelegate(ITerminalConnection connection);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <exclude/>
    public delegate void ConnectionFailedDelegate(string message);

    /// <summary>
    /// <ja>
    /// ŠÈˆÕ“I‚È“¯ŠúÚ‘±‹@”\‚Ì‚½‚ß‚Ì<see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see>‚ð’ñ‹Ÿ‚µA
    /// Ú‘±‚ÌŠ®—¹‚Ü‚½‚ÍƒGƒ‰[‚Ì”­¶‚Ü‚½‚Íƒ^ƒCƒ€ƒAƒEƒg‚Ü‚ÅAÚ‘±Š®—¹‚ð‘Ò‚Â‹@”\‚ð’ñ‹Ÿ‚µ‚Ü‚·B
    /// </ja>
    /// <en>
    /// <see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see> for a simple synchronization and the connection functions is offered, and the function to wait for connected completion is offered until generation or the time-out of completion or the error of the connection. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ŠJ”­ŽÒ‚ÍŽŸ‚Ì‚æ‚¤‚É‚·‚é‚±‚Æ‚ÅAÚ‘±‚ªŠ®—¹‚·‚é‚Ü‚Å‘Ò‚Â‚±‚Æ‚ª‚Å‚«‚Ü‚·B
    /// </para>
    /// <code>
    /// // <value>form</value>‚Íƒ†[ƒU[‚É•\Ž¦‚·‚éƒtƒH[ƒ€‚Å‚·
    /// ISynchronizedConnector sc = protocolservice.CreateSynchronizedConnector(<value>form</value>);
    /// // <value>sshparam</value>‚ÍSSHÚ‘±‚Ìƒpƒ‰ƒ[ƒ^‚Å‚·
    /// IInterrutable t = protocol_service.AsyncSSHConnect(sc.InterruptableConnectorClient, sshparam);
    /// // 30•bŠÔ‘Ò‚Â
    /// int timeout = 30 * 1000;
    /// ITerminalConnection connection = sc.WaitConnection(t, timeout);
    /// </code>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer can wait until the connection is completed by doing as follows. 
    /// </para>
    /// <code>
    /// // <value>form</value> is the form that show to user.
    /// ISynchronizedConnector sc = protocolservice.CreateSynchronizedConnector(<value>form</value>);
    /// // <value>sshparam</value> is a parameter for SSH connection.
    /// IInterrutable t = protocol_service.AsyncSSHConnect(sc.InterruptableConnectorClient, sshparam);
    /// // Wait 30second
    /// int timeout = 30 * 1000;
    /// ITerminalConnection connection = sc.WaitConnection(t, timeout);
    /// </code>
    /// </en>
    /// </remarks>
    public interface ISynchronizedConnector {
        /// <summary>
        /// <ja>
        /// Ú‘±‚ð‘Ò‚Â‹@”\‚ð‚à‚Â<see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see>‚ð•Ô‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// <see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see> that waits for the connection is returned. 
        /// </en>
        /// </summary>
        IInterruptableConnectorClient InterruptableConnectorClient { get; }
        /// <summary>
        /// <ja>
        /// Ú‘±Š®—¹‚Ü‚½‚ÍÚ‘±ƒGƒ‰[‚Ü‚½‚Íƒ^ƒCƒ€ƒAƒEƒg‚ª”­¶‚·‚é‚Ü‚Å‘Ò‚¿‚Ü‚·B
        /// </ja>
        /// <en>
        /// It waits until connected completion or connected error or the time-out occurs. 
        /// </en>
        /// </summary>
        /// <param name="connector"><ja>Ú‘±‚ðŽ~‚ß‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface to stop connection</en></param>
        /// <param name="timeout"><ja>ƒ^ƒCƒ€ƒAƒEƒg’liƒ~ƒŠ•bjBSystem.Threading.Timeout.Infinite‚ðŽw’è‚µ‚ÄA–³ŠúŒÀ‚É‘Ò‚Â‚±‚Æ‚à‚Å‚«‚Ü‚·B</ja><en>Time-out value (millisecond). It is possible to wait indefinitely by specifying System.Threading.Timeout.Infinite. </en></param>
        /// <returns><ja>Ú‘±‚ªŠ®—¹‚µ‚½<seealso cref="ITerminalConnection">ITerminalConnection</seealso>BÚ‘±‚ÉŽ¸”s‚µ‚½‚Æ‚«‚É‚Ínull</ja><en><seealso cref="ITerminalConnection">ITerminalConnection</seealso> that completes connection. When failing in the connection, return null. </en></returns>
        /// <remarks>
        /// <para>
        /// <ja>
        /// <paramref name="connector"/>‚É‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
        /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectƒƒ\ƒbƒh</see>A
        /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectƒƒ\ƒbƒh</see>A
        /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectƒƒ\ƒbƒh</see>
        /// ‚©‚ç‚Ì–ß‚è’l‚ð“n‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// The return value from the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see>, the <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, and the <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> is passed to connector. 
        /// </en>
        /// </para>
        /// </remarks>
        ITerminalConnection WaitConnection(IInterruptable connector, int timeout);
    }

    /// <summary>
    /// <ja>
    /// V‹KÚ‘±‹@”\‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that offers new connection function.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAProtocolsƒvƒ‰ƒOƒCƒ“iƒvƒ‰ƒOƒCƒ“IDuorg.poderosa.protocolsvj‚ª’ñ‹Ÿ‚µ‚Ü‚·BŽŸ‚Ì‚æ‚¤‚É‚µ‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// <code>
    /// IProtocolService protocolservice = 
    ///  (IProtocolService)PoderosaWorld.PluginManager.FindPlugin(
    ///   "org.poderosa.protocols", typeof(IProtocolService));
    /// Debug.Assert(protocolservice != null);
    /// </code>
    /// </ja>
    /// <en>
    /// This interface is offered by Protocols plug-in (plug-in ID [org.poderosa.protocols]). It is possible to get it as follows. 
    /// <code>
    /// IProtocolService protocolservice = 
    ///  (IProtocolService)PoderosaWorld.PluginManager.FindPlugin(
    ///   "org.poderosa.protocols", typeof(IProtocolService));
    /// Debug.Assert(protocolservice != null);
    /// </code>
    /// </en>
    /// </remarks>
    public interface IProtocolService {
        /// <summary>
        /// <ja>
        /// CygwinÚ‘±‚ÌƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ðŠi”[‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð¶¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of Cygwin connection.
        /// </en>
        /// </summary>
        /// <returns><ja>ƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja><en>Object with default parameter.</en></returns>
        //ICygwinParameter CreateDefaultCygwinParameter();
        /// <summary>
        /// <ja>
        /// TelnetÚ‘±‚ÌƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ðŠi”[‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð¶¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of telnet connection.
        /// </en>
        /// </summary>
        /// <returns><ja>ƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja>
        /// <en>Object that stored default parameter.</en>
        /// </returns>
        TelnetParameter CreateDefaultTelnetParameter();
        /// <summary>
        /// <ja>
        /// SSHÚ‘±‚ÌƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ðŠi”[‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð¶¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of SSH connection.
        /// </en>
        /// </summary>
        /// <returns><ja>ƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja><en>Object with default parameter.</en></returns>
        SSHLoginParameter CreateDefaultSSHParameter();

        /// <summary>
        /// <ja>
        /// SSHÚ‘±‚ÌƒTƒuƒVƒXƒeƒ€Žw’è•t‚«ƒpƒ‰ƒ[ƒ^‚ðŠi”[‚µ‚½ƒIƒuƒWƒFƒNƒg‚ð¶¬‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of SSH connection with subsystem designation.
        /// </en>
        /// </summary>
        /// <returns><ja>ƒfƒtƒHƒ‹ƒgƒpƒ‰ƒ[ƒ^‚ªŠi”[‚³‚ê‚½ƒIƒuƒWƒFƒNƒg</ja><en>Object with default parameter.</en></returns>
        ISSHSubsystemParameter CreateDefaultSSHSubsystemParameter();

        /// <summary>
        /// <ja>
        /// ”ñ“¯ŠúÚ‘±‚ÅCygwinÚ‘±‚Ìƒ^[ƒ~ƒiƒ‹ƒRƒlƒNƒVƒ‡ƒ“‚ðì‚è‚Ü‚·B
        /// </ja>
        /// <en>
        /// The terminal connection of the Cygwin connection is made for an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <param name="result_client"><ja>Ú‘±‚Ì¬”Û‚ðŽó‚¯Žæ‚éƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>Ú‘±Žž‚Ìƒpƒ‰ƒ[ƒ^</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>Ú‘±‘€ì‚ðƒLƒƒƒ“ƒZƒ‹‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface to cancel connected operation</en></returns>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ÍƒuƒƒbƒN‚¹‚¸‚ÉA‚½‚¾‚¿‚É§Œä‚ð–ß‚µ‚Ü‚·BÚ‘±‚ª¬Œ÷‚·‚é‚Æ<paramref name="result_client"/>‚Ì
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitƒƒ\ƒbƒh</see>‚ªŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        // IInterruptable AsyncCygwinConnect(IInterruptableConnectorClient result_client, ICygwinParameter destination);
        /// <summary>
        /// <ja>
        /// ”ñ“¯ŠúÚ‘±‚ÅTelnetÚ‘±‚Ìƒ^[ƒ~ƒiƒ‹ƒRƒlƒNƒVƒ‡ƒ“‚ðì‚è‚Ü‚·B
        /// </ja>
        /// <en>
        /// Create a terminal connection of the Telnet connection by an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <en>
        /// The terminal connection of the telnet connection is made for an asynchronous connection. 
        /// </en>
        /// <param name="result_client"><ja>Ú‘±‚Ì¬”Û‚ðŽó‚¯Žæ‚éƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>Ú‘±Žž‚Ìƒpƒ‰ƒ[ƒ^</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>Ú‘±‘€ì‚ðƒLƒƒƒ“ƒZƒ‹‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface to cancel connected operation</en></returns>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ÍƒuƒƒbƒN‚¹‚¸‚ÉA‚½‚¾‚¿‚É§Œä‚ð–ß‚µ‚Ü‚·BÚ‘±‚ª¬Œ÷‚·‚é‚Æ<paramref name="result_client"/>‚Ì
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitƒƒ\ƒbƒh</see>‚ªŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        IInterruptable AsyncTelnetConnect(IInterruptableConnectorClient result_client, TelnetParameter destination);
        /// <summary>
        /// <ja>
        /// ”ñ“¯ŠúÚ‘±‚ÅSSHÚ‘±‚Ìƒ^[ƒ~ƒiƒ‹ƒRƒlƒNƒVƒ‡ƒ“‚ðì‚è‚Ü‚·B
        /// </ja>
        /// <en>
        /// The terminal connection of the SSH connection is made for an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <param name="result_client"><ja>Ú‘±‚Ì¬”Û‚ðŽó‚¯Žæ‚éƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>Ú‘±Žž‚Ìƒpƒ‰ƒ[ƒ^</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>Ú‘±‘€ì‚ðƒLƒƒƒ“ƒZƒ‹‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX</ja>
        /// <en>Interface to cancel connection operation.</en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// ‚±‚Ìƒƒ\ƒbƒh‚ÍƒuƒƒbƒN‚¹‚¸‚ÉA‚½‚¾‚¿‚É§Œä‚ð–ß‚µ‚Ü‚·BÚ‘±‚ª¬Œ÷‚·‚é‚Æ<paramref name="result_client"/>‚Ì
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitƒƒ\ƒbƒh</see>‚ªŒÄ‚Ño‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        IInterruptable AsyncSSHConnect(IInterruptableConnectorClient result_client, SSHLoginParameter destination);

        /// <summary>
        /// <ja>
        /// ŠÈˆÕ“I‚È“¯ŠúÚ‘±‹@”\‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚ð•Ô‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Return the interface that offers a simple synchronization and the connection functions.
        /// </en>
        /// </summary>
        /// <param name="form"><ja>Ú‘±Žž‚Éƒ†[ƒU[‚É•\Ž¦‚·‚éƒtƒH[ƒ€</ja><en>Form displayed to user when connecting it</en></param>
        /// <returns><ja>“¯ŠúÚ‘±‹@”\‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX</ja><en>Interface that offers synchronization and connection functions</en></returns>
        ISynchronizedConnector CreateSynchronizedConnector(object form);

        /// <summary>
        /// <ja>
        /// ƒvƒƒgƒRƒ‹‚ÌƒIƒvƒVƒ‡ƒ“‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
        /// </ja>
        /// <en>
        /// Interface that shows option of protocol
        /// </en>
        /// </summary>
        IProtocolOptions ProtocolOptions { get; }
        
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        IPassphraseCache PassphraseCache { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPassphraseCache {
        void Add(string host, string account, string passphrase);
        string GetOrEmpty(string host, string account);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISSHConnectionChecker {
        //SSHÚ‘±‚ð’£‚é‚Æ‚«‚É‰î“ü‚·‚é‚½‚ß‚ÌƒCƒ“ƒ^ƒtƒF[ƒX@AgentForwarding—p‚É“±“ü‚µ‚½‚à‚Ì‚¾‚ªŠg’£‚·‚é‚©‚à
        void BeforeNewConnection(SSHConnectionParameter cp);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IProtocolTestService {
        ITerminalConnection CreateLoopbackConnection();
    }

}
