Add-Type -TypeDefinition @"
   public enum DBTYPES
   {
      MySQL,
      PGSql,
      MSSql
   }
"@

#Vars
[DBTYPES]$dbBackend = [DBTYPES]::PGSql;
$dbDateFunc = "";
#$dbTblName

$dbhost="10.20.30.40";
$dbport=5432;
$dbname="postgres";
$dbuser="postgres";
$dbpass="***";
##############################

switch ($dbBackend) {
    "MySql" {
        $dbDateFunc = "NOW()";
        continue;
    }
    "PGSql" {
        $dbDateFunc = "LOCALTIMESTAMP";
        continue;
    }
    "MSSql" {
        $dbDateFunc = "GetDate()";
        continue;
    }
}

#Check if Modul is loaded
If (!(Get-module "SimplySql")) {
    #Install-Module -Name SimplySQL
    Import-Module SimplySql
}

function OpenDBConnection() {

    switch ($dbBackend) {
        "MySql" {
            # Server=myServerAddress;Port=1234;Database=myDataBase;Uid=myUsername;Pwd=***;default command timeout=120;Connection Timeout=60;
            # TCP-Keepalives:  Keepalive=10;
            # UseCompression=True;
            # for more settings see: https://www.connectionstrings.com/mysql/ 
            Open-MySqlConnection -ConnectionString "Server=$($dbhost);Port=$($dbport);Database=$($dbname);Uid=$($dbuser);Pwd=$($dbpass);Connection Timeout=60;" -CommandTimeout 120;
            continue;
        }
        "PGSql" {
            Open-PostGreConnection -ConnectionString "Server=$($dbhost);Port=$($dbport);Database=$($dbname);User Id=$($dbuser);Password=$($dbpass);Timeout=60;" -CommandTimeout 120;
            continue;
        }
        "MSSql" {
            # Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=***;Trusted_Connection=False;Packet Size=4096;
            # Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=***;
            # ODBC:
            #   Driver={SQL Server Native Client 11.0};Server=tcp:myServerAddress\Instance,$($dbport);Database=$($dbname);Uid=$($dbuser);Pwd=$($dbpass);
            Open-SqlConnection -ConnectionString "Server=$($dbhost);Database=$($dbname);User Id=$($dbuser);Password=$($dbpass);Timeout=60;" -CommandTimeout 120;
            continue;
        }
    }
}
