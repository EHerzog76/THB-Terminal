// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED!
//
// IF YOU FIND ERRORS OR POSSIBLE IMPROVEMENTS, PLEASE LET ME KNOW.
// MAYBE TOGETHER WE CAN SOLVE THIS.
//
// YOU MAY USE THIS CODE: HOWEVER THIS GRANTS NO FUTURE RIGHTS.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using Core;

namespace Telnet
{
    //removed 25.06.2015  => Buildaction = NONE
	/// <summary>
	/// Supports telnet connectivity.
	/// <p>
	/// Version 0.70	1st running version<br/>
	/// Version 0.71	Telnet class renamed to Terminal and close method improved<br/>
	/// Version 0.72	Added custom exceptions which may be used externally
	///					Feedback of Mark H. considered, Wait() method added and WaitForChangedScreen() fixed<br/>
	///	Version 0.73	Offset problem in Virtual Screen fixed due to mail of Steve, thanks!<br/>
	///	Version 0.74	SendResponseFunctionKey(int) and fixed WaitFor[XYZ]-methods to better reflect
	///	                the timeout. Thanks Judah!<br/>
	/// </p>
	/// <list type="number">
	///		<listheader>
	///			<term>Features</term>
	///			<description>Telnet functionality implemented</description>
	///		</listheader>
	///		<item>
	///			<term>LOGOUT</term>
	///			<description>Logout functionaliy implemented</description>
	///		</item>
	///		<item>
	///			<term>NAWS</term>
	///			<description>Sends a window size</description>
	///		</item>
	///		<item>
	///			<term>TERMTYPE</term>
	///			<description>Sends an "ANSI"-terminal type</description>
	///		</item>
	///		<item>
	///			<term>Other telnet commands</term>
	///			<description>Will be answered correctly with WILL / WONT</description>
	///		</item>
	///		<item>
	///			<term>ESC-Sequences</term>
	///			<description>Method dealing with ESC-sequences</description>
	///		</item>
	///	</list>
	/// </summary>
	/// <remarks>
	/// The class is NOT thread safe for several connections,
	/// so each connection should have its own instance.
	///	<p>
	/// Credits for source code / technical information helping me:<br/>
	/// Tyler Kline		"clsScriptingTelnet"	<a href="http://www.c-sharpcorner.com/Code/2003/Jan/TelnetScripting.asp">Internet page</a> <br/>
	/// Igor Moochnick	"RemoteConsoleSolution"	<a href="http:http://www.gotdotnet.com/community/usersamples/Default.aspx?query=telnet">Internet page</a> <br/>
	/// </p>
	/// <p>
	/// <a href="http://www.networksorcery.com/enp/protocol/Terminal.htm">http://www.networksorcery.com/enp/protocol/Terminal.htm</a><br/>
	/// <a href="http://members.tripod.com/~ilkerf/cdoc/vt100ref.html">http://members.tripod.com/~ilkerf/cdoc/vt100ref.html</a><br/>
	/// <a href="http://www.klausbasan.de/misc/telnet/index.html">http://www.klausbasan.de/misc/telnet/index.html</a><br/>
	/// </p>
	/// </remarks>
	public class Terminal : IDisposable
	{
		#region Attributes and properties
		/// <summary>The version</summary>
		public const String VERSION = "0.74"; // const = static

