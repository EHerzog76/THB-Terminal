/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalParameterEx.cs,v 1.2 2010/12/10 22:29:02 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

using Core;
using Granados;

//namespace Poderosa.Protocols
namespace Protocols
{
    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹Ú‘±‚Ì‚½‚ß‚Ìƒpƒ‰ƒ[ƒ^‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that shows parameter for terminal connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <seealso cref="ITCPParameter">ITCPParameter</seealso>A
    /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>
    /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso>‚ÍAGetAdapterƒƒ\ƒbƒh‚ðŽg‚Á‚Ä
    /// ‚±‚ÌITerminalParameter‚Ö‚Æ•ÏŠ·‚Å‚«‚Ü‚·B
    /// </ja>
    /// <en>
    /// <seealso cref="ITCPParameter">ITCPParameter</seealso>,
    /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>,
    /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso> can be converted to ITerminalParameter by GetAdapter method.
    /// </en>
    /// </remarks>
    public interface ITerminalParameter : ICloneable {
        /// <summary>
        /// <ja>
        /// “à•”•‚Å‚·B
        /// </ja>
        /// <en>
        /// Internal width.
        /// </en>
        /// </summary>
        int InitialWidth { get; }
        /// <summary>
        /// <ja>
        /// “à•”‚‚³‚Å‚·B
        /// </ja>
        /// <en>
        /// Internal height.
        /// </en>
        /// </summary>
        int InitialHeight { get; }
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹ƒ^ƒCƒv‚Å‚·B</ja>
        /// <en>Terminal type.</en>
        /// </summary>
        string TerminalType { get; }
        /// <summary>
        /// <ja>ƒ^[ƒ~ƒiƒ‹–¼‚ðÝ’è‚µ‚Ü‚·B</ja>
        /// <en>Set the terminal name.</en>
        /// </summary>
        /// <param name="terminaltype"><ja>Ý’è‚·‚éƒ^[ƒ~ƒiƒ‹–¼</ja><en>Terminal name to set.</en></param>
        void SetTerminalName(string terminaltype);
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚ÌƒTƒCƒY‚ð•ÏX‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Change the terminal size.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>•ÏXŒã‚Ì•</ja><en>Width after it changes</en></param>
        /// <param name="height"><ja>•ÏXŒã‚Ì‚‚³</ja><en>Height after it changes</en></param>
        void SetTerminalSize(int width, int height);

        /// <summary>
        /// <ja>
        /// 2‚Â‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ªuŒ©‚½–Ú‚Æ‚µ‚Äv“¯‚¶‚Å‚ ‚é‚©‚Ç‚¤‚©‚ð’²‚×‚Ü‚·B
        /// </ja>
        /// <en>
        /// Comparing two interfaces examine and "Externals" examines be the same. 
        /// </en>
        /// </summary>
        /// <param name="t"><ja>”äŠr‘ÎÛ‚Æ‚È‚éƒIƒuƒWƒFƒNƒg</ja><en>Object to exemine</en></param>
        /// <returns><en>Result of comparing. If it is equal, return true. </en><ja>”äŠrŒ‹‰ÊB“™‚µ‚¢‚È‚çtrue</ja></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// uŒ©‚½–Ú‚Æ‚µ‚Äv‚Æ‚ÍASSHƒvƒƒgƒRƒ‹‚Ìƒo[ƒWƒ‡ƒ“‚Ìˆá‚¢‚È‚ÇAuÚ‘±æ‚ð”äŠr‚·‚éê‡v
        /// ‚Ì“¯ˆêŽ‹‚ðˆÓ–¡‚µ‚Ü‚·B
        /// </para>
        /// <para>
        /// MRUƒvƒ‰ƒOƒCƒ“‚Å‚Í‚±‚Ìƒƒ\ƒbƒh‚ð—˜—p‚µ‚ÄA±×‚Èˆá‚¢‚Ì€–Ú‚ª•¡”ŒÂAÅ‹ßŽg‚Á‚½Ú‘±‚Ì•”•ª‚É•\Ž¦‚³‚ê‚Ä‚µ‚Ü‚¤‚±‚Æ‚ð–h‚¢‚Å‚¢‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// "Externals" means one seeing of "The connection destinations are compared" of the difference etc. of the version of the SSH protocol. 
        /// </para>
        /// <para>
        /// The item of a trifling difference is two or more pieces, and it is prevented from being displayed by using this method in the MRU plug-in in the part of the connection used recently. 
        /// </para>
        /// </en>
        /// </remarks>
        bool UIEquals(ITerminalParameter t);
    }

    /// <summary>
    /// <ja>
    /// TelnetÚ‘±‚Ìƒpƒ‰ƒ[ƒ^‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the parameter of telnet connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <en>
    /// <para>
    /// Default parameter of Telnet connnection can be got by <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultTelnetParameter">CreateDefaultTelnetParameter method</see> on <seealso cref="IProtocolService">IProtocolService</seealso>
    /// </para>
    /// <para>
    /// This interface can be convater to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// <ja>
    /// <para>
    /// ƒfƒtƒHƒ‹ƒg‚ÌTelnetÚ‘±ƒpƒ‰ƒ[ƒ^‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
    /// <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultTelnetParameter">CreateDefaultTelnetParameterƒƒ\ƒbƒh</see>‚ðŽg‚Á‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAGetAdapterƒƒ\ƒbƒh‚ðŽg‚¤‚±‚Æ‚ÅA<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// ‚Ö‚Æ•ÏŠ·‚Å‚«‚Ü‚·B
    /// </para>
    /// </ja>
    /// </remarks>
    public interface ITCPParameter : ICloneable {
        /// <summary>
        /// <ja>
        /// Ú‘±æ‚ÌƒzƒXƒg–¼i‚Ü‚½‚ÍIPƒAƒhƒŒƒXj‚Å‚·B
        /// </ja>
        /// <en>
        /// Hostname or IP Address to connect.
        /// </en>
        /// </summary>
        string Destination { get; set; }
        /// <summary>
        /// <ja>
        /// Ú‘±æ‚Ìƒ|[ƒg”Ô†‚Å‚·B
        /// </ja>
        /// <en>
        /// Port number to connect.
        /// </en>
        /// </summary>
        int Port { get; set; }
    }

    /// <summary>
    /// <ja>
    /// SSHÚ‘±Žž‚ÌƒƒOƒCƒ“ƒpƒ‰ƒ[ƒ^‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Inteface that show the login parameter on SSH connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ƒfƒtƒHƒ‹ƒg‚ÌSSHÚ‘±ƒpƒ‰ƒ[ƒ^‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
    /// <see cref="IProtocolService.CreateDefaultSSHParameter">CreateDefaultSSHParameterƒƒ\ƒbƒh</see>‚ðŽg‚Á‚ÄŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <para>
    /// Ú‘±æ‚ÌƒzƒXƒg–¼‚âƒ|[ƒg”Ô†‚ÍAGetAdapterƒƒ\ƒbƒh‚ð—p‚¢‚Ä<seealso cref="ITCPParameter">ITCPParameter</seealso>‚Ö‚Æ
    /// •ÏŠ·‚µ‚ÄÝ’è‚µ‚Ü‚·B
    /// </para>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAGetAdapterƒƒ\ƒbƒh‚ðŽg‚¤‚±‚Æ‚ÅA<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// ‚Ö‚Æ•ÏŠ·‚Å‚«‚Ü‚·B
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// Default parameter of SSH connnection can be got by > by <see cref="Poderosa.Protocols.IProtocolService.CreateDefaultSSHParameter">CreateDefaultTelnetParameter method</see> on<seealso cref="IProtocolService">IProtocolService</seealso>
    /// </para>
    /// <para>
    /// The host name and the port number at the connection destination are converted into <seealso cref="ITCPParameter">ITCPParameter</seealso> by using the GetAdapter method and set. 
    /// </para>
    /// <para>
    /// This interface can be convater to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// </remarks>
    public interface ISSHLoginParameter : ICloneable {
        /// <summary>
        /// <ja>SSHƒvƒƒgƒRƒ‹‚Ìƒo[ƒWƒ‡ƒ“‚Å‚·B</ja>
        /// <en>Version of the SSH protocol.</en>
        /// </summary>
        SSHProtocol Method { get; set; }
        /// <summary>
        /// <ja>
        /// ”FØ•ûŽ®‚Å‚·B
        /// </ja>
        /// <en>
        /// Authentification method.
        /// </en>
        /// </summary>
        AuthenticationType AuthenticationType { get; set; }
        /// <summary>
        /// <ja>
        /// ƒƒOƒCƒ“‚·‚éƒAƒJƒEƒ“ƒg–¼iƒ†[ƒU[–¼j‚Å‚·B
        /// </ja>
        /// <en>
        /// Account name (User name) to login.
        /// </en>
        /// </summary>
        string Account { get; set; }
        /// <summary>
        /// <ja>ƒ†[ƒU‚Ì”FØ‚ÉŽg—p‚·‚é”é–§Œ®‚Ìƒtƒ@ƒCƒ‹–¼‚Å‚·B</ja>
        /// <en>Private key file name to use to user authentification.</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// ‚±‚ÌƒvƒƒpƒeƒB‚ÍAAuthenticationType‚ªAutehnticationType.PublicKey‚Ì‚Æ‚«‚Ì‚ÝŽg‚í‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// This property is used when AuthenticationType is AutehnticationType.PublicKey only.
        /// </en>
        /// </remarks>
        string IdentityFileName { get; set; }
        /// <summary>
        /// <ja>
        /// ƒpƒXƒ[ƒh‚Ü‚½‚ÍƒpƒXƒtƒŒ[ƒY‚Å‚·B
        /// </ja>
        /// <en>
        /// Password or passphrase
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// AuthenticationTypeƒvƒƒpƒeƒB‚ªAuthenticationType.Password‚Ì‚Æ‚«‚É‚ÍuƒpƒXƒ[ƒhv‚ðA
        /// AuthenticationType.PublicKey‚Ì‚Æ‚«‚É‚ÍuƒpƒXƒtƒŒ[ƒYv‚ðÝ’è‚µ‚Ü‚·B
        /// AuthenticationType.KeyboardInteractive‚Ì‚Æ‚«‚É‚ÍA‚±‚ÌƒvƒƒpƒeƒB‚Í–³Ž‹‚³‚ê‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set password when AuthenticationType is AuthenticationType.Password, and, set passphrase when AuthenticationType.PublicKey.
        /// This property is ignored if it is AuthenticationType.KeyboardInteractive.
        /// </en>
        /// </remarks>
        string PasswordOrPassphrase { get; set; }
        //ƒ†[ƒU‚ÉƒpƒXƒ[ƒh‚ð“ü—Í‚³‚¹‚é‚©‚Ç‚¤‚©Btrue‚Ì‚Æ‚«‚ÍPasswordOrPassphrase‚ÍŽg—p‚µ‚È‚¢
        /// <summary>
        /// <ja>
        /// ƒ†[ƒU[‚É ƒpƒXƒ[ƒh‚ð“ü—Í‚³‚¹‚é‚©‚Ç‚¤‚©‚Ìƒtƒ‰ƒO‚Å‚·B
        /// </ja>
        /// <en>
        /// Flag whether to make user input password
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>true‚Ìê‡APasswordOrPassphraseƒvƒƒpƒeƒB‚ÍŽg‚í‚ê‚Ü‚¹‚ñB</ja><en>If it is true, PasswordOrPassphrase property is not used.</en>
        /// </remarks>
        /// <exclude/>
        bool LetUserInputPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        IAgentForward AgentForward { get; set; }
    }

    /// <summary>
    /// </summary>
    /// <exclude/>
    public interface ISSHSubsystemParameter : ICloneable {
        string SubsystemName { get; set; }
    }

    /// <summary>
    /// <ja>
    /// CygwinÚ‘±‚·‚é‚Æ‚«‚ÉŽg‚í‚ê‚éƒpƒ‰ƒ[ƒ^‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that show the parameter using on Cygwin connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍA<seealso cref="IProtocolService">IProtocolService</seealso>‚Ì
    /// <see cref="IProtocolService.CreateDefaultCygwinParameter">CreateDefaultCygwinParameterƒƒ\ƒbƒh</see>
    /// ‚ðŒÄ‚Ño‚·‚±‚Æ‚ÅŽæ“¾‚Å‚«‚Ü‚·B
    /// </para>
    /// <para>
    /// ‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÍAGetAdapterƒƒ\ƒbƒh‚ðŽg‚¤‚±‚Æ‚ÅA<seealso cref="ITerminalParameter">ITerminalParameter</seealso>
    /// ‚Ö‚Æ•ÏŠ·‚Å‚«‚Ü‚·B
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface can be  got using <see cref="IProtocolService.CreateDefaultCygwinParameter">CreateDefaultCygwinParameter method</see> on <seealso cref="IProtocolService">IProtocolService</seealso>.
    /// </para>
    /// <para>
    /// This interface is convert to <seealso cref="ITerminalParameter">ITerminalParameter</seealso> by GetAdapter method.
    /// </para>
    /// </en>
    /// </remarks>
    public interface ICygwinParameter : ICloneable {
        /// <summary>
        /// <ja>
        /// ƒVƒFƒ‹‚Ì–¼‘O‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Get / set shell name.
        /// </en>
        /// </summary>
        string ShellName { get; set; }
        /// <summary>
        /// <ja>
        /// ƒz[ƒ€ƒfƒBƒŒƒNƒgƒŠ‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Get / set the home directory.
        /// </en>
        /// </summary>
        string Home { get; set; }
        /// <summary>
        /// <ja>
        /// ƒVƒFƒ‹‚©‚çˆø”•”•ª‚ðŽæ‚èœ‚¢‚½ƒRƒ}ƒ“ƒh•”•ª‚¾‚¯‚ð•Ô‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Only the command part where the argument part was removed from the shell is returned. 
        /// </en>
        /// </summary>
        string ShellBody { get; }
        /// <summary>
        /// <ja>
        /// Cygwin‚ÌêŠ‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// Ý’è‚³‚ê‚È‚¢ê‡‚ÍƒŒƒWƒXƒgƒŠ‚©‚çŒŸo‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Get or Set path where Cygwin is installed.
        /// If this property was not set, the path will be detected from the registry entry.
        /// </en>
        /// </summary>
        string CygwinDir { get; set; }
    }

}
