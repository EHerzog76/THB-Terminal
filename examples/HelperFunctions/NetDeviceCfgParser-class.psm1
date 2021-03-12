class NetIPClass {
    [String]$ip;
    [String]$mask;
    [Bool]$Secondary;    
}
class NetVLANClass {
    [String]$VlanID;
    [String]$Name;
    [System.Collections.ArrayList]$IPs;  #Collections.Generic.List
    
    NetVLANClass() {
        $this.IPs = New-Object System.Collections.ArrayList;      # @( @{ ip mask Secondary } )
    }
}
class NetIFClass {
    [String]$Name;
    [String]$Descr;
    [System.Collections.ArrayList]$IPs;
    [String]$vrf;
    [String]$IPHelper;
    [System.Collections.ArrayList]$HSRPIPs;
    [String]$VlanID;
    [Boolean]$TrunkMode;
    [String]$TrunkVLANs;

    NetIFClass() {
        $this.IPs = New-Object System.Collections.ArrayList;     #@( @{ ip mask Secondary } )
        $this.HSRPIPs = New-Object System.Collections.ArrayList;
    }
}
class NetACLClass {
    [String]$ACLName;
    [String]$ACLCfgEntry;   #ip access-list standard|extended ACLName
    [String]$ACLCfgAsString;
}
class NetDeviceCfgClass {
    [String]$Name;
    [String]$Location;
    [String]$Descr;
    [String]$VTYAcls;
    [String]$VTYaaaName;
    [System.Collections.ArrayList]$IFs;
    [System.Collections.ArrayList]$ACLs;
    [System.Collections.ArrayList]$VLANs;

    NetDeviceCfgClass() {
        $this.IFs = New-Object System.Collections.ArrayList;
        $this.ACLs = New-Object System.Collections.ArrayList;
        $this.VLANs = New-Object System.Collections.ArrayList;
    }
}

class NetCfgParserClass {
    [bool]$Debug;

    NetCfgParserClass() {
        $this.Debug = $false;
    }

    [String] IPv4Bits2Mask([Int] $MaskBits) {
        <#
        .SYNOPSIS
        Converts a number of bits (0-32) to an IPv4 network mask string (e.g., "255.255.255.0").
      
        .DESCRIPTION
        Converts a number of bits (0-32) to an IPv4 network mask string (e.g., "255.255.255.0").
      
        .PARAMETER MaskBits
        Specifies the number of bits in the mask.
        #>
        $mask = ([Math]::Pow(2, $MaskBits) - 1) * [Math]::Pow(2, (32 - $MaskBits));
        $bytes = [BitConverter]::GetBytes([UInt32] $mask);
        $strIPv4Mask = (($bytes.Count - 1)..0 | ForEach-Object { [String] $bytes[$_] }) -join ".";

        return($strIPv4Mask);
    }

    [void] HPSetVlan2IF([String]$VLAN, [String]$IFList, [ref]$SwitchIFs) {
        <# param (
            [String]$VLAN="",
            [String]$IFList="",
            [ref]$SwitchIFs
        ) #>
        
