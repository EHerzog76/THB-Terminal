using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

//using Poderosa.Plugins;
using Core;
using TerminalEmulator;
using ConnectionParam;
using Protocols;
using Granados;

namespace TerminalSessions
{
    /* Optional: Register the assembly in the global assembly cache:
     * 
     * gacutil –i TerminalSessions.dll
     * 
     * Register ComObject:
     *      C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe /register TerminalSessions.dll /codebase /tlb /verbose
    */
    #region InterfaceDefinition
    [Guid("A922C2BC-0C51-4ff7-90AC-7C346FAF211B"),
    InterfaceType(ComInterfaceType.InterfaceIsDual),
    ComVisible(true)]
    public interface IActiveXTerminal
    {
        [DispId(1)]
        bool open(string UserName, string Pwd, string DestHost, int Port, string ConnectionMode, string PublicKeyFile, string strLogType, string logPath);
        [DispId(2)]
        string Prompt { get; set; }
        [DispId(3)]
        string WaitFor(string[] searchFor, bool CaseSensitive, int TimeOut);
        [DispId(4)]
        string WaitForString(string searchFor);
        [DispId(5)]
        string WaitForRegEx(string regEx);
        [DispId(6)]
        bool WaitForChangedScreen();
        [DispId(7)]
        void buffer_empty();
        [DispId(8)]
        void DeleteScreen();
        [DispId(9)]
        string ShowScreen();
        [DispId(10)]
        void print(string strCmd);
        [DispId(11)]
        void put(string strCmd);
        [DispId(12)]
        string cmd(string strCmd, string strPrompt);
        [DispId(13)]
        int Timeout { get; set; }
        [DispId(14)]
        int DebugFlag { get; set; }
        [DispId(15)]
        string cmd_remove_mode { get; set; }
        [DispId(16)]
        string eof { get; }
        [DispId(17)]
        string errmode { get; set; }
        [DispId(18)]
        string errmsg { get; set; }
        [DispId(19)]
        string error { get; set; }
        [DispId(20)]
        string getline();
        [DispId(21)]
        string[] getlines();
        [DispId(22)]
        string input_record_separator { get; set; }
        [DispId(23)]
        string last_prompt { get; set; }
        [DispId(24)]
        string lastline { get; set; }
        [DispId(25)]
        int login(string UserName, string Pwd, string Prompt, int Timeout);
        [DispId(26)]
        string ofs { get; set; }
        [DispId(27)]
        string output_field_separator { get; set; }
        [DispId(28)]
        string ors { get; set; }
        [DispId(29)]
        string output_record_separator { get; set; }
        [DispId(30)]
        string rs { get; set; } // = input_record_separator
        [DispId(31)]
        void close();
        [DispId(32)]
        bool Connected { get; }
    }
    #endregion

    [Guid("ED94D1BD-A0BB-4225-8FBD-4BB6C8E4E3BE"),
    ClassInterface(ClassInterfaceType.None),
    ComDefaultInterface(typeof(IActiveXTerminal)),
    ComVisibleAttribute(true),
    ProgId("THB.Terminal.TelnetSSHLogin")]
    public class TelnetSSHLogin : IObjectSafetyImpl, IInterruptableConnectorClient, IActiveXTerminal, IDisposable
    {
        private /* static */ TelnetSSHLogin _instance;
        private bool _isRunning;
        private bool _isConnected = false;
        private ConsoleMain _Console;
        private TerminalSession _session;
        private ITerminalConnection _result;
        protected IInterruptable _connector;
        private ITerminalSettings _terminalSettings;
        private ITerminalParameter _param;
        private TerminalOptions _terminalOptions;
        TelnetParameter tcp = null;   //ITCPParameter tcp = null;
        SSHLoginParameter ssh = null;
        private string _echoBackMode = "0"; // auto, 1, 2, ...
        // timeout [s] for TCP client
        private int _timeout = 60;  // in seconds
        private bool _timedOut = false;
        public string _prompt = @"\n*(\>|\]|#)\n*\r*\s*$";
        private int _debug = 0;
        private bool disposed = false;

