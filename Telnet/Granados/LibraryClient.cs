/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.


 $Id: LibraryClient.cs,v 1.3 2010/11/19 15:40:39 kzmi Exp $
*/
using System;

namespace Granados
{
	//param connectionInfo is identical to the ConnectionInfo property of the connection 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionInfo"></param>
    /// <returns></returns>
    /// <exclude/>
	public delegate bool HostKeyCheckCallback(SSHConnectionInfo connectionInfo);

	//port forwarding check result
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	public struct PortForwardingCheckResult {
		/**
		 * if you allow this request, set 'allowed' to true.
		 */ 
		public bool allowed;

		/**
		 * if you allow this request, you must set 'channel' for this request. otherwise, 'channel' is ignored
		 */ 
		public ISSHChannelEventReceiver channel;

		/**
		 * if you disallow this request, you can set 'reason_code'.
			The following reason codes are defined:

			#define SSH_OPEN_ADMINISTRATIVELY_PROHIBITED    1
			#define SSH_OPEN_CONNECT_FAILED                 2
			#define SSH_OPEN_UNKNOWN_CHANNEL_TYPE           3
			#define SSH_OPEN_RESOURCE_SHORTAGE              4
		 */
		public int  reason_code;

		/**
		 * if you disallow this request, you can set 'reason_message'. this message can contain only ASCII characters.
		 */ 
		public string reason_message;
	}

	/// <summary>
	/// Connection specific receiver
	/// </summary>
    /// <exclude/>
	public interface ISSHConnectionEventReceiver {
		void OnDebugMessage(bool always_display, byte[] msg);
		void OnIgnoreMessage(byte[] msg);
		void OnUnknownMessage(byte type, byte[] data);
		void OnError(Exception error);
		void OnConnectionClosed();
		void OnAuthenticationPrompt(string[] prompts); //keyboard-interactive only
		PortForwardingCheckResult CheckPortForwardingRequest(string remote_host, int remote_port, string originator_ip, int originator_port);
		void EstablishPortforwarding(ISSHChannelEventReceiver receiver, SSHChannel channel);
	}

	/// <summary>
	/// Channel specific receiver 
	/// </summary>
    /// <exclude/>
	public interface ISSHChannelEventReceiver {
		void OnData(byte[] data, int offset, int length);
		void OnExtendedData(int type, byte[] data);
		void OnChannelClosed();
		void OnChannelEOF();
		void OnChannelError(Exception error);
		void OnChannelReady();
		void OnMiscPacket(byte packet_type, byte[] data, int offset, int length);
	}

}
