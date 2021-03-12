# THB-Terminal
Is a terminal-automation software.
You can access remote devices over telnet- and ssh2-protocol.
It supports ASCII-, ANSI-, VT100- and xTerm-Terminals.
The base-object of the THB-Terminal is the CfgEngine object, which has builtin support for login- and prompt- handling.
So you don´t have to check for a login-prompt, for the cli-prompt or for the --more--prompt and so on.

## How to use THB-Terminal

###  with Powershell or Powershell-core
```
#Load ConfigEngine-DLL the base-object of THB-Terminal
Add-Type -Path "C:\Progs\THB-Terminal\ConfigEngine.dll";
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();

#Load configuration from a config-file
$CfgFile = ".\ConfigEngine.cfg";
$CfgEngine.readConfig($CfgFile);
if($Debug -eq $true) {
    $CfgEngine.Debug = 1;
} else {
    $CfgEngine.Debug = 0;
}

[String]$strCliResult = "";

	# Login to the remote-device
	$LoginResult = $CfgEngine.Login("10.20.30.40", "ssh");
	if($LoginResult -eq 0) {
		Write-Output "Login failed.";
		exit(1);
	}

	Write-Output "DeviceType: $($CfgEngine.GetDeviceType)";

	if($CfgEngine.GetDeviceType -eq "Cisco-FP") {
        $strCliResult = $CfgEngine.Execute("show failover state");
    } elseif($CfgEngine.GetDeviceType -match "^Cisco.*") {
        $strCliResult = $CfgEngine.Execute("terminal length 300");
        
        $strCliResult = $CfgEngine.Execute("show version");
    } elseif($CfgEngine.GetDeviceType -eq "HP") {
        $strCliResult = $CfgEngine.Execute("show interface brief");

        foreach($strLine in $strCliResult.Split("`n")) {
            if($strLine -match "^[ ]+24[ ]+") {
                Write-Output "Found IF-24...$($strLine)";

                break;
            }
        }
    }
    Write-Output $strCliResult;

 
	if($CfgEngine.GetDeviceType -match "^Cisco$") {
		#Change into config-mode
		#	only needed for devices with config-mode e.g: Cisco (IOS, IOS-XE, IOS-XR, ...), HP-Advanced-Series, HP-Aruba, H3C, ...
		$CfgEngine.EnterConfigMode();
		$strCliResult = $CfgEngine.Execute("do show clock");
		Write-Output $strCliResult;
		
		#Exit from config-mode
		$CfgEngine.ExitConfigMode();
	}


    $CfgEngine.doLogout();
	$CfgEngine.Close();
	$CfgEngine = $null;
