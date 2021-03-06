# Instalator wężłą RepoAV
# =======================================
param (
$folder = "C:\RepoAV",
$name = $null
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



function Write-Help {
	Write-Host -ForegroundColor Yellow "Parametry wywołania:"
	Write-Host "`t-folder`t- katalog w któym zainstalowany jest węzeł repozytorium, domyślnie: C:\RepoAV"
	Write-Host "`t-name`t-  nazwa węzła, parametr wymagany"
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
      #[System.IO.Compression.ZipFileExtensions]::ExtractToDirectory( $Archive, $Destination )
	  
		foreach ($entry in $Archive.Entries) {
			$entryFullname = [System.IO.Path]::Combine($Destination, $entry.FullName)
			$entryPath = [System.IO.Path]::GetDirectoryName($entryFullName)
			if ( [System.IO.Directory]::Exists($entryPath) -eq $false) {
                    [System.IO.Directory]::CreateDirectory($entryPath)
            }
			
			
			$entryFn = [System.IO.Path]::GetFileName($entryFullname)
                if( [System.String]::IsNullOrEmpty($entryFn) -eq $false) {
                    [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $entryFullname, $true)
					Write-Host $entryFullname
                }
		}
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
}

#== End Functions ===============================================================


# Start ###################################################

# jezeli nie podano parametrow to wyświetlamy pomoc
if($psboundparameters.count -eq 0) {
	Write-Help
	Exit;	
}





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






#=======================================================================

Write-Host -ForegroundColor Green "2 - Update aplikacji RapositoryAccess..."

Expand-ZipFile ".\update\RepositoryAccess.zip" "$RAFolder" | Out-Null



#=======================================================================

Write-Host -ForegroundColor Green "3 - Update aplikacji RepAPI..."

Expand-ZipFile ".\update\RepAPI.zip" "$RepApiFolder"  | Out-Null


 
#=======================================================================
# Proca 
Write-Host -ForegroundColor Green "5 - Update usługi Proca..."

$procaServiceName = $name

& net stop "$procaServiceName";
	
Expand-ZipFile '.\update\Proca-Node.zip' $procaFolder  | Out-Null


#=======================================================================
#Write-Host -ForegroundColor Green "6 - Startowanie usługi Proca - $procaServiceName ..."
#& net start $procaServiceName

