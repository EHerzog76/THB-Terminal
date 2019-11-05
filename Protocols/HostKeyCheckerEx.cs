/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: HostKeyCheckerEx.cs,v 1.1 2010/11/19 15:41:03 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Granados;

//namespace Poderosa.Protocols
namespace Protocols
{
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISSHHostKeyVerifier {
        bool Verify(ISSHLoginParameter param, SSHConnectionInfo info);
    }

    //Extension Point
    public class HostKeyVerifierBridge {
        private ISSHHostKeyVerifier _verifier;

        public bool Vefiry(ISSHLoginParameter param, SSHConnectionInfo info) {
            if(_verifier==null) _verifier = FindHostKeyVerifier();
            if(_verifier==null)
                return true; //•’ÊKnownHosts‚­‚ç‚¢‚Í‚ ‚é‚¾‚ë‚¤BƒGƒ‰[‚É‚·‚×‚«‚©‚à‚µ‚ê‚È‚¢‚ª
            else
                return _verifier.Verify(param, info);
        }

        private ISSHHostKeyVerifier FindHostKeyVerifier() {
            /*
            ISSHHostKeyVerifier[] vs = (ISSHHostKeyVerifier[])ProtocolsPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(ProtocolsPluginConstants.HOSTKEYCHECKER_EXTENSION).GetExtensions();
            string name = PEnv.Options.HostKeyCheckerVerifierTypeName; //ˆê‰ž‰B‚µpreference
            
            //‰½‚©“ü‚Á‚Ä‚¢‚½‚ç“o˜^
            if(name.Length>0) {
                foreach(ISSHHostKeyVerifier v in vs) {
                    if(v.GetType().FullName==name) return v;
                }
            }
            */
            return null;
        }
    }
}
