using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Core;

namespace TelnetSession
{
    public interface ITermDevice
    {
        TerminalSessions.TelnetSSHLogin Shell
        {
            get;
        }
        string GetDeviceType
        {
            get;
        }
        int Debug
        {
            get;
            set;
        }
        string Prompt
        {
            get;
            set;
        }
        void DeleteCliLogins();
        void DeleteCliPwds();
        void DeleteCliEnablePwds();
        bool SetCliLogins(string[] Users, string[] Pwds);
        void SetCliPwds(string[] l_CliPwds);
        void SetCliEnablePwds(string[] l_CliPwds);
        bool Init(string ConfigFile);
        LoginStatus doLogin(string Device, string ConnectOptions, string DevType);
        LoginStatus doLogin(string Device, string ConnectOptions);
        void doLogout();
        LoginStatus Jump2NextDevice(string nextDevice, string ConnectOptions);
        string[] GetIntoConfigMode();
        string[] ExitConfigMode();
        string[] SendCmd(string l_cmd);
        bool detectNewPrompt();
        string[] GetShowVersion();
        string[] GetRunningConfig();
        void DeleteScreen();
        string ShowScreen();
        void print(string strCmd);
        void put(string strCmd);
        string WaitFor(object[] searchFor, bool CaseSensitive, int TimeOut);
        string WaitForString(string searchFor);
        string WaitForRegEx(string regEx);
    }

    public class UserPwd
    {
        public string User;
        public string Pwd;
    }
    public class DeviceHistoryData
    {
        public int bConnected;
        public string DevicePromptString;
        public string DevicePrompt;
        public string DevicePromptConfig;
        public bool EnableMode;
        public string DeviceType;
        public string Device;
    }
    public class DevicePrompts
    {
        public string DevicePrompt;
        public string DevicePromptConfig;
        public string DevicePromptString;
        public char LastPromptChar;
    }

    public enum LoginStatus { Success, InProgress, Continue, UserMode, Unknown, Failed };

    public class BaseDevice
    {
        protected List<UserPwd> UserArray = new List<UserPwd>();
        protected List<string> PWDArray = new List<string>();
        protected List<string> enPWDArray = new List<string>();
        protected List<DeviceHistoryData> DeviceHistory = new List<DeviceHistoryData>();

        protected TerminalSessions.TelnetSSHLogin shell = null;
        public int _Debug = 0;
        protected bool bConfigMode = false;
        protected string DevicePrompt = "", DevicePromptConfig = "", DeviceType = "";
        protected string unknownPrompt = @"([(\<\[]?[a-zA-Z0-9\-\.@~]+\s?[a-zA-Z0-9\-\.@~()\/:_]+\s?)[\]\>#]\s*\r?\n?$";
        /*
         ASR9000 and ASR12000: RP/0/RSP0/CPU0:PE-W-AR9200#, RP/0/RSP0/CPU0:PE-W-AR9200(config)#, RP/0/RSP0/CPU0:PE-W-AR9200(admin)#
         *        Uncommitted changes found, commit them before exiting(yes/no/cancel)? [cancel]:
         H3C = HP A-Series: <PostNitra-cs01>, 
         *                  system-view:   [PostNitra-cs01], [PostNitra-cs01-Vlan-interface29], ...  quit
         *                  display current-configuration
         Cisco and HP Procurve:
         *          PostNitra-r1>,
         *          enable: PostNitra-r1#
         *          configure terminal: PostNitra-r1(config)#, PostNitra-r1(config-if)#, ...
         Cisco WLC:
         *          (PostWlanc03) >, (PostWlanc03) config>
         *          show run-config   => Press Enter to continue...
         *          logout  => The system has unsaved changes.\nWould you like to save them now? (y/N)
        */

        public TerminalSessions.TelnetSSHLogin Shell
        {
            get { return (shell); }
        }

        public void DeleteCliLogins()
        {
            UserArray.Clear();
            return;
        }
        public void DeleteCliPwds()
        {
            PWDArray.Clear();
            return;
        }
        public void DeleteCliEnablePwds()
        {
            enPWDArray.Clear();
            return;
        }