        $IFs = $this.parseIFList($IFList);
        $ifArray = $IFs.Split(",");
        foreach($strIF in $ifArray) {
            $bIFFound = $false;
            foreach($IFObj in $SwitchIFs.Value.IFs) {
                if($IFObj.Name -eq $strIF) {
                    $IFObj.VlanID = $VLAN;
                    $bIFFound = $true;
                    break;
                }
            }
            if($bIFFound -ne $true) {
                #$IFcfg = @{ Name = $strIF Descr = "" IPs = "" vrf = "" IPHelper = "" HSRPIPs = New-Object System.Collections.ArrayList VlanID = $VLAN TrunkMode = $false TrunkVLANs = "" }
                $IFcfg = [NetIFClass]::new();
                $IFcfg.Name = $strIF;
                $IFcfg.Descr = "";
                $IFcfg.IPHelper = "";
                $IFcfg.vrf = "";
                $IFcfg.VlanID = $VLAN;
                $IFcfg.TrunkMode = $false;
                $IFcfg.TrunkVLANs = "";
                $SwitchIFs.Value.IFs.Add($IFcfg);
            }
        }
    }
    
    [void] HPSetTrunkVlan2IF([String]$VLAN, [String]$IFList, [ref]$SwitchIFs) {
        <# param (
            [String]$VLAN="",
            [String]$IFList="",
            [ref]$SwitchIFs
        ) #>
    
        $IFs = $this.parseIFList($IFList);
        $ifArray = $IFs.Split(",");
        foreach($strIF in $ifArray) {
            $bIFFound = $false;
            foreach($IFObj in $SwitchIFs.Value.IFs) {
                if($IFObj.Name -eq $strIF) {
                    $IFObj.TrunkMode = $true;
                    if($IFObj.TrunkVLANs -ne "") {
                        $IFObj.TrunkVLANs = $IFObj.TrunkVLANs + ",";
                    }
                    $IFObj.TrunkVLANs = $IFObj.TrunkVLANs + $VLAN;
                    $bIFFound = $true;
                    break;
                }
            }
            if($bIFFound -ne $true) {
                #$IFcfg = @{ Name = $strIF Descr = "" IPs = "" vrf = "" IPHelper = "" HSRPIPs = New-Object System.Collections.ArrayList VlanID = 0 TrunkMode = $true TrunkVLANs = $VLAN }
                $IFcfg = [NetIFClass]::new();
                $IFcfg.Name = $strIF;
                $IFcfg.Descr = "";
                $IFcfg.IPHelper = "";
                $IFcfg.vrf = "";
                $IFcfg.VlanID = 0;
                $IFcfg.TrunkMode = $true;
                $IFcfg.TrunkVLANs = $VLAN;
                $SwitchIFs.Value.IFs.Add($IFcfg);
            }
        }
    }
    
    [String] parseIFList([String]$IFList) {
        <# param (
            [String]$IFList=""
        ) #>
        
        $FullIFList = "";
        $IFArray = $IFList.Split(",");
        foreach($strIF in $IFArray) {
            if($strIF.IndexOf("-") -gt 0) {
                $subIFRange = $strIF.Split("-");
                $p = [int]::Parse($subIFRange[0]);
                $pMax = [int]::Parse($subIFRange[1]);
                for($p; $p -le $pMax; $p++) {
                    if($FullIFList -ne "") {
                        $FullIFList = $FullIFList + ",";
                    }
                    $FullIFList = $FullIFList + $p; 
                }
            } else {
                if($FullIFList -ne "") {
                    $FullIFList = $FullIFList + ",";
                }
                $FullIFList = $FullIFList + $strIF;
            }
        }
    
        return $FullIFList;
    }

    [NetDeviceCfgClass] cfgParserCisco([String[]]$strConfig) {
        #param (
        #    [String[]]$strConfig=@()
        #)
        
            #[NetCfgParserClass]$CfgParser = [NetCfgParserClass]::new();
            [NetDeviceCfgClass]$objCfg = [NetDeviceCfgClass]::new();  # @{ Name = ""; IFs = New-Object System.Collections.ArrayList; ACLs = New-Object System.Collections.ArrayList; }
            [NetIFClass]$IFcfg = $null;
            [NetACLClass]$ACLcfg = $null;
            $strCfgTmp = "";
            $cfgSection = "";   #if, vlan, vty, acl
            $IFName = "";
            #$Descr = ""; $vrf = ""; $VlanID = ""; $Trunk = $false;
            #$IPs = New-Object System.Collections.ArrayList;
            [NetIPClass]$objIP = $null;
            $strIP = "";
            $strMask = "";
            $isSecondaryIP = $false;
            $IPHelper = "";
            $TrunkList = "";
            $TrunkListNot = "";
            $VlanNative = "";
            $VlanAccess = "";
        
            for($i=0; $i -lt $strConfig.Length; $i++) {
                if($strConfig[$i].ToLower().StartsWith("interface ") -or ($cfgSection.Equals("if") -and ($strConfig[$i].Trim().Equals("!") -or ($strConfig[$i] -match "^[^ ].*") -or ($strConfig[$i] -match "^[ ]*$") ) )) {
                    if($IFName -ne "") {
                        if($this.Debug -eq $true) {
                            Write-Host "Saving IF: $($IFName)";
                        }
                        #Save parsed Data
                        <# @{ Name = $IFName Descr = $Descr IPs = $IPs vrf = $vrf IPHelper = $IPHelper
                            HSRPIPs = New-Object System.Collections.ArrayList
                            VlanID = $VlanID Trunk = $Trunk TrunkVLANs = $TrunkList TrunkVLANsNot = $TrunkListNot
                        } #>
                        $IFcfg.IPHelper = $IPHelper;
                        $IFcfg.TrunkVLANs = $TrunkList;
                        #$IFcfg.TrunkVLANsNot = $TrunkListNot;
                        if($IFcfg.TrunkMode) {
                            $IFcfg.VlanID = $VlanNative;
                        } else {
                            $IFcfg.VlanID = $VlanAccess;
                        }
                        $objCfg.IFs.Add($IFcfg);
        
                        #Reset Vars
                        #$IPs = New-Object System.Collections.ArrayList;   #$IPs.Clear();
                        #$IFName = ""; $vrf = ""; $Descr = ""; $VlanID = ""; $Trunk = $false;
                        $IFName = "";
                        $IPHelper = "";
                        $TrunkList = "";
                        $TrunkListNot = "";
                        $VlanNative = "";
                        $VlanAccess = "";
                        $IFcfg = $null;
                    }
                    if($cfgSection.Equals("if") -and ($strConfig[$i].Trim().Equals("!") -or ($strConfig[$i] -match "^[^ ].*") -or ($strConfig[$i] -match "^[ ]*$") ) )
                    {
                        $cfgSection = "";
                        $IFcfg = $null;
                        if($strConfig[$i].ToLower().StartsWith("interface ") -eq $false) {
                            continue;
                        }
                    }
                    if($strConfig[$i].Length -lt 4) {
                        continue;
                    }
        
                    $cfgSection = "if";
                    $IFName = $strConfig[$i].Substring(10);
                    $IFcfg = [NetIFClass]::new();
                    $IFcfg.Name = $IFName;
                    $VlanNative = $VlanAccess = 1;  #Default VLAN
                    $IFcfg.TrunkMode = $false;      #Default TrunkMode

                    if($this.Debug -eq $true) {
                        Write-Host "Found IF: $($IFName)";
                    }
                } elseif($cfgSection.Equals("if")) {
                    if($strConfig[$i].ToLower().StartsWith(" ip vrf forwarding ") -or $strConfig[$i].ToLower() -match "\s+vrf member " ) {
                        $strTemp = $strConfig[$i].Split(" ");
                        $IFcfg.vrf = $strTemp[$strTemp.Length-1];
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("ip address ") ) {
                        $strTemp = $strConfig[$i].Trim().Split(" ");
            
                        #Check IP-Addressformat   10.110.1.254/24 or 10.110.1.254 255.255.255.0
                        if($strTemp[$strTemp.Length-1].Equals("secondary") ) {
                            $strIP = $strTemp[$strTemp.Length-3];
                            $strMask = $strTemp[$strTemp.Length-2];
                            $isSecondaryIP = $true;
                        } elseif($strTemp[$strTemp.Length-2].Equals("standby") ) {
                            # Cisco ASA
                            $strIP = $strTemp[$strTemp.Length-4];
                            $strMask = $strTemp[$strTemp.Length-3];
                            $isSecondaryIP = $false;
                        } else {
                            $strIP = $strTemp[$strTemp.Length-2];
                            $strMask = $strTemp[$strTemp.Length-1];
                            $isSecondaryIP = $false;
                        }
                        if($strMask.Contains("/")) {
                            $strCfgTmp = $strMask.Split("/");
                            $strIP = $strCfgTmp[0];
                            $strMask = $this.IPv4Bits2Mask($strCfgTmp[1]);
                        }
                        if(( ($strIP -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") -and ($strMask -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") ) -or ($strIP -match "^[0-9a-fA-F:]+$")) {
                            $objIP = [NetIPClass]::new();
                            $objIP.ip = $strIP;
                            $objIP.mask = $strMask;
                            $objIP.secondary = $isSecondaryIP;
                            $IFcfg.IPs.Add($objIP);

                            if($this.Debug -eq $true) {
                                Write-Host "Found IP: $($strIP)/$($strMask)";
                            }
                        } else {
                            $strIP = "";
                            $strMask = "";
                            $isSecondaryIP = $false;

                            if($this.Debug -eq $true) {
                                Write-Host "Found no IP for IF: $($IFName)";
                            }
                        }
                    } elseif($strConfig[$i].ToLower().StartsWith(" ip helper-address ") ) {
                        $strCfgTmp = $strConfig[$i].Trim().Split(" ")
                        if($IPHelper.Length -gt 0) {
                            $IPHelper = $IPHelper + ";";
                        }
                        $IPHelper = $IPHelper + $strCfgTmp[$strCfgTmp.Length-1];
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("description ") ) {
                        $IFcfg.Descr = $strConfig[$i].Trim().Substring(12);
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("encapsulation dot1Q ") ) {
                        $strCfgTmp = $strConfig[$i].Split(" ")
                        if($strCfgTmp[$strCfgTmp.Length-1].Equals("native")) {
                            $VlanNative = $strCfgTmp[$strCfgTmp.Length-2];
                        } else {
                            $VlanNative = $strCfgTmp[$strCfgTmp.Length-1];
                        }
                        $VlanAccess = $VlanNative;
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("switchport mode access") ) {
                        $IFcfg.TrunkMode = $false;
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("switchport mode trunk") ) {
                        $IFcfg.TrunkMode = $true;
                    } elseif($strConfig[$i].ToLower().StartsWith(" vlan ") ) {
                        $VlanAccess = $strConfig[$i].Substring(6);
                    } elseif($strConfig[$i].ToLower().StartsWith(" switchport access vlan ") ) {
                        $VlanAccess = $strConfig[$i].Substring(24);
                    } elseif($strConfig[$i].ToLower().StartsWith(" switchport trunk native vlan ") ) {
                        $VlanNative = $strConfig[$i].Substring(30);
                    } elseif($strConfig[$i].ToLower().StartsWith(" switchport trunk allowed vlan ") ) {
                        if($TrunkList -ne "") {
                            $TrunkList = $TrunkList + ",";
                        }
                        if($strConfig[$i].ToLower().StartsWith(" switchport trunk allowed vlan add ") ) {
                            $TrunkList = $TrunkList + $strConfig[$i].Substring(35);;
                        } else {
                            $TrunkList = $TrunkList + $strConfig[$i].Substring(31);
                        }
                    }  elseif($strConfig[$i].ToLower().StartsWith(" switchport trunk remove vlan ") ) {
                        if($TrunkListNot -ne "") {
                            $TrunkListNot = $TrunkListNot + ",";
                        }
                        if($strConfig[$i].ToLower().StartsWith(" switchport trunk allowed vlan remove ")) {
                            $TrunkList = $TrunkListNot + $strConfig[$i].Substring(37);
                        } else {
                            $TrunkListNot = $TrunkListNot + $strConfig[$i].Substring(30);
                        }
                    }
                } elseif($strConfig[$i].ToLower() -match "line vty.*") {
                    $cfgSection = "vty";
                } elseif($cfgSection.Equals("vty")) {
                    if($strConfig[$i] -match "\s+access-class .*") {
                        $strCfgTmp = $strConfig[$i].Trim().Split(" ");
                        if($strCfgTmp.Length -gt 1) {
                            if([string]::IsNullOrWhitespace($objCfg.VTYAcls)) {
                                $objCfg.VTYAcls = $strCfgTmp[1];
                            } else {
                                if($objCfg.VTYAcls -ne $strCfgTmp[1]) {
                                    $objCfg.VTYAcls = [string]::Format("{0};{1}", $objCfg.VTYAcls, $strCfgTmp[1]);
                                }
                            }
                        }
                    } elseif($strConfig[$i] -match "\s+login authentication .*") {
                        $strCfgTmp = $strConfig[$i].Trim().Split(" ");
                        if($strCfgTmp.Length -gt 2) {
                            $objCfg.VTYaaaName = $strCfgTmp[2];
                        }
                    } elseif($strConfig[$i].Trim().Equals("!") -or ($strConfig[$i] -match "^[^ ].*") ) {
                        $cfgSection = "";
                    }
                } elseif($strConfig[$i].StartsWith("ip access-list ")) {
                    #ip access-list standard VTY-ACCESS
                    #ip access-list extended MFP-IN

                    $cfgSection = "acl";
                    if($ACLcfg) {
                        #Add previous ACL to Cfg-Obj:
                        $objCfg.ACLs.Add($ACLcfg);
                    }
                    $ACLcfg = [NetACLClass]::new();

                    $strCfgTmp = $strConfig[$i].Split(" ");
                    if($strCfgTmp.Length -gt 2) {
                        #$strCfgTmp[2];     # standard | extended or for NX-OS  ACL-Name
                        $ACLcfg.ACLName = $strCfgTmp[$strCfgTmp.Length-1];
                        $ACLcfg.ACLCfgEntry = $strConfig[$i];
                    }
                } elseif($cfgSection.Equals("acl")) {
                    if(($strConfig[$i].Trim().Equals("!")) -or ( ($strConfig[$i].IndexOf(" remark ") -eq -1) -and ($strConfig[$i].IndexOf(" permit ") -eq -1) -and ($strConfig[$i].IndexOf(" deny ") -eq -1) )) {
                        $cfgSection = "";

                        $objCfg.ACLs.Add($ACLcfg);
                        $ACLcfg = $null;
                    } else {
                        if([String]::IsNullOrWhiteSpace($ACLcfg.ACLCfgAsString)) {
                            $ACLcfg.ACLCfgAsString = $strConfig[$i].Trim();
                        } else {
                            $ACLcfg.ACLCfgAsString = [String]::Format("{0}`n{1}", $ACLcfg.ACLCfgAsString, $strConfig[$i].Trim());
                        }
                    }
                } elseif($strConfig[$i].StartsWith("access-list ")) {
                    #access-list 10 permit 172.28.92.0 0.0.0.255
                    #access-list 10 permit host 172.28.92.1

                    $cfgSection = "acl-simpel";
                    $strCfgTmp = $strConfig[$i].Split(" ");
                    if($ACLcfg) {
                        if($strCfgTmp.Length -gt 3) {
                            if($strCfgTmp[1] -ne $ACLcfg.ACLName) {
                                #Add previous ACL to Cfg-Obj:
                                $objCfg.ACLs.Add($ACLcfg);

                                $ACLcfg = [NetACLClass]::new();
                            }
                        }
                    } else {
                        $ACLcfg = [NetACLClass]::new();
                    }

                    if($strCfgTmp.Length -gt 3) {
                        $ACLcfg.ACLName = $strCfgTmp[1];
                        $ACLcfg.ACLCfgEntry = "";

                        if([String]::IsNullOrWhiteSpace($ACLcfg.ACLCfgAsString)) {
                            $ACLcfg.ACLCfgAsString = $strConfig[$i];
                        } else {
                            $ACLcfg.ACLCfgAsString = [String]::Format("{0}`n{1}", $ACLcfg.ACLCfgAsString, $strConfig[$i]);
                        }
                    }
                } elseif(($cfgSection -eq "acl-simpel") -and ($strConfig[$i].Trim().Equals("!"))) {
                    $cfgSection = "";

                    $objCfg.ACLs.Add($ACLcfg);
                    $ACLcfg = $null;
                } elseif($strConfig[$i].ToLower().StartsWith("hostname ")) {
                    $objCfg.Name = $strConfig[$i].Substring(9);
                } elseif($strConfig[$i].ToLower().StartsWith("snmp-server location ")) {
                    $objCfg.Location = $strConfig[$i].Substring(21);
                } else {
                    if($cfgSection -eq "acl-simpel") {
                        $cfgSection = "";
    
                        if($ACLcfg) {
                            $objCfg.ACLs.Add($ACLcfg);
                            $ACLcfg = $null;
                        }
                    }
                    #elseif($cfgSection.Equals("acl")) {
                    #    $cfgSection = "";

                    #    if($ACLcfg) {
                    #        $objCfg.ACLs.Add($ACLcfg);
                    #        $ACLcfg = $null;
                    #    }
                    #}
                }
            }
            return $objCfg;
        }

    [NetDeviceCfgClass] cfgParserHPProcurve([String[]]$strConfig) {
        #param (
        #    [String[]]$strConfig=@()
        #)
            #[NetCfgParserClass]$CfgParser = [NetCfgParserClass]::new();
            [NetDeviceCfgClass]$objCfg = [NetDeviceCfgClass]::new();
            [NetVLANClass]$VLANcfg = $null;
            [NetIFClass]$IFcfg = $null;
            $strCfgTmp = "";
            $cfgSection = "";   #if, vlan
            $IFName = "";
            $VlanID = "";
            #$Descr = ""; $vrf = "";
            #$IPs = New-Object System.Collections.ArrayList;
            [NetIPClass]$objIP = $null;
            $strIP = "";
            $strMask = "";
            $isSecondaryIP = $false;
            $IPHelper = "";
            $IFList = "";
            $IFListTrunk = "";
            $Trunk = $false;
            $TrunkList = "";
        
            for($i=0; $i -lt $strConfig.Length; $i++) {
                if($strConfig[$i].ToLower().StartsWith("interface ") -or ($cfgSection.Equals("if") -and $strConfig[$i].Trim().Equals("exit") )) {
                    if($IFName -ne "") {
                        #Save parsed Data
                        <# $IFcfg = @{ Name = $IFName Descr = $Descr IPs = $IPs vrf = $vrf IPHelper = $IPHelper
                            HSRPIPs = New-Object System.Collections.ArrayList
                            VlanID = $VlanID TrunkMode = $Trunk TrunkVLANs = $TrunkList } #>
                        $IFcfg.IPHelper = $IPHelper;

                        $bAddIF = $true;
                        foreach($tmpIF in $objCfg.IFs) {
                            if($IFName -eq $tmpIF.Name) {
                                $bAddIF = $false;
                                break;
                            }
                        }

                        if($bAddIF) {
                            $objCfg.IFs.Add($IFcfg);
                        }
                        #Reset Vars
                        $IFName = "";
                        #$vrf = ""; $Descr = "";
                        #$IPs = New-Object System.Collections.ArrayList;   #$IPs.Clear();
                        $IPHelper = "";
                        $VlanID = "";
                        $Trunk = $false;
                        $TrunkList = "";
                        $IFcfg = $null;
                    }
                    if($strConfig[$i].Trim().Equals("exit")) {
                        $cfgSection = "";
                        continue;
                    }
                    if($strConfig[$i].Length -lt 2) {
                        continue;
                    }
        
                    $cfgSection = "if";
                    $IFName = $strConfig[$i].Substring(10).Trim();
                    $IFcfg = [NetIFClass]::new();
                    $IFcfg.Name = $IFName;
                } elseif($cfgSection.Equals("if")) {
                    if($strConfig[$i].Trim().ToLower().StartsWith("ip address ") ) {
                        $strTemp = $strConfig[$i].Trim().Split(" ");
            
                        #Check IP-Addressformat   10.110.1.254/24 or 10.110.1.254 255.255.255.0
                        if($strTemp[$strTemp.Length-1].Equals("secondary") ) {
                            $strIP = $strTemp[$strTemp.Length-3];
                            $strMask = $strTemp[$strTemp.Length-2];
                            $isSecondaryIP = $true;
                        } else {
                            $strIP = $strTemp[$strTemp.Length-2];
                            $strMask = $strTemp[$strTemp.Length-1];
                            $isSecondaryIP = $false;
                        }
                        if($strMask.Contains("/")){
                            $strCfgTmp = $strMask.Split("/");
                            $strIP = $strCfgTmp[0];
                            $strMask = $this.IPv4Bits2Mask($strCfgTmp[1]);
                        }
                        if(( ($strIP -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") -and ($strMask -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") ) -or ($strIP -match "^[0-9a-fA-F:]+$")) {
                            # $IPs.Add(@{ ip=$strIP mask=$strMask Secondary=$isSecondaryIP });
                            $objIP = [NetIPClass]::new();
                            $objIP.ip = $strIP;
                            $objIP.mask = $strMask;
                            $objIP.Secondary = $isSecondaryIP;
                            $IFcfg.IPs.Add($objIP);
                        }
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("ip helper-address ") ) {
                        $strCfgTmp = $strConfig[$i].Trim().Split(" ")
                        if($IPHelper.Length -gt 0) {
                            $IPHelper = $IPHelper + ";";
                        }
                        $IPHelper = $IPHelper + $strCfgTmp[$strCfgTmp.Length-1];
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("name ") ) {
                        $IFcfg.Descr = $strConfig[$i].Trim().Substring(5);
                    }
                } elseif($strConfig[$i].ToLower().StartsWith("vlan ")) {
                    $VlanID = $strConfig[$i].Substring(5).Trim();
                    $cfgSection = "vlan";
                    $VLANcfg = [NetVLANClass]::new();
                    $VLANcfg.VlanID = $VlanID;
                } elseif($cfgSection.Equals("vlan")) {
                    if($strConfig[$i].Trim().Equals("exit")) {
                        #Save parsed Data
                        # @{ VlanID = $VlanID Name = $IFName IPs = $IPs }
                        $objCfg.VLANs.Add($VLANcfg);
        
                        #Reset Vars
                        $VlanID = "";
                        #$IPs = New-Object System.Collections.ArrayList;   #$IPs.Clear();
                        $IFList = "";
                        $IFListTrunk = "";
                        $Trunk = $false;
                        $TrunkList = "";
        
                        $cfgSection = "";
                    } elseif($strConfig[$i].Trim().StartsWith("name ")) {
                        $VLANcfg.Name = $strConfig[$i].Trim().Substring(5);
                    } elseif($strConfig[$i].Trim().StartsWith("untagged ")) {
                        $IFList = $strConfig[$i].Trim().Substring(9);
                        $this.HPSetVlan2IF($VlanID, $IFList, ([ref]$objCfg));
                    } elseif($strConfig[$i].Trim().StartsWith("tagged ")) {
                        $IFListTrunk = $strConfig[$i].Trim().Substring(7);
                        $this.HPSetTrunkVlan2IF($VlanID, $IFListTrunk, ([ref]$objCfg));
                    } elseif($strConfig[$i].Trim().ToLower().StartsWith("ip address ") ) {
                        $strTemp = $strConfig[$i].Trim().Split(" ");
            
                        #Check IP-Addressformat   10.110.1.254/24 or 10.110.1.254 255.255.255.0
                        if($strTemp[$strTemp.Length-1].Equals("secondary") ) {
                            $strIP = $strTemp[$strTemp.Length-3];
                            $strMask = $strTemp[$strTemp.Length-2];
                            $isSecondaryIP = $true;
                        } else {
                            $strIP = $strTemp[$strTemp.Length-2];
                            $strMask = $strTemp[$strTemp.Length-1];
                            $isSecondaryIP = $false;
                        }
                        if($strMask.Contains("/")){
                            $strCfgTmp = $strMask.Split("/");
                            $strIP = $strCfgTmp[0];
                            $strMask = $this.IPv4Bits2Mask($strCfgTmp[1]);
                        }
                        if(( ($strIP -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") -and ($strMask -match "^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$") ) -or ($strIP -match "^[0-9a-fA-F:]+$")) {
                            $objIP = [NetIPClass]::new();
                            $objIP.ip = $strIP;
                            $objIP.mask = $strMask;
                            $objIP.Secondary = $isSecondaryIP;
                            $VLANcfg.IPs.Add($objIP);
                        }
                    }
                } elseif($strConfig[$i].ToLower().StartsWith("snmp-server location ")) {
                    $objCfg.Location = $strConfig[$i].Substring(21);
                } elseif($strConfig[$i].ToLower().StartsWith("hostname ")) {
                    $objCfg.Name = $strConfig[$i].Substring(9);
                } else {
        
                }
            }
            return $objCfg;
        }

        [void] CiscoIFState([String[]]$IFStateList, [ref]$SwitchIFs) {

            $IFMatches = [regex] '^test$';
            $PortPos = -1;
            $NamePos = -1;
            $StatePos = -1;
            $VlanPos = -1;
            $DuplexPos = -1;
            §SpeedPos = -1;
            $TypePos = -1;
            $IFName = "";
            $IFArray = $null;

            for($i=0; $i -lt $IFStateList.Length; $i++) {
                if($IFStateList[$i] -ne "") {
                    if($PortPos -eq -1) {
                        if($IFStateList[$i].StartsWith("Port")) {
                            $PortPos = 0;
                            $NamePos = $IFStateList[$i].IndexOf("Name");
                            $StatePos = $IFStateList[$i].IndexOf("Status");
                            $VlanPos = $IFStateList[$i].IndexOf("Vlan");
                            $DuplexPos = $IFStateList[$i].IndexOf("Duplex");
                            $SpeedPos = $IFStateList[$i].IndexOf("Speed");
                            $TypePos = $IFStateList[$i].IndexOf("Type");
                        } else {
                            continue;
                        }
                    }

                    $IFName = $IFStateList[$i].Substring(0, $NamePos-1).Trim();
                    $IFArray = $IFStateList[$i].Substring($StatePos).Split(" ");

                    #ToDo:
                    #   Translate  Eth, Fa, Gi, Te, 40Gi, 100Gi
                }
            }
        }
}

#Export-ModuleMember -Function cfgParserCisco
#Export-ModuleMember -Function cfgParserHPProcurve
