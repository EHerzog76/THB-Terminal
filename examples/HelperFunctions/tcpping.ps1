function tcpping1($ip, $Port) {
$PortOpen = 0;

    # Create a Net.Sockets.TcpClient object to use for
    # checking for open TCP ports.
    $Socket = New-Object Net.Sockets.TcpClient
        
    # Suppress error messages
    $ErrorActionPreference = 'SilentlyContinue'
        
    # Try to connect
    $Socket.Connect($ip, $Port)
        
    # Make error messages visible again
    $ErrorActionPreference = 'Continue'
        
    # Determine if we are connected.
    if ($Socket.Connected) {
        #"${ip}: Port $Port is open"
        $PortOpen = 1;
        $Socket.Close()
    }
    else {
        #"${ip}: Port $Port is closed or filtered"
        $PortOpen = 0;
    }
        
    # Apparently resetting the variable between iterations is necessary.
    $Socket.Dispose()
    $Socket = $null
    
    return $PortOpen;
}

function tcpping($ip, $port) {
    #$ip, $port = $args[0,1] # assign values to these
    $bPortOpen = $false;
    $bConRefused = $false;
    $Socket = new-object Net.Sockets.TcpClient;

    try {
        $IAsyncResult = [IAsyncResult] $Socket.BeginConnect($ip, $port, $null, $null);
        $WaitTime = measure-command { $succ = $iasyncresult.AsyncWaitHandle.WaitOne(2000, $true) } | ForEach-Object totalseconds # %
        #$succ
        
        $bPortOpen = $Socket.Connected;
        $Socket.Close();
    } catch [System.Net.Sockets.SocketException] {
        if($_.ErrorCode -eq 10061) {
            $bConRefused = $true;
        }
    }
    catch {

    }
    if($null -ne $IAsyncResult) {
        $IAsyncResult.AsyncWaitHandle.Close();
    }
    $Socket.Dispose();
    $Socket = $null;

    #return $succ
    [PSCustomObject]@{ 
        PortOpen = $bPortOpen 
        time = $WaitTime
        Refused = $bConRefused
    }
}