        public bool SetCliLogins(string[] Users, string[] Pwds)
        {

            if (Users.Length != Pwds.Length)
                return (false);

            for (int i = 0; i < Users.Length; i++)
            {
                UserPwd up = new UserPwd();
                up.User = Users[i];
                up.Pwd = Pwds[i];
                UserArray.Add(up);
            }
            return (true);
        }

        public void SetCliPwds(string[] l_CliPwds)
        {
            foreach (string pwd in l_CliPwds)
                PWDArray.Add(pwd);

            return;
        }

        public void SetCliEnablePwds(string[] l_CliPwds)
        {
            foreach (string pwd in l_CliPwds)
                enPWDArray.Add(pwd);

            return;
        }

        public bool Init(string ConfigFile)
        {

            Hashtable CONFIG = HelperClass.ReadConf(ConfigFile);
            if (CONFIG == null)
                return (false);

            Regex re = null;

            re = new Regex(@"(?<!\\),");
            if (CONFIG.ContainsKey("cliUser") && (CONFIG["cliUser"].ToString().Length > 0) && CONFIG.ContainsKey("cliUserPwd") && (CONFIG["cliUserPwd"].ToString().Length > 0))
            {
                string[] cliUsers = re.Split(CONFIG["cliUser"].ToString());
                string[] cliUsersPwd = re.Split(CONFIG["cliUserPwd"].ToString());

                SetCliLogins(cliUsers, cliUsersPwd);
            }

            if (CONFIG.ContainsKey("cliPwd") && (CONFIG["cliPwd"].ToString().Length > 0))
            {
                string[] cliPwds = re.Split(CONFIG["cliPwd"].ToString());

                SetCliPwds(cliPwds);
            }

            if (CONFIG.ContainsKey("enablePwd") && (CONFIG["enablePwd"].ToString().Length > 0))
            {
                string[] cliEnPwds = re.Split(CONFIG["enablePwd"].ToString());

                SetCliEnablePwds(cliEnPwds);
            }

            CONFIG.Clear();
            CONFIG = null;
            return (true);
        }

        protected string Specify()
        {
            int HPMatches = 0;
            string[] l_Lines = GetShowVersion();

            foreach (string strLine in l_Lines)
            {
                if (HelperClass.RegExCompare(strLine, @"s*[A-Z]+\.\d+\.\d+\s*|\s*Boot Image:.*", RegexOptions.IgnoreCase))
                {   //HP-Procurve
                    HPMatches++;
                    if (HPMatches > 1)
                    {
                        DeviceType = "HP";
                        break;
                    }
                }
                else if (strLine.ToLower().Contains("cisco ios xr ")) //Cisco IOS XR Software or iosxr-
                {
                    DeviceType = "Cisco-IOS-XR";
                    break;
                }
                else if (strLine.ToLower().Contains("cisco adaptive security appliance"))
                {
                    DeviceType = "Cisco-ASA";
                    break;
                }else if (strLine.ToLower().Contains("cisco"))
                {
                    DeviceType = "Cisco";
                    break;
                }
            }

            if (DeviceType.Length == 0)
            {
                string[] strOutp = this.SendCmd("show inventory");
                if (strOutp != null)
                {
                    foreach (string strLine in strOutp)
                    {
                        if (HelperClass.RegExCompare(strLine, @"cisco .*wireless .*controller", RegexOptions.IgnoreCase))
                        {
                            DeviceType = "Cisco WLC";
                            break;
                        }
                    }
                }
            }
            if((DeviceType.Length == 0) || (DeviceType.Equals("HP"))) {
                string[] strOutp = this.SendCmd("display version");
                if (strOutp != null)
                {
                    foreach (string strLine in strOutp)
                    {
                        if (strLine.ToLower().Contains("comware software"))
                        {
                            DeviceType = "H3C"; //H3C, HP A-Series
                            break;
                        }
                    }
                }
            }

            if (DeviceType.Length == 0)
            {
                l_Lines = GetRunningConfig();
                foreach (string strLine in l_Lines)
                {
                    if (HelperClass.RegExCompare(strLine, @"\s*;.* Configuration Editor; Created on release", RegexOptions.IgnoreCase))
                    {
                        DeviceType = "HP";
                        break;
                    }
                }
            }

            if (_Debug > 0)
            {
                Console.WriteLine("Device-Type: " + DeviceType);
            }

            return (DeviceType);
        }

