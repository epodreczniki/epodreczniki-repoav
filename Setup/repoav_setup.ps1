# Instalator wężłą RepoAV
# =======================================
param (
$id = $null,
$folder = "C:\RepoAV",
$type = "Snode",
$repodb = $false,
$mfdb = $true,
$MFDBName = 'MaterialFormatDB',
$RDBName = 'RepDB',
$RDBServer = '(local)',
$RDBUser = $null,
$RDBPass = $null,
$IntNetwork = '10.0.0.0/8',
$IntIP = $null,
$ExtIP = $null,
$name = $null,
$otherNodes = $null,
$repsize = 30,
$vdir = 'Repository',
$RAName = "RepositoryAccess",
$RepApiName = "RepAPI",
$repfolder = "C:\Storage",
$port = 8088,
$sapass = "1234"
)

# ładowanie typów
Add-Type -As System.IO.Compression.FileSystem 



# zmienne programow
$global:SqlPackage = ${env:ProgramFiles(x86)} + '\Microsoft SQL Server\110\DAC\bin\SqlPackage.exe'
$global:appcmd = "$env:SystemRoot\system32\inetsrv\appcmd.exe"

# Sprawdznie uprawnień administratora
if( ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator") -eq $false)
{
	Write-Host -ForegroundColor Red "Instalator należy uruchomić z prawami administratora!"
	Exit;
}




#== Functions ========================================================

function Check-InputParameters
{
	$res = $true

	if( $id -eq $null)
	{
		Write-Host -ForegroundColor Red "Nie podano parametru -id"
		$res = $false;
	}

	if( $name -eq $null)
	{
		Write-Host -ForegroundColor Red "Nie podano parametru -name"
		$res = $false;
	}

	if( $IntIP -eq $null)
	{
		Write-Host -ForegroundColor Red "Nie podano parametru -IntIP"
		$res = $false;
	}
	
	if( $ExtIP -eq $null)
	{
		$ExtIP = $IntIP
	}

	
	return $res
}


function Check-SQLServer
{
	$sqlExist = $false;
	$SQLServices = gwmi -query "select * from win32_service where Name LIKE 'MSSQL%' and Description LIKE '%transaction%'"

	forEach ($SQLService in $SQLServices) {
		$sqlExist = $true
	}

	if($sqlExist -eq $false)
	{
		Write-Host -ForegroundColor Red "SQL Serwer nie zainstalowany!"
	}

	return $sqlExist
}

function Get-RoleId
{
  param(
   $type
  )
  
	if($type -eq 'SNode')
	{
		$role = 0
	} 
	if($type -eq 'Recoder')
	{
		$role = 1
	} 
	if($type -eq 'Manager')
	{
		$role = 2
	} 
	if($type -eq 'SNodeRecoder')
	{
		$role = 3
	} 

	return $role;
}

function Get-RDBConnectionString
{
	$cnn = $null
	if($RDBUser -eq $null -or $RDBPass -eq $null)
	{
		$cnn = "Data Source=$RDBServer;Initial Catalog=$RDBName;User ID=App;Password=9oRbcLamOqLbmfm"
	}
	else
	{
		$cnn = "Data Source=$RDBServer;Initial Catalog=$RDBName;User ID=$RDBUser;Password=$RDBPass"
	}
	
	return $cnn
}

function Get-MFDBConnectionString 
{
	return "Data Source=(local);Initial Catalog=$MFDBName;Integrated Security=SSPI"
}

function Write-Help {
	Write-Host -ForegroundColor Yellow "Parametry wywołania:"
	Write-Host "`t-id`t-  numeryczny identyfikator węzła, parametr wymagany"
	Write-Host "`t-name`t-  nazwa węzła, parametr wymagany"
	Write-Host "`t-IntIP`t-  wewnętrzny adres IP, parametr wymagany"
	Write-Host "`t-ExtIP`t-  zewnętrzny adres IP, jężeli nie podano - taki sam jak adres wewnętrzny"
	Write-Host "`t-folder`t- katalog w któym zainstalowany zostanie węzeł repozytorium, domyślnie: C:\RepoAV"
	Write-Host "`t-type`t- typ węzła (Snode, Recoder, Manager, SNodeRecoder), domyślnie: Snode"
	Write-Host "`t-repodb`t- czy instalować centralną bazę RepoDB, domyślnie: 0"
	Write-Host "`t-mfdb`t- czy instalować bazę wężła - MaterialFormatDB, domyślnie: 1"
	Write-Host "`t-MFDBName`t- nazwa bazy danych wężła domyślnie: MaterialFormatDB"
	Write-Host "`t-RDBName`t- nazwa centralnej bazy, domyślnie: RepDB"
	Write-Host "`t-RDBServer`t- nazwa servera centralnej bazy, domyślnie: (local)"
	Write-Host "`t-RDBUser`t- nazwa użytkonika do centralnej bazy, wymagne jęzeli RDBServer różne od (local)"
	Write-Host "`t-RDBPass`t- hasło centralnej bazy, wymagne jęzeli RDBServer różne od (local)"
	Write-Host "`t-IntNetwork `t- adres sieci wewnętrznej domyślnie:  10.0.0.0/8"
	Write-Host "`t-otherNodes `t- lista innych wężłow w systemie postaci: id1:IP1;id2:IP2"	
	Write-Host "`t-repsize `t- rozmiar lokalnego repozytorium w Gb, domyśłnie: 30"	
	Write-Host "`t-RAName `t- nazwa usługi udostępniającego pliki, domyłśnie: RepositoryAccess"	
	Write-Host "`t-RepApiName `t- nazwa usługi API dla repozytorium, domyłśnie: RepAPI"	
	Write-Host "`t-vdir `t- nazwa virtual directory udostępniającego pliki, domyłśnie: Repository"	
	Write-Host "`t-repfolder `t- katalog z dla materiałów, domyłśnie: c:\Storage"	
	Write-Host "`t-port `t- port TCP dla usługi proca, domyłśnie: 8088"	
	Write-Host "`t-sapass `t- hasło administratora w przypadku gdy RepoDB jest na innej maszynie"	
}

function Expand-ZipFile {
  #.Synopsis
  #  Expand a zip file, ensuring it's contents go to a single folder ...
  [CmdletBinding()]
  param(
    # The path of the zip file that needs to be extracted
    [Parameter(ValueFromPipelineByPropertyName=$true, Position=0, Mandatory=$true)]
    [Alias("PSPath")]
    $FilePath,
 
    # The path where we want the output folder to end up
    [Parameter(Position=1)]
    $OutputPath = $Pwd,
 
    # Make sure the resulting folder is always named the same as the archive
    [Switch]$Force
  )
  process {
    $ZipFile = Get-Item $FilePath
    $Archive = [System.IO.Compression.ZipFile]::Open( $ZipFile, "Read" )
 
    # Figure out where we'd prefer to end up
    $Destination = $OutputPath
 
    # The root folder of the first entry ...
    $ArchiveRoot = ($Archive.Entries[0].FullName -Split "/|\\")[0]
 
    Write-Verbose "Desired Destination: $Destination"
    Write-Verbose "Archive Root: $ArchiveRoot"
 
    # If any of the files are not in the same root folder ...
    if($Archive.Entries.FullName | Where-Object { @($_ -Split "/|\\")[0] -ne $ArchiveRoot }) {
      # extract it into a new folder:
      New-Item $Destination -Type Directory -Force
      [System.IO.Compression.ZipFileExtensions]::ExtractToDirectory( $Archive, $Destination )
    } else {
      # otherwise, extract it to the OutputPath
      [System.IO.Compression.ZipFileExtensions]::ExtractToDirectory( $Archive, $OutputPath )
 
      # If there was only a single file in the archive, then we'll just output that file...
      if($Archive.Entries.Count -eq 1) {
        # Except, if they asked for an OutputPath with an extension on it, we'll rename the file to that ...
        if([System.IO.Path]::GetExtension($Destination)) {
          Move-Item (Join-Path $OutputPath $Archive.Entries[0].FullName) $Destination
        } else {
          Get-Item (Join-Path $OutputPath $Archive.Entries[0].FullName)
        }
      } elseif($Force) {
        # Otherwise let's make sure that we move it to where we expect it to go, in case the zip's been renamed
        if($ArchiveRoot -ne $ZipFile.BaseName) {
          Move-Item (join-path $OutputPath $ArchiveRoot) $Destination
          Get-Item $Destination
        }
      } else {
        Get-Item (Join-Path $OutputPath $ArchiveRoot)
      }
    }
 
    $Archive.Dispose()
  }
}

function Change-RAConfig {
  param(
   $RAName,
   $RAFolder,
   $LogFolder,
   $MFDBName,
   $RDBConnString,
   $MFDBConnString
  )
  process {
  
  	Write-Host -ForegroundColor Cyan  "Modyfikacja pliku Web.config dla RepositoryAccess..."
  
	$web_conf = New-Object System.Xml.XmlDocument 
	$web_conf.Load( "$($RAFolder)\Web.config" )
	$node = $web_conf.SelectSingleNode("/configuration/loggingConfiguration/listeners/add");
	if($node -ne $null)
	{
		$logFile = "$($LogFolder)\$($RAName).log"
		$node.SetAttribute("fileName", "$($logFile)") 
	}
	$node = $web_conf.SelectSingleNode("/configuration/connectionStrings/add[@name='ContentDB']");
	if($node -ne $null)
	{
		$node.SetAttribute("connectionString", $MFDBConnString) 
	}
	$node = $web_conf.SelectSingleNode("/configuration/connectionStrings/add[@name='RepoDB']");
	if($node -ne $null)
	{
		$node.SetAttribute("connectionString", $RDBConnString) 
	}

	$node = $web_conf.SelectSingleNode("/configuration/appSettings/add[@key='NetworkAddressAndMaskWithoutAuthorization']");
	if($node -ne $null)
	{
		$node.SetAttribute("value", $IntNetwork) 
	}

	$node = $web_conf.SelectSingleNode("/configuration/repositoryNodesConfiguration/repositoryNodes");
	if($node -ne $null)
	{
		$node.RemoveAll()
	
		$node.SetAttribute("thisNodeId", "$id") 
		
		$chnode = $web_conf.CreateElement("add")
		$chnode.SetAttribute("id", $id) 
		$chnode.SetAttribute("enabled", "true") 
		$chnode.SetAttribute("address", $IntIP) 
		$node.AppendChild($chnode)
		
		if($otherNodes -ne $null)
		{
			$onode = $otherNodes.Split(';')
			foreach($on in $onode)
			{
				$ondata = $on.Split(':')
				if($ondata.length -eq 2)
				{
					$chnode = $web_conf.CreateElement("add")
					$chnode.SetAttribute("id", $ondata[0]) 
					$chnode.SetAttribute("enabled", "true") 
					$chnode.SetAttribute("address", $ondata[1]) 
					$node.AppendChild($chnode)	
				}
			}
		}
	}


	$web_conf.Save("$($RAFolder)\Web.config")
  

  }
}

function Change-RepAPIConfig {
  param(
   $RAFolder,
   $LogFolder,
   $RDBConnString,
   $MFDBConnString
  )
  process {
  
  	Write-Host -ForegroundColor Cyan  "Modyfikacja pliku Web.config dla RepAPI..."
  
	$web_conf = New-Object System.Xml.XmlDocument 
	$web_conf.Load( "$($RAFolder)\Web.config" )
	$node = $web_conf.SelectSingleNode("/configuration/loggingConfiguration/listeners/add");
	if($node -ne $null)
	{
		$logFile = "$($LogFolder)\RepAPI.log"
		$node.SetAttribute("fileName", "$($logFile)") 
	}
	$node = $web_conf.SelectSingleNode("/configuration/connectionStrings/add[@name='DefaultConnection']");
	if($node -ne $null)
	{
		$node.SetAttribute("connectionString", $RDBConnString) 
	}

	$web_conf.Save("$($RAFolder)\Web.config")
  
  }
}




function Change-ProcaConfig {
  param(
   $procaFolder,
   $port,
   $name
  )
  process {
  
  	Write-Host -ForegroundColor Cyan  "Modyfikacja pliku Proca3.exe.config..."
  
	$web_conf = New-Object System.Xml.XmlDocument 
	$web_conf.Load( "$($procaFolder)\Proca3.exe.config" )
	
	$node = $web_conf.SelectSingleNode("/configuration/loggingConfiguration/listeners/add");
	if($node -ne $null)
	{
		$logFile = "$($LogFolder)\Proca.log"
		$node.SetAttribute("fileName", "$($logFile)") 
	}
	
	$node = $web_conf.SelectSingleNode("/configuration/serviceConfiguration");
	if($node -ne $null)
	{
		$node.SetAttribute("port", $port) 
		$node.SetAttribute("name", $name) 
	}

	$web_conf.Save("$($procaFolder)\Proca3.exe.config") 
  }
}

function Register-IISApp {
  param(
   $name,
   $folder
  )
  process {
	& $global:appcmd list vdir /app.name:"Default Web Site/$name" /physicalPAth:"$folder" | Out-Null


	if($LASTEXITCODE -eq 0) {
		Write-Host "Usługa $name juz istnieje isnieje"
		Write-Host Usuwannie $name
		& $global:appcmd delete app /app.name:"Default Web Site/$name"
	}

	& $global:appcmd add app /site.name:"Default `Web Site" /path:/$name /physicalPath:"$folder" 
  }
}


function Prepare-Folders {
  param(
   $mainfolder,
   $subfolders
  )
  process {
	$driveletter = Split-Path $mainfolder -Qualifier

	
	if( (New-Object System.IO.DriveInfo($driveletter)).DriveType -ne 'NoRootDirectory' -eq $false)
	{
		Write-Host -ForegroundColor Red "Dysk $driveletter nie istnieje!"
		return $false
	}
	
	if((Test-Path $folder) -eq $false)
	{
		Write-Host -ForegroundColor Gray  $folder
		New-Item -ItemType directory -Path $folder | Out-Null
	}
	if((Test-Path $folder) -eq $false)
	{
		Write-Host -ForegroundColor Red "Nie udało się utworzenie folderu: " $folder
		return $false
	}


	forEach ($subfolder in $subFolders) {
		if((Test-Path $subfolder) -eq $false)
		{
			Write-Host -ForegroundColor Gray  $subfolder
			New-Item -ItemType directory -Path $subfolder | Out-Null
		}
		
	}


	
	Remove-Item $tmpFolder\* -Recurse


  
  	return $true
  }
}
function Write-CurrentParameter {
	Write-Host -ForegroundColor Yellow "Instalator RepoAV..."

	Write-Host -ForegroundColor Gray  "`tInstalacja w folderze: `t" $folder
	Write-Host -ForegroundColor Gray  "`tIdentyfikator węzła: `t" $id
	Write-Host -ForegroundColor Gray  "`tTyp węzła:`t" $type 
	Write-Host -ForegroundColor Gray  "`tBaza RepDB bęzie instalowana:`t" $repodb
	Write-Host -ForegroundColor Gray  "`tBaza MAterialFormat DB bęzie instalowana:`t" $mfdb
	Write-Host -ForegroundColor Gray  "`tNazwa lokalnej bazy repozytorium:`t" $MFDBName 
	Write-Host -ForegroundColor Gray  "`tNazwa centralnej bazy repozytorium:`t" $RDBName 
	Write-Host -ForegroundColor Gray  "`tNazwa serwera centralnej bazy repozytorium:`t" $RDBServer 
	Write-Host -ForegroundColor Gray  "`tNazwa użytkownika centralnej bazy repozytorium:`t" $RDBUser 
	Write-Host -ForegroundColor Gray  "`tAdres sieci wewnętrznej:`t" $IntNetwork 
	Write-Host -ForegroundColor Gray  "`tAdres IP w sieci wewnętrznej:`t" $IntIP 
	Write-Host -ForegroundColor Gray  "`tAdres IP w sieci zewnętrznej:`t" $ExtIP 
	Write-Host -ForegroundColor Gray  "`tInne węzły:`t" $otherNodes
	Write-Host -ForegroundColor Gray  "`tMaksymalny rozmiar repozytorium:`t" $repsize 
	Write-Host -ForegroundColor Gray  "`tKatalog wirtualny repozytorium:`t" $vdir 
	Write-Host -ForegroundColor Gray  "`tUsługa udistępniająca materiały:`t" $RAName 
	Write-Host -ForegroundColor Gray  "`tUsługa API Repozytorium:`t" $RepApiName 
	Write-Host -ForegroundColor Gray  "`tKatalog wirtualny repozytorium:`t" $RepAPIN
	Write-Host -ForegroundColor Gray  "`tFolder dla materiałow w repozytoim:`t" $repfolder
	Write-Host -ForegroundColor Gray  "`tPort usługi Proca:`t" $port 
}

#== End Functions ===============================================================
	Write-Help

# Start ###################################################

# jezeli nie podano parametrow to wyświetlamy pomoc
if($psboundparameters.count -eq 0) {
	Write-Help
	Exit;	
}


# sprawdzenie parametró wywołania
if((Check-InputParameters) -eq $false)
{
	Exit;
}


# # test obecności SQL Serwera
if((Check-SQLServer) -eq $false)
{
	Exit;
}

# oblicznie zmiennych
$role = Get-RoleId $type
$RDBConnectionString = Get-RDBConnectionString
$MFDBConnectionString = Get-MFDBConnectionString


Write-CurrentParameter

#=======================================================================

Write-Host -ForegroundColor Green "1 - Tworzenie folderów"

# Foldery
$tmpFolder = $folder + '\temp'
$srvFolder = $folder + '\Services'
$logFolder = $folder + '\Logs'
$dbFolder = $folder + '\Database'
$procaFolder = $folder + '\Proca\'
$recToolsFolder = $folder + '\RecoderTools\'

$RAFolder = $srvFolder + '\RepositoryAccess\'
$RepApiFolder = $srvFolder + '\RepAPI\'


$subFolders = @(
	$tmpFolder,
	$srvFolder,
	$logFolder,
	$dbFolder,
	$procaFolder,
	$recToolsFolder,
	$RAFolder,
	$RepApiFolder
)

if((Prepare-Folders $folder $subFolders) -eq $false)
{
	Exit;
}


#=2=====================================================================
#Bazy danych

Write-Host -ForegroundColor Green "2 - Bazy danych ..."
if($repodb -eq $true)
{
	Write-Host "Tworzenie bazy danych RepoDB..."
	& sqlcmd -i .\install\RepDB.sql  -v Folder = """$folder""" -v dbname = """$RDBName"""
	Expand-ZipFile '.\install\RepDB.zip' $tmpFolder | Out-Null
	& $global:SqlPackage /Action:Publish  /SourceFile:”$($tmpFolder)/RepDB.dacpac” /TargetDatabaseName:$($RDBName) /TargetServerName:”.”
	& sqlcmd -i .\EPRecode.sql

}

if($mfdb -eq $true)
{
	Write-Host -ForegroundColor Green "Tworzenie bazy danych MaterialFormatDB..."
	& sqlcmd -i .\install\MaterialFormatDB.sql  -v Folder = """$folder""" -v dbname = """$MFDBName"""
	Expand-ZipFile '.\install\MaterialFormatDB.zip' $tmpFolder | Out-Null
	& $global:SqlPackage /Action:Publish  /SourceFile:”$($tmpFolder)/MaterialFormatDB.dacpac” /TargetDatabaseName:$($MFDBName) /TargetServerName:”.”
	
	
}

Remove-Item $tmpFolder\* -Recurse


#=3=====================================================================
Write-Host -ForegroundColor Green "3 - Nadanie praw dla NT AUTHORITY\SYSTEM..."
& sqlcmd -E -d master -Q "EXEC sp_addsrvrolemember 'NT AUTHORITY\SYSTEM', 'sysadmin'"




#=4=====================================================================


if($RDBName -ne $null)
{
	Write-Host -ForegroundColor Green "4 - Rejestracja węzła bazie danych RepoDB..."

	$raUrl = "http://{IpAddress}:80/$RAName/{UniqueId}"

	$cmd = "Execute [dbo].[AddNode] @id=$id,@Role=$role,@ExternalAddress='$ExtIP',@InternalAddress='$IntIP', @Url='$raUrl',@Enabled=1,@Name='$name',@ProcaPortNumber=$port" 
	& sqlcmd -S $RDBServer -U sa -P $sapass -d $RDBName -Q $cmd

	Write-Host -ForegroundColor Green "5 - Wpisy w tabeli GlobalData..."
	
	if(($role -eq 0) -or ($role -eq 3)) 
	{
		$RAAddress = "http://$ExtIP/$RAName/"
		$cmd = "Execute [dbo].[SetGlobalData] @Key = N'RepositoryAccessNLB', @Value = N'$RAAddress', @Description = NULL"
		& sqlcmd -S $RDBServer -U sa -P $sapass  -d $RDBName -Q $cmd
	}
	
	$cmd = "Execute [dbo].[SetGlobalData] @Key = N'APIService', @Value = N'$RepApiName', @Description = NULL"
	& sqlcmd -S $RDBServer -U sa -P $sapass -d $RDBName -Q $cmd

	$ManagerAddress = "http://$($ExtIP):$port/Manager/"
	
	$cmd = "Execute [dbo].[SetGlobalData] @Key = N'ManagerAPINLB', @Value = N'$ManagerAddress', @Description = NULL"
	& sqlcmd -S $RDBServer -U sa -P $sapass -d $RDBName -Q $cmd
}


if($MFDBName -ne $null)
{
	Write-Host -ForegroundColor Green "5 - Wpisy w tabeli GlobalData w bazie MaterialFormat..."
	
	$cmd = "Execute [dbo].[SetGlobalData] @Key = N'LocalRepositoryPath', @Value = N'$repfolder', @Description = NULL"
	& sqlcmd -E -d $MFDBName -Q $cmd

	$cmd = "Execute [dbo].[SetGlobalData] @Key = N'RepositorySize', @Value = $repsize, @Description = NULL"
	& sqlcmd -E -d $MFDBName -Q $cmd

	$cmd = "Execute [dbo].[SetGlobalData] @Key = N'RepositoryVirtualDir', @Value = $vdir, @Description = NULL"
	& sqlcmd -E -d $MFDBName -Q $cmd
}



#=======================================================================

Write-Host -ForegroundColor Green "6 - Tworzenie aplikacji RapositoryAccess..."

Remove-Item $RAFolder -Recurse
Expand-ZipFile ".\install\RepositoryAccess.zip" "$RAFolder"  | Out-Null

#Web.config dla RepositoryAccess
Change-RAConfig -RAName $RAName -RAFolder $RAFolder -LogFolder $logFolder -MFDName  $MFDBName -RDBName -RDBConnString $RDBConnectionString -MFDBConnString $MFDBConnectionString

Register-IISApp -name $RAName -folder $RAFolder


#=======================================================================

Write-Host -ForegroundColor Green "7 - Tworzenie aplikacji RepAPI..."

Remove-Item $RepApiFolder -Recurse
Expand-ZipFile ".\install\RepAPI.zip" "$RepApiFolder"  | Out-Null

#Web.config dla RepositoryAccess
Change-RepAPIConfig -RAFolder $RepApiFolder -LogFolder $logFolder -RDBConnString $RDBConnectionString -MFDBConnString $MFDBConnectionString

Register-IISApp -name $RepApiName -folder $RepApiFolder


#=======================================================================

Write-Host -ForegroundColor Green "8 - Tworzenie Virtual Directory dla udosepniania materiałów ..."
if((Test-Path $repfolder) -eq $false)
{
	New-Item -ItemType directory -Path $repfolder | Out-Null
}
& $global:appcmd delete  vdir /vdir.name:"Default Web Site/$RAName/$vdir" 
& $global:appcmd add vdir /app.name:"Default Web Site/$RAName" /path:/$vdir /physicalPath:"$repfolder"


#=======================================================================

Write-Host -ForegroundColor Green "9 - Repository Tools ..."
if((Test-Path $recToolsFolder) -eq $false)
{
	New-Item -ItemType directory -Path $recToolsFolder | Out-Null
}
Remove-Item $recToolsFolder -Recurse
Expand-ZipFile ".\install\RecoderTools.zip" "$recToolsFolder"  | Out-Null


#=======================================================================
# Proca 
Write-Host -ForegroundColor Green "10 - Rejestracja mime type..."

& $global:appcmd set config -section:staticContent /+"[fileExtension='.vtt',mimeType='text/vtt']"

 
#=======================================================================
# Proca 
Write-Host -ForegroundColor Green "11 - Tworzenie usługi Proca..."

$procaServiceName = $name

$ProcaServices = gwmi -query "select * from win32_service where Name LIKE '$procaServiceName'" 
$procaExist = $false;

forEach ($ProcaService in $ProcaServices) {
	$procaExist = $true
}

if($procaExist) {
	Write-Host -ForegroundColor Magenta "Usługa już istnieje. Usuwanie";
	& sc.exe stop "$procaServiceName";
	
	Start-Sleep -s 6 
	
	& sc.exe delete "$procaServiceName";
}


Remove-Item  $procaFolder\* -Recurse	
Expand-ZipFile '.\install\Proca-Node.zip' $procaFolder  | Out-Null
Change-ProcaConfig -procaFolder $procaFolder -port $port -name $name 

#plik konfiguracyjny podsystemu LocalNode
Write-Host -ForegroundColor Cyan "Plik konfiguracyjny podsystemu LocalNode"
if((Test-Path "$procaFolder\Config") -eq $false)
{
	New-Item -ItemType directory -Path "$procaFolder\Config" | Out-Null
}
Copy-Item ".\install\LocalNode.xml" "$procaFolder\Config"

#ustawienie parametrów w local node
$localNodeConfig = "$procaFolder\Config\LocalNode.xml"
(Get-Content "$localNodeConfig") -replace '\%RepoDBConnection\%',"$RDBConnectionString" | Set-Content $localNodeConfig
(Get-Content "$localNodeConfig") -replace '\%MFDBConnection\%',"$MFDBConnectionString" | Set-Content $localNodeConfig
(Get-Content "$localNodeConfig") -replace '\%NodeId\%',"$id" | Set-Content $localNodeConfig


#plik konfiguracyjny podsystemu Recoder
Write-Host -ForegroundColor Cyan "Plik konfiguracyjny podsystemu Recoder"
if((Test-Path "$procaFolder\Config") -eq $false)
{
	New-Item -ItemType directory -Path "$procaFolder\Config" | Out-Null
}
Copy-Item ".\install\Recoder.xml" "$procaFolder\Config"

#ustawienie parametrów w Recoder.xml
$recoderConfig = "$procaFolder\Config\Recoder.xml"
(Get-Content "$recoderConfig") -replace '\%TempDirectoryPath\%',"$tmpFolder" | Set-Content $recoderConfig
(Get-Content "$recoderConfig") -replace '\%ToolsDirectoryPath\%',"$recToolsFolder" | Set-Content $recoderConfig



#=======================================================================
Write-Host -ForegroundColor Green "12 - Instalacja usługi Proca - $procaServiceName ..."
& "$($procaFolder)\Proca3.exe" -i

#=======================================================================
Write-Host -ForegroundColor Green "13 - Startowanie usługi Proca - $procaServiceName ..."
& net start $procaServiceName

