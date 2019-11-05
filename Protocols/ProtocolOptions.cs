/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolOptions.cs,v 1.3 2010/12/19 14:11:47 kzmi Exp $
 */
using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Text;

using Core;
//using Poderosa.Preferences;

//namespace Poderosa.Protocols
namespace Protocols
{
    /// <summary>
    /// <ja>
    /// IPv4‚ÆIPv6‚Ì—Dæ‡ˆÊ‚ðŒˆ‚ß‚Ü‚·B
    /// </ja>
    /// <en>
    /// Decide the priority level of IPv4 and IPv6
    /// </en>
    /// </summary>
    //[EnumDesc(typeof(IPVersionPriority))]
    public enum IPVersionPriority {
        /// <summary>
        /// <ja>IPv4‚ÆIPv6‚Ì—¼•û‚ðŽg‚¢‚Ü‚·B</ja>
        /// <en>Both IPv4 and IPv6 are used.</en>
        /// </summary>
        //[EnumValue(Description="Enum.IPVersionPriority.Both")]
        Both,
        /// <summary>
        /// <ja>IPv4‚µ‚©Žg‚¢‚Ü‚¹‚ñB</ja>
        /// <en>Only IPv4 is used.</en>
        /// </summary>
        //[EnumValue(Description="Enum.IPVersionPriority.V4Only")]
        V4Only,
        /// <summary>
        /// <ja>
        /// IPv6‚µ‚©Žg‚¢‚Ü‚¹‚ñB
        /// </ja>
        /// <en>Only IPv6 is used.</en>
        /// </summary>
        //[EnumValue(Description="Enum.IPVersionPriority.V6Only")]
        V6Only
    }

    /// <summary>
    /// <ja>
    /// Ú‘±ƒIƒvƒVƒ‡ƒ“‚ð’ñ‹Ÿ‚·‚éƒCƒ“ƒ^[ƒtƒFƒCƒX‚Å‚·B
    /// </ja>
    /// <en>
    /// It is an interface that offers connected option. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>‚±‚ÌƒCƒ“ƒ^[ƒtƒFƒCƒX‚Ì‰ðà‚ÍA‚Ü‚¾‚ ‚è‚Ü‚¹‚ñB</ja><en>It has not explained this interface yet. </en>
    /// </remarks>
    public interface IProtocolOptions {
        string[] CipherAlgorithmOrder {
            get;
            set;
        }
		string[] HostKeyAlgorithmOrder {
            get;
            set;
		}
		int SSHWindowSize {
            get;
            set;
		}
		bool SSHCheckMAC {
            get;
            set;
		}

		bool RetainsPassphrase {
            get;
            set;
		}
        int SocketConnectTimeout {
            get;
            set;
        }
		bool  UseSocks {
            get;
            set;
		}
		string SocksServer {
            get;
            set;
		}
		int SocksPort {
            get;
            set;
		}
		string SocksAccount {
            get;
            set;
		}
		string SocksPassword {
            get;
            set;
		}
		string SocksNANetworks {
            get;
            set;
		}
        string HostKeyCheckerVerifierTypeName {
            get;
            set;
        }
        IPVersionPriority IPVersionPriority {
            get;
            set;
        }
        bool LogSSHEvents {
            get;
            set;
        }

        //PreferenceEditor‚Ì‚Ý
        int SocketBufferSize {
            get;
        }
        bool ReadSerializedPassword {
            get;
        }
        bool SavePassword {
            get;
        }
        bool SavePlainTextPassword {
            get;
        }
    }

    public class ProtocolOptions : /* SnapshotAwarePreferenceBase, */ IProtocolOptions {

        //SSH
        private Boolean _retainsPassphrase;
        private String _cipherAlgorithmOrder;
        private String _hostKeyAlgorithmOrder;
        private Int32 _sshWindowSize;
        private Boolean _sshCheckMAC;
        private String _hostKeyCheckerVerifierTypeName;

        //ƒ\ƒPƒbƒg
        private Int32 _socketConnectTimeout;
        private IPVersionPriority _ipVersionPriority;
        private Boolean _logSSHEvents;

        //SOCKSŠÖŒW
        private Boolean _useSocks;
        private String _socksServer;
        private Int32 _socksPort;
        private String _socksAccount;
        private String _socksPassword;
        private String _socksNANetworks;

        //PreferenceEditor‚Ì‚Ý
        private Int32 _socketBufferSize;
        private Boolean _readSerializedPassword;
        private Boolean _savePassword;
        private Boolean _savePlainTextPassword;

