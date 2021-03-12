param(
	[string]$Params=""
)

$cmdArgs = $Params.Split(' ');
$vpn = "";
$DeviceIP = "";
$CfgTmpl = "";
$Debug = $false;

$ResultObj = @{
    Result = "Ok"
    Error = ""
    ErrorNr =0
};

if($cmdArgs.Length -gt 2) {
    $vpn = $cmdArgs[0];
    $DeviceIP = $cmdArgs[1];
    $CfgTmpl = $cmdArgs[2];
}

if($cmdArgs.Length -gt 3) {
    if($cmdArgs[3].ToLower().Equals("d")) {
        $Debug = $true;
    }
}

function ShowHelp() {
    $scriptName = $MyInvocation.MyCommand.Name;
    Write-Host "Usage of ${scriptName}:";
    Write-Host "";
    Write-Host "    VPN-Name DeviceIP Cfg-Template";
    Write-Host "";
}

if(($vpn -eq "") -or ($DeviceIP -eq "") -or ($CfgTmpl -eq "")) {
    Write-Host "Error: vpn and DeviceIP and CfgTmpl  must be defined !";
    ShowHelp;
    exit;
}

#Load ConfigEngine-DLL
Add-Type -Path "C:\Progs\THB-Terminal\ConfigEngine.dll";

# Load Helper-functions
$HelperFunctionsDir = "C:\Scripts\HelperFunctions"
Get-ChildItem "$HelperFunctionsDir\*.ps1" | %{.$_}

function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath;
}

$AppPath = Get-ScriptDirectory;
$CfgFile = "C:\Scripts\Network-Inventory\ConfigEngine.cfg";
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();  #New-Object ConfigEngine.ConfigEngine;
$CfgEngine.readConfig($CfgFile);
if($Debug -eq $true) {
    $CfgEngine.Debug = 1;

    Write-Output "CfgEngine-Version: $($CfgEngine.Version)";
} else {
    $CfgEngine.Debug = 0;
}

[String]$strCfg = "";
[String]$strCliResult = "";

$CfgTmplFile = "$($AppPath)$($CfgTmpl)";
if([System.IO.File]::Exists($CfgTmplFile) -eq $false) {
    $ResultObj.Result = "Config-Template: $($CfgTmpl) dose not exists!";
    $ResultObj.Error = "Config-Template: $($CfgTmpl) dose not exists!";
    $ResultObj.ErrorNr = 404;

    ConvertTo-Json -InputObject $ResultObj -Depth 5;
    exit(1);
}


$conMode = "ssh";
    $ModeCheck = tcpping $DeviceIP 22;
    if($ModeCheck.PortOpen -eq $true) {
        #Write-Host "SSH: ${ModeCheck}";
        $conMode = "ssh";
    } else {
        $ModeCheck = tcpping $DeviceIP 23;
        if($ModeCheck.PortOpen -eq $true) {
            #Write-Host "Telnet: ${ModeCheck}";
            $conMode = "telnet";
        }
    }

    #No Response from Device
    if($ModeCheck.PortOpen -eq $false) {
        $ResultObj.Result = $ResultObj.Result + "`nNo Connection to: $($DeviceIP)";

        ConvertTo-Json -InputObject $ResultObj -Depth 5;
        exit(0);
    }

#Device-Login
#Write-Output "Login to: $($DeviceIP)";
$LoginCount = 5;
$LoginResult = 0;
while(($LoginCount -gt 0) -and ($LoginResult -eq 0)) {
    $LoginResult = $CfgEngine.Login($DeviceIP, $conMode);
    $LoginCount = $LoginCount-1;
}
try {
if($LoginResult -eq 0) {
    $ResultObj.Error = "Error: Login failed !";
    $ResultObj.ErrorNr = 403;
} else {
    $ResultObj.Result = $ResultObj.Result + "`nLogin to: $($DeviceIP) successfull.";

    $strCliResult = $CfgEngine.Execute("terminal length 300");

    #Get-Content
    foreach ($strCfg in [System.IO.File]::ReadLines($CfgTmplFile)) {
        if($strCfg.StartsWith("#Config") -or $strCfg.StartsWith("#config")) {
            $CfgEngine.EnterConfigMode();
        } elseif($strCfg.StartsWith("#ExitConfig") -or $strCfg.StartsWith("#exitconfig")) {
            $CfgEngine.ExitConfigMode();
        } else {
            $strCliResult = $CfgEngine.Execute($strCfg);

            $ResultObj.Result = $ResultObj.Result + "`n$($strCliResult).";
        }
    }

    $CfgEngine.doLogout();
}
$CfgEngine.Close();
} catch {
    Write-Host $_.Exception.Message;
}
$CfgEngine = $null;

if($LoginResult -eq 0) {
    write-output '{Result:"",Error:"Login failed.",ErrorNr:403}';
} else {
    ConvertTo-Json -InputObject $ResultObj -Depth 5;
}
