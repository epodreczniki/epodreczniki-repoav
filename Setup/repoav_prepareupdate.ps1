
param (
$conf = "Release",
$copyPDB = $true
)

Add-Type -As System.IO.Compression.FileSystem


$filestocopyCommon = @(
	'Proca3.exe',
	'Subsystem.dll',
	'Subsystem.Interface.dll',
	'BaseDBAccess.dll',
	'RepDBAccess.dll',
	'PSNC.Util.dll',
	'Common.dll', 
	'TaskQueue.dll', 	
	'MaterialFormatDBAccess.dll',
	'Microsoft.Practices.EnterpriseLibrary.Logging.dll', 
	'Microsoft.Practices.EnterpriseLibrary.Common.dll', 
	'Newtonsoft.Json.dll',
	'MediaParser.dll',
	'MediaInfoWrapper.dll'
)

$filestocopyCommonPDB = @(
	'Proca3.pdb',
	'Subsystem.pdb',
	'Subsystem.Interface.pdb',
	'BaseDBAccess.pdb',
	'RepDBAccess.pdb',
	'PSNC.Util.pdb',
	'Common.pdb', 
	'TaskQueue.pdb', 	
	'MaterialFormatDBAccess.pdb',
	'MediaParser.pdb',
	'MediaInfoWrapper.pdb'
)

$filestocopyMNode = @(
	'Manager.dll',
	'Manager.Interface.dll'
)

$filestocopyMNodePDB = @(
	'Manager.pdb',
	'Manager.Interface.pdb'
)

$filestocopySNode = @(
	'SNode.dll',
	'SNode.Interface.dll'
)

$filestocopySNodePDB = @(
	'SNode.pdb',
	'SNode.Interface.pdb'
)

$filestocopyRNode = @(
	'Recoder.dll',
	'Recoder.Interface.dll'
)

$filestocopyRNodePDB = @(
	'Recoder.pdb',
	'Recoder.Interface.pdb'
)

$filestocopyLib = @(
	'MediaInfo64.dll',
	'MediaInfo32.dll'
)


