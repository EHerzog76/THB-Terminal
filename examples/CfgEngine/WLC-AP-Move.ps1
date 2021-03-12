param(
	[string]$Params=""
)

$cmdArgs = $Params.Split(' ');
$vpn = "";
$DeviceIP = "";
$APFilter = "";

$ResultObj = @{
    Result = "Ok"
    Error = ""
    ErrorNr =0
};

if($cmdArgs.Length -gt 1) {
    $vpn = $cmdArgs[0];
    $DeviceIP = $cmdArgs[1];
}
if($cmdArgs.Length -gt 2) {
    $APFilter = $cmdArgs[2];
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
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();  #New-Object ConfigEngine.ConfigEngine;
$CfgEngine.readConfig($CfgFile);
$CfgEngine.Debug = 0;

[String]$strCliResult = "";
[String]$strAPCfg = "";
[String]$IPKey = "";
$APsByIPNet = New-Object "System.Collections.Generic.Dictionary[[string],[System.Collections.ArrayList]]";
[System.Collections.ArrayList]$APSublist = $null;
$IPOctets = $null;
[String]$APMac = "";
[String]$APIP = "";
[String]$APName = "";
$reAP = new-object System.Text.RegularExpressions.RegEx('^(?<APName>[^ ]+) +[0-9]+ +(?<Type>[^ ]+) +(?<MAC>[0-9a-z:]+) +.*[ ][ ]+(?<Region>[A-Z][A-Z]) +(?<IP>[0-9\.]+) +.*$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);


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
$LoginCount = 2;
$LoginResult = 0;
while(($LoginCount -gt 0) -and ($LoginResult -eq 0)) {
    $LoginResult = $CfgEngine.Login($DeviceIP, $conMode);
    $LoginCount = $LoginCount-1;
}
$CfgEngine.Debug = 0;
try {
if($LoginResult -eq 0) {
    $ResultObj.Error = "Error: Login failed !";
    $ResultObj.ErrorNr = 403;
} else {
    #$strCliResult = $CfgEngine.Execute("config paging disable");
    $strCliResult = $CfgEngine.Execute("show ap summary");
    $strResults = $strCliResult.Split("`n");

Write-Host "Lines: $($strResults.Length)";
    $parsingStart = 0;
    for($i=0; $i -lt $strResults.Length; $i++) {
        if($parsingStart -eq 0) {
            if($strResults[$i].StartsWith("----")) {
                $parsingStart = 1;
            }
        } else {
            $APMatches = $reAP.Match($strResults[$i]);
            if(($APMatches) -and ($APMatches.Groups.Count -gt 5)) {
                $APMac = $APMatches.Groups[3].Value; 
                $APIP = $APMatches.Groups[5].Value;
                #$IPOctets = $APIP.Split(".");
                #$IPKey = "$($IPOctets[0]).$($IPOctets[1]).$($IPOctets[2])";
                #if($APsByIPNet.ContainsKey($IPKey) -eq $false) {
                #    $APsByIPNet.Add($IPKey, [System.Collections.ArrayList]::new());
                #}

                #$APsByIPNet[$IPKey].Add($APMatches.Groups[1].Value);
                $APName = $APMatches.Groups[1].Value;
                if($APName -like $APFilter) {
                    Write-Host "$($APName) with IP:  $($APIP)";

                    $strAPCfg = $CfgEngine.Execute("show ap config general $($APName)");
                    [System.IO.File]::WriteAllText("C:\NetInvData\$($vpn)\$($DeviceIP)\$($APName).cfg", $strAPCfg);
                }
            }
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