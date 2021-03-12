param(
    [string]$DeviceIP="",
    [string]$TFTServer="10.20.30.40",
    [string]$RootDir="/HP-Procurve",
    [switch]
    [bool]$withReboot,
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

function GetFirmware([string]$strType) {
[string]$strOS = "";

    $FirmwareLookup = @{
        J9085A="2610-R_11_122.swi";
        J9086A="2610-R_11_122.swi";
        J9087A="2610-R_11_122.swi";
        J9088A="2610-R_11_122.swi";
        J9089A="2610-R_11_122.swi";
        J4899A="2600-H_10_119.swi";
        J4899B="2600-H_10_119.swi";
        J4899C="2600-H_10_119.swi";
        J4900A="2600-H_10_119.swi";
        J4900B="2600-H_10_119.swi";
        J4900C="2600-H_10_119.swi";
        J8164A="2600-H_10_119.swi";
        J8165A="2600-H_10_119.swi";
        J8762A="2600-H_10_119.swi";
        J9623A="RA_15_17_0013.swi;RA_16_02_0028.swi";
        J9020A="U_11_66.swi";
	    J9019B="Q_11_78.swi"
        }
    
    if($FirmwareLookup.ContainsKey($strType)) {
        $strOS = $FirmwareLookup[$strType];
    }

    return($strOS);
}

function GetFirmwareVersion([string]$strType) {
[string]$strVer = "";

    $FirmwareVerLookup = @{
        J9085A="R.11.122";
        J9086A="R.11.122";
        J9087A="R.11.122";
        J9088A="R.11.122";
        J9089A="R.11.122";
        J4899A="H.10.119";
        J4899B="H.10.119";
        J4899C="H.10.119";
        J4900A="H.10.119";
        J4900B="H.10.119";
        J4900C="H.10.119";
        J8164A="H.10.119";
        J8165A="H.10.119";
        J8762A="H.10.119";
        J9623A="RA.15.17.0013;RA.16.02.0028";
        J9020A="U.11.66";
        J9019B="Q.11.78"
    }
    
    if($FirmwareVerLookup.ContainsKey($strType)) {
        $strVer = $FirmwareVerLookup[$strType];
    }

    return($strVer);
}

function getSwitchOS() {
[string]$l_BootLoc = "";
$retObj = [PSCustomObject]@{
    Firmware1 = ""
    Firmware2 = ""
}

    $strCliResult = $CfgEngine.Execute("show flash");
    $CliLines = $strCliResult.Split("`n");
    foreach($strLine in $CliLines) {
        if($strLine -match "^Default Boot.*") {
            $l_strArray = $strLine.Split(":");

            if($l_strArray.Length -gt 1) {
                $l_BootLoc = $l_strArray[1].Trim();
            }
        } elseif($strLine -match "^Boot ROM Version.*") {
            $l_strArray = $strLine.Split(":");

            if($l_strArray.Length -gt 1) {
                $l_strTmp = $l_strArray[1].Trim();
            }
        } elseif($strLine -match "^Primary Image.*") {
            $l_strArray = $strLine.Split(":");

            if($l_strArray.Length -gt 1) {
                #e.g.:   8829329 08/09/11 RA.15.05.0006
                $l_strArray1 = $l_strArray[1].Trim().Split(" ");
                if($l_strArray1.Length -gt 2) {
                    $retObj.Firmware1 = $l_strArray1[$l_strArray1.Length-1];
                }
            }
        } elseif($strLine -match "^Secondary Image.*") {
            $l_strArray = $strLine.Split(":");

            if($l_strArray.Length -gt 1) {
                #e.g.:   8829329 08/09/11 RA.15.05.0006
                $l_strArray2 = $l_strArray[1].Trim().Split(" ");
                if($l_strArray2.Length -gt 2) {
                    $retObj.Firmware2 = $l_strArray2[$l_strArray2.Length-1];
                } 
            }
        } else {

        }
    }

    return($retObj);
}

function getVersion([string]$strOS) {
    $strVer = $null;

    if($strOS -ne $null) {
        $strVer = $strOS.Split(" ");

        if($strVer.Length -eq 3) {
            return($strVer[2]);
        }
    } else {

    }

    return("");
}

function cmpVersions([string]$src, [string]$dst) {
[int]$x = 0;
[int]$iSrcVer = 0;
[int]$iDstVer = 0;
    $verResult = 0;
    $dstArray = $dst.Split(";");

    for($v=0; $v -lt $dstArray.Length; $v++) {
        $dstVer = $dstArray[$v].Split(".");
        $srcVer = $src.Split(".");

        if($srcVer[0] -ne $dstVer[0]) {
            return(-1);
        }

        if($srcVer.Length -ne $dstVer.Length) {
            return(-1);
        }

        for($x=1; $x -lt $srcVer.Length; $x++) {
            if([int]::TryParse($srcVer[$x], [ref] $iSrcVer)) {
                if([int]::TryParse($dstVer[$x], [ref] $iDstVer)) {

                    if($iSrcVer -lt $iDstVer) {
                        $verResult = 2;
                        return($verResult);
                    } elseif($iSrcVer -gt $iDstVer) {
                        $verResult = 1;
                        return($verResult);
                    } else {
                        $verResult = 0;
                    }
                } else {
                    return(-2);
                }
            } else {
                return(-2);
            }
        }
    }
    return($verResult);
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
Get-ChildItem "$HelperFunctionsDir\*.ps1" | ForEach-Object{.$_}

function Get-ScriptDirectory {
    Split-Path -parent $PSCommandPath;
}

#$AppPath = Get-ScriptDirectory;
$CfgFile = "C:\Scripts\Network-Inventory\ConfigEngine.cfg";
[ConfigEngine.ConfigEngine]$CfgEngine = [ConfigEngine.ConfigEngine]::new();
$CfgEngine.readConfig($CfgFile);
if($Debug -eq $true) {
    $CfgEngine.Debug = 1;
} else {
    $CfgEngine.Debug = 0;
}

[String]$strCliResult = "";
[String]$strDeviceType = "";
[String]$strFirmware = "";
[string]$strPath = "";
[String]$Firmware1 = "";
[String]$Firmware2 = "";
[String]$BootLoc = "";
[String]$strTmp = "";
[String]$strCmd = "";
[String]$strVersion = "";
[bool]$bDoNext = $false;
$strArray = $null;
$strArray1 = $null;
$strArray2 = $null;
$CliLines = $null;
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
    if($CfgEngine.GetDeviceType -match "^Cisco$") {
        $strCliResult = $CfgEngine.Execute("terminal length 300");
        
    } elseif($CfgEngine.GetDeviceType -eq "HP") {
        $strCliResult = $CfgEngine.Execute("show run");
        $CliLines = $strCliResult.Split("`n");
        foreach($strLine in $CliLines) {
            if($strLine -match "; J[0-9A-Z]+ Configuration Editor;.*") {
                $strArray = $strLine.Split(" ");

                if(($null -ne $strArray) -and ($strArray.Length -gt 1)) {
                    $strDeviceType = $strArray[1].Trim();
                }
                break;
            }
        }

        if($strDeviceType -eq "") {
            $ResultObj.Result = $ResultObj.Result + "`n$($DeviceIP): unknown Device-Type.";
            $ResultObj.Error = "Error: Device-Type is unknown !";
            $ResultObj.ErrorNr = 404;
        } else {
            if($Debug) {
                Write-Host "Device-Type: $($strDeviceType).";
            }
            $strFirmware = GetFirmware $strDeviceType;
            if($strFirmware -eq "") {
                $ResultObj.Result = $ResultObj.Result + "`n$($DeviceIP): No Firmeware for Device-Type.";
                $ResultObj.Error = "Error: No Firmeware for Device-Type: $($strDeviceType) !";
                $ResultObj.ErrorNr = 404;
            }
        }

        if($strFirmware -ne "") {
            $OSVersions = getSwitchOS;

            $strVersion = GetFirmwareVersion($strDeviceType);

            $cmpVer1 = cmpVersions -src $OSVersions.Firmware1 -dst $strVersion;
            $cmpVer2 = cmpVersions -src $OSVersions.Firmware2 -dst $strVersion;
            if($Debug) {
                Write-Host "Firmware-Compare-1: $($cmpVer1)";
                Write-Host "Firmware-Compare-2: $($cmpVer2)";
            }

            $bDoNext = $false;

            #Check only Primary-Image
            if($cmpVer1 -eq 2) {
                $bDoNext = $true;
            }

            if($bDoNext -eq $false) {
                if($cmpVer1 -eq 0) {
                    $ResultObj.Result = $ResultObj.Result + "`nFirmware is already OK.";
                    $ResultObj.Error = "";
                    $ResultObj.ErrorNr = 0;

                    if($withReboot) {
                        $strCliResult = $CfgEngine.Execute("show flash");
                        if($strCliResult.Contains($strVersion) -eq $false) {
                            $bDoNext = $true;
                        }
                    }
                } elseif ($cmpVer1 -lt 0) {
                    $ResultObj.Result = $ResultObj.Result + "`nFirmware-Update stoped !";
                    $ResultObj.Error = "Error: Firmware-Update stoped !";
                    $ResultObj.ErrorNr = 404;
                } elseif($cmpVer1 -eq 1) {
                    $ResultObj.Result = $ResultObj.Result + "`nFirmware is newer and OK.";
                    $ResultObj.Error = "";
                    $ResultObj.ErrorNr = 0;
                }
            } else {
                if($RootDir -eq "") {
                    $strPath = "/";
                } elseif($RootDir.EndsWith("/")) {
                    $strPath = $RootDir;
                } else {
                    $strPath = $RootDir + "/";
                }
                if($strPath.StartsWith("/")) {

                } else {
                    $strPath = "/" + $strPath;
                }
                $strCmd = "copy tftp flash $($TFTServer) $($strPath)$($strFirmware) primary";
                Write-Host "$($strCmd)";
                $CfgEngine.DeleteScreen();
                $CfgEngine.print($strCmd);
                $bDoNext = $true;
                $strCliResult = $CfgEngine.ShowScreen();
                ###The Primary OS Image will be deleted, continue [y/n]?   => y without [Enter]
                if($strCliResult -cmatch ".*\[[y/n]\]") {
                    $CfgEngine.print("y");
                } elseif($strCliResult -cmatch ".*\[[Y/N]\]") {
                    $CfgEngine.print("Y");
                } elseif($strCliResult -cmatch ".*\[(yes|no)\/(yes|no)\]") {
                    $CfgEngine.print("yes");
                } elseif($strCliResult -cmatch ".*\[(Yes|No)\/(Yes|No)\]") {
                    $CfgEngine.print("Yes");
                } else {
                    $bDoNext = $false;
                }
            }

            if($bDoNext) {
                ###0123K
                ###          {When ready: Screen will be cleared}
                ### Validating and Writing System Software to FLASH...
                ###Post-PF1006-SW02#
                $CfgEngine.DeleteScreen();
                $strWaits = @("#");
                $bDoNext = $false;

                #ToDo:
                #   Check for Max-Waittime !!!
                #   Screen:   Validating and Writing System Software to FLASH
                while($bDoNext -eq $false) {
                    $CfgEngine.DeleteScreen();
                    $strCliResult = $CfgEngine.Execute("!");
                    if($null -eq $strCliResult) {
                        if($Debug) {
                            Write-Host "Waiting for Firmeware-Download...";
                        }
                    } else {
                        $bDoNext = $true;
                        if($Debug) {
                            Write-Host "TRACE: $($strCliResult)";
                        }
                    }
                }

                $OSVersions = getSwitchOS;
                if($Debug) {
                    Write-Host "TRACE: $($OSVersions.Firmware1) / $($OSVersions.Firmware2)";    #$strFirmware
                }
            }

            $CfgEngine.Execute("wr memory");
            $bDoNext = $true;
            if($withReboot) {
                $CfgEngine.DeleteScreen();
                $CfgEngine.print("reload");
                ###Device will be rebooted, do you want to continue [y/n]?  y
                $CfgEngine.DeleteScreen();
                $CfgEngine.print("y");
                #
                ###Do you want to save current configuration [y/n]?  y
                $strWaits = @("[y/n]");
                $bDoNext = $CfgEngine.WaitFor($strWaits, $true, 2);
                if($bDoNext) {
                    $CfgEngine.print("y");
                }
                $ResultObj.Result = $ResultObj.Result + "`n$Rebooted.";
            }

            $ResultObj.Result = $ResultObj.Result + "`n$($DeviceIP): OK.";
            $ResultObj.Error = "";
            $ResultObj.ErrorNr = 0;
        }
    }
    
    if(($withReboot -eq $false) -or ($bDoNext -eq $false)) {
        #$CfgEngine.EnterConfigMode();
        #$CfgEngine.ExitConfigMode();
        $CfgEngine.doLogout();
    }
}
$CfgEngine.Close();
} catch {
    Write-Host $_.Exception.Message;
}

$CfgEngine = $null;

#Write-Host 'CfgEngine before Close - Press any key to continue...';
#$k = [System.Console]::ReadKey();

if($LoginResult -eq 0) {
    write-output '{Result:"",Error:"Login failed.",ErrorNr:403}';
} else {
    ConvertTo-Json -InputObject $ResultObj -Depth 5;
}
