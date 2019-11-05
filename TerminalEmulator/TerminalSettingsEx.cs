/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSettingsEx.cs,v 1.1 2010/11/19 15:41:11 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
//using System.Drawing;

using ConnectionParam;
//using Poderosa.View;
//using Poderosa.Terminal;
using Core;

namespace TerminalEmulator
{
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalSettingsChangeListener {
        void OnBeginUpdate(ITerminalSettings current);
        void OnEndUpdate(ITerminalSettings current);
    }

    /// <summary>
    /// <ja>
    /// ŠeŽíƒƒOÝ’è‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚ÌŠî’ê‚Å‚·B
    /// </ja>
    /// <en>
    /// Base class of interface of log setting.
    /// </en>
    /// </summary>
    public interface ILogSettings {
        /// <summary>
        /// <ja>ƒƒOÝ’è‚ð•¡»‚µ‚Ü‚·B</ja><en>Duplicate the log setting.</en>
        /// </summary>
        /// <returns><en>Interface that shows object after it duplidates</en><ja>•¡»Œã‚ÌƒIƒuƒWƒFƒNƒg‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX</ja></returns>
        ILogSettings Clone();
    }
    //ƒƒOÝ’è@Terminal‚ÌÝ’èã‚Í•¡”ƒXƒgƒŠ[ƒ€‚Éo—Í‚Å‚«‚é‚æ‚¤‚É‚È‚Á‚Ä‚¢‚é‚ªATerminalSettingã‚Íƒtƒ@ƒCƒ‹‚Ö‚Ì‚PŽí‚Ì‚Ý
    /// <summary>
    /// <ja>
    /// ŠÈˆÕ‚ÈƒƒOÝ’è‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that shows simple log setting
    /// </en>
    /// </summary>
    public interface ISimpleLogSettings : ILogSettings {
        /// <summary>
        /// <ja>
        /// ƒƒO‚ÌŽí—Þ‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Type of log.
        /// </en>
        /// </summary>
        LogType LogType { get; set; }
        /// <summary>
        /// <ja>
        /// ƒƒO‚ÌƒpƒX‚ðŽ¦‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Path of the log.
        /// </en>
        /// </summary>
        string LogPath { get; set; }
        /// <summary>
        /// <ja>
        /// ƒƒO‚ð’Ç‹L‚·‚é‚©‚µ‚È‚¢‚©‚ðŽ¦‚µ‚Ü‚·Btrue‚Ì‚Æ‚«’Ç‹L‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Whether the log is append is shown. At true, append
        /// </en>
        /// </summary>
        bool LogAppend { get; set; }
    }