```

## How to install THB-THB-Terminal

### Install Powershell or Powershell-core
You can use Powershell-core for Windows and Linux.
Powershell is only for Windows.
For more informations see online:
	https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1
	https://github.com/powershell/powershell
	
### Install THB-Terminal
Copy the content of this bin-folder to your local machine.
And use it as shown in the examples.

## ConfigEngine-Object

### CfgEngine.Login

    int CfgEngine.Login("Device-IP", "Telnet")
    int CfgEngine.Login("Device-IP", "SSH")
    int CfgEngine.Login("Device-IP", "SSH", "Device-Type")
		for "Device-Type" see below: CfgEngine.GetDeviceType
		
Result 
	•0 Login failed.
	•1 Login was successfull.
Description
    Login to a device. 

### CfgEngine.doLogout

   void CfgEngine.doLogout()

Result
    No Result ist returned.
Description
    Logout from device. 

### CfgEngine.Jump2NextDevice

   int CfgEngine.Jump2NextDevice(DeviceIP, ConnectOptions)
		only for telnet-access.
		
Result
	•0 Login failed.
	•1 Login was successfull in Usermode.
	•2 Login was successfull in Enablemode.
Description
    Open a connection to a other device. 

### CfgEngine.EnterConfigMode

   void CfgEngine.EnterConfigMode()

Result
    No result.
Description
    Change into config-mode. 

### CfgEngine.ExitConfigMode

   void CfgEngine.ExitConfigMode()

Result
    No result.
Description
    Exit from config-mode. 

### CfgEngine.Execute

   string CfgEngine.Execute(Command)

Result
    The output of the executed command.
Description
    Execute a command, returns the output of this command and
    waits for the prompt. 

### CfgEngine.print

   void CfgEngine.print(Command)

Result
    No result.
Description
    Execute a command and do not wait for the prompt. 

### CfgEngine.put

   void CfgEngine.put(Command)

Result
    No result.
Description
    Send a command without a \n and do not wait for the prompt. 

### CfgEngine.ShowScreen

   string CfgEngine.ShowScreen()

Result
    Returns the current content of the session.
Description
    Show the current content of the telnet-/ssh- session. 

### CfgEngine.DeleteScreen

   void CfgEngine.DeleteScreen()

Result
    No result.
Description
    Clear the current content of the telnet-/ssh- session. 

### CfgEngine.GetDeviceType

   string CfgEngine.GetDeviceType

Result
    Returns the detected device-type.
    HP, Cisco, Cisco-IOS-XR, Cisco WLC, H3C
Description
    Cisco............Cisco IOS or IOS-XE based device.
    Cisco-IOS-XR.....Cisco IOS-XR based device.
    Cisco WLC........Cisco WLAN Controller.
    Cisco-ASA........Cisco Adaptive Security Appliance.
	Cisco-FP.........Cisco FirePower Security Appliance.
    HP...............HP Procurve device.
    H3C..............HP A-Series or H3C device.
	Linux............Linux, Unix, ...


### CfgEngine.WaitFor

   string CfgEngine.WaitFor(string[] searchFor, bool CaseSensitive, int TimeOut)

Result
    Returns the detected content of the session.
Description
    Search for some strings and returns the result or null. 

### CfgEngine.WaitForString

   string CfgEngine.WaitForString(string searchFor)

Result
    Returns the detected content of the session.
Description
    Search for a string and returns the result or null. 

### CfgEngine.WaitForRegEx

   string CfgEngine.WaitForRegEx(string RegExp)

Result
    Returns the detected content of the session.
Description
    Search for a Regular-Expression-string and returns the result or null. 

### CfgEngine.detectNewPrompt

   string CfgEngine.detectNewPrompt()

Result
    Returns true if a new prompt was found and false if no prompt was found.
Description
    Tries to detect the new console-prompt. 

### CfgEngine.setCliUsers

    CfgEngine.setCliUsers (only for adding new Users)
     CfgEngine.setCliUsers = "NameOfUser1,NameOfUser2,Username3"

Result
    No result.
Description
    Add new CLI-Users, which are used at CfgEngine.Login.
    Important set also a Password for each added User with CfgEngine.setCliUserPwds.


### CfgEngine.setCliUserPwds

    CfgEngine.setCliUserPwds (only for adding new Passwords)
    CfgEngine.setCliUserPwds = "Pwd4User1,Pwd4User2,Pwd4Username3"

Result
    No Result.
Description
    Add new CLI-User-Passwords, which are used at CfgEngine.Login.
    Important set also a Username for each added password with CfgEngine.setCliUsers.


### CfgEngine.setCliPwds

    CfgEngine.setCliPwds (only for adding new Passwords)
     CfgEngine.setCliPwds = "cli-Pwd1,cli-Pwd2,cli-Pwd3"

Result
    No Result.
Description
    Add new CLI-Passwords, which are used at CfgEngine.Login.


### CfgEngine.setEnablePwds

    CfgEngine.setEnablePwds (only for adding new enable-Passwords)
     CfgEngine.setEnablePwds = "cli-enable-Pwd1,cli-enable-Pwd2,cli-enable-Pwd3"

Result
    No Result.
Description
    Add new CLI-enable-Passwords, which are used at CfgEngine.EnterConfigMode.


### CfgEngine.DeleteCliLogins

    CfgEngine.DeleteCliLogins (Delete all predefined CLI-Logins)

Result
    No Result.
Description
    Delete all predefined CLI-Logins, which are used at CfgEngine.Login.


### CfgEngine.DeleteCliPwds

    CfgEngine.DeleteCliPwds (Deletes all cli-Passwords)

Result
    No Result.
Description
    Delete all predefined CLI-Passwords, which are used at CfgEngine.Login.


### CfgEngine.DeleteEnablePwds

    CfgEngine.DeleteEnablePwds (Delete all enable-Passwords)

Result
    No Result.
Description
    Delete all predefined CLI-enable-Passwords, which are used at CfgEngine.EnterConfigMode.


### CfgEngine.Debug

    CfgEngine.Debug

Result
    No Result.
Description
    Set the Debug-Mode, only for testing.

### CfgEngine.Version

	CfgEngine.Version
	
Result
	Returns the version-string of this library.
Description

### CfgEngine.ConnectionStatus
### CfgEngine.GetDeviceType
### CfgEngine.KEXAlgoritmen
### CfgEngine.CiphterAlgoritmen
### CfgEngine.HostKeyAlgoritmen
### CfgEngine.MACAlgoritmen
