param(
    [string]$DeviceIP="",
    [switch]
    [bool]$Debug
)

$ResultObj = @{
    Result = "Ok"
    Error = ""
    ErrorNr =0
};

function ShowHelp() {
    $scriptName = $MyInvocation.MyCommand.Name;
    Write-Host "Usage of ${scriptName}:";
    Write-Host "";
    Write-Host "    DeviceIP Debug";
    Write-Host "";
}

if(($DeviceIP -eq "")) {
    Write-Host "Error: DeviceIP must be defined !";
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

#$AppPath = Get-ScriptDirectory;
$CfgFile = "C:\Scripts\Network-Inventory\ConfigEngine.cfg";
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();
$CfgEngine.readConfig($CfgFile);
if($Debug -eq $true) {
    $CfgEngine.Debug = 1;

    Write-Output "CfgEngine-Version: $($CfgEngine.Version)";
} else {
    $CfgEngine.Debug = 0;
}

[String]$strCfg = "";
[String]$strCliResult = "";

#$PingResult = Get-WmiObject -Class Win32_PingStatus -Filter "Address='$($DeviceIP)' and timeout=1000" | Select-Object -Property Address, StatusCode, ResponseTime, ReplySize;

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

    Write-Output "DeviceType: $($CfgEngine.GetDeviceType)";
    if($CfgEngine.GetDeviceType -eq "Cisco-FP") {
        $strCliResult = $CfgEngine.Execute("show failover state");
    } elseif($CfgEngine.GetDeviceType -match "^Cisco.*") {
        $strCliResult = $CfgEngine.Execute("terminal length 300");
        
        $strCliResult = $CfgEngine.Execute("show version");
    } elseif($CfgEngine.GetDeviceType -eq "HP") {
        $strCliResult = $CfgEngine.Execute("show interface brief");

        foreach($strLine in $strCliResult.Split("`n")) {
            if($strLine -match "^[ ]+49[ ]+") {
                Write-Output "Found IF-49...$($strLine)";

                break;
            }
        }
    }
    Write-Output $strCliResult;

    
    #$CfgEngine.EnterConfigMode();
    #$strCliResult = $CfgEngine.Execute("do show clock");
    #Write-Output $strCliResult;
    #$CfgEngine.ExitConfigMode();


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
