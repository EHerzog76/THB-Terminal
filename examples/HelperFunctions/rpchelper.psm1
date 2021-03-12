function RPCinit() {
if (("THBRpc.RpcTask" -as [type]) -eq $null) {
    Add-Type -TypeDefinition @"

namespace THBRpc
{
    public enum FuncMethods { Start, Stop, Create, Delete, Update, State }

    public class RpcTask
    {
        public string FuncCall; //Workflow, WkfTask, ...
        public FuncMethods Method;
        public string Type;
        public long ID;
        public string Name;
        public string TaskDir;
        public string TaskName;
        public string Params;
        public string TaskResult;
        public string InstanceID;
        public string StartTime;
        public string EndTime;
		public int MaxRuntime;
        //public string CreatedByClientNode;
        //public string CreatedByUser;
    }

    public enum RpcErrorCodes
    {
        Success = 0,
        Error = 1,
        Busy = 429,
        NotFound = 404,
        UnkownMethod = 253,
        UnkownFunction = 254,
        NoData = 255
    }

    public class RpcTaskResult
    {
        public string State;
        public string Msg;
        public string Error;
        public System.UInt16 ErrorNr;
    }
}

"@

#Load DLLs
    #[System.Reflection.Assembly]::LoadFile("C:\Progs\libZMQ\ZeroMQ.dll");   #clrzmq.dll
    Add-Type -Path "C:\Progs\libZMQ\ZeroMQ.dll";        #clrzmq.dll
}
}

RPCinit;

#Vars:
$script:enc = [system.Text.Encoding]::Unicode;
$script:encUTF8 = [system.Text.Encoding]::UTF8;
[ZeroMQ.ZContext]$script:zmqCtx = $null;
[ZeroMQ.ZSocket]$script:zmqSocket = $null;
[ZeroMQ.ZMessage]$script:zmqMsg = [ZeroMQ.ZMessage]::new(); # New-Object ZeroMQ.ZMessage;
[ZeroMQ.ZMessage]$script:zmqRecvMsg = $null;                 #New-Object ZeroMQ.ZMessage;
[ZeroMQ.ZFrame]$script:zmqFrame = $null;
[THBRpc.RpcTask]$script:rpcTask = [THBRpc.RpcTask]::new();  # New-Object THBRpc.RpcTask;
[THBRpc.RpcTaskResult]$script:rpcResult = [THBRpc.RpcTaskResult]::new();  # New-Object THBRpc.RpcTaskResult;
#####################

function RPCopen([String]$CfgEngineSrv="10.20.30.40", [int]$CfgEngineSrvPort=8002) {

    #Init ZeroMQ
    [ZeroMQ.ZContext]$script:zmqCtx = [ZeroMQ.ZContext]::new();    #New-Object ZeroMQ.ZContext;    #[ZeroMQ.ZContext]::new();
    $script:zmqSocket = [ZeroMQ.ZSocket]::Create($zmqCtx, [ZeroMQ.ZSocketType]::REQ);
    $script:zmqSocket.IdentityString = [Guid]::NewGuid().ToString();
    $script:zmqSocket.SendHighWatermark = 0;
    #$script:zmqSocket.SetOption([ZeroMQ.ZSocketOption]::LINGER, 0);
    $strPort = $CfgEngineSrvPort.ToString();
    $script:zmqSocket.Connect("tcp://$($CfgEngineSrv):$($strPort)");
}

function RPCclose() {
    if ($script:zmqSocket)
    {
        $script:zmqSocket.SetOption([ZeroMQ.ZSocketOption]::LINGER, 0);
        $script:zmqSocket.Close();
        $script:zmqSocket.Dispose();
        $script:zmqSocket = $null;
    }
    # -------------------------<NEVER BEFORE SAFE DISMANTLING ALL SOCKETS>---
    $script:zmqCtx.Shutdown();                   # optional  API-call
    $script:zmqCtx.Terminate();                  # mandatory API-call
    $script:zmqCtx = $null;
}

    #=======================================================================
    # Purpose:Send Data via ZeroMQ
