using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Protocols
{
    public class ConsoleMain : IProtocolService, IProtocolTestService, IConsoleMain, IDisposable
    {
        //private static ConsoleMain _instance;
        private ProtocolOptions _protocolOptionsSupplier;
        private PassphraseCache _passphraseCache;
        //private IPoderosaLog _poderosaLog;
        //private PoderosaLogCategoryImpl _netCategory;
        private TCPParameter _tcpParam;
        private SSHLoginParameter _sshParam;
        private bool disposed = false;

        public ConsoleMain()
        {
            //_instance = this;
            _protocolOptionsSupplier = new ProtocolOptions("");
            _protocolOptionsSupplier.DefineItems();
            _passphraseCache = new PassphraseCache();
            //_poderosaLog = new PoderosaLog(); // ((IPoderosaApplication)poderosa.GetAdapter(typeof(IPoderosaApplication))).PoderosaLog;
            //_netCategory = new PoderosaLogCategoryImpl("Network");

            //IConnectionResultEventHandler is not needed now
            //_connectionResultEventHandler = new KeyAgent(); //IConnectionResultEventHandler();
            //new ISSHHostKeyVerifier
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (_passphraseCache != null)
                        _passphraseCache = null;
                    if (_protocolOptionsSupplier != null)
                        _protocolOptionsSupplier = null;
                }

                //free unmanaged objects
                //AdditionalCleanup();

                this.disposed = true;
            }
        }
        ~ConsoleMain()
        {
            Dispose(false);
        }

        public ConsoleMain Instance
        {
            get
            {
                return this; //_instance;
            }
        }

        public TelnetParameter CreateDefaultTelnetParameter()
        {
            return new TelnetParameter();
        }

        public SSHLoginParameter CreateDefaultSSHParameter()
        {
            return new SSHLoginParameter();
        }
        public ISSHSubsystemParameter CreateDefaultSSHSubsystemParameter()
        {
            return new SSHSubsystemParameter();
        }
        public IInterruptable AsyncTelnetConnect(IInterruptableConnectorClient result_client, TelnetParameter destination)
        {
            _tcpParam = destination;
            InterruptableConnector swt = new TelnetConnector(destination);
            swt.AsyncConnect(result_client, destination);
            return swt;
        }
        public IInterruptable AsyncSSHConnect(IInterruptableConnectorClient result_client, SSHLoginParameter destination)
        {
            _sshParam = destination;
            InterruptableConnector swt = new SSHConnector(destination, new HostKeyVerifierBridge());
            //ITCPParameter tcp = new SSHLoginParameter(destination); // (ITCPParameter)destination.GetAdapter(typeof(ITCPParameter));
            swt.AsyncConnect(result_client, (ITCPParameter)destination);
            return swt;
        }
        public ISynchronizedConnector CreateSynchronizedConnector(object form)
        {
            return new SilentClient(form);
        }

        public IProtocolOptions ProtocolOptions
        {
            get
            {
                return _protocolOptionsSupplier; // .OriginalOptions;
            }
        }
        public IPassphraseCache PassphraseCache
        {
            get
            {
                return _passphraseCache;
            }
        }
        public TCPParameter TCPParamter
        {
            get
            {
                return _tcpParam;
            }
        }
        public SSHLoginParameter SSHLoginParamter
        {
            get
            {
                return _sshParam;
            }
        }

        //internal IExtensionPoint ConnectionResultEventHandler
        //{
        //    get
        //    {
        //        return _connectionResultEventHandler;
        //    }
        //}

        public ProtocolOptions ProtocolOptionsSupplier
        {
            get
            {
                return _protocolOptionsSupplier;
            }
        }

        //INetLog
        public void NetLog(string text)
        {
            //_poderosaLog.AddItem(_netCategory, text);
            Console.WriteLine(text);
        }

        //IProtocolTestService
        public ITerminalConnection CreateLoopbackConnection()
        {
            return new RawTerminalConnection(new LoopbackSocket(), new EmptyTerminalParameter());
        }
    }

        public class ProtocolUtil
        {
            private ConsoleMain _ConMain;

            public ProtocolUtil(ConsoleMain clsConMain)
            {
                _ConMain = clsConMain;
            }

            //“à•”‚Å‚ITCPParameter‚ICygwinParameter‚Ì‚Ç‚Á‚¿‚©‚µ‚©ŒÄ‚Î‚ê‚È‚¢‚Í‚¸
            public void FireConnectionSucceeded(ITerminalParameter param)
            {
                ITerminalParameter t = param; //(ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
                Debug.Assert(t != null);
                if (_ConMain != null)
                {   /*
                    foreach (IConnectionResultEventHandler h in ConsoleMain.Instance.ConnectionResultEventHandler.GetExtensions())
                    {
                        h.OnSucceeded(t);
                    }   */
                }
            }
            public void FireConnectionFailure(ITerminalParameter param, string msg)
            {
                ITerminalParameter t = param; // (ITerminalParameter)param.GetAdapter(typeof(ITerminalParameter));
                Debug.Assert(t != null);
                if (_ConMain != null)
                {   /*
                    foreach (IConnectionResultEventHandler h in ConsoleMain.Instance.ConnectionResultEventHandler.GetExtensions())
                    {
                        h.OnFailed(t, msg);
                    }   */
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
        public class PassphraseCache : IPassphraseCache
        {
            private TypedHashtable<string, string> _data;

            public PassphraseCache()
            {
                _data = new TypedHashtable<string, string>();
            }
            ~PassphraseCache()
            {
                if (_data != null)
                {
                    _data.Clear();
                    _data = null;
                }
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

        //Generic”ÅHashtable
        //System.Collections.Generic.Dictionary ‚ÍAItemƒvƒƒpƒeƒB‚ªuƒL[‚ª‘¶Ý‚µ‚È‚¢‚Æ—áŠO‚ð“Š‚°‚év‚Æ‚¢‚¤ƒNƒT‚êŽd—l‚Ì‚½‚ß
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <exclude/>
        public class TypedHashtable<K, V>
        {
            private Hashtable _data;

            public TypedHashtable()
            {
                _data = new Hashtable();
            }
            ~TypedHashtable()
            {
                if (_data != null)
                {
                    _data.Clear();
                    _data = null;
                }
            }
            public int Count
            {
                get
                {
                    return _data.Count;
                }
            }
            public void Add(K key, V value)
            {
                _data[key] = value;
            }
            public V this[K key]
            {
                get
                {
                    return (V)_data[key];
                }
                set
                {
                    _data[key] = value;
                }
            }
            public void Remove(K key)
            {
                _data.Remove(key);
            }
            public void Clear()
            {
                if(_data != null)
                    _data.Clear();
            }
            public bool Contains(K key)
            {
                return _data.Contains(key);
            }
            public ICollection Values
            { //‚±‚ê‚Íƒ^ƒCƒvƒZ[ƒt‚É‚Å‚«‚È‚¢‚È
                get
                {
                    return _data.Values;
                }
            }
            public ICollection Keys
            {
                get
                {
                    return _data.Keys;
                }
            }
            public IDictionaryEnumerator GetEnumerator()
            {
                return _data.GetEnumerator();
            }
        }
}