        public string GetDeviceType
        {
            get { return (DeviceType); }
        }

        public int Debug
        {
            get { return (_Debug); }
            set { _Debug = value; }
        }

        public string[] GetIntoConfigMode()
        {
            string strOutput = "";
            string[] strLines = { "" };

            //Change to Config-Mode of Router
            if(DeviceType.Equals("H3C"))
                strOutput = shell.cmd("system-view", DevicePromptConfig);
            else
                strOutput = shell.cmd("configure terminal", DevicePromptConfig);

            bConfigMode = true;
            if (_Debug > 0)
            {
                Console.WriteLine("Entered-Configmode:");
                if (strOutput != null)
                    Console.WriteLine(strOutput);
            }
            if (strOutput != null)
                strLines = strOutput.Split(new char[] { '\n' });

            return (strLines);
        }

        public string[] ExitConfigMode()
        {
            string strOutput = "";
            string[] strLines = { "" };

            //Exit from Config-Mode
            if (DeviceType.Equals("H3C"))
                strOutput = shell.cmd("return", DevicePrompt);
            else if (DeviceType.Equals("Cisco-IOS-XR"))
            {
                string _tmpPrompt = DevicePrompt + @"|[\[(](?:[Yy](?:es)?|[Nn]o?)[\\/|](?:[Yy](?:es)?|[Nn]o?)(?:[\\/|](?:[Cc]?(?:ancel)?|[Aa]?(?:ll)?))*[)\]]\s*[?:]?\s*(?:\[[a-zA-Z]+\])?\s*[?:]?\s*$";
                strOutput = shell.cmd("end", _tmpPrompt);
                if(!HelperClass.RegExCompare(strOutput, DevicePrompt, RegexOptions.IgnoreCase)) {
                    strOutput = shell.cmd("n", DevicePrompt);
                }
            }
            else
                strOutput = shell.cmd("end", DevicePrompt);

            bConfigMode = false;
            if (_Debug > 0)
            {
                Console.WriteLine("Exited-Configmode:");
                if (strOutput != null)
                    Console.WriteLine(strOutput);
            }

            if (strOutput != null)
                strLines = strOutput.Split(new char[] { '\n' });

            return (strLines);
        }

        public string[] SendCmd(string l_cmd)
        {
            string l_ExpectedPrompt = DevicePrompt;
            string strOutput = "";
            string[] strLines = { "" };
            bool MoreFound = false;
            Regex re = null;
            Regex reMore = new Regex("[\\r\\n]* *\\<?--+ ?more( ?--+| ).*$", RegexOptions.IgnoreCase);
            Regex reEnter = new Regex("[\\r\\n]* *press enter to continue.*$", RegexOptions.IgnoreCase);

            if (bConfigMode)
                l_ExpectedPrompt = DevicePromptConfig;

            //Check also for "-- MORE --"
            l_ExpectedPrompt += "|\\<?--+ ?[Mm][Oo][Rr][Ee] ?--|-- ?[Mm][Oo][Rr][Ee] |[Pp]ress [Ee]nter to continue";

            //
            //Write to Device-Terminal
            //
            strOutput += shell.cmd(l_cmd, l_ExpectedPrompt);
            do
            {
                MoreFound = reMore.IsMatch(strOutput);
                if (MoreFound)
                {
                    strOutput = reMore.Replace(strOutput, "\n");
                    shell.DeleteScreen();
                    shell.put(" ");
                    //ToDo: check if result is null => no Prompt was found
                    shell.WaitForRegEx(l_ExpectedPrompt);
                    strOutput += shell.ShowScreen();
                }
                else if (reEnter.IsMatch(strOutput))
                {
                    MoreFound = true;
                    strOutput = reEnter.Replace(strOutput, "\n");
                    shell.DeleteScreen();
                    shell.put("\n");
                    //ToDo: check if result is null => no Prompt was found
                    shell.WaitForRegEx(l_ExpectedPrompt);
                    strOutput += shell.ShowScreen();
                }
            } while (MoreFound);

            //Set Prompt without --More--
            l_ExpectedPrompt = DevicePrompt;
            if (bConfigMode)
                l_ExpectedPrompt = DevicePromptConfig;

            if (_Debug > 0)
            {
                Console.WriteLine("Result of Cmd: " + l_cmd);
                if (strOutput != null)
                    Console.WriteLine(strOutput);
            }
            if (strOutput != null)
            {
                re = new Regex(@"[\n\r]*" + l_ExpectedPrompt);
                strOutput = re.Replace(strOutput, "");      // Remove Prompt from Output
                strLines = strOutput.Split(new char[] { '\n' });
            }

            return (strLines);
        }