    //•¡”o—Í
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IMultiLogSettings : ILogSettings, IEnumerable<ILogSettings> {
        void Reset(ILogSettings log); //‚±‚ê’P“Æ‚É‚·‚é
        void Add(ILogSettings log);
        void Remove(ILogSettings log);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="newvalue"></param>
    /// <exclude/>
    public delegate void ChangeHandler<T>(T newvalue);

    /// <summary>
    /// <ja>
    /// ƒ^[ƒ~ƒiƒ‹Ý’è‚ð‘€ì‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// Interface that control terminal setting.
    /// </en>
    /// </summary>
    public interface ITerminalSettings : IListenerRegistration<ITerminalSettingsChangeListener> {
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹Ý’è‚Ì•¡»‚ðì‚è‚Ü‚·B
        /// </ja>
        /// <en>
        /// Duplicate terminal setting.
        /// </en>
        /// </summary>
        /// <returns><ja>•¡»‚³‚ê‚½ƒ^[ƒ~ƒiƒ‹Ý’èƒIƒuƒWƒFƒNƒg‚ðŽ¦‚·ƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B</ja><en>Interface that shows duplicated terminal setting object</en></returns>
        ITerminalSettings Clone();
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹Ý’è‚ðƒCƒ“ƒ|[ƒg‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Import the terminal setting.
        /// </en>
        /// </summary>
        /// <param name="src"><ja>ƒCƒ“ƒ|[ƒg‚·‚éƒ^[ƒ~ƒiƒ‹Ý’èB</ja><en>Terminal setting to import.</en></param>
        void Import(ITerminalSettings src);

        //•ÏX‚·‚é‚Æ‚«‚ÍStartUpdate...EndUpdate‚ðs‚¤BEndUpdate‚ÌŽž“_‚ÅƒŠƒXƒi‚É’Ê’m
        /// <summary>
        /// <ja>
        /// ƒvƒƒpƒeƒB‚Ì•ÏX‚ðŠJŽn‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Start changing the property.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        void BeginUpdate();
        /// <summary>
        /// <ja>
        /// ƒvƒƒpƒeƒB‚Ì•ÏX‚ðŠ®—¹‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Finish changing the property.
        /// </en>
        /// <remarks>
        /// <ja>‚±‚Ìƒƒ\ƒbƒh‚ðŒÄ‚Ño‚·‚ÆƒvƒƒpƒeƒB‚Ì•ÏX‚ªŠ®—¹‚µ‚½‚à‚Ì‚Æ‚³‚êAŠeŽíƒCƒxƒ“ƒg‚ª”­¶‚µ‚Ü‚·B</ja><en>It is assumed that the change in property was completed when this method is called, and generates various events. </en>
        /// </remarks>
        /// </summary>
        void EndUpdate();
        /// <summary>
        /// <ja>
        /// ƒGƒ“ƒR[ƒh•ûŽ®‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set / get the encode type.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        EncodingType Encoding { get; set; }
        /// <summary>
        /// <ja>
        /// ƒ^[ƒ~ƒiƒ‹‚ÌŽí—Þ‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set / get the type of termina.
        /// </en>
        /// </summary>
        TerminalType TerminalType { get; set; }
        /// <summary>
        /// <ja>
        /// ‰üsƒR[ƒh‚ÌŽæ‚èˆµ‚¢•û–@‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set / get the rule of line feed code.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        LineFeedRule LineFeedRule { get; set; }
        /// <summary>
        /// <ja>
        /// ‘—MŽž‚Ì‰üsƒR[ƒh‚ÌŽí—Þ‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set / get the line feed code when transmitting.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        NewLine TransmitNL { get; set; }
        /// <summary>
        /// <ja>
        /// ƒ[ƒJƒ‹ƒGƒR[‚Ì—L–³‚ðŽæ“¾^Ý’è‚µ‚Ü‚·B
        /// </ja>
        /// <en>
        /// Set / get the local echo.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        bool LocalEcho { get; set; }
        /// <summary>
        /// <ja>
        /// ƒhƒLƒ…ƒƒ“ƒgƒo[‚É•\Ž¦‚·‚éƒLƒƒƒvƒVƒ‡ƒ“‚Å‚·B
        /// </ja>
        /// <en>
        /// Caption displayed in document bar.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        string Caption { get; set; }

        /// <summary>
        /// <ja>
        /// ƒhƒLƒ…ƒƒ“ƒgƒo[‚É•\Ž¦‚·‚éƒAƒCƒRƒ“‚Å‚·B
        /// </ja>
        /// <en>
        /// Icon displayed in document bar
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// <para>
        /// ƒvƒƒpƒeƒB‚ð•ÏX‚·‚é‘O‚É‚ÍA<see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚µAƒvƒƒpƒeƒB‚Ì•ÏX‚ªI‚í‚Á‚½‚çA
        /// <see cref="EndUpdate">EndUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚³‚È‚¯‚ê‚Î‚È‚è‚Ü‚¹‚ñB
        /// </para>
        /// <para>
        /// <see cref="BeginUpdate">BeginUpdateƒƒ\ƒbƒh</see>‚ðŒÄ‚Ño‚·‘O‚ÉƒvƒƒpƒeƒB‚ð•ÏX‚µ‚æ‚¤‚Æ‚·‚é‚Æ—áŠO‚ª”­¶‚µ‚Ü‚·B
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="BeginUpdate">BeginUpdate method</see> is called before property is changed, and the change in property ends, it is necessary to call the <see cref="EndUpdate">EndUpdate method</see>. 
        /// </para>
        /// <para>
        /// When it starts changing property before the <see cref="BeginUpdate">BeginUpdate method</see> is called, the exception is generated.
        /// </para>
        /// </en>
        /// </remarks>
        //Image Icon { get; set; }

        int DebugFlag { get; set; }

        /// <summary>
        /// <ja>
        /// ƒƒO‚ÌÝ’èî•ñ‚Å‚·B
        /// </ja>
        /// <en>
        /// Setting of log.
        /// </en>
        /// </summary>
        IMultiLogSettings LogSettings { get; }

        //TODO ‚±‚ê‚ç‚ÍITerminalSettingsChangeListener‚É“‡‚¹‚æ
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        event ChangeHandler<string> ChangeCaption;
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        //event ChangeHandler<RenderProfile> ChangeRenderProfile;
        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        event ChangeHandler<EncodingType> ChangeEncoding;
    }

}
