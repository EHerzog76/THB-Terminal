option explicit

Dim AppPath, AppFileName
Dim Session
Dim strResults, strLine, IPAdr

	'call IsHostCscript()
	call GetAppPath()
	WScript.Echo AppPath
	call parseCmdLine()

	Set Session = CreateObject("THB.Terminal.TelnetSession")
    Session.Debug = 1

	AppFileName = Left(WScript.ScriptName, InStrRev(WScript.ScriptName, ".")-1)
    'Read Config-File
    if( NOT Session.Init(AppPath & AppFileName & ".conf") ) Then
		WScript.Echo "Error: Could not read Config-File: " & AppPath & AppFileName & ".conf"
		WScript.Quit(1)
    end if

    '10.229.108.50, 172.28.223.140 172.28.223.138, 172.25.150.29, 172.25.163.2, "172.25.159.162" od.: 172.28.223.138, 172.19.7.1
    if (Session.doLoginObj("172.28.223.140", "") <> 0)	Then 'TelnetDevice.LoginStatus.Success
        WScript.Quit(1)
    end if

    Session.Debug = 0
    
	strResults = Session.SendCmdObj("show run")
	WScript.Echo "Lines: " & UBound(strResults)
	for each strLine in strResults
		WScript.Echo strLine
	next

	Session.Debug = 1
	'if (Session.Jump2NextDeviceObj("172.28.223.139", "") = 0) Then	'TelnetDevice.LoginStatus.Success
	'	Session.SendCmdObj "show version"
	'	Session.doLogout
	'end if

	call Session.GetIntoConfigModeObj()
	if(Session.GetDeviceType = "HP" ) Then
		strResults = Session.SendCmdObj("show version")
	else
		strResults = Session.SendCmdObj("do show version")
	end if

	for each strLine in strResults
		WScript.Echo strLine
	next
  
	Session.ExitConfigModeObj
      
	if(Session.GetDeviceType = "HP" ) Then
		strResults = Session.SendCmdObj("show time")
	else
		strResults = Session.SendCmdObj("show clock")
	end if
	for each strLine in strResults
		WScript.Echo strLine
	next


	'Query Interface
	strResults = GetIFDetails("GigabitEthernet2/0/0", "ip address")
	for each strLine in strResults
		if(RegExCompare(strLine, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+ [0-9]+\..*") ) Then
			IPAdr = Split(strLine, " ")
			WScript.Echo "IP-Net: " & Session.GetIPNet(IPAdr(0), IPAdr(1))
			WScript.Echo "IP+1: " & Session.Add2IP(IPAdr(0), 1)
		end if
	Next
	
	Session.doLogout

WScript.Quit(0)
'*********************** End of Main ***********************************

'**************************************************************
' Start of Helper-Functions
'**************************************************************
'**************************************************************
' GetIFDetails
'	IFName, IFParam => e.g. description, bandwidth, ip address, ...
'**************************************************************
Function GetIFDetails(l_IfName, l_IfParam)
Dim strVar
Dim l_Counter
Dim strLines, resultArray(1)

  'Read Interface-Config
  strLines = Session.SendCmdObj("show running-config interface " & l_IfName)
  
  l_Counter = 1
  for each strVar in strLines
	if( RegExCompare(strVar, "^ " & l_IfParam & " .*$") ) Then
		strVar = Replace(strVar, " " & l_IfParam & " ", "")  ' strVar =~ s/^ $l_IfParam (.*)$/$1/
		if(RegExCompare(strVar, " secondary") ) Then
		  strVar = Replace(strVar, " secondary", "")
		  Redim resultArray(l_Counter+1)
		  
		  resultArray(l_Counter) = strVar
		else
		  resultArray(0) = strVar
		end if
		l_Counter = l_Counter+1
  	end if
  Next
  'if( $fDebug ) {
  '  print($l_IfParam.": ")

   ' $l_Counter = 0
   ' for $l_Counter (0 .. $#resultArray ) {
   '   printf("\t%s\n", $resultArray[$l_Counter])
   ' }
   ' print "\n"
  '}
  
  GetIFDetails = resultArray
End Function

'**************************************************************
' Check4CfgError
'   Params: Output from last Config-Command as Array
'
'   Check for IOS Config-Error
'   Return FALSE on Success or TRUE on Error
'**************************************************************
Function Check4CfgError(l_cmdResults)
Dim l_obj, l_result
  
  l_result = False
  if( UBound(l_cmdResults) > 0 ) Then
    for each l_obj in l_cmdResults
      'if( $DEBUG > 1) { print "Check for Config-Error: ".$l_obj."\n"; }
      if(RegExCompare(l_obj, "\% Invalid input detected .*\^.*marker.") ) Then
		l_result = True
	  end if
    next
  end if
  
  Check4CfgError = l_result
End Function

'**************************************************************
' RegExCompare
'
'	RegEx-Filter
'**************************************************************
Function RegExCompare(l_SrcString, MatchPattern)
Dim regEx, bResult

	Set regEx = New RegExp
	
	regEx.Pattern = MatchPattern
  	regEx.IgnoreCase = true		' ignore case
	bResult = False

	if(regEx.Test(l_SrcString) ) Then
		bResult = True
		'WScript.Echo "Found Match"
	end if
	
	RegExCompare = bResult
End Function

'**************************************************************
' Determines which program is being used to run this script. 
' Returns true if the script host is cscript.exe
'**************************************************************
function IsHostCscript()
on error resume next
    
dim strFullName 
dim strCommand 
dim i, j 
dim bReturn
    
    bReturn = false    
    strFullName = WScript.FullName
    i = InStr(1, strFullName, ".exe", 1)
    if i <> 0 then        
        j = InStrRev(strFullName, "\", i, 1)
        if j <> 0 then
            strCommand = Mid(strFullName, j+1, i-j-1)
            if LCase(strCommand) = "cscript" then
                bReturn = true
	    else
		wscript.echo "This Script have to be startet with CSCRIPT !"
		WScript.Quit(1)
            end if  
        end if
    end if
    if Err <> 0 then
        wscript.echo "This Script have to be startet with CSCRIPT !"
	WScript.Quit(1)
    end if
    
    IsHostCscript = bReturn
end function

'*****************************************
'Sub parseCmdLine:
'	parses the Commandline-Parameters
'*****************************************
sub parseCmdLine()
Dim i
Dim Args

	Set Args = WScript.Arguments
	for i = 0 To Args.Count -1
		if(LCase(Args(i)) = "/?") Then
			call ShowHelp()
		end if
	next
End Sub

'*****************************************
'Sub GetAppPath:
'	Gets the Applicationpath
'*****************************************
Sub GetAppPath()
Dim strLen

	strLen = LEN(WScript.ScriptFullName) - LEN(WScript.ScriptName)
	AppPath = Left(WScript.ScriptFullName, strLen)
End Sub

Sub ShowHelp()
	WScript.Echo ""
	WScript.Echo "Usage of " & WScript.ScriptName & ":"
	WScript.Echo vbtab & "/?...............Show Help."
	WScript.Echo ""
End Sub
