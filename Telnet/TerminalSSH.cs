using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Granados.Crypto;
using Granados.IO;
using Granados.SSH1;
using Granados.SSH2;
using Granados.Util;
using Granados.PKI;
using Core;

namespace Telnet
{
    //removed 25.06.2015  => Buildaction = NONE
    public class TerminalSSH : IDisposable
    {
        #region Attributes and properties
        /// <summary>The version</summary>
        public const String VERSION = "0.1"; // const = static

        private /* static */ Granados.SSHConnection _conn;
        private Reader reader = null;
        public int ConnectionStatus = 0;    //static

        // host name
        private string hostName = null;
        private string userName = "";
        private string Password = "";
        // port
        private int port = 22;
        // timeout [s] for TCP client
        private int timeoutReceive = 0; // timeout in seconds
        // timeout [s] for TCP client
        private int timeoutSend = 0; // timeout in seconds
        // buffer
        private byte[] buffer = null;
        //...
        // Force logout request from server
        private bool forceLogout = false;
        // Does server echo?
        private bool serverEcho = false;
        // the virtual screen
        public VirtualScreen _virtualScreen = null;
        /// <summary>
        /// Property virtual screen
        /// </summary>
        public VirtualScreen VirtualScreen
        {
            get
            {
                return _virtualScreen;
            }
        }
        /// <summary>
        /// Server echo on?
        /// </summary>
        public bool EchoOn
        {
            get
            {
                return this.serverEcho;
            }
        }

        // width of vs
        private int vsWidth = 0;
        // height of vs
        private int vsHeight = 0;

        // client initiated NAWS
        private bool clientInitNaws = false;
        // NAVS negotiated 
        private bool nawsNegotiated = false;
        // 1st response -> send my own WILLs not requested as DOs before
        private bool firstResponse = true;

        // some constants
        const int RECEIVEBUFFERSIZE = 10 * 1024; // read a lot
        //...
        const string ENDOFLINE = "\r\n"; // CR LF
        const int SCREENXNULLCOORDINATE = 0;
        const int SCREENYNULLCOORDINATE = 0;
        const int TRAILS = 25; // trails until timeout in "wait"-methods


        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">IP address, e.g. 192.168.0.20</param>
        public TerminalSSH(string hostName, string userName, string Password) : this(hostName, userName, Password, 22, 10, 80, 40)
	    {
		    // nothing further
	    }

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hostName">IP address, e.g. 192.168.0.20</param>
		/// <param name="port">Port, usually 23 for telnet</param>
		/// <param name="timeoutSeconds">Timeout for connections [s], both read and write</param>
		/// <param name="virtualScreenWidth">Screen width for the virtual screen</param>
		/// <param name="virtualScreenHeight">Screen height for the virtual screen</param>
		public TerminalSSH(string hostName, string userName, string Password, int port, int timeoutSeconds, int virtualScreenWidth, int virtualScreenHeight) 
		{
			this.hostName = hostName;
            this.userName = userName;
            this.Password = Password;
			this.port = port;
			this.timeoutReceive = timeoutSeconds;
			this.timeoutSend = timeoutSeconds;
			this.serverEcho = false;
			this.clientInitNaws = false;
			this.firstResponse = true;
			this.nawsNegotiated = false;
			this.forceLogout = false;
			this.vsHeight = virtualScreenHeight;
			this.vsWidth = virtualScreenWidth;
		}

        /// <summary>
		/// Destructor, calls Close()
		/// </summary>
		~TerminalSSH() 
		{
			this.Close();
		}

		/// <summary>
		/// Dispose part, calls Close()
		/// </summary>
		public void Dispose() 
		{
			this.Close();
        }

