using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Core;
using TerminalSessions;

namespace TelnetSession
{
    /* Optional: Register the assembly in the global assembly cache:
     * 
     * gacutil –i TelnetSessions.dll
     * 
     * Register ComObject:
     *      C:\WINDOWS\Microsoft.NET\Framework64\v2.0.50727\RegAsm.exe /register TelnetSession.dll /codebase /tlb /verbose
    */
    #region InterfaceDefinition
    [Guid("406361C7-E7BA-4D7B-8DD1-B3739C506E3E"),
    InterfaceType(ComInterfaceType.InterfaceIsDual),
    ComVisible(true)]
    public interface IActiveXTelnet
    {
        [DispId(1)]
        int doLoginObj(string Dev, string ConOptions);
        [DispId(2)]
        void doLogout();
        [DispId(3)]
        int Jump2NextDeviceObj(string nextDevice, string ConOptions);
        [DispId(4)]
        object[] GetIntoConfigModeObj();
        [DispId(5)]
        object[] ExitConfigModeObj();
        [DispId(6)]
        //[return: MarshalAs(UnmanagedType.Struct, SafeArraySubType = VarEnum.VT_ARRAY)]
        //[return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_VARIANT)]
        object[] SendCmdObj(/* [ArgumentUsage("Send this string to the Device.")] */ string l_cmd);
        [DispId(7)]
        object[] GetShowVersionObj();
        [DispId(8)]
        object[] GetRunningConfigObj();
        [DispId(9)]
        string GetDeviceType { get; }
        [DispId(10)]
        bool Init(string ConfigFile);
        [DispId(11)]
        int Debug { get; set;  }

        //TelentSSHLogin-Passthrough
        [DispId(12)]
        string Prompt { get; set; }
        [DispId(13)]
        string WaitFor(object[] searchFor, bool CaseSensitive, int TimeOut);
        [DispId(14)]
        string WaitForString(string searchFor);
        [DispId(15)]
        string WaitForRegEx(string regEx);
        [DispId(16)]
        void DeleteScreen();
        [DispId(17)]
        string ShowScreen();
        [DispId(18)]
        void print(string strCmd);
        [DispId(19)]
        void put(string strCmd);

        //IP-Funtions
        [DispId(20)]
        string GetIPNet(string IP, string Subnet);
        [DispId(21)]
        string GetIPBCast(string IP, string Subnet);
        [DispId(22)]
        string Add2IP(string IP, long Offset);
    }
    #endregion

    [Guid("7BA015F1-9BD6-4C9A-B5E0-415A07EB5541"),
    ClassInterface(ClassInterfaceType.None),
    ComDefaultInterface(typeof(IActiveXTelnet)),
    ComVisibleAttribute(true),
    ProgId("THB.Terminal.TelnetSession")]
    public class TelnetDevice : BaseDevice, ITermDevice, /* IObjectSafetyImpl,*/ IActiveXTelnet, IDisposable
    {
        //[Serializable()]

        //string unknownPrompt = @"([(\<\[]?[a-zA-Z0-9\-\.@~()]+\s?[a-zA-Z0-9\-\.@~()\/:]+\s?)[\>#]\s*\r?\n?$";
            //@"([a-zA-Z0-9\-\.@~()]+\s?[a-zA-Z0-9\-\.@~()]+\s?)[\>#]\s*\r?\n?$";
            //@"([a-zA-Z0-9\-\.@~()]+\s?){1}[\>#]\s*\r?\n?$"; // /[>#]$/m  # “/[s().-]*[\$#>]s?(?:\enable\))?\s**$/”
        int bConnected = 0, Timeout = 15, PWDCounter = 0, DeviceCounter = 0;
        bool EnableMode = false; //, bConfigMode = false;
        string DevicePromptString = "";
        private bool disposed = false;

        public TelnetDevice() {
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
                    if (shell != null)
                    {
                        if (shell.Connected)
                            shell.close();
                        shell = null;
                    }
                    if (UserArray != null)
                        UserArray.Clear();
                    UserArray = null;
                    if (PWDArray != null)
                        PWDArray.Clear();
                    PWDArray = null;
                    if (enPWDArray != null)
                        enPWDArray.Clear();
                    enPWDArray = null;
                    if (DeviceHistory != null)
                        DeviceHistory.Clear();
                    DeviceHistory = null;
                }