function New-ZipFile {
	#.Synopsis
	#  Create a new zip file, optionally appending to an existing zip...
	[CmdletBinding()]
	param(
		# The path of the zip to create
		[Parameter(Position=0, Mandatory=$true)]
		$ZipFilePath,
 
		# Items that we want to add to the ZipFile
		[Parameter(Position=1, Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
		[Alias("PSPath","Item")]
		[string[]]$InputObject = $Pwd,
 
		# Append to an existing zip file, instead of overwriting it
		[Switch]$Append,
 
		# The compression level (defaults to Optimal):
		#   Optimal - The compression operation should be optimally compressed, even if the operation takes a longer time to complete.
		#   Fastest - The compression operation should complete as quickly as possible, even if the resulting file is not optimally compressed.
		#   NoCompression - No compression should be performed on the file.
		[System.IO.Compression.CompressionLevel]$Compression = "Optimal"
	)
	begin {
		# Make sure the folder already exists
		[string]$File = Split-Path $ZipFilePath -Leaf
		[string]$Folder = $(if($Folder = Split-Path $ZipFilePath) { Resolve-Path $Folder } else { $Pwd })
		$ZipFilePath = Join-Path $Folder $File
		# If they don't want to append, make sure the zip file doesn't already exist.
		if(!$Append) {
			if(Test-Path $ZipFilePath) { Remove-Item $ZipFilePath }
		}
		$Archive = [System.IO.Compression.ZipFile]::Open( $ZipFilePath, "Update" )
	}
	process {
		foreach($path in $InputObject) {
			foreach($item in Resolve-Path $path) {
				# Push-Location so we can use Resolve-Path -Relative
				Push-Location (Split-Path $item)
				# This will get the file, or all the files in the folder (recursively)
				foreach($file in Get-ChildItem $item -Recurse -File -Force | % FullName) {
					# Calculate the relative file path
					$relative = (Resolve-Path $file -Relative).Replace(".\temp\", "")
					# Add the file to the zip
					$null = [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($Archive, $file, $relative, $Compression)
				}
				Pop-Location
			}
		}
	}
	end {
		$Archive.Dispose()
		Get-Item $ZipFilePath
	}
}

function Copy-ToTemp {
	[CmdletBinding()]
	param(
		[Parameter(Position=0, Mandatory=$true)]
		$fileList,
		
		[Parameter(Position=1, Mandatory=$true)]
		$inputPath
	)
	process {
		foreach($item in $fileList) {
			$filePath = "..\$($inputPath)\$item"
			if(Test-Path $filePath)
			{
				Write-Host "Kopiowanie " $item
				Copy-Item $filePath .\temp
			}
		}
	}
}

Write-Host -ForegroundColor Yellow "RepoAV - przygotowanie pakietu uaktulnienia..."


if((Test-Path update) -eq $false)
{
	New-Item -ItemType directory -Path update | Out-Null
}

if((Test-Path temp) -eq $false)
{
	New-Item -ItemType directory -Path temp | Out-Null
}

# Rebuild całego projektu
Write-Host -ForegroundColor Yellow "Kompilowanie projektu..."

$msbuild = Join-Path $Env:windir "\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
$devenv = Join-Path (Split-Path $Env:VS120COMNTOOLS -parent) "\IDE\devenv.com"
$solution = "..\RepoAV\RepoAV.sln"
$adminpanelProj = "..\RepoAV\AdminPanel\AdminPanel.csproj"
$repapiProj = "..\RepoAV\RepApi\RepApi.csproj"
$raProj = "..\RepoAV\RepositoryAccess\RepositoryAccess.csproj"


& $msbuild $solution  /p:Configuration=$conf /p:Platform="Any CPU" /t:Rebuild   /p:VisualStudioVersion=12.0 


Write-Host -ForegroundColor Yellow "Pakowanie plików..."


Write-Host -ForegroundColor White "Proca-SNode"

Copy-ToTemp $filestocopyCommon "RepoAV\bin\$conf"
Copy-ToTemp $filestocopyMNode  "RepoAV\bin\$conf"
Copy-ToTemp $filestocopySNode  "RepoAV\bin\$conf"
Copy-ToTemp $filestocopyRNode "RepoAV\bin\$conf"
Copy-ToTemp $filestocopyLib "RepoAV\Lib"

if($copyPDB -eq $true)
{
	Copy-ToTemp $filestocopyCommonPDB "RepoAV\bin\$conf"
	Copy-ToTemp $filestocopyMNodePDB  "RepoAV\bin\$conf"
	Copy-ToTemp $filestocopySNodePDB  "RepoAV\bin\$conf"
	Copy-ToTemp $filestocopyRNodePDB "RepoAV\bin\$conf"
}

New-Zipfile -ZipFilePath  '.\update\Proca-Node.zip' -InputObject '.\temp' | Out-Null
Remove-Item .\temp\* -recurse


#Tworzenie paczki dla RepositoryAccess
& $msbuild  $raProj /p:DeployOnBuild=true /p:PublishProfile=..\RepositoryAccess\Properties\PublishProfiles\Install.pubxml /p:VisualStudioVersion=12.0 
Remove-Item .\temp\*.config 
New-Zipfile -ZipFilePath  '.\update\RepositoryAccess.zip' -InputObject '.\temp' | Out-Null
Remove-Item .\temp\* -recurse



#Tworzenie paczki dla RepAPI
& $msbuild  $repapiProj /p:DeployOnBuild=true /p:PublishProfile=..\RepApi\Properties\PublishProfiles\Install.pubxml /p:VisualStudioVersion=12.0 
Remove-Item .\temp\*.config 
New-Zipfile -ZipFilePath  '.\update\RepAPI.zip' -InputObject '.\temp' | Out-Null
Remove-Item .\temp\* -recurse