        #region Connect / Close
        /// <summary>
		/// Connect to the ssh server
		/// </summary>
		/// <returns>true if connection was successful</returns>
        public bool Connect()
        {
            // check for buffer
            if (this.buffer == null)
                this.buffer = new byte[RECEIVEBUFFERSIZE];

            // virtual screen
            if (_virtualScreen == null)
                _virtualScreen = new VirtualScreen(this.vsWidth, this.vsHeight, 1, 1);
            /*
            // set the callbacks
            if (this.callBackReceive == null)
                this.callBackReceive = new AsyncCallback(ReadFromStream);
            if (this.callBackSend == null)
                this.callBackSend = new AsyncCallback(WriteToStream);
            */
            // flags
            this.serverEcho = false;
            this.clientInitNaws = false;
            this.firstResponse = true;
            this.nawsNegotiated = false;
            this.forceLogout = false;

            //Open SSH-Connection
            Granados.SSHConnectionParameter f = new Granados.SSHConnectionParameter();
            f.EventTracer = new Tracer(); //to receive detailed events, set ISSHEventTracer
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            f.Protocol = Granados.SSHProtocol.SSH2; //this sample works on both SSH1 and SSH2
            string host_ip = hostName;
            f.UserName = userName;
            string password = Password;
            s.Connect(new IPEndPoint(IPAddress.Parse(host_ip), port)); //22 is the default SSH port

            //former algorithm is given priority in the algorithm negotiation
            f.PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.RSA, PublicKeyAlgorithm.DSA };
            f.PreferableCipherAlgorithms = new Granados.CipherAlgorithm[] { Granados.CipherAlgorithm.Blowfish, Granados.CipherAlgorithm.TripleDES };
            f.WindowSize = 0x1000; //this option is ignored with SSH1
            reader = new Reader(); //simple event receiver
            reader.parentObj = this;

            Granados.AuthenticationType at = Granados.AuthenticationType.Password;
            f.AuthenticationType = at;

            if (at == Granados.AuthenticationType.KeyboardInteractive)
            {
                //Creating a new SSH connection over the underlying socket
                _conn = Granados.SSHConnection.Connect(f, reader, s);
                reader._conn = _conn;
                Debug.Assert(_conn.AuthenticationResult == Granados.AuthenticationResult.Prompt);
                Granados.AuthenticationResult r = ((SSH2Connection)_conn).DoKeyboardInteractiveAuth(new string[] { password });
                Debug.Assert(r == Granados.AuthenticationResult.Success);
            }
            else
            {
                //NOTE: if you use public-key authentication, follow this sample instead of the line above:
                //f.AuthenticationType = AuthenticationType.PublicKey;
                f.IdentityFile = AppDomain.CurrentDomain.BaseDirectory + "Key-File.key";
                f.Password = password;
                f.KeyCheck = delegate(Granados.SSHConnectionInfo info)
                {
                    byte[] h = info.HostKeyMD5FingerPrint();
                    foreach (byte b in h) Debug.Write(String.Format("{0:x2} ", b));
                    return true;
                };

                //Creating a new SSH connection over the underlying socket
                _conn = Granados.SSHConnection.Connect(f, reader, s);
                reader._conn = _conn;
                ConnectionStatus = 1;   //TerminalSSH.ConnectionStatus = 1;
            }

            //Opening a shell
            Granados.SSHChannel ch = _conn.OpenShell(reader);
            reader._pf = ch;

            //you can get the detailed connection information in this way:
            //Granados.SSHConnectionInfo ci = _conn.ConnectionInfo;

            //Go to sample shell
            //SampleShell(reader);

            return true;
        }

        /// <summary>
		/// Closes external resources.
		/// Safe, can be called multiple times
		/// </summary>
        public void Close()
        {
            _conn.Close();

            // clean up
            // fast, "can be done several" times
            _virtualScreen = null;
            this.buffer = null;
            //this.callBackReceive = null;
            //this.callBackSend = null;
            this.forceLogout = false;
        }

        /// <summary>
        /// Is connection still open?
        /// </summary>
        /// <returns>true if connection is open</returns>
        public bool IsOpenConnection()
        {
            bool _isConnected = false;
            if (ConnectionStatus > 0)
                _isConnected = true;
            return (_isConnected);
            //return (TerminalSSH.ConnectionStatus > 0);
        }
        #endregion

