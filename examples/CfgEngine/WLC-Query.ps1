param(
	[string]$Params=""
)

$cmdArgs = $Params.Split(' ');
$vpn = "";
$DeviceIP = "";
$APImageVer = "8.8.125.0";

$ResultObj = @{
    Result = "Ok"
    Error = ""
    ErrorNr =0
};

if($cmdArgs.Length -gt 1) {
    $vpn = $cmdArgs[0];
    $DeviceIP = $cmdArgs[1];
}

function ShowHelp() {
    $scriptName = $MyInvocation.MyCommand.Name;
    Write-Host "Usage of ${scriptName}:";
    Write-Host "";
    Write-Host "    VPN-Name DeviceIP Cfg-Template";
    Write-Host "";
}

if(($vpn -eq "") -or ($DeviceIP -eq "")) {
    Write-Host "Error: vpn and DeviceIP  must be defined !";
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
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();  #New-Object ConfigEngine.ConfigEngine;
$CfgEngine.readConfig($CfgFile);
$CfgEngine.Debug = 0;

[String]$strCliResult = "";
[String]$IPKey = "";
$APsByIPNet = New-Object "System.Collections.Generic.Dictionary[[string],[System.Collections.ArrayList]]";
[System.Collections.ArrayList]$APSublist = $null;
$IPOctets = $null;
[String]$APName = "";

###[System.Text.RegularExpressions.Regex]
#$re = New-Object System.Text.RegularExpressions.RegEx("a.")
#$regex = new-object System.Text.RegularExpressions.RegEx('^\s*GO\s*$', ([System.Text.RegularExpressions.RegexOptions]::MultiLine -bor [System.Text.RegularExpressions.RegexOptions]::IgnoreCase));
$reAP = new-object System.Text.RegularExpressions.RegEx('^(?<APName>[^ ]+) +[0-9]+ +(?<Type>[^ ]+) +(?<MAC>[0-9a-z:]+) +.*[ ][ ]+(?<Region>[A-Z][A-Z]) +(?<IP>[0-9\.]+) +.*$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);
$reAPImage = new-object System.Text.RegularExpressions.RegEx('^(?<APName>[^ ]+) +(?<PImage>[0-9\.]+) +(?<BImage>[0-9\.]+) +(?<End>.*)$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);
$rePreDLCount = new-object System.Text.RegularExpressions.RegEx('^(?:  |\t)+predownloading[\.]+ +([0-9]+) *$', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase);

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
                Write-Host "$($APMatches.Groups[1].Value) / $($APMatches.Groups[5].Value)";
                $IPOctets = $APMatches.Groups[5].Value.Split(".");
                $IPKey = "$($IPOctets[0]).$($IPOctets[1]).$($IPOctets[2])";
                if($APsByIPNet.ContainsKey($IPKey) -eq $false) {
                    $APsByIPNet.Add($IPKey, [System.Collections.ArrayList]::new());
                }
                #$APSublist = $APsByIPNet[$IPKey];
                #$APSublist.Add($APMatches.Groups[1]);
                $APsByIPNet[$IPKey].Add($APMatches.Groups[1].Value);
                Write-Host "$($APMatches.Groups[1].Value)  added to $($IPKey)";
            }
        }
    }

    #
    # Do Work per AP and only 1AP/Subnet
    #
    $APCount = 0;
    $MaxAPCount = 1;
    $HaveAPs = $true;
    #
    #Run Until all APs are proccessed
    while($HaveAPs -eq $true) {
        $HaveAPs = $false;

        #Run 1AP for each /24 IP-SubNet
        [String[]]$IPNetKeys = (1..$APsByIPNet.Keys.Count);
        $APsByIPNet.Keys.CopyTo($IPNetKeys, 0);
        foreach($IPKey in $IPNetKeys) {
            $APSublist = $APsByIPNet[$IPKey];
            if($APSublist.Count -gt 0){
                $HaveAPs = $true;
            }

            $APName = $APSublist[$APSublist.Count-1];

            #Check AP-Image
            $strCliResult = $CfgEngine.Execute("show ap image $($APName)");
            if(!$strCliResult) {
                Write-Host "Failed: show ap image $($APName)";
                continue;
            }
            $strResults = $strCliResult.Split("`n");

            $parsingStart = 0;
            for($i=0; $i -lt $strResults.Length; $i++) {
                if($parsingStart -eq 0) {
                    if($strResults[$i].StartsWith("----")) {
                        $parsingStart = 1;
                    }
                } else {
                    $APMatches = $reAPImage.Match($strResults[$i]);
                    if(($APMatches) -and ($APMatches.Groups.Count -gt 3)) {
                        Write-Host "$($APMatches.Groups[1].Value): Primary:$($APMatches.Groups[2].Value)  Backup:$($APMatches.Groups[3].Value)";

                        # Check Backup- & Pre-Image
                        if(($($APMatches.Groups[2].Value) -eq $APImageVer) -or ($($APMatches.Groups[3].Value) -eq $APImageVer)) {

                        } else {
                            $strCliResult = $CfgEngine.Execute("config ap image predownload primary $($APName)");
                            Write-Host "$($APName) start Image download: $($strCliResult)";
                            $APCount = $APCount +1;

                            while($APCount -gt $MaxAPCount) {
                                #Wait for Downloads
                                $strCliResult = $CfgEngine.Execute("show ap image $($APName)");
                                $strResults = $strCliResult.Split("`n");
                                for($r=0; $r -lt $strResults.Length; $r++) {
                                    if($strResults[$r].Contains("Predownloading")) {
                                        $DLCountMatch = $rePreDLCount.Match($strResults[$r]);
                                        if(($DLCountMatch) -and ($DLCountMatch.Groups.Count -gt 1)) {
                                            break;
                                        }
                                    }
                                }
                                if(($DLCountMatch) -and ($DLCountMatch.Groups.Count -gt 1)) {
                                    $APCount = $DLCountMatch.Groups[1].Value;
                                }

                                if($APCount -gt ($MaxAPCount-1)) {
                                    Start-Sleep -Seconds 3;
                                }
                            }
                        }
                        $APSublist.Remove($APName);
                        $APsByIPNet[$IPKey] = $APSublist;

                        break;
                    }
                }
            }
        }
    }
    #$strCliResult = $CfgEngine.Execute("config paging enable");
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