        public DevicePrompts detectPrompt(string unknownPrompt)
        {
            string strTemp = "", strOutput = "";
            char LastPromptChar = '\0';
            DevicePrompts prompts = new DevicePrompts();

            //Get full Prompt
            strOutput = shell.cmd("", unknownPrompt);
            if ((strOutput == null) || (strOutput.Length == 0))
            {
                //Found no Prompt Exit...
                prompts = null;
                return(null);
            }
            string[] strLines = strOutput.Split(new char[] { '\n' });
            foreach (string strLine in strLines)
            {
                strTemp = strLine.Replace("\n", "");
                strTemp = strTemp.Replace("\r", "");
                strTemp = strTemp.Trim(); // strTemp.Replace(" ", "");
                if (strTemp.Length > 0)
                {
                    LastPromptChar = strTemp.ToCharArray(strTemp.Length - 1, 1)[0];
                    strTemp = strTemp.Substring(0, strTemp.Length - 1);  // Remove Last Char
                    prompts.DevicePrompt = HelperClass.quotemeta(strTemp);
                }
            }
            prompts.DevicePromptString = prompts.DevicePrompt;
            if ((LastPromptChar == '#') || (DeviceType.Equals("H3C")) || (DeviceType.Equals("HP") && (LastPromptChar == ']')))
            {
                //EnableMode = true;

                if (prompts.DevicePrompt.Length < 10)
                    prompts.DevicePromptConfig = prompts.DevicePrompt.Substring(0, prompts.DevicePrompt.Length);
                else
                    prompts.DevicePromptConfig = prompts.DevicePrompt.Substring(0, 10);
                if (prompts.DevicePromptConfig.EndsWith("\\"))
                    prompts.DevicePromptConfig = prompts.DevicePromptConfig.Substring(0, prompts.DevicePromptConfig.Length - 1);

                prompts.DevicePromptConfig = prompts.DevicePromptConfig + LastPromptChar;
                //DevicePromptConfig =~ s/(.*)(.)$/$1.*\\(config.*\\)$2/;	# s/(.*)([^\s])\s?$/$1.*\\(config.*\\)$2/;
                if ((DeviceType.Equals("HP") && (LastPromptChar == ']')) || DeviceType.Equals("H3C"))
                {
                    prompts.DevicePromptConfig = prompts.DevicePromptConfig.Substring(0, prompts.DevicePromptConfig.Length - 1) + ".*-.*" + LastPromptChar;
                    //DeviceType = "H3C";
                }
                else if (DeviceType.Equals("HP"))
                {
                    prompts.DevicePromptConfig = prompts.DevicePromptConfig.Substring(0, prompts.DevicePromptConfig.Length - 1) + ".*\\(.*\\)" + LastPromptChar;
                }
                else
                {
                    prompts.DevicePromptConfig = prompts.DevicePromptConfig.Trim();
                    //DevicePromptConfig = DevicePromptConfig.Substring(0, DevicePromptConfig.Length-1) + ".*\\(config.*\\)" + "#";
                    prompts.DevicePromptConfig = prompts.DevicePromptConfig.Substring(0, prompts.DevicePromptConfig.Length - 1) + ".*\\((?:c.*|.*-config)\\)" + LastPromptChar;
                }
                prompts.DevicePromptConfig = prompts.DevicePromptConfig + "[ \\r\\n]*$";
            }
            else
                prompts.DevicePromptConfig = "";

            prompts.DevicePrompt = prompts.DevicePrompt + LastPromptChar + "[ \\r\\n]*$";
            prompts.LastPromptChar = LastPromptChar;
            if (_Debug > 0)
            {
                Console.WriteLine("\nDebug-Info:");
                Console.WriteLine("new Prompt: " + prompts.DevicePrompt);
                Console.WriteLine("Config-Prompt: " + prompts.DevicePromptConfig);
            }
            return (prompts);
        }

