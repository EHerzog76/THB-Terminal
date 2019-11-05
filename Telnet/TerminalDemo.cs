// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED!
//
// IF YOU FIND ERRORS OR POSSIBLE IMPROVEMENTS, PLEASE LET ME KNOW.
// MAYBE TOGETHER WE CAN SOLVE THIS.
//
// YOU MAY USE THIS CODE: HOWEVER THIS GRANTS NO FUTURE RIGHTS.

using System;
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

namespace Telnet.Demo
{
#if ENABLE_TUTORIAL
	/// <summary>
	/// Demo for the telnet class:
	/// <p>
	/// <a href="http://www.klausbasan.de/misc/telnet/index.html">Further details</a>
	/// </p>
	/// </summary>
	public class TerminalDemo
	{
        private static Granados.SSHConnection _conn;
        public static int ConnectionStatus = 0;


		/// <summary>
		/// The main entry point for the application.
		/// Can be used to test the programm and run it from command line.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			// DemoMSTelnetServer(args);
			// DemoRH73TelnetServer(args);
            
            //MyDemo(args);
            MyDemoSSH(args);
		}

        private static void MyDemo(string[] args)
        {
            string f = null;
            //172.25.148.46
            Terminal tn = new Terminal("172.28.223.138", 23, 10, 80, 40); // hostname, port, timeout [s], width, height
            tn.Connect(); // physcial connection
            do
            {
                f = tn.WaitForRegEx(@"(Username|Login|[Pp]assword)[ :\n]*$");
                if (f == null)
                {
                    Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
                    throw new TerminalException("No password prompt found");
                }
                Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
                if (f.ToLower().Contains("username") || f.ToLower().Contains("login"))
                {
                    tn.SendResponse("EHerzog", true);
                    f = tn.WaitForRegEx(@"[Pp]assword[ :\n]*$");
                    if (f == null)
                        throw new TerminalException("No password prompt found");
                    Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
                }
                tn.VirtualScreen.CleanScreen();
                tn.SendResponse("1q2w3e4r!", true);	// send password
                f = tn.WaitForRegEx(@"^[ \n]*[A-Za-z0-9\-_]+(\#|\$|\>)[\s\n]*$");      //Wait for Prompt
                if (f == null)
                    throw new TerminalException("No 1st menu screen found");
                Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
                Console.WriteLine("Found Prompt: " + f + "...");

                tn.VirtualScreen.CleanScreen();
                tn.SendResponse("exit", true);
            } while (false);
            tn.SendLogout(true);
            tn.Close(); // physically close on TcpClient
            Console.WriteLine("\n\nEnter to continue ...\n");
            Console.ReadLine();
        } // Demo

        private static void MyDemoSSH(string[] args){
            Granados.SSHConnectionParameter f = new Granados.SSHConnectionParameter();
			f.EventTracer = new Tracer(); //to receive detailed events, set ISSHEventTracer
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            f.Protocol = Granados.SSHProtocol.SSH2; //this sample works on both SSH1 and SSH2
            string host_ip = "192.168.192.165";
			f.UserName = "root";
            string password = "1q2w3e4r!";
			s.Connect(new IPEndPoint(IPAddress.Parse(host_ip), 22)); //22 is the default SSH port

            //former algorithm is given priority in the algorithm negotiation
            f.PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.RSA, PublicKeyAlgorithm.DSA };
            f.PreferableCipherAlgorithms = new Granados.CipherAlgorithm[] { Granados.CipherAlgorithm.Blowfish, Granados.CipherAlgorithm.TripleDES };
            f.WindowSize = 0x1000; //this option is ignored with SSH1
            Reader reader = new Reader(); //simple event receiver

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
            else {
                //NOTE: if you use public-key authentication, follow this sample instead of the line above:
                //f.AuthenticationType = AuthenticationType.PublicKey;
                f.IdentityFile = AppDomain.CurrentDomain.BaseDirectory + "Key-File.key";
                f.Password = password;
                f.KeyCheck = delegate(Granados.SSHConnectionInfo info)
                {
                    byte[] h = info.HostKeyMD5FingerPrint();
                    foreach(byte b in h) Debug.Write(String.Format("{0:x2} ", b));
                    return true;
                };

                //Creating a new SSH connection over the underlying socket
                _conn = Granados.SSHConnection.Connect(f, reader, s);
                reader._conn = _conn;
                TerminalDemo.ConnectionStatus = 1;
            }