        public TelnetSSHLogin()
        {
            _instance = this;
            _isRunning = true;
            _Console = new ConsoleMain();
            _terminalOptions = new TerminalOptions("");
            _terminalSettings = new TerminalSettings();
            _timeout = _Console.ProtocolOptions.SocketConnectTimeout / 1000;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if (_debug > 0)
                Console.WriteLine("TelnetSSH-Dispose-End");

            if (!this.disposed)
            {
                if (disposing)
                {
                    _isRunning = false;
                    this.close();
                    if (_Console != null)
                        _Console = null;
                    if (_terminalOptions != null)
                        _terminalOptions = null;
                    if (_terminalSettings != null)
                        _terminalSettings = null;
                }

                //free unmanaged objects
                //AdditionalCleanup();

                this.disposed = true;
            }
        }
        ~TelnetSSHLogin()
        {
            Dispose(false);
            if (_debug > 0)
                Console.WriteLine("TelnetSSH-End");
        }

        public bool open(string UserName, string Pwd, string DestHost, int Port, string ConnectionMode, string PublicKeyFile, string strLogType, string logPath)
        {
            bool bOpenResult = false;
            ConnectionMethod m;
            LogType logType = LogType.None;

            if (ConnectionMode.ToLower().Equals("telnet"))
                m = ConnectionMethod.Telnet;
            else if (ConnectionMode.ToLower().Equals("ssh1"))
                m = ConnectionMethod.SSH1;
            else
                m = ConnectionMethod.SSH2;

            if (strLogType.Length > 0)
            {
                if (strLogType.ToLower().Equals("binary"))
                    logType = LogType.Binary;
                else if (strLogType.ToLower().Equals("xml"))
                    logType = LogType.Xml;
                else
                    logType = LogType.Default;
            }

            PrepareTerminalParameter(UserName, Pwd, DestHost, Port, m, PublicKeyFile, logType, logPath);
            bOpenResult = StartConnection();

            return(bOpenResult);
        }

        public bool StartConnection()
        {
            bool bConResult = false;
            _isConnected = false;
            /*if (_session != null)
                if (_session.TerminalConnection != null)
                    _isConnected = _session.TerminalConnection.IsClosed;
            */
            if(_Console == null)
                _Console = new ConsoleMain();
            //ISynchronizedConnector synchCon = _Console.CreateSynchronizedConnector(null);

            if (ssh != null)
                _connector = _Console.AsyncSSHConnect(this /* synchCon.InterruptableConnectorClient */, ssh);
            else
                _connector = _Console.AsyncTelnetConnect(this /* synchCon.InterruptableConnectorClient */, tcp);

            if (_connector == null)
            {
                _isRunning = false;
                return (bConResult);
            }

            //_result = synchCon.WaitConnection(_connector, _timeout * 1000);
            while ((!_timedOut) && (!_isConnected))
            {
                Thread.Sleep(100);
            }

            _result = ((InterruptableConnector)_connector).Result;
            if (_result == null)
            {
                _connector = null;
                _isRunning = false;
                return (bConResult);
            }

            try
            {
                _session = new TerminalSession(((InterruptableConnector)_connector).Result, _terminalSettings, _terminalOptions);
                _session._parent = this;
                //SessionHost host = new SessionHost(this, session);
                _session.InternalStart();  // => _output.Connection.Socket.RepeatAsyncRead(_terminal);
                bConResult = true;
            }
            catch (Exception ex)
            {
                bConResult = false;
                if (_debug > 0)
                    Console.WriteLine(ex.Message);
            }

            return(bConResult);
        }