        public string[] GetShowVersion() {
            string[] l_strLines = { "" };
            //string strOutput = "";
            //
            //Read Show-Version from Device
            //
            if(DeviceType.Equals("H3C"))
                l_strLines = this.SendCmd("display version");
            else
                l_strLines = this.SendCmd("show version"); // strOutput = shell.cmd("show version", DevicePrompt);

            if( _Debug > 0) {
                Console.WriteLine("Show-Version:");
                if (l_strLines != null) //strOutput
                    Console.WriteLine(String.Join("\n", l_strLines));
            }
  
            //if( strOutput != null )
            //    l_strLines = strOutput.Split(new char[] { '\n' });
  
            return(l_strLines);
        }

        public virtual bool detectNewPrompt()
        {
            return (false);
        }

        public string[] GetRunningConfig() {
            string[] l_strLines = { "" };

            //
            //Read Running-Config from Router
            //
            // shell.cmd_remove_mode(1);
            if(DeviceType.Equals("H3C"))
                l_strLines = this.SendCmd("display current-configuration");
            else
                l_strLines = this.SendCmd("show running-config");
            if (_Debug > 0)
            {
                Console.WriteLine("Running-Config:");
                if ((l_strLines != null) && (l_strLines.Length >0))
                    foreach (string strLine in l_strLines)
                        Console.WriteLine(strLine);
            }
  
            return(l_strLines);
        }

        #region TelnetSSHLogin-Passthrough
        public int Timeout
        {
            get { return shell.Timeout; }
            set { shell.Timeout = value; }
        }
        public string Prompt
        {
            get
            {
                return shell.Prompt;
            }
            set
            {
                shell.Prompt = value;
            }
        }
        public void DeleteScreen()
        {
            shell.DeleteScreen();
        }
        public string ShowScreen()
        {
            return shell.ShowScreen();
        }
        public void print(string strCmd)
        {
            shell.print(strCmd);
        }
        public void put(string strCmd)
        {
            shell.put(strCmd);
        }
        public string WaitFor(object[] searchFor, bool CaseSensitive, int TimeOut)
        {
            List<string> strArray = new List<string>();
            foreach (object newObj in searchFor)
            {
                strArray.Add(newObj.ToString());
            }
            return shell.WaitFor(strArray.ToArray(), CaseSensitive, TimeOut);
        }
        public string WaitForString(string searchFor)
        {
            return shell.WaitForString(searchFor);
        }
        public string WaitForRegEx(string regEx)
        {
            return shell.WaitForRegEx(regEx);
        }
        #endregion

        #region IP-Functions
        public string GetIPNet(string IP, string Subnet)
        {
            IPClass clsIP = new IPClass(IP, Subnet);
            return (clsIP.IPNet);
        }
        public string GetIPBCast(string IP, string Subnet)
        {
            IPClass clsIP = new IPClass(IP, Subnet);
            return (clsIP.IPBCast);
        }
        public string Add2IP(string IP, long Offset)
        {
            IPClass clsIP = new IPClass(IP, "255.255.255.0");
            return (clsIP.Add2IP(IP, Offset));
        }
        #endregion
    }
}
