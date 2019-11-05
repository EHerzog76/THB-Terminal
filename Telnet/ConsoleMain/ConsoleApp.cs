using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;

using Core;
using TerminalSessions;
using Protocols;
using ConnectionParam;
using TelnetSession;

namespace ConsoleMain
{
    public class ConsoleApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;

            //CW2kClass cw2k = new CW2kClass();
            //return;

            //SSHTest sshTest = new SSHTest();
            //return;

            //ConTestTelnet termTel = new ConTestTelnet();
            //termTel = null;

            Console.WriteLine("Press key to continue...");
            cki = Console.ReadKey(true);
            ConTestAtHome termAtHome = new ConTestAtHome();
            termAtHome = null;
            Console.WriteLine("Press a key for Program-End...");
            cki = Console.ReadKey(true);
            Console.WriteLine("End of Program.");
            return;

/*
            bool bConfigure = false, bShow = false;
            string WlanID = "";
            string APNameFilter = null;
            int argCounter = 0;
            while(argCounter < args.Length)
            {
                if (args[argCounter].Equals("/?"))
                {
                    Console.WriteLine("Usage of WLC-Config:\n");
                    Console.WriteLine("\t/configure.............configure WLAN-VLAN mapping.\n");
                    Console.WriteLine("\t/checkWLAN:<WLAN-ID>...print WLAN-VLAN mapping.\n");
                    Console.WriteLine("\t/restartAPs:<AP-Name-Filter>...e.g. zuba*.\n");
                    return;
                }
                else if (args[argCounter].ToLower().Equals("/configure"))
                {
                    bConfigure = true;
                }
                else if (args[argCounter].ToLower().StartsWith("/checkwlan:"))
                {
                    bShow = true;
                    WlanID = args[argCounter].Substring(11);
                }
                else if(args[argCounter].ToLower().StartsWith("/restartaps:")) {
                    APNameFilter = args[argCounter].Substring(12);
                }
                argCounter++;
            }
            if(APNameFilter != null) {
                ConWLCAPRestart term = new ConWLCAPRestart(APNameFilter);
                term = null;
            } else {
                ConTelnetWLC term = new ConTelnetWLC(bConfigure, bShow, WlanID);
                term = null;
            }

            Console.WriteLine("End of Program.");
            return;
*/
/*
            ConsoleClass ConCls = new ConsoleClass();
            while (ConCls.ConMain.IsRunning)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("End of Program.");
 */
        }
    }

    public class ConTestAtHome
    {
        ITermDevice ConMain = null;

        public ConTestAtHome()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string AppPath = System.IO.Path.GetDirectoryName(AppName);
            string AppFileName = System.IO.Path.GetFileNameWithoutExtension(AppName);

            string[] arrCnfLines = null;
            LoginStatus LoginResult = LoginStatus.Unknown;

            if (!AppPath.EndsWith("\\"))
                AppPath += "\\";

            ConMain = new TelnetDevice();
            ConMain.Debug = 1;

            //Read Config-File
            ConMain.Init(AppPath + AppFileName + ".conf");

            LoginResult = ConMain.doLogin("192.168.192.240", ""); //10.0.21.254
            if ((LoginResult != LoginStatus.Success) && (LoginResult != LoginStatus.UserMode))
            {
                return;
            }

            arrCnfLines = ConMain.SendCmd("show ip inter brief");
            foreach (string strLine in arrCnfLines)
                Console.WriteLine(strLine);

            LoginResult = ConMain.Jump2NextDevice("172.19.5.10", "");  //172.18.31.1, 10.229.111.254, 10.229.108.50, 10.24.0.11
            if ((LoginResult == LoginStatus.Success) || (LoginResult == LoginStatus.UserMode))
            {
                ConMain.SendCmd("show interface brief");
                ConMain.doLogout();
            }

            ConMain.doLogout();
            ConMain = null;
        }
    }

    public class ConTestTelnet
    {
        ITermDevice ConMain = null;

        public ConTestTelnet()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string AppPath = System.IO.Path.GetDirectoryName(AppName);
            string AppFileName = System.IO.Path.GetFileNameWithoutExtension(AppName);

            string[] arrCnfLines = null;
            LoginStatus LoginResult = LoginStatus.Unknown;

            if (!AppPath.EndsWith("\\"))
                AppPath += "\\";

            ConMain = new TelnetDevice();
            ConMain.Debug = 1;

            //Read Config-File
            ConMain.Init(AppPath + AppFileName + ".conf");

            LoginResult = ConMain.doLogin("192.168.192.230", "");
            if ((LoginResult != LoginStatus.Success) && (LoginResult != LoginStatus.UserMode) )
            {
                return;
            }

            ConMain.SetCliLogins(new string[] { "Username" }, new string[] { "Pwd***" });
            ConMain.SetCliEnablePwds(new string[] {"EnablePwd***"});
            LoginResult = ConMain.Jump2NextDevice("10.116.25.138", "");
            if ((LoginResult != LoginStatus.Success) && (LoginResult != LoginStatus.UserMode) )
            {
                ConMain.doLogout();
                return;
            }
            arrCnfLines = ConMain.SendCmd("show ip inter brief");
            foreach (string strLine in arrCnfLines)
                Console.WriteLine(strLine);

            ConMain.doLogout();
            ConMain.doLogout();
            return;

            if (ConMain.GetDeviceType.Equals("Cisco WLC"))
            {
                arrCnfLines = ConMain.SendCmd("show run-config");
                foreach (string strLine in arrCnfLines)
                    Console.WriteLine(strLine);

                ConMain.doLogout();
                return;
            }

            ConMain.Debug = 0;
            arrCnfLines = ConMain.SendCmd("show run");
            foreach (string strLine in arrCnfLines)
                Console.WriteLine(strLine);

            ConMain.Debug = 1;
            if (ConMain.GetDeviceType.Equals("Cisco-IOS-XR"))
            {
                ConMain.GetIntoConfigMode();
                //arrCnfLines = ConMain.SendCmd("vrf Post");
                ConMain.ExitConfigMode();
            }
            else
            {
                LoginResult = ConMain.Jump2NextDevice("172.19.5.2", "");  //172.18.31.1, 10.229.111.254, 10.229.108.50, 10.24.0.11
                if ((LoginResult == LoginStatus.Success) || (LoginResult == LoginStatus.UserMode))
                {
                    if (ConMain.GetDeviceType.Equals("H3C"))
                    {
                        ConMain.SendCmd("display version");
                        ConMain.GetIntoConfigMode();
                        ConMain.SendCmd("display clock");
                        ConMain.SendCmd("interface Vlan-interface 100");
                        ConMain.SendCmd("description Test Vlan-100");
                        ConMain.SendCmd("quit");
                        ConMain.ExitConfigMode();
                    }
                    else
                        ConMain.SendCmd("show version");
                    ConMain.doLogout();
                }
            }

            if (ConMain.GetDeviceType.Equals("Cisco") || ConMain.GetDeviceType.Equals("HP"))
            {
                ConMain.GetIntoConfigMode();
                if (ConMain.GetDeviceType.Equals("HP"))
                    arrCnfLines = ConMain.SendCmd("show version");
                else
                {
                    ConMain.SendCmd("ip dhcp pool Test");
                    ConMain.SendCmd("network 10.10.10.0 255.255.255.0");
                    ConMain.SendCmd("exit");
                    ConMain.SendCmd("no ip dhcp pool Test");
                    arrCnfLines = ConMain.SendCmd("do show version");
                }
                foreach(string strLine in arrCnfLines)
                    Console.WriteLine(strLine);

                ConMain.ExitConfigMode();
            }
            if(ConMain.GetDeviceType.Equals("HP") ) {
                arrCnfLines = ConMain.SendCmd("show time");
            } else {
                arrCnfLines = ConMain.SendCmd("show clock");
            }
            foreach (string strLine in arrCnfLines)
                Console.WriteLine(strLine);

            ConMain.doLogout();
        }

    }

    public class ConTelnetWLC
    {
        ITermDevice ConMain = null;

        public ConTelnetWLC(bool DoConfig, bool DoShow, string WlanID)
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string AppPath = System.IO.Path.GetDirectoryName(AppName);
            string AppFileName = System.IO.Path.GetFileNameWithoutExtension(AppName);
            LoginStatus loginState = LoginStatus.Unknown;
            string[] arrCnfLines = null;
            string[] arrAPConfig = null;
            Regex reAPName = new Regex(@"([^ ]+) {2,20}[0-9]+ +.*");
            Regex WlanVlan = new Regex(@"[\s\t]+wlan ([0-9]+)\s*:\.+ ([0-9]+)", RegexOptions.IgnoreCase);
            Match mAP = null, mVLAN = null;
            string APName = "";
            bool bFoundFlexVlanMode = false;
            Regex reVlanNr = new Regex(@".*:\.+ *([0-9]+)");

            ConMain = new TelnetDevice();
            ConMain.Debug = 1;

            //Read Config-File
            ConMain.Init(AppPath + AppFileName + ".conf");

            loginState = ConMain.doLogin("10.110.10.3", "", "Cisco");
            if ((loginState != LoginStatus.Success) && (loginState != LoginStatus.UserMode))
            {
                return;
            }

            ConMain.Debug = 1;
            arrCnfLines = ConMain.SendCmd("show ap summary");
            if (!DoConfig)
                ConMain.Debug = 0;
            foreach (string strLine in arrCnfLines)
            {
                //Console.WriteLine(strLine);
                APName = "";
                mAP = reAPName.Match(strLine);
                if (mAP.Success)
                {
                    APName = mAP.Groups[1].Value;
                    //Console.WriteLine("\t" + APName);

                    bFoundFlexVlanMode = false;
                    arrAPConfig = ConMain.SendCmd("show ap config general " + APName);
                    foreach (string strAPCfg in arrAPConfig)
                    {
                        if (bFoundFlexVlanMode)
                        {
                            if (DoShow)
                            {
                                mVLAN = WlanVlan.Match(strAPCfg);
                                if (mVLAN.Success)
                                {
                                    Console.WriteLine(APName + ";WlanID-VLAN;" + mVLAN.Groups[1].Value + "=" + mVLAN.Groups[2].Value);
                                }
                            }
                            else if (strAPCfg.ToLower().StartsWith("	wlan 2 :..."))
                            {
                                //Set VLAN for WLAN-ID: 10
                                mVLAN = reVlanNr.Match(strAPCfg);
                                if (mVLAN.Success)
                                {
                                    if (!DoConfig)
                                    {
                                        Console.WriteLine("config ap disable " + APName);
                                        Console.WriteLine("config ap flexconnect vlan wlan 10 " + mVLAN.Groups[1].Value + " " + APName);
                                        Console.WriteLine("config ap enable " + APName);
                                    }
                                    else
                                    {
                                        ConMain.SendCmd("config ap disable " + APName);
                                        ConMain.SendCmd("config ap flexconnect vlan wlan 10 " + mVLAN.Groups[1].Value + " " + APName);
                                        ConMain.SendCmd("config ap enable " + APName);
                                    }
                                }
                            }
                        }
                        else if((bFoundFlexVlanMode) && (strAPCfg.ToLower().StartsWith("flexconnect vlan acl mappings")) ){
                            break;
                        }
                        else if (strAPCfg.ToLower().StartsWith("flexconnect vlan mode :."))
                            bFoundFlexVlanMode = true;
                    }
                }
            }

            ConMain.Debug = 1;
            ConMain.doLogout();
        }
    }

    public class ConWLCAPRestart
    {
        ITermDevice ConMain = null;

        public ConWLCAPRestart(string APNameFilter)
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string AppPath = System.IO.Path.GetDirectoryName(AppName);
            string AppFileName = System.IO.Path.GetFileNameWithoutExtension(AppName);
            LoginStatus loginState = LoginStatus.Unknown;
            string[] arrCnfLines = null;
            string[] arrAPConfig = null;
            Regex reAPName = new Regex(@"([^ ]+) {2,20}[0-9]+ +.*");
            Match mAP = null;
            string APName = "";

            ConMain = new TelnetDevice();
            ConMain.Debug = 1;

            //Read Config-File
            ConMain.Init(AppPath + AppFileName + ".conf");

            loginState = ConMain.doLogin("10.110.10.3", "", "Cisco");
            if ((loginState != LoginStatus.Success) && (loginState != LoginStatus.UserMode))
            {
                return;
            }

            //ConMain.Debug = 1;
            arrCnfLines = ConMain.SendCmd("show ap summary");
            foreach (string strLine in arrCnfLines)
            {
                //Console.WriteLine(strLine);
                APName = "";
                mAP = reAPName.Match(strLine);
                if (mAP.Success)
                {
                    APName = mAP.Groups[1].Value;
                    //Console.WriteLine("\t" + APName);

                    if (APNameFilter.Equals("") || (APName.ToLower().Contains(APNameFilter)))
                    {
                        if (APName.ToLower().StartsWith("loc_1030") || APName.ToLower().StartsWith("loc_2000")
                            || APName.ToLower().StartsWith("loc_1060") || APName.ToLower().StartsWith("loc_1140")
                            || APName.ToLower().StartsWith("loc_1100") || APName.ToLower().StartsWith("loc_2700")
                            || APName.ToLower().StartsWith("loc_6800") || APName.ToLower().StartsWith("loc_2100")
                            || APName.ToLower().StartsWith("loc_2500"))
                        {
                            //Skip this APs
                        }
                        else
                        {
                            Console.WriteLine(APName + " init Reload");
                            ConMain.DeleteScreen();
                            ConMain.print("config ap reset " + APName);
                            ConMain.WaitForString("(y/n)");
                            arrAPConfig = ConMain.SendCmd("y");
                            Console.WriteLine(APName + " Reloading");
                        }
                    }
                    else
                    {
                    }
                }
            }

            ConMain.Debug = 1;
            ConMain.doLogout();
        }
    }

    public class SSHTest
    {
        public SSHTest()
        {
            string AppName = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string AppPath = System.IO.Path.GetDirectoryName(AppName);
            if (!AppPath.EndsWith("\\"))
                AppPath += "\\";
            string AppFileName = System.IO.Path.GetFileNameWithoutExtension(AppName);

            string[] arrCnfLines = null;

            ITermDevice termSSH = new SSHDevice();
            //Read Config-File
            termSSH.Init(AppPath + AppFileName + ".conf");
            termSSH.Debug = 1;
            //10.229.108.50, 172.19.200.253
            //ASA-FW: 192.168.165.164
            LoginStatus LoginResult = termSSH.doLogin("192.168.165.161", "");
            if ((LoginResult == LoginStatus.Success) || (LoginResult == LoginStatus.UserMode))
            {
                if (termSSH.GetDeviceType.Equals("Cisco WLC"))
                {
                    arrCnfLines = termSSH.SendCmd("show interface summary");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));
                }
                else if (termSSH.GetDeviceType.Equals("Cisco-ASA"))
                {
                    arrCnfLines = termSSH.SendCmd("show version");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));
                    arrCnfLines = termSSH.SendCmd("show mode");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));

                    termSSH.GetIntoConfigMode();
                    arrCnfLines = termSSH.SendCmd("show clock");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));
                    termSSH.ExitConfigMode();
                    termSSH.DeleteScreen();
                    termSSH.print("changeto context lic");
                    termSSH.WaitForRegEx(@"[\n\r]+.*[>#]\s*[\r]?[\n]?");
                    if (termSSH.detectNewPrompt())
                        Console.WriteLine("Found new Prompt: " + termSSH.Prompt);
                    else
                        Console.WriteLine("No Prompt found: " + termSSH.Prompt);
                } else
                {
                    arrCnfLines = termSSH.SendCmd("show version");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));
                    termSSH.GetIntoConfigMode();
                    if (termSSH.GetDeviceType.Equals("HP"))
                        arrCnfLines = termSSH.SendCmd("show time");
                    else
                        arrCnfLines = termSSH.SendCmd("do show clock");
                    if (arrCnfLines != null)
                        Console.WriteLine(String.Join("\n", arrCnfLines));
                    termSSH.ExitConfigMode();
                }
                termSSH.doLogout();
            }
            else
            {
            }
        }
    }

    public class CW2kClass
    {
        public TelnetSSHLogin ConMain = null;
        private string unknownPrompt = @"\n*[a-zA-Z0-9\-\.@~ \[\]]+[\>#\$]\s*\r?\n?$";
        private string Prompt = "";

        public CW2kClass()
        {
            string strCmd = "";
            string DeviceName = "device-992-xyz";
            string strOutput = "";
            Regex re = null;
            Match match = null;
            string DeviceID = "";

            ConMain = new TelnetSSHLogin();
            ConMain.Timeout = 120;
            bool bResult = ConnectSSH("AppUser", "Pwd***", "192.168.192.52");
            if (bResult)
            {
                strCmd = "/opt/CSCOpx/bin/dcrcli -u admin";
                strCmd += " cmd=lsids dn=" + DeviceName;
                strOutput = ConMain.cmd(strCmd);
                if (strOutput.Length > 0)
                {
                    DeviceID = "";

                    //Check if Device exists already
                    re = new Regex(@"\nID\s*=\s*([0-9]+)\s+.*", RegexOptions.IgnoreCase);
                    match = re.Match(strOutput);
                    if (match.Success)
                    {
                        DeviceID = match.Groups[1].Value;
                    }

                    if (DeviceID.Length == 0)
                    {
                        Console.WriteLine("CW2K Device dose not exist.");
                    }
                    else
                    {
                        Console.WriteLine("CW2K Device exist with ID: " + DeviceID + ".");
                    }
                }
            }
        }

        private bool ConnectSSH(string User, string Pwd, string Host)
        {
            string strOutput = "";
            Regex re = new Regex(@"\n([^\n].*)\s*\n*$", RegexOptions.IgnoreCase);

            bool bResult = ConMain.open(User, Pwd, Host, 0, "ssh2", "", "", "");
            if (bResult)
            {
                strOutput = ConMain.WaitForRegEx(unknownPrompt);
                strOutput = ConMain.ShowScreen(); //objCon.WaitForRegEx(unknownPrompt);
                Match m = re.Match(strOutput);
                if (m.Success)
                {
                    Prompt = m.Groups[1].Value;
                    Prompt = Prompt.Replace(@"\", "\\\\");
                    Prompt = Prompt.Replace(@"[", "\\[");
                    Prompt = Prompt.Replace(@"]", "\\]");
                    Prompt = Prompt.Replace(@"(", "\\(");
                    Prompt = Prompt.Replace(@")", "\\)");
                    Prompt = Prompt.Replace(@"{", "\\{");
                    Prompt = Prompt.Replace(@"}", "\\}");
                    Prompt = Prompt.Replace(@"$", "\\$");
                    // Prompt = "\n+" + Prompt;
                    Prompt = @"[\r\n]*" + Prompt + @"\s*[\r\n]?$";
                    ConMain.Prompt = Prompt;
                    bResult = true;
                }
                else
                    bResult = false;

                ConMain.DeleteScreen();
            }

            return (bResult);
        }
    }

    public class ConsoleClass
    {
        public TelnetSSHLogin ConMain = null;

        public ConsoleClass()
        {
            ConMain = new TelnetSSHLogin();
            //ConMain.PrepareTerminalParameter("Username", "Pwd***", "192.168.192.165", 0, ConnectionMethod.SSH2, "", LogType.Default, "");
            ConMain.PrepareTerminalParameter("Username", "Pwd***", "172.25.156.2", 0, ConnectionMethod.Telnet, "", LogType.Binary, "");
            if (!ConMain.StartConnection())
                return;

            string strOutput = "";

            strOutput = ConMain.WaitFor(new string[] { "Username:", "Login:", "Password:", "Press any key to continue" }, false, 0);
            if ((strOutput == null) || (strOutput.Length == 0))
                return;

            if (strOutput.ToLower().Contains("press any key to continue"))
            {
                ConMain.print(" ");
                strOutput = ConMain.WaitFor(new string[] { "username:", "login:", "password:" }, false, 0);
                if ((strOutput == null) || (strOutput.Length == 0))
                    return;
            }

            if(strOutput.ToLower().Contains("username:") || strOutput.ToLower().Contains("login:")) {
                ConMain.print("Username");
                strOutput = ConMain.WaitForString("Password:");
                if ((strOutput == null) || (strOutput.Length == 0))
                    return;

                strOutput = ConMain.cmd("Pwd***");
            } else if(strOutput.ToLower().Contains("password:")) {
                strOutput = ConMain.cmd("Pwd1***");
            } else {
                Console.WriteLine("Error: Found no Loginprompt.");
                return;
            }

            if ((strOutput == null) || (strOutput.Length == 0)) {
                Console.WriteLine("Error: Found no Prompt after Login.");
            }
            Console.WriteLine(ConMain.ShowScreen());

            strOutput = ConMain.cmd("terminal length 0");
            strOutput = ConMain.cmd("terminal length 1000");
            strOutput = ConMain.cmd("terminal width 0");

            strOutput = ConMain.cmd("show version");
            if (strOutput.Length > 0)
                Console.Write(strOutput);
            strOutput = ConMain.cmd("show run");
            if (strOutput.Length > 0)
                Console.Write(strOutput);

            ConMain.print("exit");
            ConMain.close();
            /*
            strOutput = ConMain.WaitForString(">");
            Console.WriteLine(ConMain.ShowScreen());
            ConMain.print("exit");
            strOutput = ConMain.WaitForString(":");
            ConMain.print("y");
            strOutput = ConMain.WaitForString(":");
            ConMain.print("n");

            Console.WriteLine(ConMain.ShowScreen());
            */
            return;

            strOutput = ConMain.cmd("ls -alh /");
            if (strOutput.Length > 0)
                Console.Write(strOutput);

            strOutput = ConMain.cmd("ls -a /");
            if (strOutput.Length > 0)
                Console.Write(strOutput);

            strOutput = ConMain.cmd("ls -alh /");
            if (strOutput.Length > 0)
                Console.Write(strOutput);
            
            ConMain.print("exit");
        }

        ~ConsoleClass()
        {
            Console.WriteLine("End all.");
        }
    }
}