        public ITerminalParameter PrepareTerminalParameter(string UserName, string Pwd, string DestHost, int Port, ConnectionMethod m, string PublicKeyFile, LogType logType, string logPath)
        {
            string msg = null;

            if(_terminalOptions == null)
                _terminalOptions = new TerminalOptions("");
            if(_terminalSettings == null)
                _terminalSettings = new TerminalSettings();

            try
            {
                if (Port == 0)
                {
                    if (m == ConnectionMethod.Telnet)
                        Port = 23;
                    else
                        Port = 22;
                }

                tcp = _Console.CreateDefaultTelnetParameter();
                tcp.Destination = DestHost;
                tcp.Port = Port;
                if (m == ConnectionMethod.SSH1 || (m == ConnectionMethod.SSH2))
                {
                    SSHLoginParameter sp = _Console.CreateDefaultSSHParameter();
                    ssh = sp;
                    ssh.Destination = DestHost;
                    ssh.Port = Port;
                    ssh.Method = m == ConnectionMethod.SSH1 ? SSHProtocol.SSH1 : SSHProtocol.SSH2;
                    ssh.Account = UserName;
                    ssh.PasswordOrPassphrase = Pwd;
                }

                if (DestHost.Length == 0)
                    msg = "Message.TelnetSSHLogin.HostIsEmpty";

                //ƒƒOÝ’è
                ISimpleLogSettings logsettings = null;
                if (logType != LogType.None)
                {
                    if (logPath.Length == 0)
                        logPath = AppDomain.CurrentDomain.BaseDirectory;// +DestHost;
                    logsettings = new SimpleLogSettings();
                    logsettings.LogPath = logPath;
                    logsettings.LogType = logType;
                    LogFileCheckResult r = LogUtil.CheckLogFileName(logPath);
                    if (r == LogFileCheckResult.Cancel || r == LogFileCheckResult.Error) return null;
                    logsettings.LogAppend = (r == LogFileCheckResult.Append);
                    if (logsettings == null) {
                        return null; //“®ìƒLƒƒƒ“ƒZƒ‹
                    }
                    _terminalOptions.DefaultLogType = logType;
                    _terminalOptions.DefaultLogDirectory = logPath;
                }

                _param = (ITerminalParameter)tcp; //(ITerminalParameter)tcp.GetAdapter(typeof(ITerminalParameter));
                TerminalType terminal_type = TerminalType.VT100;
                _param.SetTerminalName(ToTerminalName(terminal_type));

                if (ssh != null)
                {
                    Debug.Assert(ssh != null);
                    if (PublicKeyFile.Length > 0)
                    {
                        ssh.AuthenticationType = AuthenticationType.PublicKey;
                        if (!File.Exists(PublicKeyFile))
                            msg = "Message.TelnetSSHLogin.KeyFileNotExist";
                        else
                            ssh.IdentityFileName = PublicKeyFile;
                    }
                }

                ITerminalSettings settings = this.TerminalSettings;
                settings.BeginUpdate();
                settings.Caption = DestHost;
                //settings.Icon = IconList.LoadIcon(IconList.ICON_NEWCONNECTION);
                settings.Encoding = EncodingType.ISO8859_1;
                settings.LocalEcho = false;
                settings.TransmitNL = NewLine.CRLF; // .LF; //.CR;
                settings.LineFeedRule = ConnectionParam.LineFeedRule.Normal;
                settings.TerminalType = terminal_type;
                settings.DebugFlag = _debug;
                if (logsettings != null) settings.LogSettings.Reset(logsettings);
                settings.EndUpdate();

                if (msg != null)
                {
                    if (_debug > 0) //ShowError(msg);
                        Console.WriteLine(msg);
                    return null;
                }
                else
                    return (ITerminalParameter)tcp; //.GetAdapter(typeof(ITerminalParameter));
            }
            catch (Exception ex)
            {
                if (_debug > 0)
                    Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void close()
        {
            if(_session != null)
                _session.InternalTerminate();
            if (_connector != null)
            {
                if(typeof(SSHConnector) == _connector.GetType())
                    ((SSHConnector)_connector).Result.Close();
                else
                    ((TelnetConnector)_connector).Result.Close();
            }
            if (tcp != null)
                tcp = null;
            if (ssh != null)
                ssh = null;
            if (_param != null)
                _param = null;
            _connector = null;
            _session = null;
            _isConnected = false;
            _timedOut = false;
        }
        public TelnetSSHLogin Instance
        {
            get
            {
                return _instance;
            }
        }

        public TerminalOptions TerminalOptions
        {
            get
            {
                return _terminalOptions;
            }
        }

        public void Transmit(string data)
        {
            if (_result != null)
            {
                ((TerminalConnection)_result).Transmit(data);
            }
        }

        public bool IsRunning
        {
            get {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
            }
        }

        public bool Connected
        {
            get
            {
                bool bConResult = false;
                if(_session != null)
                    if(_session.TerminalConnection != null)
                        if(!_session.TerminalConnection.IsClosed)
                            bConResult = true;

                return bConResult;
            }
        }

        public ITerminalConnection Result
        {
            get
            {
                return _result;
            }
        }

        public ITerminalSettings TerminalSettings
        {
            get
            {
                return _terminalSettings;
            }
            set
            {
                _terminalSettings = value;
            }
        }

        private void InterruptConnecting()
        {
            _connector.Interrupt();
        }

        private bool IsConnecting
        {
            get
            {
                return _connector != null;
            }
        }

        public /* static */ string ToTerminalName(TerminalType tt)
        {
            switch (tt)
            {
                case TerminalType.KTerm:
                    return "kterm";
                case TerminalType.XTerm:
                    return "xterm";
                default:
                    return "vt100";
            }
        }

        #region IInterruptableConnectorClient
        public void SuccessfullyExit(ITerminalConnection result)
        {
            _isConnected = true;
            if (_debug > 0)
                Console.WriteLine("Successfully Connected.");
        }
        public void ConnectionFailed(string message)
        {
            _timedOut = true;
            if (_debug > 0)
                Console.WriteLine("Connection failed.");
        }
        public ConsoleMain ConMain
        {
            get
            {
                return (_Console);
            }
        }
        #endregion

        public string Prompt
        {
            get {
                return _prompt;
            }
            set {
                _prompt = value;
            }
        }
        public int Timeout
        {
            get {
                return _timeout;
            }
            set {
                if (value > 0)
                    _timeout = value;
                else
                    _timeout = 60;
            }
        }
        public int DebugFlag
        {
            get {
                return _debug;
            }
            set {
                _debug = value;
            }
        }
        #region WaitFor-methods
        /// <summary>
        /// Wait for a particular strings
        /// </summary>
        /// <param name="searchFor">strings to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitFor(string[] searchFor, bool CaseSensitive, int TimeOut)
        {
            if (_session.Terminal == null)
                return null;

            return _session.Terminal.GetDocument().WaitFor(searchFor, CaseSensitive, TimeOut);
        }

        /// <summary>
        /// Wait for a particular string
        /// </summary>
        /// <param name="searchFor">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForString(string searchFor)
        {
            if (_session.Terminal == null)
                return null;

            return _session.Terminal.GetDocument().WaitForString(searchFor, false, this._timeout);
        }

        /// <summary>
        /// Wait for a particular regular expression
        /// </summary>
        /// <param name="regEx">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForRegEx(string regEx)
        {
            if (_session.Terminal == null)
                return null;

            return _session.Terminal.GetDocument().WaitForRegEx(regEx, this._timeout);
        }

        /// <summary>
        /// Wait for changed screen. Read further documentation 
        /// on <code>WaitForChangedScreen(int)</code>.
        /// </summary>
        /// <returns>changed screen</returns>
        public bool WaitForChangedScreen()
        {
            if (_session.Terminal == null)
                return true;

            return _session.Terminal.GetDocument().WaitForChangedScreen(this._timeout);
        }
        #endregion

        #region Screen
        public void buffer_empty()
        {
            this.DeleteScreen();
        }
        public void DeleteScreen()
        {
            if (_session.Terminal != null)
                _session.Terminal.GetDocument().VirtualScreen.DeleteScreen();
        }
        public string ShowScreen()
        {
            if (_session != null)
                if (_session.Terminal != null)
                    return _session.Terminal.GetDocument().VirtualScreen.Hardcopy();
                
            return null;
        }
        #endregion

        #region Send-Commands
        public void print(string strCmd)
        {
            strCmd += "\n";    // \r\n
            _session.TerminalConnection.Socket.Transmit(strCmd);
        }

        public void put(string strCmd)
        {
            if((strCmd != null) && (strCmd.Length > 0))
                _session.TerminalConnection.Socket.Transmit(strCmd);
        }

        public string cmd(string strCmd)
        {
            return cmd(strCmd, Prompt);
        }

        public string cmd(string strCmd, string strPrompt)
        {
            string strResult = "";

            if (strPrompt.Length == 0)
                strPrompt = this.Prompt;

            //Log2File(_session.Terminal.GetDocument().ShowScreen());
            _session.Terminal.GetDocument().VirtualScreen.DeleteScreen();
            strCmd += "\n";    // \r\n
            _session.TerminalConnection.Socket.Transmit(strCmd);
            //WaitForRegEx(@"\[.*\]#\s*");
            string strWaitResult = WaitForRegEx(strPrompt);
            if ((strWaitResult != null) && (strWaitResult.Length > 0))
            {
                strResult = _session.Terminal.GetDocument().ShowScreen();
            }

            //Log2File(_session.Terminal.GetDocument().ShowScreen());
            return strResult;
        }
        #endregion

        #region Addons
        /*
                this.TerminalSettings.LocalEcho = false;
                this.TerminalSettings.TransmitNL = NewLine.CRLF; // .LF; //.CR;
                this.TerminalSettings.LineFeedRule = ConnectionParam.LineFeedRule.Normal;
        */
        public string cmd_remove_mode {
            get { return _echoBackMode; }
            set{ _echoBackMode = value; }
        }
        public string eof { get { return (""); } }
        public string errmode
        {
            get{ return (""); }
            set { return; }
        }
        public string errmsg
        {
            get { return(""); }
            set { return; }
        }
        public string error
        {
            get{ return(""); }
            set { return; }
        }
        public string getline()
        {
            return ("");
        }
        public string[] getlines()
        {
            string[] strLines = new string[] { "" };

            return(strLines);
        }
        public string input_record_separator
        {
            get { return(""); }
            set { return; }
        }
        public string rs
        {
            get { return (""); }
            set { return; }
        } // = input_record_separator
        public string last_prompt
        {
            get { return (""); }
            set { return; }
        }
        public string lastline
        {
            get { return (""); }
            set { return; }
        }
        public int login(string UserName, string Pwd, string Prompt, int Timeout)
        {
            return (0);
        }
        public string ofs
        {
            get { return (""); }
            set { return; }
        }
        public string output_field_separator
        {
            get { return (""); }
            set { return; }
        }
        public string ors
        {
            get { return (""); }
            set { return; }
        }
        public string output_record_separator
        {
            get { return (""); }
            set { return; }
        }
        #endregion //Addons

        private void Log2File(string LogString)
        {
            FileStream fs = File.Open(AppDomain.CurrentDomain.BaseDirectory + "_Console-Log.log", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            Console.SetOut(sw);
            Console.WriteLine("================================================");
            Console.Write(LogString);

            // Close previous output stream and redirect output to standard output.
            Console.Out.Close();    //sw.Close();
            sw = new StreamWriter(Console.OpenStandardOutput());
            sw.AutoFlush = true;
            Console.SetOut(sw);
            fs.Close();
        }
        /*private void StartNewConsole()
        {
            Core.Win32.AllocConsole();
			Console.WriteLine(@"hello. It looks like you double clicked me to start AND you want console mode. Here's a new console.");
			Console.WriteLine("press any key to continue ...");
            Core.Win32.FreeConsole();
        }*/

        private void StartNewProcess(string strInput) {
            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo();
            myProcessStartInfo.FileName = "cmd.exe";
            //myProcessStartInfo.Arguments = @"Xcopy C:\ot\build.txt C:\ot\Dircopy";
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.ErrorDialog = true;
            myProcessStartInfo.RedirectStandardError = false;
            myProcessStartInfo.RedirectStandardOutput = false;
            myProcessStartInfo.RedirectStandardInput = true;
            myProcess.StartInfo = myProcessStartInfo;

            myProcess.Start();

            StreamWriter StdIn = myProcess.StandardInput;
            //myProcess.StandardInput.AutoFlush = true;
            //StreamReader myStreamReader = myProcess.StandardError;
            //StreamReader StdOut = myProcess.StandardOutput;

            StdIn.Write("Echo " + strInput);
            StdIn.Close();

            myProcess.WaitForExit();
            int i = myProcess.ExitCode;
            myProcess.Close();
        }
    }
}