                //free unmanaged objects
                //AdditionalCleanup();

                this.disposed = true;
            }
        }
        ~TelnetDevice()
        {
            Dispose(false);
        }

        public int doLoginObj(string Dev, string ConOptions)
        {
            return ((int)doLogin(Dev, ConOptions));
        }

        public LoginStatus doLogin(string Device, string ConnectOptions)
        {
            return(doLogin(Device, ConnectOptions, ""));
        }

        public LoginStatus doLogin(string Device, string ConnectOptions, string DevType)
        {
            string strOutput = "";
            string savedDevicePrompt = "";
            PWDCounter = 0;
            DevicePromptString = "";
            DevicePrompt = "";
            DevicePromptConfig = "";
            EnableMode = false;
            bConfigMode = false;
            DeviceType = DevType;
            LoginStatus bResult = LoginStatus.InProgress;

            while ((bResult == LoginStatus.InProgress) && (PWDCounter < UserArray.Count))
            {
                bResult = Login(Device, ConnectOptions);

                if((bConnected > 0) && (bResult == LoginStatus.InProgress))
                    bConnected --;
            }
            if( _Debug > 0 )
                Console.WriteLine("Login-Result: " + bResult);

            if( DevicePrompt.Length == 0) {
                if( _Debug > 0 )
                    Console.WriteLine("\nExit by me !");
                if(bConnected > 0)
                    bConnected --;
                return(LoginStatus.Failed);
            }

            //Check if we are in Enable-Mode or not
            if (EnableMode)
            {
                if( _Debug > 0)
                    Console.WriteLine("\nWe are in Enable-Mode.");
            } else {
                //DevicePromptString =~ s/^[\r\n]*[a-zA-Z0-9\-][>]\s*\r?\n?$/$1/;  # s/^(.*)[>]$/$1/
                savedDevicePrompt = DevicePrompt;
                DevicePrompt = "";
                bResult = Enable();
                if( _Debug > 0 )
                    Console.WriteLine("Enable-Result: " + bResult.ToString());
                if( DevicePrompt.Length == 0) {
                    if (bResult == LoginStatus.UserMode)
                    {
                        DevicePrompt = savedDevicePrompt;
                        EnableMode = false;
                        if (_Debug > 0)
                            Console.WriteLine("\nEnable failed !");
                    }
                    else
                    {
                        if (_Debug > 0)
                            Console.WriteLine("\nExit by me !");
                        return (LoginStatus.Failed);
                    }
                }
            }

            if( DeviceType.Equals("HP") ) {
                strOutput = shell.cmd("terminal length 1000", DevicePrompt);
                //strOutput = shell.cmd("screen-length disable", DevicePrompt);  // for H3C
            }
            else if (DeviceType.Equals("Cisco") || DeviceType.Equals("Cisco-IOS-XR") || (DeviceType.Length == 0))
            {
                strOutput = shell.cmd("terminal length 0", DevicePrompt);
            }else if(DeviceType.Equals("H3C")){
                strOutput = shell.cmd("screen-length disable", DevicePrompt);
            }
            strOutput = shell.cmd("terminal width 0", DevicePrompt);

            if(( DeviceType.Length == 0 ) || DeviceType.Equals("HP")) {
                Specify();
                if( DeviceType.Equals("HP") )
                    strOutput = shell.cmd("terminal length 1000", DevicePrompt);
                else if (DeviceType.Equals("H3C"))
                    strOutput = shell.cmd("screen-length disable", DevicePrompt);
            }
            return (bResult);
        }

        public void doLogout() {
            string l_DevType = DeviceType;
            string l_DevPrompt = DevicePrompt;
            string strOutput = "";
            string match = "";
  
            if(DeviceCounter > 0) {
                if(bConfigMode) {
                    //@strLines = $shell->cmd(String   => "end", Timeout  => $Timeout, Prompt   => $DevicePrompt);
                    ExitConfigMode();
                }
                if(l_DevType.Equals("HP") ) {
                    l_DevPrompt = l_DevPrompt.Replace('#', '>');
      
                    strOutput = shell.cmd("exit", l_DevPrompt);
                }
                DeviceCounter --;
    
                // Restore Status from Last Device
                DeviceHistoryData DeviceData = DeviceHistory[DeviceCounter];
                bConnected = DeviceData.bConnected;
                DevicePromptString = DeviceData.DevicePromptString;
                DevicePrompt = DeviceData.DevicePrompt;
                DevicePromptConfig = DeviceData.DevicePromptConfig;
                EnableMode = DeviceData.EnableMode;
                DeviceType = DeviceData.DeviceType;
                //Device = DeviceData.Device;
                DeviceHistory.RemoveAt(DeviceCounter);


                if(l_DevType.Equals("HP") ) {
                    match = "";
                    string[] LogoutPrompt = { "Do you want to log out [y/n]", "[y/n]" };

                    //@strLines = $shell->cmd(String   => "exit", Timeout  => $Timeout, Prompt   => 'Do you want to log out [y/n]');
                    shell.print("exit");
                    match = shell.WaitFor(LogoutPrompt, false, Timeout);
                    // if ( (match == null) || (match.Length == 0) ) {}
                    if( match.ToLower().Contains("do you want to log out [y/n]") ) {
	                    match = "";
	                    shell.print("y");
	                    match = shell.WaitForRegEx(DevicePromptString + @"|Do you want to save current configuration \[y\/n\]|\[y\/n\]");
	                    if( (match != null) && ( match.ToLower().Contains("/[y/n]") ))
	                        strOutput = shell.cmd("n", DevicePrompt);
	
	                    strOutput = shell.cmd(" ", DevicePrompt);
                    }
                }
                else if (l_DevType.Equals("H3C"))
                {
                    strOutput = shell.cmd("quit", DevicePrompt);
                } else {
                    strOutput = shell.cmd("exit", DevicePrompt);
                }
                if( _Debug > 0 )
                    Console.WriteLine("Last Device-Promts are restored: " + DevicePrompt);
                } else {
                    if (l_DevType.Equals("H3C"))
                    {
                        if (bConfigMode)
                            ExitConfigMode();
                        shell.print("quit");
                    }
                    else
                    {
                        shell.print("end");
                        //sleep 1;
                        shell.print("logout");
                        if (shell.Connected)
                            shell.print("exit");
                    }
                    if (shell.Connected)
                        shell.close();
                    if (shell != null)
                        shell = null;
    
                    if( _Debug > 0 )
                        Console.WriteLine("Telnet-Session is closed.");
            }
        }

        public int Jump2NextDeviceObj(string nextDevice, string ConOptions)
        {
            return ((int)Jump2NextDevice(nextDevice, ConOptions));
        }

        public LoginStatus Jump2NextDevice(string nextDevice, string ConnectOptions) {
            LoginStatus l_LoginResult = LoginStatus.InProgress;

            DeviceHistoryData DeviceData = new DeviceHistoryData();
            DeviceData.bConnected = bConnected;
            DeviceData.DevicePromptString = DevicePromptString;
            DeviceData.DevicePrompt = DevicePrompt;
            DeviceData.DevicePromptConfig = DevicePromptConfig;
            DeviceData.EnableMode = EnableMode;
            DeviceData.DeviceType = DeviceType;
            DeviceData.Device = nextDevice;
            DeviceHistory.Add(DeviceData);

            l_LoginResult = doLogin(nextDevice, ConnectOptions);
            if ((l_LoginResult == LoginStatus.Success) || (l_LoginResult == LoginStatus.UserMode))
            {
                //Login success
                DeviceCounter ++;
            } else {
                //Restore Status from Last Device
                bConnected = DeviceData.bConnected;
                DevicePromptString = DeviceData.DevicePromptString;
                DevicePrompt = DeviceData.DevicePrompt;
                DevicePromptConfig = DeviceData.DevicePromptConfig;
                EnableMode = DeviceData.EnableMode;
                DeviceType = DeviceData.DeviceType;
                //Device = DeviceData.Device;
                DeviceHistory.RemoveAt(DeviceHistory.Count - 1);
            }
  
            return(l_LoginResult);
        }

        public object[] GetIntoConfigModeObj()
        {
            string[] strArray = GetIntoConfigMode();
            if (strArray == null)
                return (new object[] { });

            List<object> obj = new List<object>();

            foreach (string newStr in strArray)
            {
                obj.Add((object)newStr);
            }
            return (obj.ToArray());
        }

        public object[] ExitConfigModeObj()
        {
            string[] strArray = ExitConfigMode();
            if (strArray == null)
                return (new object[] { });

            List<object> obj = new List<object>();

            foreach (string newStr in strArray)
            {
                obj.Add((object)newStr);
            }
            return (obj.ToArray());
        }

        private LoginStatus Login(string host, string ConnectOptions)
        {
            string match = "";
            string l_ConStr = "";
            string strTemp = "";
            //char LastPromptChar = '\0';
            LoginStatus bResult = LoginStatus.InProgress;

            if (bConnected == 0)
            {
                if (shell != null)  //if ((shell != null) && shell.Connected)
                    shell.close();
                shell = null;
                shell = new TerminalSessions.TelnetSSHLogin();
                if (_Debug > 0)
                {
                    shell.DebugFlag = _Debug;
                    Console.WriteLine("Debug-Info:\nConnect to " + host);
                }
                //shell.cmd_remove_mode = "1";
                if (!shell.open("", "", host, 0, "telnet", "", "", ""))
                    return (LoginStatus.Failed);
                else
                    bConnected = 1;
            }
            else
            {
                shell.buffer_empty();
                if( _Debug > 0 )
                    Console.WriteLine("Debug-Info:\nConnect to " + host);
                // shell.print("telnet " + host + " /vrf " + VRF);
                l_ConStr = host;
                if( ConnectOptions.Trim().Length != 0) 
                    l_ConStr = l_ConStr + " " + ConnectOptions;
                Console.WriteLine("Will connect to: " + l_ConStr);
                shell.print("telnet " + l_ConStr);
                bConnected ++;
            }
            // sleep 1sec.
            string[] LoginPrompt = { "User:", "Username:", "Login:", "Login Name:", "Password:", "Press any key to continue" };
            if (DeviceCounter > 0)  //Add Prompt from source-Device
                LoginPrompt = (string.Join(",", LoginPrompt) + "," + DeviceHistory[DeviceCounter-1].DevicePrompt).Split(new char[] {','});
            match = shell.WaitFor(LoginPrompt, false, Timeout);
            if( _Debug > 0 )
                Console.WriteLine("Debug-Info:\nConnected, start Login.");

            if( ( match == null ) || (match.Length == 0) ) {
                if (_Debug > 0)
                    Console.WriteLine("Connection failed, found no Loginprompt.");
                return(LoginStatus.Unknown);
            }

            strTemp = shell.ShowScreen();
            if (strTemp == null)
                strTemp = "";
            else if( strTemp.ToUpper().Contains("HEWLETT") )
                DeviceType = "HP";	// HEWLETT-PACKARD
            
            strTemp = "";
            if( match.ToLower().Contains("press any key to continue") ) {
                match = "";
                shell.buffer_empty();
                string[] LoginPrompt2 = { "User:", "Username:", "Login:", "Login Name:", "Password:" };
                shell.put(" ");
                match = shell.WaitFor(LoginPrompt2, false, Timeout);
                if ((match == null) || (match.Length == 0))
                {
                    if (_Debug > 0)
                        Console.WriteLine("Connection failed, found no Loginprompt.");
                    return (LoginStatus.Unknown);
                }
            }

            if ((match.ToLower().Contains("user:")) || (match.ToLower().Contains("username:")) || (match.ToLower().Contains("login:")) || (match.ToLower().Contains("login name:")))
            {
                //$PWDCounter = 0;
                bResult = LoginStatus.Continue;
                while ((PWDCounter < UserArray.Count) && (bResult > LoginStatus.InProgress) && (bResult < LoginStatus.Unknown))
                {
                    bResult = TryUserLogin(unknownPrompt, unknownPrompt, UserArray[PWDCounter].User, UserArray[PWDCounter].Pwd, PWDCounter);
                    PWDCounter ++;
                }
                while ((bConnected > 1) && (bResult == LoginStatus.Continue))
                {
                    //  Send Dummy-Login to get back to the CLI
                    bResult = TryUserLogin(unknownPrompt, unknownPrompt, "Exit", "Exit", PWDCounter);
                    PWDCounter++;
                }
                if (bResult > LoginStatus.Success)
                    return (bResult);
            } else if( match.ToLower().Contains("password:") ) {
                bResult = LoginStatus.Continue;
                while ((PWDCounter < PWDArray.Count) && (bResult > LoginStatus.InProgress) && (bResult < LoginStatus.Unknown))
                {
                    bResult = TryPWD(PWDArray[PWDCounter], PWDCounter);
                    if(_Debug > 0)
                        Console.WriteLine("Test-" + PWDCounter.ToString() + ":" + bResult.ToString());
                    PWDCounter ++;
                }
                while ((bConnected > 1) && (bResult == LoginStatus.Continue))
                {
                    //Check for Password:  and send Dummy-Pwd until last DevicePrompt is detected
                    bResult = TryPWD("Exit", PWDCounter);
                    PWDCounter++;
                }
                if(bResult > 0)
                    return(bResult);
            }
            if( _Debug > 0 )
                Console.WriteLine("\nDebug-Info:     Login success.");

            //Get full Prompt
            DevicePrompt = "";
            DevicePrompts devPrompts = detectPrompt(unknownPrompt);
            if (devPrompts == null)
            {
                //Found no Prompt Exit...
                return (LoginStatus.Unknown);
            }
            if (devPrompts.LastPromptChar == '#')
            {
                EnableMode = true;
            }
            DevicePrompt = devPrompts.DevicePrompt;
            DevicePromptConfig = devPrompts.DevicePromptConfig;
            DevicePromptString = devPrompts.DevicePromptString;
  
            return(LoginStatus.Success);
        }

        private LoginStatus TryUserLogin(string strPrompt, string matchPrompt, string UserName, string Pwd, int LoginCounter) {  
            LoginStatus pwdResult = LoginStatus.InProgress;
            string match = "";
  
            shell.buffer_empty();
            if( _Debug > 0 ) {
                Console.WriteLine("\nDebug-Info:\n");
                Console.WriteLine("Login-Try-" + LoginCounter.ToString() + ": " + UserName);
            }

            shell.print(UserName);
            match = shell.WaitForString("Password");
            if( (match == null) || (match.Length == 0) ) {
                if( _Debug > 0 )
                    Console.WriteLine(shell.ShowScreen() + "---");
                return(LoginStatus.Failed);
            }
  
            shell.print(Pwd);
            if (bConnected > 1)
                match = shell.WaitForRegEx(strPrompt + "|(User:)|(Username:)|(Login:)|(Login Name:)|(" + DeviceHistory[DeviceCounter].DevicePrompt + ")");
            else
                match = shell.WaitForRegEx(strPrompt + "|(User:)|(Username:)|(Login:)|(Login Name:)");
            if( (match == null) || (match.Length == 0) ) {
                if ((shell != null) && !shell.Connected)
                    return (LoginStatus.InProgress);    //We are Disconnected from Remote-Device
                else
                {
                    if((_Debug > 0) && (shell != null) && shell.Connected)
                        Console.WriteLine(shell.ShowScreen() + "---");
                    return (LoginStatus.Failed);
                }
            }
            if ((bConnected > 1) && HelperClass.RegExCompare(match, DeviceHistory[DeviceCounter].DevicePrompt, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                //We are back on the source-Device
                pwdResult = LoginStatus.InProgress;
            } else if (HelperClass.RegExCompare(match, matchPrompt, RegexOptions.Multiline)) {
                if( _Debug > 0)
                    Console.WriteLine("\nFound Match-" + LoginCounter.ToString() + ": " + match);
                pwdResult = LoginStatus.Success;
            }
            else if ((match.ToLower().Contains("user:")) || (match.ToLower().Contains("username:")) || (match.ToLower().Contains("login:")) || (match.ToLower().Contains("login name:")))
            {
                pwdResult = LoginStatus.Continue;   // 2
            } else if( match.Contains(">") ) {
                pwdResult = LoginStatus.UserMode;
            } else {
                pwdResult = LoginStatus.Unknown;
            }
  
            return(pwdResult);
        }

        private LoginStatus TryPWD(string Pwd, int LoginCounter)
        {
            LoginStatus pwdResult = LoginStatus.InProgress;
            string match = "";
            
            shell.buffer_empty();
            if( _Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("Login-Try-" + LoginCounter.ToString() + ": " + Pwd);
            }
            shell.print(Pwd);
            //  sleep 1;
            if (bConnected > 1)
                match = shell.WaitForRegEx("Password:|" + unknownPrompt + "|" + DeviceHistory[DeviceCounter].DevicePrompt);
            else
                match = shell.WaitForRegEx("Password:|" + unknownPrompt);
            //ToDo:
            //	instead of: }else if( $match =~ /[>]\s?$|[#]\s?$/m )  use  $unknownPrompt
            if( (match == null) || (match.Length == 0) ) {
                if( _Debug > 0 )
                    Console.WriteLine("\nNo Prompt Found.");
                if((shell != null) && !shell.Connected)
                    pwdResult = LoginStatus.InProgress; // We are disconnected from Remote-Device
                else
                    pwdResult = LoginStatus.Failed;
            }
            else if ((bConnected > 1) && HelperClass.RegExCompare(match, DeviceHistory[DeviceCounter].DevicePrompt, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                //We are back on the source-Device
                pwdResult = LoginStatus.InProgress;
            } else if (HelperClass.RegExCompare(match, unknownPrompt, RegexOptions.Multiline))  /* @"[>]\s?$|[#]\s?$" */
            {
                if( _Debug > 0 )
                    Console.WriteLine("\nFound Match-" + LoginCounter.ToString() + ": " + match);
                pwdResult = LoginStatus.Success;
            } else if( match.ToLower().Contains("password:") ) {
                pwdResult = LoginStatus.Continue;
            } else {
                pwdResult = LoginStatus.Unknown;
            }

            return(pwdResult);
        }

        private LoginStatus TryenPWD() {
            LoginStatus pwdResult = LoginStatus.InProgress;
            string match = "";
  
            if( _Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("enLogin-Try-" + PWDCounter.ToString() + ": " + enPWDArray[PWDCounter]);
            }
            shell.buffer_empty();
            shell.print(enPWDArray[PWDCounter]);
            //sleep 1;
            match = shell.WaitForRegEx("Password:|" + DevicePromptString + @"[>#]");
            if ((match == null) || (match.Length == 0) ) {
                if( _Debug > 0 )
                    Console.WriteLine(shell.ShowScreen() + "---");
                return (LoginStatus.Failed); //LoginStatus.InProgress
            }

            if (HelperClass.RegExCompare(match, DevicePromptString + @"[#]\s*\r?\n?$", RegexOptions.Multiline))
            {
                if( _Debug > 0 ) 
                    Console.WriteLine("Found Match-" + PWDCounter.ToString() + ": " + match);
                pwdResult = LoginStatus.Success;
            }
            else if (HelperClass.RegExCompare(match, DevicePromptString + @"[>]\s*\r?\n?$", RegexOptions.Multiline))
            {
                shell.buffer_empty();
                if ((PWDCounter + 1) < enPWDArray.Count)
                {
                    shell.print("\nenable");
                    pwdResult = LoginStatus.Continue;
                    match = shell.WaitForRegEx(".*Password:.*");
                    if ((match == null) || (match.Length == 0))
                    {
                        if (_Debug > 0)
                            Console.WriteLine(shell.ShowScreen() + "---");
                        return (LoginStatus.Failed);     //LoginStatus.InProgress
                    }
                }
                else
                    return (LoginStatus.UserMode);
            } else if( match.ToLower().Contains("password:") ) {
                pwdResult = LoginStatus.Continue;
            }
            return(pwdResult);
        }

        private LoginStatus Enable()
        {
            LoginStatus pwdResult = LoginStatus.InProgress;
 
            shell.buffer_empty();
            shell.print("\nenable");
            string match = "";
            string[] LoginPrompt = { "User:", "Username:", "Login:", "Password:" };
            //sleep 1;
            match = shell.WaitFor(LoginPrompt, false, Timeout);
            if (match == null)
            {
                //No Enable-Prompt found !
                return (LoginStatus.UserMode);
            }
            else if ((match.ToLower().Contains("user:")) || (match.ToLower().Contains("username:")) || (match.ToLower().Contains("login:")))
            {
                PWDCounter = 0;
                pwdResult = LoginStatus.Continue;
                while ((PWDCounter < UserArray.Count) && (pwdResult > LoginStatus.InProgress) && (pwdResult < LoginStatus.Unknown))
                {
                    pwdResult = TryUserLogin(DevicePromptString + "(>|#)", DevicePromptString + "#", UserArray[PWDCounter].User, UserArray[PWDCounter].Pwd, PWDCounter);
                    PWDCounter++;
                    if(pwdResult == LoginStatus.UserMode) {
                        shell.buffer_empty();
                        if(PWDCounter < UserArray.Count)
                            shell.print("\nenable");
                    }
                }
                if (pwdResult == LoginStatus.UserMode)
                    return (pwdResult);
                else if (pwdResult > LoginStatus.Success)
                    return (LoginStatus.InProgress);
            } else if( match.ToLower().Contains("password:") ) {
                PWDCounter = 0;
                pwdResult = LoginStatus.Continue;
                while ((PWDCounter < enPWDArray.Count) && (pwdResult > LoginStatus.InProgress) && (pwdResult < LoginStatus.Unknown))
                {
                    pwdResult = TryenPWD();
                    PWDCounter ++;
                }
                if (pwdResult == LoginStatus.UserMode)
                    return (pwdResult);
                else if(pwdResult > LoginStatus.Success) 
                    return(LoginStatus.InProgress);
            }
            //
            //Get full Prompt
            DevicePrompts devPrompts = detectPrompt(DevicePromptString + "#");
            if (devPrompts != null)
            {
                DevicePrompt = devPrompts.DevicePrompt;
                DevicePromptConfig = devPrompts.DevicePromptConfig;
                DevicePromptString = devPrompts.DevicePromptString;
            }
            /*
            string strTemp = "";
            char LastPromptChar = '\0';
            string strOutput = shell.cmd("", DevicePromptString + "#");
            string[] strLines = strOutput.Split(new char[] { '\n' } );
            foreach(string strLine in strLines ) {
                strTemp = strLine.Replace("\n", "");
                strTemp = strTemp.Replace("\r", "");
                strTemp = strTemp.Trim(); // strTemp.Replace(" ", "");
                if(strTemp.Length > 0) {
                    LastPromptChar = strTemp.Substring(strTemp.Length-1, 1).ToCharArray()[0];
                    strTemp = strTemp.Substring(0, strTemp.Length-1);  //Remove Last Char
                    DevicePrompt = HelperClass.quotemeta(strTemp);
                }
            }
            DevicePromptString = DevicePrompt;
            DevicePromptConfig = DevicePrompt;
            if( DevicePromptConfig.Length > 10) {
                DevicePromptConfig = DevicePromptConfig.Substring(0, 10);
            }
            if (DeviceType.Equals("HP"))
                DevicePromptConfig = DevicePromptConfig + @".*\(.*\)" + LastPromptChar;
            else
            {
                //DevicePromptConfig = DevicePromptConfig + @".*\(config.*\)" + LastPromptChar;
                DevicePromptConfig = DevicePromptConfig + @".*\(c.*\)" + LastPromptChar;
            }
            DevicePromptConfig = DevicePromptConfig + "[ \\r\\n]*$";
            DevicePrompt = DevicePrompt + LastPromptChar + "[ \\r\\n]*$";
            if( _Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("new Prompt: " + DevicePrompt);
                Console.WriteLine("Config-Prompt: " + DevicePromptConfig);
            }
            */
            return (LoginStatus.Success);
        }

        public object[] SendCmdObj(string l_cmd)
        {
            string[] strArray = SendCmd(l_cmd);
            if (strArray == null)
                return (new object[] { });

            List<object> obj = new List<object>();

            foreach (string newStr in strArray)
            {
                obj.Add((object)newStr);
            }
            return (obj.ToArray());
        }

        public object[] GetShowVersionObj()
        {
            string[] strArray = GetShowVersion();
            if (strArray == null)
                return (new object[] { });

            List<object> obj = new List<object>();

            foreach (string newStr in strArray)
            {
                obj.Add((object)newStr);
            }
            return (obj.ToArray());
        }

        public object[] GetRunningConfigObj()
        {
            string[] strArray = GetRunningConfig();
            if (strArray == null)
                return (new object[] { });

            List<object> obj = new List<object>();

            foreach (string newStr in strArray)
            {
                obj.Add((object)newStr);
            }
            return (obj.ToArray());
        }

        public override bool detectNewPrompt()
        {
            DevicePrompts devPrompts = detectPrompt(unknownPrompt);
            if (devPrompts == null)
            {
                return(false);
            }

            if (devPrompts.LastPromptChar == '#')
            {
                EnableMode = true;
            }
            DevicePrompt = devPrompts.DevicePrompt;
            DevicePromptConfig = devPrompts.DevicePromptConfig;
            DevicePromptString = devPrompts.DevicePromptString;

            return (true);
        }
    }
}
