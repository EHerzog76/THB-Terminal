param(
    [string]$DeviceIP="",
    [string]$ConMode="ssh",
	[string]$DeviceType = "",
    [switch]
    [bool]$Debug,
	[string]$Params=""
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

$cmdArgs = "";
if($Params.Length -gt 0) {
	$cmdArgs = $Params.Split(' ');
	
	if($cmdArgs.Length -gt 0) {
		$DeviceIP = $cmdArgs[0];
	}
	
	if($cmdArgs.Length -gt 1) {
		if($cmdArgs[1].ToLower().Equals("d")) {
			$Debug = $true;
		}
	}
}

if(($DeviceIP -eq "")) {
    Write-Host "Error: DeviceIP must be defined !";
    ShowHelp;
    exit;
}

[string]$PerfReport = "";
[int]$LastTime = 0;
[int]$DeltaTime = 0;
$stopwatch =  [system.diagnostics.stopwatch]::StartNew();

#Load ConfigEngine-DLL
Add-Type -Path "C:\Progs\THB-Terminal\ConfigEngine.dll";

$LastTime = $stopwatch.Elapsed.TotalMilliseconds;
$DeltaTime = $LastTime;
$PerfReport = "1:) Loadtime of CfgEngine: $($DeltaTime)";


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

    $DeltaTime = $stopwatch.Elapsed.TotalMilliseconds;
    $DeltaTime = $DeltaTime - $LastTime;
    $PerfReport = "$PerfReport`n2:) Befor login: $($DeltaTime)";

#Device-Login
#Write-Output "Login to: $($DeviceIP)";
$LoginCount = 2;
$LoginResult = 0;
while(($LoginCount -gt 0) -and ($LoginResult -eq 0)) {
    $LoginResult = $CfgEngine.Login($DeviceIP, $ConMode, $DeviceType);
    $LoginCount = $LoginCount-1;

    if($LoginResult -eq 0) {

        Write-Output $CfgEngine.ConnectionStatus;

		if($CfgEngine.ConnectionStatus -eq "NoResponse") {
			break;
		}
	}
}
if($LoginResult -eq 0) {
    $ResultObj.Error = "Error: Login failed !";
    $ResultObj.ErrorNr = 403;
} else {
    $ResultObj.Result = $ResultObj.Result + "`nLogin to: $($DeviceIP) successfull.";

    $DeltaTime = $stopwatch.Elapsed.TotalMilliseconds;
    $DeltaTime = $DeltaTime - $LastTime;
    $PerfReport = "$PerfReport`n3:) After login: $($DeltaTime)";

    Write-Output "DeviceType: $($CfgEngine.GetDeviceType)";
    if($CfgEngine.GetDeviceType -eq "Cisco-FP") {
        $strCliResult = $CfgEngine.Execute("show failover state");
    } elseif($CfgEngine.GetDeviceType -match "^Cisco.*") {
        $strCliResult = $CfgEngine.Execute("terminal length 300");
        
        $strCliResult = $CfgEngine.Execute("show version");
    } elseif($CfgEngine.GetDeviceType -eq "HP") {
        $strCliResult = $CfgEngine.Execute("show interface brief");
    }
    Write-Output $strCliResult;

    $DeltaTime = $stopwatch.Elapsed.TotalMilliseconds;
    $DeltaTime = $DeltaTime - $LastTime;
    $PerfReport = "$PerfReport`n4:) Cmd-Executed: $($DeltaTime)";

    #$CfgEngine.EnterConfigMode();
    #$strCliResult = $CfgEngine.Execute("do show clock");
    #Write-Output $strCliResult;
    #$CfgEngine.ExitConfigMode();


    $CfgEngine.doLogout();
}
$CfgEngine.Close();
$CfgEngine = $null;

$stopwatch.Stop();

$DeltaTime = $stopwatch.Elapsed.TotalMilliseconds;
    $DeltaTime = $DeltaTime - $LastTime;
    $PerfReport = "$PerfReport`n5:) End: $($DeltaTime)";

if($LoginResult -eq 0) {
    write-output '{Result:"",Error:"Login failed.",ErrorNr:403}';
} else {
    ConvertTo-Json -InputObject $ResultObj -Depth 5;
}

Write-Output $PerfReport;
