using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleMain
{
    class UserPwd {
        public string User;
        public string Pwd;
    }
    class DeviceHistoryData {
        public int bConnected;
        public string DevicePromptString;
        public string DevicePrompt;
        public string DevicePromptConfig;
        public bool EnableMode;
        public string DeviceType;
        public string Device;
    }

    class TelnetDevice
    {
        public enum LoginStatus {Success, InProgress, Continue, UserMode, Unknown, Failed};

        List<UserPwd> UserArray = new List<UserPwd>();
        List<string> PWDArray = new List<string>();
        List<string> enPWDArray = new List<string>();
        List<DeviceHistoryData> DeviceHistory = new List<DeviceHistoryData>();

        TerminalSessions.TelnetSSHLogin shell = null;
        public int Debug = 0;
        string unknownPrompt = @"([a-zA-Z0-9\-\.@~]+\s?){1}[\>#]\s*\r?\n?$"; // /[>#]$/m  # “/[s().-]*[\$#>]s?(?:\enable\))?\s**$/”
        int bConnected = 0, Timeout = 15, PWDCounter = 0, DeviceCounter = 0;
        bool EnableMode = false, bConfigMode = false;
        string DevicePromptString = "", DevicePrompt = "", DevicePromptConfig = "", DeviceType = "";

        public bool SetCliLogins(string[] Users, string[] Pwds) {
  
            if(Users.Length != Pwds.Length)
                return(false);

            for(int i=0; i< Users.Length; i++) {
                UserPwd up = new UserPwd();
                up.User = Users[i];
                up.Pwd = Pwds[i];
                UserArray.Add(up);
            }
            return(true);
        }

        public void SetCliPwds(string[] l_CliPwds) {
            foreach(string pwd in l_CliPwds)
                PWDArray.Add(pwd);

            return;
        }

        public void SetCliEnablePwds(string[] l_CliPwds)
        {
            foreach (string pwd in l_CliPwds)
                enPWDArray.Add(pwd);

            return;
        }

        public LoginStatus doLogin(string Device, string ConnectOptions)
        {
            string strOutput = "";
            PWDCounter = 0;
            DevicePromptString = "";
            DevicePrompt = "";
            DevicePromptConfig = "";
            EnableMode = false;
            bConfigMode = false;
            DeviceType = "";
            LoginStatus bResult = LoginStatus.InProgress;

            while ((bResult == LoginStatus.InProgress) && (PWDCounter < UserArray.Count))
            {
                bResult = Login(Device, ConnectOptions);
    
                if((bConnected > 0) && (bResult == LoginStatus.InProgress))
                    bConnected --;
            }
            if( Debug > 0 )
                Console.WriteLine("Login-Result: " + bResult);

            if( DevicePrompt.Length == 0) {
                if( Debug > 0 )
                    Console.WriteLine("\nExit by me !");
                if(bConnected > 0)
                    bConnected --;
                return(LoginStatus.Failed);
            }

            //Check if we are in Enable-Mode or not
            if (EnableMode)
            {
                if( Debug > 0)
                    Console.WriteLine("\nWe are in Enable-Mode.");
            } else {
                //DevicePromptString =~ s/^[\r\n]*[a-zA-Z0-9\-][>]\s*\r?\n?$/$1/;  # s/^(.*)[>]$/$1/
                DevicePrompt = "";
                bResult = Enable();
                if( Debug > 0 )
                    Console.WriteLine("Enable-Result: " + bResult.ToString());
                if( DevicePrompt.Length == 0) {
                  if( Debug > 0)
                      Console.WriteLine("\nExit by me !");
                  return(LoginStatus.UserMode);
                }
            }

            if( DeviceType.Equals("HP") ) {
                strOutput = shell.cmd("terminal length 1000", DevicePrompt);
            } else if( DeviceType.Equals("Cisco") || (DeviceType.Length == 0) ) {
                strOutput = shell.cmd("terminal length 0", DevicePrompt);
            }
            strOutput = shell.cmd("terminal width 0", DevicePrompt);

            if( DeviceType.Length == 0 ) {
                Specify();
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
                } else {
                    strOutput = shell.cmd("exit", DevicePrompt);
                }
                if( Debug > 0 )
                    Console.WriteLine("Last Device-Promts are restored: " + DevicePrompt);
                } else {
                    shell.print("end");
                    //sleep 1;
                    shell.print("exit");
                    if(shell.Connected)
                        shell.close();
    
                    if( Debug > 0 )
                        Console.WriteLine("Telnet-Session is closed.");
            }
        }

        string Specify() {  
            string[] l_Lines = GetShowVersion();
  
            foreach(string strLine in l_Lines) {
                if(HelperClass.RegExCompare(strLine, "hp|procurve", RegexOptions.IgnoreCase) ) {
                    DeviceType = "HP";
                } else if(strLine.ToLower().Contains("cisco") ) {
                    DeviceType = "Cisco";
                }
            }
  
            if(DeviceType.Length == 0) {
                l_Lines = GetRunningConfig();
                foreach(string strLine in l_Lines) {
                    if(HelperClass.RegExCompare(strLine, @"\s*;.* Configuration Editor; Created on release", RegexOptions.IgnoreCase) ) {
                        DeviceType = "HP";
                        continue;
                    }
                }
            }
  
            if( Debug > 0 ) {
                Console.WriteLine("Device-Type: " + DeviceType);
            }
  
            return(DeviceType);
        }

        public string GetDeviceType
        {
            get { return (DeviceType); }
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
  
            if( doLogin(nextDevice, ConnectOptions) == LoginStatus.Success ) {
                //Login success
                DeviceCounter ++;
                l_LoginResult = LoginStatus.Success;
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

        public string[] GetIntoConfigMode() {
            string strOutput = "";
            string[] strLines = { "" };
  
            //Change to Config-Mode of Router
            //
            strOutput = shell.cmd("configure terminal", DevicePromptConfig);

            bConfigMode = true;
            if( Debug > 0 ) {
                Console.WriteLine("Entered-Configmode:");
                if(strOutput != null)
                    Console.WriteLine(strOutput);
            }
            if( strOutput != null )
                strLines = strOutput.Split(new char[] { '\n' });
    
            return(strLines);
        }

        public string[] ExitConfigMode() {
            string strOutput = "";
            string[] strLines = { "" };
            
            //Exit from Config-Mode
            //
            strOutput = shell.cmd("end", DevicePrompt);

            bConfigMode = false;
            if( Debug > 0 ) {
                Console.WriteLine("Exited-Configmode:");
                if( strOutput != null)
                    Console.WriteLine(strOutput);
            }
  
            if( strOutput != null )
                strLines = strOutput.Split(new char[] { '\n' } );

            return(strLines);
        }

        private LoginStatus Login(string host, string ConnectOptions)
        {
            string match = "", strOutput = "";
            string l_ConStr = "";
            string strTemp = "";
            char LastPromptChar = '\0';
            LoginStatus bResult = LoginStatus.InProgress;

            if (bConnected == 0)
            {
                if (shell != null)  //if ((shell != null) && shell.Connected)
                    shell.close();
                shell = null;
                shell = new TerminalSessions.TelnetSSHLogin();
                if (Debug > 0)
                {
                    shell.DebugFlag = Debug;
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
                if( Debug > 0 )
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
            string[] LoginPrompt = { "Username:", "Login:", "Login Name:", "Password:", "Press any key to continue" };
            if (DeviceCounter > 0)  //Add Prompt from source-Device
                LoginPrompt = (string.Join(",", LoginPrompt) + "," + DeviceHistory[DeviceCounter-1].DevicePrompt).Split(new char[] {','});
            match = shell.WaitFor(LoginPrompt, false, Timeout);
            if( Debug > 0 )
                Console.WriteLine("Debug-Info:\nConnected, start Login.");

            if( ( match == null ) || (match.Length == 0) ) {
                if (Debug > 0)
                    Console.WriteLine("Connection failed, found no Loginprompt.");
                return(LoginStatus.Unknown);
            }

            strTemp = shell.ShowScreen();
            if( strTemp.ToUpper().Contains("HEWLETT") )
                DeviceType = "HP";	// HEWLETT-PACKARD
            
            strTemp = "";
            if( match.ToLower().Contains("press any key to continue") ) {
                match = "";
                shell.buffer_empty();
                string[] LoginPrompt2 = { "Username:", "Login:", "Login Name:", "Password:" };
                shell.put(" ");
                match = shell.WaitFor(LoginPrompt2, false, Timeout);
                if ((match == null) || (match.Length == 0))
                {
                    if (Debug > 0)
                        Console.WriteLine("Connection failed, found no Loginprompt.");
                    return (LoginStatus.Unknown);
                }
            }

            if ((match.ToLower().Contains("username:")) || (match.ToLower().Contains("login:")) || (match.ToLower().Contains("login name:")))
            {
                //$PWDCounter = 0;
                bResult = LoginStatus.Continue;
                while ((PWDCounter <= UserArray.Count) && (bResult > LoginStatus.InProgress) && (bResult < LoginStatus.Unknown))
                {
                    bResult = TryUserLogin(unknownPrompt, unknownPrompt);
                    PWDCounter ++;
                }
                if (bResult > LoginStatus.Success)
                    return (bResult);
            } else if( match.ToLower().Contains("password:") ) {
                bResult = LoginStatus.Continue;
                while ((PWDCounter <= PWDArray.Count) && (bResult > LoginStatus.InProgress) && (bResult < LoginStatus.Unknown))
                {
                    bResult = TryPWD();
                    if(Debug > 0)
                        Console.WriteLine("Test-" + PWDCounter.ToString() + ":" + bResult.ToString());
                    PWDCounter ++;
                }
                if(bResult > 0)
                    return(bResult);
            }
            if( Debug > 0 )
                Console.WriteLine("\nDebug-Info:     Login success.");

            //Get full Prompt
            DevicePrompt = "";
            strOutput = shell.cmd("", unknownPrompt);
            if( (strOutput == null) || (strOutput.Length == 0) ) {
                //Found no Prompt Exit...
                return(LoginStatus.Unknown);
            }
            string[] strLines = strOutput.Split(new char[] { '\n' });
            foreach(string strLine in strLines) {
                strTemp = strLine.Replace("\n", "");
                strTemp = strTemp.Replace("\r", "");
                strTemp = strTemp.Trim(); // strTemp.Replace(" ", "");
                if(strTemp.Length > 0) {
                    LastPromptChar = strTemp.ToCharArray(strTemp.Length-1, 1)[0];
                    strTemp = strTemp.Substring(0, strTemp.Length-1);  // Remove Last Char
                    DevicePrompt = HelperClass.quotemeta(strTemp);
                }
            }
            DevicePromptString = DevicePrompt;
            if( LastPromptChar == '#') {
                EnableMode = true;
        
                DevicePromptConfig = DevicePrompt.Substring(0, 10);
                if( DevicePromptConfig.EndsWith("\\") )
                    DevicePromptConfig = DevicePromptConfig.Substring(0, DevicePromptConfig.Length-1);

                DevicePromptConfig = DevicePromptConfig + LastPromptChar;
                //DevicePromptConfig =~ s/(.*)(.)$/$1.*\\(config.*\\)$2/;	# s/(.*)([^\s])\s?$/$1.*\\(config.*\\)$2/;
                if( DeviceType.Equals("HP") ) {
                    DevicePromptConfig = DevicePromptConfig.Substring(0, DevicePromptConfig.Length-1) + ".*\\(.*\\)" + "#";
                } else {
                    DevicePromptConfig = DevicePromptConfig.Trim();
                    DevicePromptConfig = DevicePromptConfig.Substring(0, DevicePromptConfig.Length-1) + ".*\\(config.*\\)" + "#";
                }
                DevicePromptConfig = DevicePromptConfig + "[ \\r\\n]*$";
            } else
                DevicePromptConfig = "";
  
            DevicePrompt = DevicePrompt + LastPromptChar + "[ \\r\\n]*$";
            if( Debug > 0 ) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("new Prompt: " + DevicePrompt);
                Console.WriteLine("Config-Prompt: " + DevicePromptConfig);
            }
  
            return(LoginStatus.Success);
        }

        private LoginStatus TryUserLogin(string strPrompt, string matchPrompt) {  
            LoginStatus pwdResult = LoginStatus.InProgress;
            string match = "";
  
            shell.buffer_empty();
            if( Debug > 0 ) {
                Console.WriteLine("\nDebug-Info:\n");
                Console.WriteLine("Login-Try-" + PWDCounter.ToString() + ": " + UserArray[PWDCounter].User);
            }
  
            shell.print(UserArray[PWDCounter].User);
            match = shell.WaitForString("Password");
            if( (match == null) || (match.Length == 0) ) {
                if( Debug > 0 )
                    Console.WriteLine(shell.ShowScreen() + "---");
                return(LoginStatus.Failed);
            }
  
            shell.print(UserArray[PWDCounter].Pwd);
            if (bConnected > 1)
                match = shell.WaitForRegEx(strPrompt + "|(Username:)|(Login:)|(Login Name:)|(" + DeviceHistory[DeviceCounter].DevicePrompt + ")");
            else
                match = shell.WaitForRegEx(strPrompt + "|(Username:)|(Login:)|(Login Name:)");
            if( (match == null) || (match.Length == 0) ) {
                if ((shell != null) && !shell.Connected)
                    return (LoginStatus.InProgress);    //We are Disconnected from Remote-Device
                else
                {
                    if((Debug > 0) && (shell != null) && shell.Connected)
                        Console.WriteLine(shell.ShowScreen() + "---");
                    return (LoginStatus.Failed);
                }
            }
            if ((bConnected > 1) && HelperClass.RegExCompare(match, DeviceHistory[DeviceCounter].DevicePrompt, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                //We are back on the source-Device
                pwdResult = LoginStatus.InProgress;
            } else if (HelperClass.RegExCompare(match, matchPrompt, RegexOptions.Multiline)) {
                if( Debug > 0)
                    Console.WriteLine("\nFound Match-" + PWDCounter.ToString() + ": " + match);
                pwdResult = LoginStatus.Success;
            }
            else if ((match.ToLower().Contains("username:")) || (match.ToLower().Contains("login:")) || (match.ToLower().Contains("login name:")))
            {
                pwdResult = LoginStatus.Continue;   // 2
            } else if( match.Contains(">") ) {
                pwdResult = LoginStatus.UserMode;
            } else {
                pwdResult = LoginStatus.Unknown;
            }
  
            return(pwdResult);
        }

        private LoginStatus TryPWD() {
            LoginStatus pwdResult = LoginStatus.InProgress;
            string match = "";
            
            shell.buffer_empty();
            if( Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("Login-Try-" + PWDCounter.ToString() + ": " + PWDArray[PWDCounter]);
            }
            shell.print(PWDArray[PWDCounter]);
            //  sleep 1;
            if (bConnected > 1)
                match = shell.WaitForRegEx("Password:|" + unknownPrompt + "|" + DeviceHistory[DeviceCounter].DevicePrompt);
            else
                match = shell.WaitForRegEx("Password:|" + unknownPrompt);
            //ToDo:
            //	instead of: }else if( $match =~ /[>]\s?$|[#]\s?$/m )  use  $unknownPrompt
            if( (match == null) || (match.Length == 0) ) {
                if( Debug > 0 )
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
            } else if (HelperClass.RegExCompare(match, @"[>]\s?$|[#]\s?$", RegexOptions.Multiline))
            {
                if( Debug > 0 )
                    Console.WriteLine("\nFound Match-" + PWDCounter.ToString() + ": " + match);
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
  
            if( Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("enLogin-Try-" + PWDCounter.ToString() + ": " + enPWDArray[PWDCounter]);
            }
            shell.buffer_empty();
            shell.print(enPWDArray[PWDCounter]);
            //sleep 1;
            match = shell.WaitForRegEx("Password:|" + DevicePromptString + @"[\>#]");
            if ((match == null) || (match.Length == 0) ) {
                if( Debug > 0 )
                    Console.WriteLine(shell.ShowScreen() + "---");
                return (LoginStatus.Failed); //LoginStatus.InProgress
            }

            if (HelperClass.RegExCompare(match, DevicePromptString + @"[#]\s*\r?\n?$", RegexOptions.Multiline))
            {
                if( Debug > 0 ) 
                    Console.WriteLine("Found Match-" + PWDCounter.ToString() + ": " + match);
                pwdResult = LoginStatus.Success;
            }
            else if (HelperClass.RegExCompare(match, DevicePromptString + @"[>]\s*\r?\n?$", RegexOptions.Multiline))
            {
                shell.buffer_empty();
                shell.print("\nenable");
                pwdResult = LoginStatus.Continue;
                match = shell.WaitForRegEx(".*Password:.*");
                if ((match == null) || (match.Length == 0) ) {
                    if( Debug > 0)
                        Console.WriteLine(shell.ShowScreen() + "---");
                    return (LoginStatus.Failed);     //LoginStatus.InProgress
                }
            } else if( match.ToLower().Contains("password:") ) {
                pwdResult = LoginStatus.Continue;
            }
            return(pwdResult);
        }

        private LoginStatus Enable()
        {
            LoginStatus pwdResult = LoginStatus.InProgress;
            string strTemp = "";
 
            shell.buffer_empty();
            shell.print("\nenable");
            string match = "";
            string[] LoginPrompt = {"Username:", "Login:", "Password:"};
            //sleep 1;
            match = shell.WaitFor(LoginPrompt, false, Timeout);
            if( ( match.ToLower().Contains("username:") ) || ( match.ToLower().Contains("login:") ) ) {
                PWDCounter = 0;
                pwdResult = LoginStatus.Continue;
                while ((PWDCounter <= UserArray.Count) && (pwdResult > LoginStatus.InProgress) && (pwdResult < LoginStatus.Unknown))
                {
                    pwdResult = TryUserLogin(DevicePromptString + "(>|#)", DevicePromptString + "#" );
	                if(pwdResult == LoginStatus.UserMode) {
                        shell.buffer_empty();
                        shell.print("\nenable");
                    }
                    PWDCounter ++;
                }
                if(pwdResult > LoginStatus.Success)
                    return(LoginStatus.InProgress);
            } else if( match.ToLower().Contains("password:") ) {
                PWDCounter = 0;
                pwdResult = LoginStatus.Continue;
                while ((PWDCounter <= enPWDArray.Count) && (pwdResult > LoginStatus.InProgress) && (pwdResult < LoginStatus.Unknown))
                {
                    pwdResult = TryenPWD();
                    PWDCounter ++;
                }
                if(pwdResult > LoginStatus.Success) 
                    return(LoginStatus.InProgress);
            }
            //
            //Get full Prompt
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
            if( DeviceType.Equals("HP") )
                DevicePromptConfig = DevicePromptConfig + @".*\(.*\)" + LastPromptChar;
            else
                DevicePromptConfig = DevicePromptConfig + @".*\(config.*\)" + LastPromptChar;
            DevicePromptConfig = DevicePromptConfig + "[ \\r\\n]*$";
            DevicePrompt = DevicePrompt + LastPromptChar + "[ \\r\\n]*$";
            if( Debug > 0) {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("new Prompt: " + DevicePrompt);
                Console.WriteLine("Config-Prompt: " + DevicePromptConfig);
            }

            return(LoginStatus.Success);
        }

        public string[] SendCmd(string l_cmd) {
            string l_ExpectedPrompt = DevicePrompt;
            string strOutput = "";
            string[] strLines = { "" };
            Regex re = null;
  
            if( bConfigMode )
                l_ExpectedPrompt = DevicePromptConfig;
  
            //
            //Write to Device-Terminal
            //
            strOutput = shell.cmd(l_cmd, l_ExpectedPrompt);

            if( Debug > 0 ) {
                Console.WriteLine("Result of Cmd: " + l_cmd);
                if( strOutput != null )
                  Console.WriteLine(strOutput);
            }
            if( strOutput != null ) {
                re = new Regex(@"[\n\r]*" + l_ExpectedPrompt);
                strOutput = re.Replace(strOutput, "");      // Remove Prompt from Output
                strLines = strOutput.Split(new char[] { '\n' });
            }
  
            return(strLines);
        }

        public string[] GetShowVersion() {
            string[] l_strLines = { "" };
            //
            //Read Show-Version from Router
            //
            string strOutput = shell.cmd("show version", DevicePrompt);

            if( Debug > 0) {
                Console.WriteLine("Show-Version:");
                if( strOutput != null )
                    Console.WriteLine(strOutput);
            }
  
            if( strOutput != null )
                l_strLines = strOutput.Split(new char[] { '\n' });
  
            return(l_strLines);
        }

        public string[] GetRunningConfig() {
            string[] l_strLines = { "" };

            //
            //Read Running-Config from Router
            //
            // shell.cmd_remove_mode(1);
            string strOutput = shell.cmd("show running-config", DevicePrompt);

            if( Debug > 0) {
                Console.WriteLine("Running-Config:");
                if( strOutput != null )
                    Console.WriteLine(strOutput);
            }
  
            if( strOutput != null )
                l_strLines = strOutput.Split(new char[] { '\n' });
  
            return(l_strLines);
        }
    }
}