        #region Send response to SSH server
        /// <summary>
        /// Send a response to the server
        /// </summary>
        /// <param name="response">response String</param>
        /// <param name="endLine">terminate with appropriate end-of-line chars</param>
        /// <returns>true if sending was OK</returns>
        public bool SendResponse(string response, bool endLine)
        {
            try
            {
                if (!this.IsOpenConnection() || _conn == null) //((_conn != null) && !_conn.IsOpen)
                    return false;
                if (response == null || response.Length < 1)
                    return true; // nothing to do
                byte[] sendBuffer = (endLine) ? System.Text.Encoding.ASCII.GetBytes(response + ENDOFLINE) : System.Text.Encoding.ASCII.GetBytes(response);
                if (sendBuffer == null || sendBuffer.Length < 1)
                    return false;
                reader._pf.Transmit(sendBuffer);
                return true;
            }
            catch
            {
                return false;
            }
        } // SendResponse
        #endregion

        #region Send function key response to Telnet server
		/// <summary>
		/// Send a Funktion Key response to the server
		/// </summary>
		/// <param name="key">Key number 1-12</param>
		/// <returns>true if sending was OK</returns>
        public bool SendResponseFunctionKey(int key)
        {
            return false;
        }
        #endregion

        #region Send SSH logout sequence
        /// <summary>
        /// Send a synchronously SSH logout-response
        /// </summary>
        /// <returns></returns>
        public bool SendLogout()
        {
            return this.SendLogout(true);
        }

        /// <summary>
        /// Send a SSH logout-response
        /// </summary>
        /// <param name="synchronous">Send synchronously (true) or asynchronously (false)</param>
        /// <returns></returns>
        public bool SendLogout(bool synchronous)
        {
            try
            {
                if (synchronous)
                {
                    _conn.Disconnect("");
                }
                else
                {
                    _conn.Close();    //Close TCP-Socket directly
                }
                return true;
            }
            catch
            {
                return false;
            }
        } // sendLogout
        #endregion

        #region WaitFor-methods
        /// <summary>
        /// Wait for a particular string
        /// </summary>
        /// <param name="searchFor">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForString(string searchFor)
        {
            return this.WaitForString(searchFor, false, this.timeoutReceive);
        }