function SendData([String]$msg)
{
    $data=$script:enc.GetBytes($msg);
    $script:zmqSocket.Send($data, $data.Length, [ZeroMQ.SocketFlags]::None);
}
    #=======================================================================
    # Purpose:Send Message via ZeroMQ
function SendMsg([String]$msg)
{
    $script:zmqSocket.SendMessage($msg);
}

# Purpose:Send Message as CfgEngine-RpcCall via ZeroMQ
 function RpcCall([THBRpc.RpcTask]$RpcObj)
{
    [String]$strTmp = "";
    $strTmp = ConvertTo-Json -InputObject $RpcObj -Depth 15

    $script:zmqFrame = New-Object ZeroMQ.ZFrame($strTmp);
    $script:zmqMsg.Add($zmqFrame);
    $script:zmqSocket.SendMessage($zmqMsg);
    $script:zmqMsg.Clear();
}

#=======================================================================
# Purpose:Receive Message via ZeroMQ
 function RecvMsg()
{
    $script:zmqRecvMsg = $script:zmqSocket.ReceiveMessage();
    if($script:zmqRecvMsg -eq $null) {
        return $null;
    } else {
        $RecvData=$script:encUTF8.GetString($script:zmqRecvMsg[0].Read());
        $script:zmqRecvMsg.Clear();
    }
    return ConvertFrom-Json -InputObject $RecvData;
}

function RpcStartJob([THBRpc.RpcTask]$rTask, [bool]$Wait4Task) {
    $jobQueued = 0;
    $rpcResult = $null;

    $rTask.FuncCall = "WkfTask";
    $rTask.Method = [THBRpc.FuncMethods]::Start;
    if(($null -eq $rTask.MaxRuntime) -or ($rTask.MaxRuntime -eq 0)) {
        $rTask.MaxRuntime = 30;	    #in Minutes
    }
    #$rTask.Name = "TestScript.ps1";
    #$rTask.Params = "1 Test-Call from Powershell";

    while($jobQueued -eq 0) {
        RpcCall($rTask);

        $rpcResult = RecvMsg;
        if($rpcResult.ErrorNr -eq 0) {
            $jobQueued = 1;
            Write-Host $rpcResult;
        } else {
            Start-Sleep -milliseconds 300
        }
    }

    if($Wait4Task -eq $true) {
        $rpcTaskID = $rpcResult.Msg
        $rpcResult.ErrorNr = 0;

        if($rpcResult.ErrorNr -eq 0) {
            $isJobReady = $false;

            # Wait for Task-End
            $rpcTask.FuncCall = "WkfTask";
            $rpcTask.Method = [THBRpc.FuncMethods]::State;
            $rpcTask.InstanceID = $rpcTaskID;

            while($isJobReady -eq $false) {
                RpcCall $rpcTask;

                $rpcResult = RecvMsg;
                if(($rpcResult.ErrorNr -eq 0) -and ($rpcResult.State -eq "ready")) {
                    $isJobReady = $true;

                    Write-Host $rpcResult;
                } elseif($rpcResult.ErrorNr -eq 429) {
                    #Bussy
                } elseif($rpcResult.ErrorNr -eq 404) {
                    $isJobReady = $true;
                }
            }
        }
    }

    return($rpcResult);
}

function RPCJobState([String]$JobID) {
    [THBRpc.RpcTask]$rpcTask = New-Object THBRpc.RpcTask;
    $rpcResult = $null;

    $rpcTask.FuncCall = "WkfTask";
    $rpcTask.Method = [THBRpc.FuncMethods]::State;
    $rpcTask.InstanceID = $JobID;

    RpcCall $rpcTask;
    $rpcResult = RecvMsg;

    return($rpcResult);
}

Export-ModuleMember -Function * -Alias *;    # -variable *
