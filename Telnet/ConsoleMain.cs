using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Protocols
{
    class ConsoleMain
    {
        private static ConsoleMain _instance;
        private ProtocolOptionsSupplier _protocolOptionsSupplier;
        private PassphraseCache _passphraseCache;
        private IPoderosaLog _poderosaLog;
        private PoderosaLogCategoryImpl _netCategory;

        public ConsoleMain()
        {
        }

        public static ConsoleMain Instance
        {
            get
            {
                return _instance;
            }
        }

        public void InitializePlugin(object poderosa)
        {
            _instance = this;
            _protocolOptionsSupplier = new ProtocolOptionsSupplier();
            _passphraseCache = new PassphraseCache();
            _poderosaLog = new PoderosaLog(); // ((IPoderosaApplication)poderosa.GetAdapter(typeof(IPoderosaApplication))).PoderosaLog;
            _netCategory = new PoderosaLogCategoryImpl("Network");

            //new IConnectionResultEventHandler
            //new ISSHHostKeyVerifier

            //_connectionResultEventHandler = pm.CreateExtensionPoint(ProtocolsPluginConstants.RESULTEVENTHANDLER_EXTENSION, typeof(IConnectionResultEventHandler), this);
            //pm.CreateExtensionPoint(ProtocolsPluginConstants.HOSTKEYCHECKER_EXTENSION, typeof(ISSHHostKeyVerifier), ConsoleMain.Instance);
            //PEnv.Init((ICoreServices)poderosa.GetAdapter(typeof(ICoreServices)));

            //ProtocolsPlugin.Instance.PoderosaWorld.Culture.AddChangeListener("Protocols.strings");
        }
        public ITCPParameter CreateDefaultTelnetParameter()
        {
            return new TelnetParameter();
        }

        public ISSHLoginParameter CreateDefaultSSHParameter()
        {
            return new SSHLoginParameter();
        }
        public ISSHSubsystemParameter CreateDefaultSSHSubsystemParameter()
        {
            return null; // new SSHSubsystemParameter();
        }
        public IInterruptable AsyncTelnetConnect(IInterruptableConnectorClient result_client, ITCPParameter destination)
        {
            InterruptableConnector swt = new TelnetConnector(destination);
            swt.AsyncConnect(result_client, destination);
            return swt;
        }
        public IInterruptable AsyncSSHConnect(IInterruptableConnectorClient result_client, ISSHLoginParameter destination)
        {
            InterruptableConnector swt = new SSHConnector(destination, new HostKeyVerifierBridge());
            ITCPParameter tcp = (ITCPParameter)destination.GetAdapter(typeof(ITCPParameter));
            swt.AsyncConnect(result_client, tcp);
            return swt;
        }
        //public ISynchronizedConnector CreateFormBasedSynchronozedConnector(IPoderosaForm form)
        //{
        //    return new SilentClient(form);
        //}

        public IProtocolOptions ProtocolOptions
        {
            get
            {
                return _protocolOptionsSupplier.OriginalOptions;
            }
        }
        public IPassphraseCache PassphraseCache
        {
            get
            {
                return _passphraseCache;
            }
        }

        //internal IExtensionPoint ConnectionResultEventHandler
        //{
        //    get
        //    {
        //        return _connectionResultEventHandler;
        //    }
        //}

        public ProtocolOptionsSupplier ProtocolOptionsSupplier
        {
            get
            {
                return _protocolOptionsSupplier;
            }
        }

        public void NetLog(string text)
        {
            _poderosaLog.AddItem(_netCategory, text);
        }

        //IProtocolTestService
        public ITerminalConnection CreateLoopbackConnection()
        {
            return new RawTerminalConnection(new LoopbackSocket(), new EmptyTerminalParameter());
        }
    }

        internal static class ProtocolUtil
        {
            //“à•”‚Å‚ÍITCPParameter‚©ICygwinParameter‚Ì‚Ç‚Á‚¿‚©‚µ‚©ŒÄ‚Î‚ê‚È‚¢‚Í‚¸
            public static void FireConnectionSucceeded(IAdaptable param)
            {
                ITerminalParameter t = (ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
                Debug.Assert(t != null);
                if (ConsoleMain.Instance != null)
                {
                    foreach (IConnectionResultEventHandler h in ProtocolsPlugin.Instance.ConnectionResultEventHandler.GetExtensions())
                    {
                        h.OnSucceeded(t);
                    }
                }
            }
            public static void FireConnectionFailure(IAdaptable param, string msg)
            {
                ITerminalParameter t = (ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
                Debug.Assert(t != null);
                if (ConsoleMain.Instance != null)
                {
                    foreach (IConnectionResultEventHandler h in ProtocolsPlugin.Instance.ConnectionResultEventHandler.GetExtensions())
                    {
                        h.OnFailed(t, msg);
                    }
                }
            }

            public static string ProtocolsPluginHomeDir
            {
                get
                {
                    return AppDomain.CurrentDomain.BaseDirectory + "Protocols\\";
                }
            }
        }

        //ƒƒ‚ƒŠ‚É‚Ì‚Ý•ÛŽ
        internal class PassphraseCache : IPassphraseCache
        {
            private TypedHashtable<string, string> _data;

            public PassphraseCache()
            {
                _data = new TypedHashtable<string, string>();
            }
            public void Add(string host, string account, string passphrase)
            {
                _data.Add(String.Format("{0}@{1}", account, host), passphrase);
            }

            public string GetOrEmpty(string host, string account)
            {
                string t = _data[String.Format("{0}@{1}", account, host)];
                return t == null ? String.Empty : t;
            }
        }
}
