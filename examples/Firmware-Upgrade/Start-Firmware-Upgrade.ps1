foreach($line in [System.IO.File]::ReadLines("C:\Scripts\Firmware-Upgrade\SwitchList.txt"))
{
   if($line -match "[0-9]+\..*") {
       Write-Host "powershell.exe C:\Scripts\CfgEngine\FirmwareUpgrade.ps1 -DeviceIP $($line) -Debug";
       powershell.exe C:\Scripts\CfgEngine\FirmwareUpgrade.ps1 -DeviceIP $($line) -Debug
   }
}