        private bool _cipherAlgorithmOrderWasChecked = false;

		private const string DEFAULT_CIPHER_ALGORITHM_ORDER = "AES256CTR;AES256;AES192CTR;AES192;AES128CTR;AES128;Blowfish;TripleDES";

        public ProtocolOptions(String folder) {
        }

        public /* override */ void DefineItems() {
            //SSHŠÖŒW
            _retainsPassphrase     = false;
            //Note: Validator Required
            _cipherAlgorithmOrder  = DEFAULT_CIPHER_ALGORITHM_ORDER;
            _cipherAlgorithmOrderWasChecked = false;
            _hostKeyAlgorithmOrder = "DSA;RSA";
            _sshWindowSize         = 4096;
            _sshCheckMAC           = true;
            _hostKeyCheckerVerifierTypeName = "Poderosa.Usability.SSHKnownHosts";
            _logSSHEvents          = false;
            _socketConnectTimeout  = 60000;
            _ipVersionPriority = IPVersionPriority.Both;

            //SOCKSŠÖŒW
            _useSocks              = false;
            _socksServer           = "";
            _socksPort             = 1080;
            _socksAccount          = "";
            _socksPassword         = "";
            _socksNANetworks       = "";

            //PreferenceEditor‚Ì‚Ý
            _socketBufferSize = 0x1000;
            _readSerializedPassword = false;
            _savePassword = false;
            _savePlainTextPassword = false;
        }
        public ProtocolOptions Import(ProtocolOptions src) {
            //((Debug.Assert(src._folder.Id==_folder.Id);

            //SSHŠÖŒW
            _retainsPassphrase = src._retainsPassphrase;

            _cipherAlgorithmOrder  = src._cipherAlgorithmOrder;
            _cipherAlgorithmOrderWasChecked = false;
            _hostKeyAlgorithmOrder = src._hostKeyAlgorithmOrder;
            _sshWindowSize         = src._sshWindowSize;
            _sshCheckMAC           = src._sshCheckMAC;
            _hostKeyCheckerVerifierTypeName = src._hostKeyCheckerVerifierTypeName;
            _logSSHEvents          = src._logSSHEvents;

            _socketConnectTimeout  = src._socketConnectTimeout;
            _ipVersionPriority     = src._ipVersionPriority;

            //SOCKSŠÖŒW
            _useSocks              = src._useSocks;
            _socksServer           = src._socksServer;
            _socksPort             = src._socksPort;
            _socksAccount          = src._socksAccount;
            _socksPassword         = src._socksPassword;
            _socksNANetworks       = src._socksNANetworks;

            _socketBufferSize = src._socketBufferSize;
            _readSerializedPassword = src._readSerializedPassword;
            _savePassword = src._savePassword;
            _savePlainTextPassword = src._savePlainTextPassword;

            return this;
        }


		public string[] CipherAlgorithmOrder {
			get {
                //if (!_cipherAlgorithmOrderWasChecked) {
                //    _cipherAlgorithmOrder.Value = fixCipherAlgorithms(_cipherAlgorithmOrder.Value);
                //    _cipherAlgorithmOrderWasChecked = true;
                //}
				return _cipherAlgorithmOrder.Split( new char[]{';'} );
			}
			set {
                //_cipherAlgorithmOrder.Value = fixCipherAlgorithms(RuntimeUtil.ConcatStrArray(value, ';'));
                _cipherAlgorithmOrderWasChecked = true;
			}
		}
		
		public string[] HostKeyAlgorithmOrder {
			get {
				return _hostKeyAlgorithmOrder.Split(';');
			}
			set {
                _hostKeyAlgorithmOrder = RuntimeUtil.ConcatStrArray(value, ';');
			}
		}
		public int SSHWindowSize {
			get {
				return _sshWindowSize;
			}
			set {
				_sshWindowSize = value;
			}
		}
		public bool SSHCheckMAC {
			get {
				return _sshCheckMAC;
			}
			set {
				_sshCheckMAC = value;
			}
		}

		public bool RetainsPassphrase {
			get {
				return _retainsPassphrase;
			}
			set {
				_retainsPassphrase = value;
			}
		}
        public int SocketConnectTimeout {
            get {
                return _socketConnectTimeout;
            }
            set {
                _socketConnectTimeout = value;
            }
        }