		// client connection 
		private TcpClient tcpClient = null;
		// host name
		private string hostName = null;
		// port
		private int port = 0;
		// timeout [s] for TCP client
		private int timeoutReceive = 0; // timeout in seconds
		// timeout [s] for TCP client
		private int timeoutSend = 0; // timeout in seconds
		// buffer
		private byte[] buffer = null;
		// callback method
		private AsyncCallback callBackReceive = null;
		// callback method
		private AsyncCallback callBackSend = null;
		// Force logout request from server
		private bool forceLogout = false;
		// Does server echo?
		private bool serverEcho = false;
		// the virtual screen
		private VirtualScreen virtualScreen = null;
		/// <summary>
		/// Property virtual screen
		/// </summary>
		public VirtualScreen VirtualScreen 
		{
			get 
			{
				return this.virtualScreen;
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
		const int SENDBUFFERSIZE = 25; // only small reponses -> only for DOs, WILLs, not for user's responses
		const byte ESC = 27;
		const byte CR = 13;
		const byte LF = 10;
		const String F1 = "\033OP"; // function key
		const String F2 = "\033OQ";
		const String F3 = "\033OR";
		const String F4 = "\033OS";
		const String F5 = "\033[15~";
		const String F6 = "\033[17~";
		const String F7 = "\033[18~";
		const String F8 = "\033[19~";
		const String F9 = "\033[20~";
		const String F10 = "\033[21~";
		const String F11 = "\033[23~";
		const String F12 = "\033[24~";

		const string ENDOFLINE = "\r\n"; // CR LF
		const int SCREENXNULLCOORDINATE = 0;
		const int SCREENYNULLCOORDINATE = 0;
		const int TRAILS = 25; // trails until timeout in "wait"-methods

		// Regular expressions for ESC sequences
		private static Regex REGEXPCURSORRIGHT = new Regex("\\[\\d*C", RegexOptions.Compiled);
		private static Regex REGEXPCURSORLEFT = new Regex("\\[\\d*D", RegexOptions.Compiled);
		private static Regex REGEXPCURSORPOSITION = new Regex("\\[\\d*;\\d*[Hf]", RegexOptions.Compiled);
		private static Regex REGEXPCURSORYPOSITION = new Regex("\\[\\d+;", RegexOptions.Compiled); // line
		private static Regex REGEXPCURSORXPOSITION = new Regex(";\\d+[Hf]", RegexOptions.Compiled); // column
		private static Regex REGEXPSCROLLINGREGION = new Regex("\\[\\d*;\\d*r", RegexOptions.Compiled); 
		private static Regex REGEXPNUMBER = new Regex("\\d+", RegexOptions.Compiled);
		private static Regex REGEXPIP = new Regex(@"\d?\d?\d\.\d?\d?\d\.\d?\d?\d\.\d?\d?\d", RegexOptions.Compiled);
		
		// Telnet commands
		const byte TNC_SE		= 240; // End of subnegotiation parameters
		const byte TNC_NOP		= 241; // No operation
		const byte TNC_DATAMARK	= 242; // F2 The data stream portion of a Synch. This should always be accompanied by a TCP Urgent notification. 
		const byte TNC_BRK		= 243; // F3 Break NVT character BRK.
		const byte TNC_IP		= 244; // F4 Interrupt Process The function IP. 
		const byte TNC_AO		= 245; // F5 The function AO. Abort output
		const byte TNC_AYT		= 246; // F6 Are You There The function AYT. 
		const byte TNC_EC		= 247; // F7 Erase character. The function EC. 
		const byte TNC_EL		= 248; // F8 Erase line. The function EL.
		const byte TNC_GA		= 249; // F9 Go ahead The GA signal. 
		const byte TNC_SB		= 250; // FA Option code: Indicates that what follows is subnegotiation of the indicated option.
		const byte TNC_WILL		= 251; // FB Option code: Indicates the desire to begin performing, or confirmation that you are now performing, the indicated option.
		const byte TNC_WONT		= 252; // FC Option code: Indicates the refusal to perform, or continue performing, the indicated option.
		const byte TNC_DO		= 253; // FD Option code: Indicates the request that the other party perform, or confirmation that you are expecting the other party to perform, the indicated option.
		const byte TNC_DONT		= 254; // FE Option code: Indicates the demand that the other party stop performing, or confirmation that you are no longer expecting the other party to perform, the indicated option.
		const byte TNC_IAC		= 255; // FF Data Byte 255
		// Telnet options
		const byte TNO_TRANSBIN		= 0;  // 00 transmit binary
		const byte TNO_ECHO			= 1;  // 00 echo
		const byte TNO_LOGOUT		= 18; // 12 Logout
		const byte TNO_TERMTYPE		= 24; // 18 Terminal size
		const byte TNO_NAWS			= 31; // 1F Window size
		const byte TNO_TERMSPEED	= 32; // 20 Terminal speed
		const byte TNO_REMOTEFLOW	= 33; // 21 Remote flow control
		const byte TNO_XDISPLAY		= 35; // 23 X-Display location
		const byte TNO_NEWENV		= 39; // 27 New environment option
		// TELNET others
		const byte TNX_SEND			= 1;  // 01 send, e.g. used with SB terminal type
		const byte TNX_IS			= 0;  // 00 is, e.g. used with SB terminal type
		#endregion

		#region Constructors and destructors
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hostName">IP address, e.g. 192.168.0.20</param>
		public Terminal(string hostName) : this(hostName, 23, 10, 80, 40)
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
		public Terminal(string hostName, int port, int timeoutSeconds, int virtualScreenWidth, int virtualScreenHeight) 
		{
			this.hostName = hostName;
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
		~Terminal() 
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
		#endregion

		#region Connect / Close
		/// <summary>
		/// Connect to the telnet server
		/// </summary>
		/// <returns>true if connection was successful</returns>
		public bool Connect() 
		{
			// check for buffer
			if (this.buffer==null)
				this.buffer = new byte[RECEIVEBUFFERSIZE];

			// virtual screen
			if (this.virtualScreen==null)
				this.virtualScreen = new VirtualScreen(this.vsWidth, this.vsHeight, 1, 1);

			// set the callbacks
			if (this.callBackReceive==null)
				this.callBackReceive = new AsyncCallback(ReadFromStream);
			if (this.callBackSend==null)
				this.callBackSend = new AsyncCallback(WriteToStream);

			// flags
			this.serverEcho = false;
			this.clientInitNaws = false;
			this.firstResponse = true;
			this.nawsNegotiated = false;
			this.forceLogout = false;

			// return physical connection
			if (this.tcpClient!=null) 
				return true; // we still have a connection -> ?? better reconnect ??
			else 
			{
				try 
				{
					// TODO: Improve performance ...?
					// This is not working:	IPAddress ipAddress = IPAddress.Parse(this.hostName);
					//						IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, this.port);
					// because it addresses local endpoints
					this.tcpClient = new TcpClient(this.hostName, this.port);
					this.tcpClient.ReceiveTimeout = this.timeoutReceive;
					this.tcpClient.SendTimeout = this.timeoutSend;
					this.tcpClient.NoDelay = true;
					this.tcpClient.GetStream().BeginRead(this.buffer, 0, this.buffer.Length, this.callBackReceive, null);
					return true;
				}
				catch 
				{
					this.tcpClient = null;
					return false;
				}
			}
		} // Connect()

		/// <summary>
		/// Closes external resources.
		/// Safe, can be called multiple times
		/// </summary>
		public void Close() 
		{
			// physical connection
			if (this.tcpClient!=null) 
			{
				try 
				{
					if (this.tcpClient.GetStream() != null) 
					{
						// it is important to close the stream
						// because somehow tcpClient does not physically breaks down
						// the connection - on "one connection" telnet server the 
						// server remains blocked if not doing it!
						try 
						{
							this.tcpClient.GetStream().Close();
						} 
						catch 
						{
							// further handling would go here
						}
					}
					this.tcpClient.Close();
					this.tcpClient = null;
				} 
				catch 
				{
					this.tcpClient = null;
				}
			}

			// clean up
			// fast, "can be done several" times
			this.virtualScreen = null;
			this.buffer = null;
			this.callBackReceive = null;
			this.callBackSend = null;
			this.forceLogout = false;

		} // Close()

		/// <summary>
		/// Is connection still open?
		/// </summary>
		/// <returns>true if connection is open</returns>
		public bool IsOpenConnection() 
		{
			return (this.tcpClient!=null);
		}
		#endregion
		
		#region Send response to Telnet server
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
				if (!this.IsOpenConnection() || this.tcpClient==null)
					return false;
				if (response==null || response.Length<1)
					return true; // nothing to do
				byte[] sendBuffer = (endLine) ? System.Text.Encoding.ASCII.GetBytes(response + ENDOFLINE) : System.Text.Encoding.ASCII.GetBytes(response);
				if (sendBuffer==null || sendBuffer.Length<1)
					return false;
				this.tcpClient.GetStream().BeginWrite(sendBuffer, 0, sendBuffer.Length, this.callBackSend, null);
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
			if (key <1 || key > 12)
				return false;
			switch(key)       
			{         
				case 1:   
					return this.SendResponse(F1, false);
				case 2:            
					return this.SendResponse(F2, false);
				case 3:            
					return this.SendResponse(F3, false);
				case 4:            
					return this.SendResponse(F4, false);
				case 5:            
					return this.SendResponse(F5, false);
				case 6:            
					return this.SendResponse(F6, false);
				case 7:            
					return this.SendResponse(F7, false);
				case 8:            
					return this.SendResponse(F8, false);
				case 9:            
					return this.SendResponse(F9, false);
				case 10:            
					return this.SendResponse(F10, false);
				case 11:            
					return this.SendResponse(F11, false);
				case 12:            
					return this.SendResponse(F12, false);
				default:            
					// this should never be reached
					return false;
			}
		}
		#endregion

		#region Send Telnet logout sequence
		/// <summary>
		/// Send a synchronously telnet logout-response
		/// </summary>
		/// <returns></returns>
		public bool SendLogout() 
		{
			return this.SendLogout(true);
		}

		/// <summary>
		/// Send a telnet logout-response
		/// </summary>
		/// <param name="synchronous">Send synchronously (true) or asynchronously (false)</param>
		/// <returns></returns>
		public bool SendLogout(bool synchronous) 
		{
			byte[] lo = {TNC_IAC, TNC_DO, TNO_LOGOUT};
			try 
			{
				if (synchronous) 
				{
					this.tcpClient.GetStream().Write(lo, 0, lo.Length);
				} 
				else 
				{
					this.tcpClient.GetStream().BeginWrite(lo, 0, lo.Length, this.callBackSend, null);
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
			if (this.virtualScreen==null || searchFor==null || searchFor.Length < 1)
				return null;
			// use the appropriate timeout setting, which is the smaller number
			int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
			DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
			string found = null;
			do
			{
				lock(this.virtualScreen) 
				{
                    found = this.virtualScreen.FindOnScreen(searchFor, caseSensitive);	
				}
				if (found!=null)
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
			if (this.virtualScreen==null || regEx==null || regEx.Length < 1)
				return null;
			int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
			DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
			string found = null;
			do // at least once
			{
				lock(this.virtualScreen) 
				{
					found = this.virtualScreen.FindRegExOnScreen(regEx);	
				}
				if (found!=null)
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
			if (this.virtualScreen==null || timeoutSeconds < 0)
				return false;

			// reset flag: This has been added after the feedback of Mark
			if (this.virtualScreen.ChangedScreen)
				this.virtualScreen.Hardcopy(false); 

			// Only reset
			if (timeoutSeconds <= 0)
				return false;

			// wait for changes, the goal is to test at TRAILS times, if not timing out before
			int sleepTimeMs = this.GetWaitSleepTimeMs(timeoutSeconds);
			DateTime endTime = this.TimeoutAbsoluteTime(timeoutSeconds);
			do // run at least once
			{
				lock(this.virtualScreen) 
				{
					if (this.virtualScreen.ChangedScreen)
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
			if (seconds>0)
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
			if (timeoutSeconds<0 && this.timeoutReceive <0)
				return 0;
			else if (timeoutSeconds<0)
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

		#region Callback function ReadFromStream
		/// <summary>
		/// Callback function to read from the network stream
		/// </summary>
		/// <param name="asyncResult">Callback result</param>
		private void ReadFromStream(IAsyncResult asyncResult)
		{
			if (asyncResult == null || this.tcpClient == null)
				this.Close();
			
			// read
			try
			{
				// bytes read
				// NOT needed: this.callBackReceive.EndInvoke(asyncResult); -> exception
				int bytesRead = this.tcpClient.GetStream().EndRead(asyncResult);
				
				if (bytesRead > 0)
				{
					// Translate the data and write output to Virtual Screen
					// DO this thread save to make sure we do not "READ" from screen meanwhile
					lock(this.virtualScreen) 
					{
						this.ParseAndRespondServerStream(bytesRead);
					}
			
					// Reinitialize callback
					this.CleanBuffer(bytesRead);
					if (this.forceLogout)
						this.Close();
					else
						this.tcpClient.GetStream().BeginRead(this.buffer, 0, this.buffer.Length, this.callBackReceive, null);
				}
				else
					// the connection was terminated by the server
					this.Close();
			}
			catch 
			{
				// the connection was terminated by the server
				this.Close();
			} 
		} // read from network stream
		#endregion

		#region Callback function: Write to stream
		/// <summary>
		/// Callback function to write to the network stream
		/// </summary>
		/// <param name="asyncResult">Callback result</param>
		private void WriteToStream(IAsyncResult asyncResult) 
		{
			if (asyncResult == null || this.tcpClient == null)
				this.Close();

			// write 
			try 
			{
				this.tcpClient.GetStream().EndWrite(asyncResult);
			}
			catch 
			{
				// the connection was terminated by the server
				this.Close();
			} // catch
		} // write network stream
		#endregion

		#region ParseAndRespondServerStream
		/// <summary>
		/// Go thru the data received and answer all technical server
		/// requests (TELNET negotiations).
		/// </summary>
		/// <param name="bytesRead">number of bytes read</param>
		/// <remarks>
		/// Thread saftey regarding the virtual screen needs to be considered
		/// </remarks>
		private void ParseAndRespondServerStream(int bytesRead) 
		{
			// reponse to server
			MemoryStream response = new MemoryStream(SENDBUFFERSIZE); // answer usually will be small: "a few bytes only"

			// cycle thru the buffer
			int bc=0;
			while (this.buffer!=null && bc < bytesRead && bc < this.buffer.Length ) 
			{	
				try 
				{
					switch(this.buffer[bc]) 
					{
						// ESC
						case ESC:
							bc = this.ParseEscSequence(bc, response);
							break;
						case CR:
							this.virtualScreen.WriteByte(CR);
							break;
						case LF:
							this.virtualScreen.WriteByte(LF);
							break;
							// DO
						case TNC_IAC:
							bc++;
							switch(this.buffer[bc]) 
							{
								case TNC_DO:
									bc++;
									switch(this.buffer[bc]) 
									{
										// DO ...
										case TNO_TERMSPEED:
											Terminal.TelnetWont(TNO_TERMSPEED, response); // no negotiation about speed
											break;
										case TNO_NAWS:
											if (!this.clientInitNaws)
												Terminal.TelnetWill(TNO_NAWS, response); // negotiation about window size
											Terminal.TelnetSubNAWS(this.virtualScreen.Width, this.virtualScreen.Height, response);
											this.nawsNegotiated = true;
											break;
										case TNO_TERMTYPE:
											Terminal.TelnetWill(TNO_TERMTYPE, response); // negotiation about terminal type
											break;
										case TNO_XDISPLAY:
											Terminal.TelnetWont(TNO_XDISPLAY, response); // no negotiation about X-Display
											break;
										case TNO_NEWENV:
											Terminal.TelnetWont(TNO_NEWENV, response); // no environment
											break;
										case TNO_ECHO:
											Terminal.TelnetWont(TNO_ECHO, response); // no echo from client
											break;
										case TNO_REMOTEFLOW:
											Terminal.TelnetWont(TNO_REMOTEFLOW, response); // no echo from client
											break;
										case TNO_LOGOUT:
											Terminal.TelnetWill(TNO_LOGOUT, response); // no echo from client
											this.forceLogout = true;
											break;
										default:
											// all we do not understand =>
											// WONT
											Terminal.TelnetWont(this.buffer[bc], response); // whatever -> WONT
											break;
									} // SWITCH DO XX
									break; 
									// DONT
								case TNC_DONT:
									bc++; // ignore DONTs
									break;
								// SUB-NEGOTIATION
								case TNC_SB:
									bc++;
									switch(this.buffer[bc]) 
									{
										// SUB ...
										case TNO_TERMTYPE:
											bc++; // the follwing byte is usually a send-request ("SEND"), just read the byte 
											Terminal.TelnetSubIsANSI(response);
											break;
										case TNO_NAWS:
											bc++; // the follwing byte is usually a send-request ("SEND"), just read the byte 
											Terminal.TelnetSubNAWS(this.virtualScreen.Width, this.virtualScreen.Height, response);
											this.nawsNegotiated = true;
											break;
										default:
											// read until the end of the subrequest
											while (buffer[bc]!=TNC_SE && bc < buffer.Length) 
											{
												bc++;
											}
											break;
									} // SUB NEG XX
									break; 
									// WILL AND WONTs FROM SERVER
								case TNC_WILL:
									// Server's WILLs
									bc++;
									switch (this.buffer[bc]) 
									{
										case TNO_ECHO:
											this.serverEcho = true;
											Terminal.TelnetDo(this.buffer[bc], response);
											break;
										case TNO_LOGOUT:
											// usually a reponse on my logout
											// I do no say anything but force the end
											this.forceLogout = true;
											break;
										default:
											// other WILLs OF SERVER -> confirm
											Terminal.TelnetDo(this.buffer[bc], response);
											break;
									}
									break;
								case TNC_WONT:
									bc++; 
									// Server's WONTs
									bc++;
									switch (this.buffer[bc]) 
									{
										case TNO_ECHO:
											this.serverEcho = false;
											break;
										default:
											break;
									}
									break;				
								default:
									break;
								}
							break;
						default:
							// no command, data
							this.virtualScreen.WriteByte(this.buffer[bc]);
							break;
					} // switch
					bc++;
				} 
				catch (IndexOutOfRangeException eor) 
				{
					string e = eor.ToString();
					break;
				}
			} // while

			//
			// send the response
			//
			if (response != null && this.firstResponse) 
			{
				// send some own WILLs even if not asked as DOs
				if (!this.nawsNegotiated) 
				{
					Terminal.TelnetWill(TNO_NAWS, response);
					this.clientInitNaws = true;
				}
			} // 1st response

			//
			// respond synchronously !
			// -> we know that we really have send the bytes
			//
			if (response!=null && response.Position>0) 
			{
				byte[] r = Terminal.MemoryStreamToByte(response);
				if (r!=null && r.Length>0) 
				{
					this.tcpClient.GetStream().Write(r, 0, r.Length);
					this.tcpClient.GetStream().Flush();
					this.firstResponse = false;
				}
			}

			// clean up
			try 
			{
				if (response!=null)
					response.Close();
				response = null;
			} 
			catch 
			{
				// ignore
			}
		} // method
		#endregion

		#region ParseEscSequence
		/// <summary>
		/// Deal with ESC Sequences as in VT100, ..
		/// </summary>
		/// <param name="bc">current buffer counter</param>
		/// <param name="response">Stream for the response (back to Telnet server)</param>
		/// <returns>new buffer counter (last byte dealed with)</returns>
		/// <remarks>
		/// Thread saftey regarding the virtual screen needs to be considered
		/// </remarks>
		private int ParseEscSequence(int bc, MemoryStream response) 
		{
			// some sequences can only be terminated by the end characters
			// (they contain wildcards) => 
			// a switch / case decision is not feasible
			if (this.buffer==null) 
				return bc;

			// find the byte after ESC
			if (this.buffer[bc] == ESC)
				bc++;

			// now handle sequences
			int m = -1;

			// Cursor Movement Commands 
			//  Index                           ESC D
			//  Next Line                       ESC E
			//  Reverse index                   ESC M
			//  Save cursor and attributes      ESC 7
			//  Restore cursor and attributes   ESC 8
			if ((m = this.MatchSequence(bc, "D")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "E")) > -1) 
			{
				this.virtualScreen.CursorNextLine();
				return (bc + m -1); 
			}
			else if ((m = this.MatchSequence(bc, "M")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "7")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "8")) > -1) 
			{
				return (bc + m -1);
			}
				// Cursor movements
				//  Cursor forward (right)          ESC [ Pn C, e.g. "[12C"	
				//  Cursor backward (left)          ESC [ Pn D,	e.g. "[33D"
				//	Direct cursor addressing        ESC [ Pl; Pc H  or
				//									ESC [ Pl; Pc f
				// Reg Exp: \[ = [  \d = 0-9  + 1 time or more
			else if ((m = this.MatchRegExp(bc, REGEXPCURSORLEFT)) > -1) 
			{
				this.virtualScreen.MoveCursor(-1);
				return (bc + m -1); 
			}
			else if ((m = this.MatchRegExp(bc, REGEXPCURSORRIGHT)) > -1) 
			{
				this.virtualScreen.MoveCursor(1);
				return (bc + m -1); 
			}
			else if ((m = this.MatchRegExp(bc, REGEXPCURSORPOSITION)) > -1) 
			{
				string sequence = Encoding.ASCII.GetString(this.buffer, bc, m);
                int nx = NewCursorXPosition(sequence);
                int ny = NewCursorYPosition(sequence);
				this.virtualScreen.MoveCursorTo(nx, ny);
				return (bc + m -1);
			}
				// Scrolling region 
				//  ESC [ Pt ; Pb r
			else if ((m = this.MatchRegExp(bc, REGEXPSCROLLINGREGION)) > -1) 
			{
				return (bc + m -1);
			}
				// Line Size (Double-Height and Double-Width) Commands
				//  Change this line to double-height top half      ESC # 3
				//  Change this line to double-height bottom half   ESC # 4
				//  Change this line to single-width single-height  ESC # 5
				//  Change this line to double-width single-height  ESC # 6
			else if ((m = this.MatchSequence(bc, "#3")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "#4")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "#5")) > -1) 
				return (bc + m -1); 
			else if ((m = this.MatchSequence(bc, "#6")) > -1) 
				return (bc + m -1);
				// Erasing
				//  From cursor to end of line              ESC [ K  or ESC [ 0 K
				//  From beginning of line to cursor        ESC [ 1 K
				//  Entire line containing cursor           ESC [ 2 K
				//  From cursor to end of screen            ESC [ J  or ESC [ 0 J
				//  From beginning of screen to cursor      ESC [ 1 J
				//  Entire screen                           ESC [ 2 J
			else if ((m = this.MatchSequence(bc, "[K")) > -1) 
			{
				this.virtualScreen.CleanLine(this.virtualScreen.CursorX, this.virtualScreen.CursorXRight);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[0K")) > -1) 
			{
				this.virtualScreen.CleanLine(this.virtualScreen.CursorX, this.virtualScreen.CursorXRight);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[1K")) > -1) 
			{
				this.virtualScreen.CleanLine(this.virtualScreen.CursorXLeft, this.virtualScreen.CursorX);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[2K")) > -1) 
			{
				this.virtualScreen.CleanLine(this.virtualScreen.CursorXLeft, this.virtualScreen.CursorXRight);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[J")) > -1) 
			{
				this.virtualScreen.CleanFromCursor();
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[0J")) > -1)
			{
				this.virtualScreen.CleanFromCursor();
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[1J")) > -1) 
			{
				this.virtualScreen.CleanToCursor();
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[2J")) > -1) 
			{
				// erase entire screen
				this.virtualScreen.CleanScreen();
				return (bc + m -1);
			}
				// Hardcopy                ESC # 7
				// Graphic processor ON    ESC 1
				// Graphic processor OFF   ESC 2
			else if ((m = this.MatchSequence(bc, "#7")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "1")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "2")) > -1) 
			{
				return (bc + m -1);
			}
				// TAB stops
				//  Set tab at current column               ESC H
				//  Clear tab at curent column              ESC [ g or ESC [ 0 g
				//  Clear all tabs                          ESC [ 3 g
			else if ((m = this.MatchSequence(bc, "H")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[g")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[0g")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[3g")) > -1) 
			{
				return (bc + m -1);
			}
				// Modes:
				//  Line feed/new line   New line    ESC [20h   Line feed   ESC [20l
				//  Cursor key mode      Application ESC [?1h   Cursor      ESC [?1l
				//  ANSI/VT52 mode       ANSI        N/A        VT52        ESC [?2l
				//  Column mode          132 Col     ESC [?3h   80 Col      ESC [?3l
				//  Scrolling mode       Smooth      ESC [?4h   Jump        ESC [?4l
				//  Screen mode          Reverse     ESC [?5h   Normal      ESC [?5l
				//  Origin mode          Relative    ESC [?6h   Absolute    ESC [?6l
				//  Wraparound           On          ESC [?7h   Off         ESC [?7l
				//  Auto repeat          On          ESC [?8h   Off         ESC [?8l
				//  Interlace            On          ESC [?9h   Off         ESC [?9l
				//  Graphic proc. option On          ESC 1      Off         ESC 2
				//  Keypad mode          Application ESC =      Numeric     ESC >
			else if ((m = this.MatchSequence(bc, "[20h")) > -1) 
			{
				this.virtualScreen.CursorNextLine();
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[20l")) > -1) 
			{
				response.WriteByte(10);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "[?1h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?1l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?3h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?3l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?4h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?4l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?5h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?5l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?6h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?6l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?7h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?7l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?8h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?8l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?9h")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "[?9l")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "1")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "2")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "=")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, ">")) > -1) 
			{
				return (bc + m -1);
			}
				// VT 52 compatibility mode
				//  Cursor Up                               ESC A
				//  Cursor Down                             ESC B
				//  Cursor Right                            ESC C
				//  Cursor Left                             ESC D
				//  Select Special Graphics character set   ESC F
				//  Select ASCII character set              ESC G
				//  Cursor to home                          ESC H
				//  Reverse line feed                       ESC I
				//  Erase to end of screen                  ESC J
				//  Erase to end of line                    ESC K
				//  Direct cursor address                   ESC Ylc         (see note 1)
				//  Identify                                ESC Z           (see note 2)
				//  Enter alternate keypad mode             ESC =
				//  Exit alternate keypad mode              ESC >
				//  Enter ANSI mode                         ESC <
			else if ((m = this.MatchSequence(bc, "A")) > -1) 
			{
				this.virtualScreen.MoveCursorVertical(-1);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "B")) > -1) 
			{
				this.virtualScreen.MoveCursorVertical(1);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "C")) > -1) 
			{
				this.virtualScreen.MoveCursorVertical(1);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "D")) > -1) 
			{
				this.virtualScreen.MoveCursorVertical(-1);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "F")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "G")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "H")) > -1) 
			{
				this.virtualScreen.CursorReset();
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "I")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "J")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "K")) > -1) 
			{
				this.virtualScreen.CleanLine(this.virtualScreen.CursorX, this.virtualScreen.CursorXRight);
				return (bc + m -1);
			}
			else if ((m = this.MatchSequence(bc, "Ylc")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "Z")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "=")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, "<")) > -1) 
				return (bc + m -1);
			else if ((m = this.MatchSequence(bc, ">")) > -1) 
				return (bc + m -1);
			// nothing found
			return bc;
		} // ParseEscSequence
		#endregion

		#region MatchSequence- and MatchRegEx-methods
		/// <summary>
		/// Does the sequence match the buffer starting at 
		/// current index?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">Bytes need to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, byte[] sequence) 
		{
			return this.MatchSequence(bufferCounter, sequence, null);
		}

		/// <summary>
		/// Does the sequence match the buffer starting at 
		/// current index?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">Bytes need to match</param>
		/// <param name="ignoreIndex">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// 
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, byte[] sequence, int ignoreIndex) 
		{
			return this.MatchSequence(bufferCounter, sequence, new int[] {ignoreIndex});
		}

		/// <summary>
		/// Does the sequence match the buffer starting at 
		/// current index?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">Bytes need to match</param>
		/// <param name="ignoreIndex1">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <param name="ignoreIndex2">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, byte[] sequence, int ignoreIndex1, int ignoreIndex2) 
		{
			return this.MatchSequence(bufferCounter, sequence, new int[] {ignoreIndex1, ignoreIndex2});
		}

		/// <summary>
		/// Does the sequence match the buffer starting at 
		/// current index?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">Bytes need to match</param>
		/// <param name="ignoreIndex">Index of bytes which do not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, byte[] sequence, int[] ignoreIndex) 
		{
			if (sequence==null || this.buffer==null)
				return -1;
			if (this.buffer.Length < (bufferCounter + sequence.Length))
				return -1; // overflow
			for (int i=0; i < sequence.Length; i++) 
			{
				if (this.buffer[bufferCounter+i] != sequence[i]) 
				{
					// not a match
					if (ignoreIndex==null || ignoreIndex.Length<1)
						return -1; // no wildcards
					bool wildcard = false;
					for (int wc=0; wc < ignoreIndex.Length; wc++) 
					{
						if (ignoreIndex[wc]==i) 
						{
							wildcard = true;
							break;
						}
					} // for "wildcard"
					if (!wildcard)
						return -1; // no wildcard found
				} // no match
			}
			return sequence.Length;
		}

		/// <summary>
		/// Does the sequence match the buffer?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">String needs to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, string sequence) 
		{
			if (sequence==null)
				return -1;
			return this.MatchSequence(bufferCounter, Encoding.ASCII.GetBytes(sequence));
		}

		/// <summary>
		/// Does the sequence match the buffer?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">String needs to match</param>
		/// <param name="ignoreIndex">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, string sequence, int ignoreIndex) 
		{
			if (sequence==null)
				return -1;
			return this.MatchSequence(bufferCounter, Encoding.ASCII.GetBytes(sequence), ignoreIndex);
		}

		/// <summary>
		/// Does the sequence match the buffer?
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="sequence">String needs to match</param>
		/// <param name="ignoreIndex1">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <param name="ignoreIndex2">Index of the byte which does not NEED to match, e.g. 2 means the 3rd byte (index 2) does not need to match</param>
		/// <returns>Number of characters matching</returns>
		private int MatchSequence(int bufferCounter, string sequence, int ignoreIndex1, int ignoreIndex2) 
		{
			if (sequence==null)
				return -1;
			return this.MatchSequence(bufferCounter, Encoding.ASCII.GetBytes(sequence), ignoreIndex1, ignoreIndex2);
		}

		/// <summary>
		/// Match a regular Expression
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="regExp">Regular expression</param>
		/// <returns>Number of characters matching</returns>
		private int MatchRegExp(int bufferCounter, String regExp) 
		{
			if (regExp==null || regExp.Length<1)
				return -1;
			else
				return this.MatchRegExp(bufferCounter, new Regex(regExp));
		}

		/// <summary>
		/// Match a regular Expression
		/// </summary>
		/// <param name="bufferCounter">Current buffer counter</param>
		/// <param name="r">Regular expression object</param>
		/// <returns>Number of characters matching</returns>
		private int MatchRegExp(int bufferCounter, Regex r) 
		{
			if (r==null || this.buffer==null || this.buffer.Length < bufferCounter)
				return -1;
			// build a dummy string
			// which can be checked
			string dummyString = null;
			const int dsl = 10;
			if (this.buffer.Length >= ( bufferCounter + dsl))
				dummyString = Encoding.ASCII.GetString(this.buffer, bufferCounter, dsl);
			else
				dummyString = Encoding.ASCII.GetString(this.buffer, bufferCounter, this.buffer.Length - bufferCounter);
			if (dummyString==null || dummyString.Length<1)
				return -1;
			Match m = r.Match(dummyString);
			if (m.Success && m.Index==0) 
			{
				return m.Length;
			} 
			else
				return -1;
		}
		#endregion

		#region Cursor movements in virtual screen
		/// <summary>
		/// Find the X position in a VT cursor position sequence.
		/// This only works if the sequence is a valid position sequence!
		/// </summary>
		/// <param name="escSequence">Valid position sequence</param>
		/// <returns>X position (column)</returns>
        private /* static */ int NewCursorXPosition(string escSequence) 
		{
			const int DEFAULT = SCREENXNULLCOORDINATE;
			if (escSequence==null)
				return -1; // error
			Match m = Terminal.REGEXPCURSORXPOSITION.Match(escSequence);
			if (!m.Success)
				return DEFAULT; // default;
			else 
			{
				m = Terminal.REGEXPNUMBER.Match(m.Value);
				if (m.Success)
				{
					try 
					{
						return int.Parse(m.Value);
					}
					catch 
					{
						return DEFAULT;
					}
				} 
				else 
				{
					return DEFAULT;
				}
			}
		} // method

		/// <summary>
		/// Find the Y position in a VT cursor position sequence.
		/// This only works if the sequence is a valid position sequence!
		/// </summary>
		/// <param name="escSequence">Valid position sequence</param>
		/// <returns>Y position (column)</returns>
		private /* static */ int NewCursorYPosition(string escSequence) 
		{
			const int DEFAULT = SCREENYNULLCOORDINATE;
			if (escSequence==null)
				return -1; // error
			Match m = Terminal.REGEXPCURSORYPOSITION.Match(escSequence);
			if (!m.Success)
				return DEFAULT; // default;
			else 
			{
				m = Terminal.REGEXPNUMBER.Match(m.Value);
				if (m.Success)
				{
					try 
					{
						return int.Parse(m.Value);
					}
					catch 
					{
						return DEFAULT;
					}
				} 
				else 
				{
					return DEFAULT;
				}
			}
		} // method

		/// <summary>
		/// Move the cursor for n-positions.
		/// </summary>
		/// <param name="escSequence">Valid ESC sequence for cursor right/left</param>
		/// <returns>move about n positions</returns>
		private static int CursorMovements(string escSequence) 
		{
			const int DEFAULT = 1;
			if (escSequence==null)
				return -1; // error
			Match m = Terminal.REGEXPNUMBER.Match(escSequence);
			if (!m.Success)
				return DEFAULT; // default;
			else 
			{
				try 
				{
					return int.Parse(m.Value);
				}
				catch 
				{
					return DEFAULT;
				}
			} // else
		} // method
		#endregion

		#region Telnet sub-responses as WILL, WONT ..
		/// <summary>
		/// Add a "WILL" response, e.g. "WILL negotiate about terminal size"
		/// </summary>
		/// <param name="willDoWhat"></param>
		/// <param name="response"></param>
 		private static void TelnetWill(byte willDoWhat, MemoryStream response) 
		{
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_WILL);
			response.WriteByte(willDoWhat);
		}

		/// <summary>
		/// Add a "WONT" response, e.g. "WONT negotiate about terminal size"
		/// </summary>
		/// <param name="wontDoWhat"></param>
		/// <param name="response"></param>
		private static void TelnetWont(byte wontDoWhat, MemoryStream response) 
		{
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_WONT);
			response.WriteByte(wontDoWhat);
		}

		/// <summary>
		/// Add a "DO" response, e.g. "DO ..."
		/// </summary>
		/// <param name="doWhat"></param>
		/// <param name="response"></param>
		private static void TelnetDo(byte doWhat, MemoryStream response) 
		{
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_DO);
			response.WriteByte(doWhat);
		}

		/// <summary>
		/// Add a "DONT" response, e.g. "DONT ..."
		/// </summary>
		/// <param name="dontDoWhat"></param>
		/// <param name="response"></param>
		private static void TelnetDont(byte dontDoWhat, MemoryStream response) 
		{
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_DONT);
			response.WriteByte(dontDoWhat);
		}

		/// <summary>
		/// Add a telnet sub-negotiation for ANSI 
		/// terminal
		/// </summary>
		/// <param name="response">MemoryStream</param>
		private static void TelnetSubIsANSI(MemoryStream response) 
		{
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_SB);
			response.WriteByte(TNO_TERMTYPE);
			response.WriteByte(TNX_IS);
			response.WriteByte(65); // "A"
			response.WriteByte(78); // "N"
			response.WriteByte(83); // "S"
			response.WriteByte(73); // "I"
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_SE);
		} // method

		/// <summary>
		/// Telnet sub send terminal size.
		/// </summary>
		/// <param name="w">window width</param>
		/// <param name="h">window height</param>
		/// <param name="response">MemoryStream</param>
		private static void TelnetSubNAWS(int w, int h, MemoryStream response) 
		{
			byte wl =  (byte) (0x00FF & w); 
			byte wh =  (byte) (0xFF00 & w); 
			byte hl =  (byte) (0x00FF & h); 
			byte hh =  (byte) (0xFF00 & h); 
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_SB);
			response.WriteByte(TNO_NAWS);
			response.WriteByte(wh);
			response.WriteByte(wl);
			response.WriteByte(hh);
			response.WriteByte(hl);
			response.WriteByte(TNC_IAC);
			response.WriteByte(TNC_SE);
		} // method
		#endregion

		#region Misc. helper-methods
		/// <summary>
		/// Cleans the buffer - not necessary since the values
		/// would just be overwritten - but useful for debugging!
		/// </summary>
		/// <param name="bytesRead">Bytes read and need cleaning</param>
		private void CleanBuffer(int bytesRead) 
		{
			if (this.buffer==null)
				return;
			for (int i=0; i < bytesRead && i < this.buffer.Length; i++) 
			{
				this.buffer[i] = 0;
			}
		} // method

		/// <summary>
		/// The MemoryStream bas a bigger byte buffer than bytes
		/// were really written to it. This method fetches all bytes
		/// up the the position written to.
		/// </summary>
		/// <param name="ms">MemoryStream</param>
		/// <returns>really written bytes</returns>
		private static byte[] MemoryStreamToByte(MemoryStream ms) 
		{
			// I've tried several options to convert this
			// This one here works but may be improved.
			// ms.Read(wb, 0, wb.Length); did not work
			// ms.ToArray delivers the whole buffer not only the written bytes
			if (ms==null)
				return null;
			else if (ms.Position<2)
				return new byte[0];

			// convert
			byte[] wb = new byte[ms.Position];
			byte[] allBytes = ms.ToArray();
			for (int i=0; i<wb.Length && i<allBytes.Length; i++) 
			{
				wb[i]=allBytes[i];
			}
			return wb;
		} // method

		/// <summary>
		/// Helper to find a valid IP with a string
		/// </summary>
		/// <param name="candidate">search this string for IP</param>
		/// <returns>IP address or null</returns>
		public /* static */ string FindIPAddress(string candidate) 
		{
			if (candidate==null)
				return null;
			Match m = REGEXPIP.Match(candidate);
			if (m!=null && m.Success)
				return m.Value;
			else
				return null;
		}
		#endregion

	} // class

	#region Custom exceptions
	/// <summary>
	/// Exception dealing with connectivity
	/// </summary>
	public class TelnetException : ApplicationException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Exception's message</param>
		public TelnetException (string message) : base(message)
		{
			// further code
		}
	} // Exception class

	/// <summary>
	/// Exception dealing with parsing ...
	/// </summary>
	public class TerminalException : ApplicationException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">Exception's message</param>
		public TerminalException (string message) : base(message)
		{
			// further code
		}
	} // Exception class
	#endregion
} // namespace