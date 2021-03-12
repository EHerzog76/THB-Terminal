function IP2Int
{
param([parameter(Position=0,Mandatory=$TRUE)][string]$IPAddress)
    if(!$IPAddress.Contains('.')) {Write-error -Message "Not a valid IP Address" -RecommendedAction "please use periods to denote each section of the ip address"; Return}

    $SplitIP = @(0x0,0x0,0x0,0x0);
    $IPOct = $IPAddress.Split('.')
    for($i = 0; $i -lt 4; $i++)
    {
        $SplitIP[$i] = [string]::Format("{0:x2}",[int]$IPOct[$i])
    }
    $IPHEX = "0x$($SplitIP[0])$($SplitIP[1])$($SplitIP[2])$($SplitIP[3])"
    $CompIP = [int]("0x$IPHEX")

    Return $CompIP
}
function Int2IP
{
    Param([parameter(Position=0,Mandatory=$TRUE)][int]$Number)
    $ipHEX = [string]::Format("{0:x8}",$Number)
    $SplitHEX = @(0,0,0,0)
    for($i = 0; $i -lt 4; $i++)
    { $SplitHEX[$i] = "0x$($ipHEX.substring(($i*2),2))" }
    Return "$([int]$SplitHEX[0]).$([int]$SplitHEX[1]).$([int]$SplitHEX[2]).$([int]$SplitHEX[3])"
}

function IPv4Bits2Mask {
    <#
    .SYNOPSIS
    Converts a number of bits (0-32) to an IPv4 network mask string (e.g., "255.255.255.0").
  
    .DESCRIPTION
    Converts a number of bits (0-32) to an IPv4 network mask string (e.g., "255.255.255.0").
  
    .PARAMETER MaskBits
    Specifies the number of bits in the mask.
    #>
    param(
      [parameter(Mandatory=$true)]
      [ValidateRange(0,32)]
      [Int] $MaskBits
    )
    $mask = ([Math]::Pow(2, $MaskBits) - 1) * [Math]::Pow(2, (32 - $MaskBits))
    $bytes = [BitConverter]::GetBytes([UInt32] $mask)
    (($bytes.Count - 1)..0 | ForEach-Object { [String] $bytes[$_] }) -join "."
  }

  function IPv4Mask2Bits {
    <#
    .SYNOPSIS
    Returns the number of bits (0-32) in a network mask string (e.g., "255.255.255.0").
  
    .DESCRIPTION
    Returns the number of bits (0-32) in a network mask string (e.g., "255.255.255.0").
  
    .PARAMETER MaskString
    Specifies the IPv4 network mask string (e.g., "255.255.255.0").
    #>
    param(
      [parameter(Mandatory=$true)]
      [ValidateScript({Test-IPv4MaskString $_})]
      [String] $MaskString
    )
    $mask = ([IPAddress] $MaskString).Address
    for ( $bitCount = 0; $mask -ne 0; $bitCount++ ) {
      $mask = $mask -band ($mask - 1)
    }
    $bitCount
  }

function Test-IPv4MaskString {
    #.SYNOPSIS
    #Tests whether an IPv4 network mask string (e.g., "255.255.255.0") is valid.
  
    #.DESCRIPTION
    #Tests whether an IPv4 network mask string (e.g., "255.255.255.0") is valid.
  
    #.PARAMETER MaskString
    #Specifies the IPv4 network mask string (e.g., "255.255.255.0").
    #
    param(
      [parameter(Mandatory=$true)]
      [String] $MaskString
    )
    $validBytes = '0|128|192|224|240|248|252|254|255'
    $maskPattern = ('^((({0})\.0\.0\.0)|'      -f $validBytes) +
           ('(255\.({0})\.0\.0)|'      -f $validBytes) +
           ('(255\.255\.({0})\.0)|'    -f $validBytes) +
           ('(255\.255\.255\.({0})))$' -f $validBytes)
    $MaskString -match $maskPattern
}