		public bool  UseSocks {
			get {
				return _useSocks;
			}
			set {
				_useSocks = value;
			}
		}
		public string SocksServer {
			get {
				return _socksServer;
			}
			set {
				_socksServer = value;
			}
		}
		public int SocksPort {
			get {
				return _socksPort;
			}
			set {
				_socksPort = value;
			}
		}
		public string SocksAccount {
			get {
				return _socksAccount;
			}
			set {
				_socksAccount = value;
			}
		}
		public string SocksPassword {
			get {
				return _socksPassword;
			}
			set {
				_socksPassword = value;
			}
		}
		public string SocksNANetworks {
			get {
				return _socksNANetworks;
			}
			set {
				_socksNANetworks = value;
			}
		}
        public string HostKeyCheckerVerifierTypeName {
            get {
                return _hostKeyCheckerVerifierTypeName;
            }
            set {
                _hostKeyCheckerVerifierTypeName = value;
            }
        }
        public IPVersionPriority IPVersionPriority {
            get {
                return _ipVersionPriority;
            }
            set {
                _ipVersionPriority = value;
            }
        }
        public bool LogSSHEvents {
            get {
                return _logSSHEvents;
            }
            set {
                _logSSHEvents = value;
            }
        }

        public int SocketBufferSize {
            get {
                return _socketBufferSize;
            }
        }
        public bool ReadSerializedPassword {
            get {
                return _readSerializedPassword;
            }
        }
        public bool SavePassword {
            get {
                return _savePassword;
            }
        }
        public bool SavePlainTextPassword {
            get {
                return _savePlainTextPassword;
            }
        }


        private string fixCipherAlgorithms(string algorithms) {
            if (algorithms == null)
                return null;
            string[] inputAlgorithms = algorithms.Split(';');
            string[] defaultAlgorithms = DEFAULT_CIPHER_ALGORITHM_ORDER.Split(';');
            string[] validAlgorithms = new string[defaultAlgorithms.Length];
            int index = 0;
            foreach (string algo in inputAlgorithms) {
                for (int i = 0; i < defaultAlgorithms.Length; i++) {
                    if (defaultAlgorithms[i] != null && defaultAlgorithms[i] == algo) {
                        validAlgorithms[index++] = algo;
                        defaultAlgorithms[i] = null;
                        break;
                    }
                }
            }
            foreach (string algo in defaultAlgorithms) {
                if (algo != null)
                    validAlgorithms[index++] = algo;
            }
            return RuntimeUtil.ConcatStrArray(validAlgorithms, ';');
        }
    }


	public class ProtocolOptionsSupplier {
        private ProtocolOptions _originalOptions;
        private String _originalFolder;

        /*
		//SSH
		[ConfigBoolElement(Initial=false)]  protected bool _retainsPassphrase;
		[ConfigStringArrayElement(Initial = new string[] { "AES128", "Blowfish", "TripleDES" })]
		                                    protected string[] _cipherAlgorithmOrder;
		[ConfigStringArrayElement(Initial = new string[] { "DSA", "RSA" })]
		                                    protected string[] _hostKeyAlgorithmOrder;
		[ConfigIntElement(Initial=4096)]    protected int _sshWindowSize;
		[ConfigBoolElement(Initial=true)]   protected bool _sshCheckMAC;

		//SOCKSŠÖŒW
		[ConfigBoolElement(Initial=false)]  protected bool  _useSocks;
		[ConfigStringElement(Initial="")]   protected string _socksServer;
		[ConfigIntElement(Initial=1080)]    protected int _socksPort;
		[ConfigStringElement(Initial="")]   protected string _socksAccount;
		[ConfigStringElement(Initial="")]   protected string _socksPassword;
		[ConfigStringElement(Initial="")]   protected string _socksNANetworks;
         */


        //IPreferenceSupplier

        public string PreferenceID {
            get {
                return ""; // ProtocolsPlugin.PLUGIN_ID;
            }
        }

        public void InitializePreference(String folder) {
            _originalFolder = folder;
            _originalOptions = new ProtocolOptions(folder);
            _originalOptions.DefineItems();
        }

        public object QueryAdapter(String folder, Type type) {
            //Debug.Assert(folder.Id==_originalFolder.Id);
            if(type==typeof(IProtocolOptions))
                return _originalFolder==folder? _originalOptions : new ProtocolOptions(folder).Import(_originalOptions);
            else
                return null;
        }

        public string GetDescription() {
            return "";
        }

        public void ValidateFolder(String folder, bool output)
        {
            //TODO
        }

        public IProtocolOptions OriginalOptions {
            get {
                return _originalOptions;
            }
        }
    }
}