        /// <summary>
        /// Wait for a particular string
        /// </summary>
        /// <param name="searchFor">string to be found</param>
        /// <param name="caseSensitive">case sensitive search</param>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForString(string searchFor, bool caseSensitive, int timeoutSeconds)
        {
            if (_virtualScreen == null || searchFor == null || searchFor.Length < 1)
                return null;
            // use the appropriate timeout setting, which is the smaller number
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            string found = null;
            do
            {
                lock (_virtualScreen)
                {
                    found = _virtualScreen.FindOnScreen(searchFor, caseSensitive);
                }
                if (found != null)
                    return found;
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return found;
        }

        /// <summary>
        /// Wait for a particular regular expression
        /// </summary>
        /// <param name="regEx">string to be found</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForRegEx(string regEx)
        {
            return this.WaitForRegEx(regEx, this.timeoutReceive);
        }

        /// <summary>
        /// Wait for a particular regular expression
        /// </summary>
        /// <param name="regEx">string to be found</param>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <returns>string found or null if not found</returns>
        public string WaitForRegEx(string regEx, int timeoutSeconds)
        {
            if (_virtualScreen == null || regEx == null || regEx.Length < 1)
                return null;
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            string found = null;
            do // at least once
            {
                lock (_virtualScreen)
                {
                    found = _virtualScreen.FindRegExOnScreen(regEx);
                }
                if (found != null)
                    return found;
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return found;
        }

        /// <summary>
        /// Wait for changed screen. Read further documentation 
        /// on <code>WaitForChangedScreen(int)</code>.
        /// </summary>
        /// <returns>changed screen</returns>
        public bool WaitForChangedScreen()
        {
            return this.WaitForChangedScreen(this.timeoutReceive);
        }

        /// <summary>
        /// Waits for changed screen: This method here resets
        /// the flag of the virtual screen and afterwards waits for
        /// changes.
        /// <p>
        /// This means the method detects changes after the call
        /// of the method, NOT prior.
        /// </p>
        /// <p>
        /// To reset the flag only use <code>WaitForChangedScreen(0)</code>.
        /// </p>
        /// </summary>
        /// <param name="timeoutSeconds">timeout [s]</param>
        /// <remarks>
        /// The property ChangedScreen of the virtual screen is
        /// reset after each call of Hardcopy(). It is also false directly
        /// after the initialization.
        /// </remarks>
        /// <returns>changed screen</returns>
        public bool WaitForChangedScreen(int timeoutSeconds)
        {
            // 1st check
            if (_virtualScreen == null || timeoutSeconds < 0)
                return false;

            // reset flag: This has been added after the feedback of Mark
            if (_virtualScreen.ChangedScreen)
                _virtualScreen.Hardcopy(false);

            // Only reset
            if (timeoutSeconds <= 0)
                return false;

            // wait for changes, the goal is to test at TRAILS times, if not timing out before
            int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
            DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
            do // run at least once
            {
                lock (_virtualScreen)
                {
                    if (_virtualScreen.ChangedScreen)
                        return true;
                }
                Thread.Sleep(sleepTimeMs);
            } while (DateTime.Now <= endTime);
            return false;
        } // WaitForChangedScreen

        /// <summary>
        /// Wait (=Sleep) for n seconds
        /// </summary>
        /// <param name="seconds">seconds to sleep</param>
        public void Wait(int seconds)
        {
            if (seconds > 0)
                Thread.Sleep(seconds * 1000);
        } // Wait

        /// <summary>
        /// Helper method: 
        /// Get the appropriate timeout, which is the bigger number of
        /// timeoutSeconds and this.timeoutReceive (TCP client timeout)
        /// </summary>
        /// <param name="timeoutSeconds">timeout in seconds</param>
        private int GetWaitTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds < 0 && this.timeoutReceive < 0)
                return 0;
            else if (timeoutSeconds < 0)
                return this.timeoutReceive; // no valid timeout, return other one
            else
                return (timeoutSeconds >= this.timeoutReceive) ? timeoutSeconds : this.timeoutReceive;
        }

        /// <summary>
        /// Helper method: 
        /// Get the appropriate sleep time based on timeout and TRIAL
        /// </summary>
        /// <param name="timeoutSeconds">timeout ins seconds</param>
        private int GetWaitSleepTimeMs(int timeoutSeconds)
        {
            return (this.GetWaitTimeout(timeoutSeconds) * 1000) / TRAILS;
        }

        /// <summary>
        /// Helper method: 
        /// Get the end time, which is "NOW" + timeout
        /// </summary>
        /// <param name="timeoutSeconds">timeout int seconds</param>
        private DateTime TimeoutAbsoluteTime(int timeoutSeconds)
        {
            return DateTime.Now.AddSeconds(this.GetWaitTimeout(timeoutSeconds));
        }
        #endregion

        #region ParseAndRespondServerStream
		/// <summary>
		/// Go thru the data received and answer all technical server
		/// requests (negotiations).
		/// </summary>
		/// <param name="bytesRead">number of bytes read</param>
		/// <remarks>
		/// Thread saftey regarding the virtual screen needs to be considered
		/// </remarks>
        public void ParseAndRespondServerStream(int bytesRead)
        {
        }
        #endregion

