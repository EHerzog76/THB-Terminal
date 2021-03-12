function ipcalc() {
[CmdletBinding()] 
param ( 
    [Parameter(Mandatory=$True,Position=1)] 
    [string]$IPAddress, 
    [Parameter(Mandatory=$False,Position=2)] 
    [string]$Netmask, 
    [switch]$IncludeTextOutput, 
    [switch]$IncludeBinaryOutput 
    ) 
 
#region HelperFunctions 
 
# Function to convert IP address string to binary 
function toBinary ($dottedDecimal){
 $binary = ""
 $dottedDecimal.split(".") | ForEach-Object {$binary=$binary + $([convert]::toString($_,2).padleft(8,"0"))} 
 return $binary 
} 
 
# Function to binary IP address to dotted decimal string 
function toDottedDecimal ($binary){
 $i=0
 do {$dottedDecimal += "." + [string]$([convert]::toInt32($binary.substring($i,8),2)); $i+=8 } while ($i -le 24) 
 return $dottedDecimal.substring(1) 
} 
 
# Function to convert CIDR format to binary 
function CidrToBin ($cidr){ 
    if($cidr -le 32){ 
        [Int[]]$array = (1..32) 
        for($i=0;$i -lt $array.length;$i++){ 
            if($array[$i] -gt $cidr){$array[$i]="0"}else{$array[$i]="1"} 
        } 
        $cidr =$array -join "" 
    } 
    return $cidr 
} 
 
# Function to convert network mask to wildcard format 
function NetMasktoWildcard ($wildcard) { 
    foreach ($bit in [char[]]$wildcard) { 
        if ($bit -eq "1") { 
            $wildcardmask += "0" 
            } 
        elseif ($bit -eq "0") { 
            $wildcardmask += "1" 
            } 
        } 
    return $wildcardmask 
    } 
#endregion 
 
 
# Check to see if the IP Address was entered in CIDR format. 
$wildcardbinary=""
if ($IPAddress -like "*/*") { 
    $CIDRIPAddress = $IPAddress 
    $IPAddress = $CIDRIPAddress.Split("/")[0] 
    $cidr = [convert]::ToInt32($CIDRIPAddress.Split("/")[1]) 
    if ($cidr -le 32 -and $cidr -ne 0) { 
        $ipBinary = toBinary $IPAddress 
        Write-Verbose $ipBinary 
        $smBinary = CidrToBin($cidr) 
        Write-Verbose $smBinary 
        $Netmask = toDottedDecimal($smBinary) 
        $wildcardbinary = NetMasktoWildcard ($smBinary) 
        } 
    else { 
        Write-Warning "Subnet Mask is invalid!" 
        Exit 
        } 
    } 
 
# Address was not entered in CIDR format. 
else { 
    if (!$Netmask) { 
        $Netmask = Read-Host "Netmask" 
    }
    $ipBinary = toBinary $IPAddress 
    if ($Netmask -eq "0.0.0.0") { 
        Write-Warning "Subnet Mask is invalid!" 
        Exit 
        } 
    else { 
        $smBinary = toBinary $Netmask 
        $wildcardbinary = NetMasktoWildcard ($smBinary) 
        } 
    } 
 
 
# First determine the location of the first zero in the subnet mask in binary (if any) 
$netBits=$smBinary.indexOf("0") 
 
# If there is a 0 found then the subnet mask is less than 32 (CIDR). 
if ($netBits -ne -1) { 
    $cidr = $netBits 
    #validate the subnet mask 
    if(($smBinary.length -ne 32) -or ($smBinary.substring($netBits).contains("1") -eq $true)) { 
        Write-Warning "Subnet Mask is invalid!" 
        Exit 
        } 
    # Validate the IP address 
    if($ipBinary.length -ne 32) { 
        Write-Warning "IP Address is invalid!" 
        Exit 
        } 
    #identify subnet boundaries 
    $networkID = toDottedDecimal $($ipBinary.substring(0,$netBits).padright(32,"0")) 
    $networkIDbinary = $ipBinary.substring(0,$netBits).padright(32,"0") 
    $firstAddress = toDottedDecimal $($ipBinary.substring(0,$netBits).padright(31,"0") + "1") 
    $firstAddressBinary = $($ipBinary.substring(0,$netBits).padright(31,"0") + "1") 
    $lastAddress = toDottedDecimal $($ipBinary.substring(0,$netBits).padright(31,"1") + "0") 
    $lastAddressBinary = $($ipBinary.substring(0,$netBits).padright(31,"1") + "0") 
    $broadCast = toDottedDecimal $($ipBinary.substring(0,$netBits).padright(32,"1")) 
    $broadCastbinary = $ipBinary.substring(0,$netBits).padright(32,"1") 
    $wildcard = toDottedDecimal ($wildcardbinary) 
    $Hostspernet = ([convert]::ToInt32($broadCastbinary,2) - [convert]::ToInt32($networkIDbinary,2)) - 1 
   } 
 
# Subnet mask is 32 (CIDR) 
else { 
     
    # Validate the IP address 
    if($ipBinary.length -ne 32) { 
        Write-Warning "IP Address is invalid!" 
        Exit 
        } 
 
    #identify subnet boundaries 
    $networkID = toDottedDecimal $($ipBinary) 
    $networkIDbinary = $ipBinary 
    $firstAddress = toDottedDecimal $($ipBinary) 
    $firstAddressBinary = $ipBinary 
    $lastAddress = toDottedDecimal $($ipBinary) 
    $lastAddressBinary = $ipBinary 
    $broadCast = toDottedDecimal $($ipBinary) 
    $broadCastbinary = $ipBinary 
    $wildcard = toDottedDecimal ($wildcardbinary) 
    $Hostspernet = 1 
    $cidr = 32 
    } 
 
#region Output 
 
# Include a ipcalc.pl style text output (not an object) 
if ($IncludeTextOutput) { 
    Write-Host "`nAddress:`t`t$IPAddress" 
    Write-Host "Netmask:`t`t$Netmask = $cidr" 
    Write-Host "Wildcard:`t`t$wildcard" 
    Write-Host "=>" 
    Write-Host "Network:`t`t$networkID/$cidr" 
    Write-Host "Broadcast:`t`t$broadCast" 
    Write-Host "HostMin:`t`t$firstAddress" 
    Write-Host "HostMax:`t`t$lastAddress" 
    Write-Host "Hosts:`t`t$Hostspernet`n" 
    } 
 
# Output custom object with or without binary information. 
if ($IncludeBinaryOutput) { 
    [PSCustomObject]@{ 
        Address = $IPAddress 
        Netmask = $Netmask 
        Wildcard = $wildcard 
        Network = "$networkID/$cidr" 
        Broadcast = $broadCast 
        HostMin = $firstAddress 
        HostMax = $lastAddress 
        Hosts = $Hostspernet 
        AddressBinary = $ipBinary 
        NetmaskBinary = $smBinary 
        WildcardBinary = $wildcardbinary 
        NetworkBinary = $networkIDbinary 
        HostMinBinary = $firstAddressBinary 
        HostMaxBinary = $lastAddressBinary 
        BroadcastBinary = $broadCastbinary 
        } 
    } 
else { 
    [PSCustomObject]@{ 
        Address = $IPAddress 
        Netmask = $Netmask 
        Wildcard = $wildcard 
        Network = "$networkID/$cidr" 
        Broadcast = $broadCast 
        HostMin = $firstAddress 
        HostMax = $lastAddress 
        Hosts = $Hostspernet 
        }     
    } 
#endregion
}