            //Opening a shell
            Granados.SSHChannel ch = _conn.OpenShell(reader);
            reader._pf = ch;

			//you can get the detailed connection information in this way:
            Granados.SSHConnectionInfo ci = _conn.ConnectionInfo;

			//Go to sample shell
			SampleShell(reader);
        }

		/// <summary>
		/// Demo for a MS Telnet server
		/// </summary>
		private static void DemoMSTelnetServer(string[] args)
		{
			string f = null;
			Terminal tn = new Terminal("giga", 23, 10, 80, 40); // hostname, port, timeout [s], width, height
			tn.Connect(); // physcial connection
			do 
			{
				f = tn.WaitForString("Login");
				if (f==null)
					throw new TerminalException("No login possible");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("telnet", true);	// send username
				f = tn.WaitForString("Password");
				if (f==null) 
					throw new TerminalException("No password prompt found");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("telnet", true);	// send password 
				f = tn.WaitForString(">");
				if (f==null) 
					throw new TerminalException("No > prompt found");
				tn.SendResponse("dir", true);		// send dir command
				if (tn.WaitForChangedScreen())
					Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
			} while (false);
			tn.Close(); // physically close on TcpClient
			Console.WriteLine("\n\nEnter to continue ...\n");
			Console.ReadLine();
		} // Demo

		/// <summary>
		/// Demo for a Linux RedHat 7.3 telnet server
		/// </summary>
		private static void DemoRH73TelnetServer(string[] args)
		{
			string f = null;
			Terminal tn = new Terminal("10.10.20.140", 23, 10, 80, 40); // hostname, port, timeout [s], width, height
			tn.Connect(); // physcial connection
			do 
			{
				f = tn.WaitForString("Login");
				if (f==null) 
					break; // this little clumsy line is better to watch in the debugger
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("kba", true);	// send username
				f = tn.WaitForString("Password");
				if (f==null) 
					break;
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("vmware", true);	// send password 
				f = tn.WaitForString("$");			// bash
				if (f==null) 
					break;
				tn.SendResponse("df", true);		// send Shell command
				if (tn.WaitForChangedScreen())
					Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
			} while (false);
			tn.Close(); // physically close on TcpClient
			Console.WriteLine("\n\nEnter to continue ...\n");
			Console.ReadLine();
		} // Demo

		/// <summary>
		/// Demo for a RT311 Router
		/// </summary>
		private static void DemoRT311Router(string[] args)
		{
			string f = null;
			Terminal tn = new Terminal("router", 23, 10, 80, 40); // hostname, port, timeout [s], width, height
			tn.Connect(); // physcial connection
			do 
			{
				f= tn.WaitForString("Password");
				if (f==null) 
					throw new TerminalException("No password prompt found");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("1234", true);	// send password
				f = tn.WaitForString("Enter");
				if (f==null) 
					throw new TerminalException("No 1st menu screen found");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("24", true);	// send "24" to get to next screen 
				f = tn.WaitForString("Enter", false, 30); // String, case sensitive, timeout
				if (f==null) 
					throw new TerminalException("No 2nd menu screen found");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				tn.SendResponse("1", true);		// send "1" to get to next screen
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				f = tn.FindIPAddress(tn.WaitForRegEx(@"WAN.+?(\d?\d?\d\.\d?\d?\d\.\d?\d?\d\.\d?\d?\d)")); // search for 1st IP-like address next to "WAN"
				if (f==null) 
					throw new TerminalException("No IP address found");
				Console.WriteLine(tn.VirtualScreen.Hardcopy().TrimEnd());
				Console.WriteLine("\n\nEXTERNAL IP " + f);
			} while (false);
			tn.SendLogout(true);
			tn.Close(); // physically close on TcpClient
			Console.WriteLine("\n\nEnter to continue ...\n");
			Console.ReadLine();
		} // Demo

        private static void SampleShell(Reader reader)
        {
            byte[] b = new byte[1];
            while (TerminalDemo.ConnectionStatus > 0 && _conn.IsOpen)
            {
                int input = System.Console.Read();

                b[0] = (byte)input;
                reader._pf.Transmit(b);
            }
            _conn.Disconnect("");
            //_conn.Close();    //Close TCP-Socket directly
        }
	} // class

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
	class Reader : Granados.ISSHConnectionEventReceiver, Granados.ISSHChannelEventReceiver {
		public Granados.SSHConnection _conn;
		public bool _ready;

		public void OnData(byte[] data, int offset, int length) {
			System.Console.Write(Encoding.ASCII.GetString(data, offset, length));
		}
		public void OnDebugMessage(bool always_display, byte[] data) {
			Debug.WriteLine("DEBUG: "+ Encoding.ASCII.GetString(data));
		}
		public void OnIgnoreMessage(byte[] data) {
			Debug.WriteLine("Ignore: "+ Encoding.ASCII.GetString(data));
		}
		public void OnAuthenticationPrompt(string[] msg) {
			Debug.WriteLine("Auth Prompt "+(msg.Length>0? msg[0] : "(empty)"));
		}

		public void OnError(Exception error) {
			Debug.WriteLine("ERROR: "+ error.Message);
			Debug.WriteLine(error.StackTrace);
		}
		public void OnChannelClosed() {
			Debug.WriteLine("Channel closed");
            //TerminalDemo.ConnectionStatus = 1;
			//_conn.AsyncReceive(this);
		}
		public void OnChannelEOF() {
			_pf.Close();
			Debug.WriteLine("Channel EOF");
		}
		public void OnExtendedData(int type, byte[] data) {
			Debug.WriteLine("EXTENDED DATA");
		}
		public void OnConnectionClosed() {
			Debug.WriteLine("Connection closed");
            TerminalDemo.ConnectionStatus = 0;
		}
		public void OnUnknownMessage(byte type, byte[] data) {
			Debug.WriteLine("Unknown Message " + type);
		}
		public void OnChannelReady() {
			_ready = true;
		}
		public void OnChannelError(Exception error) {
			Debug.WriteLine("Channel ERROR: "+ error.Message);
		}
		public void OnMiscPacket(byte type, byte[] data, int offset, int length) {
            Debug.WriteLine("MiscPacket " + data.ToString());
		}

		public Granados.PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator_host, int originator_port) {
            Granados.PortForwardingCheckResult r = new Granados.PortForwardingCheckResult();
			r.allowed = true;
			r.channel = this;
			return r;
		}
        public void EstablishPortforwarding(Granados.ISSHChannelEventReceiver rec, Granados.SSHChannel channel)
        {
			_pf = channel;
		}

        public Granados.SSHChannel _pf;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    class Tracer : Granados.ISSHEventTracer
    {
		public void OnTranmission(string type, string detail) {
			Debug.WriteLine("T:"+type+":"+detail);
		}
		public void OnReception(string type, string detail) {
			Debug.WriteLine("R:"+type+":"+detail);
		}
	}

    class AgentForwardClient : Granados.IAgentForward
    {
        private SSH2UserAuthKey[] _keys;
        public SSH2UserAuthKey[] GetAvailableSSH2UserAuthKeys() {
            if(_keys==null) {
                SSH2UserAuthKey k = SSH2UserAuthKey.FromSECSHStyleFile(@"C:\P4\Tools\keys\aaa", "aaa");
                _keys = new SSH2UserAuthKey[] { k };
            }
            return _keys;
        }

        public void NotifyPublicKeyDidNotMatch() {
            Debug.WriteLine("KEY NOT MATCH");
        }
        public bool CanAcceptForwarding() {
            return true;
        }

        public void Close() {
        }

        public void OnError(Exception ex) {
        }
    }
#endif
} // namespace