        /*
        private static void SampleShell(Reader reader)
        {
            byte[] b = new byte[1];
            while (TerminalSSH.ConnectionStatus > 0)
            {
                int input = System.Console.Read();

                b[0] = (byte)input;
                reader._pf.Transmit(b);
            }
        }
        */
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    class Reader : Granados.ISSHConnectionEventReceiver, Granados.ISSHChannelEventReceiver
    {
        public Granados.SSHConnection _conn;
        public Granados.SSHChannel _pf;
        public TerminalSSH parentObj = null;
        public bool _ready;

        public void OnData(byte[] data, int offset, int length)
        {
            System.Console.Write(Encoding.ASCII.GetString(data, offset, length));
            lock (parentObj._virtualScreen)
            {
                parentObj.ParseAndRespondServerStream(length);
            }
        }
        public void OnDebugMessage(bool always_display, byte[] data)
        {
            Debug.WriteLine("DEBUG: " + Encoding.ASCII.GetString(data));
        }
        public void OnIgnoreMessage(byte[] data)
        {
            Debug.WriteLine("Ignore: " + Encoding.ASCII.GetString(data));
        }
        public void OnAuthenticationPrompt(string[] msg)
        {
            Debug.WriteLine("Auth Prompt " + (msg.Length > 0 ? msg[0] : "(empty)"));
        }

        public void OnError(Exception error)
        {
            Debug.WriteLine("ERROR: " + error.Message);
            Debug.WriteLine(error.StackTrace);
        }
        public void OnChannelClosed()
        {
            Debug.WriteLine("Channel closed");
            //TerminalSSH.ConnectionStatus = 1;
            //_conn.AsyncReceive(this);
        }
        public void OnChannelEOF()
        {
            _pf.Close();
            Debug.WriteLine("Channel EOF");
        }
        public void OnExtendedData(int type, byte[] data)
        {
            Debug.WriteLine("EXTENDED DATA");
        }
        public void OnConnectionClosed()
        {
            Debug.WriteLine("Connection closed");
            if(parentObj != null)
                parentObj.ConnectionStatus = 0;
            //TerminalSSH.ConnectionStatus = 0;
        }
        public void OnUnknownMessage(byte type, byte[] data)
        {
            Debug.WriteLine("Unknown Message " + type);
        }
        public void OnChannelReady()
        {
            _ready = true;
        }
        public void OnChannelError(Exception error)
        {
            Debug.WriteLine("Channel ERROR: " + error.Message);
        }
        public void OnMiscPacket(byte type, byte[] data, int offset, int length)
        {
            Debug.WriteLine("MiscPacket " + data.ToString());
        }

        public Granados.PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator_host, int originator_port)
        {
            Granados.PortForwardingCheckResult r = new Granados.PortForwardingCheckResult();
            r.allowed = true;
            r.channel = this;
            return r;
        }
        public void EstablishPortforwarding(Granados.ISSHChannelEventReceiver rec, Granados.SSHChannel channel)
        {
            _pf = channel;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    class Tracer : Granados.ISSHEventTracer
    {
        public void OnTranmission(string type, string detail)
        {
            Debug.WriteLine("T:" + type + ":" + detail);
        }
        public void OnReception(string type, string detail)
        {
            Debug.WriteLine("R:" + type + ":" + detail);
        }
    }

    class AgentForwardClient : Granados.IAgentForward
    {
        private SSH2UserAuthKey[] _keys;
        public SSH2UserAuthKey[] GetAvailableSSH2UserAuthKeys()
        {
            if (_keys == null)
            {
                SSH2UserAuthKey k = SSH2UserAuthKey.FromSECSHStyleFile(@"C:\P4\Tools\keys\aaa", "aaa");
                _keys = new SSH2UserAuthKey[] { k };
            }
            return _keys;
        }

        public void NotifyPublicKeyDidNotMatch()
        {
            Debug.WriteLine("KEY NOT MATCH");
        }
        public bool CanAcceptForwarding()
        {
            return true;
        }

        public void Close()
        {
        }

        public void OnError(Exception ex)
        {
        }
    }
}
