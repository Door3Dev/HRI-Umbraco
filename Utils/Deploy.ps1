# FAIL TO RUN? TRY "Set-ExecutionPolicy RemoteSigned" in the powershell before
# LOG FILE: HriDeploy.log;

# Deployment servers
$servers = @{
	#'Dev' = 'https://192.168.100.105:8172'; # For test only, deployment to dev has been automated
	'Uat' = 'https://192.168.112.22:8172';
	'Prod13' = 'https://192.168.112.13:8172';
	'Prod14' = 'https://192.168.112.13:8172'
}

$env = Read-Host 'Environment (UAT,Prod1, Prod2,etc.)'
# Exit if no registered environment
if (-Not $servers.ContainsKey($env)) {
	Write-Error 'Selected environment can''t be found in the list'
	Exit
}
# Get data from user
$build = Read-Host 'Build (Dev,UAT,etc.)'
$user = Read-Host 'User HealthRepublic\'
$password = Read-Host 'Password' -AsSecureString
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
					[Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

# Selected server for deployment
$selectedServer = $servers.Get_Item($env)

# MSBuild tool path
$msbuild = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
# Solution file path
$msSolution = '..\HRI\HRI.Umbraco.sln'
# Build configuration
$msBuildConfiguration = '/P:Configuration=' + $build
# Web Deploy user credentials
$msUser = '/P:UserName=HealthRepublic\' + $user
$msPassword = '/P:Password=' + $plainPassword
# Web Deploy publish profile
$msPublishProfile ='/P:PublishProfile=' + $build
# Web Deploy URL
$msDeployUrl = '/P:MsDeployServiceUrl=' + $selectedServer + '/MsDeploy.axd'

# Command generation
$cmd = "$msbuild $msSolution  $msBuildConfiguration $msUser $msPassword $msPublishProfile $msDeployUrl"
$cmd += " /P:VisualStudioVersion=12.0 /P:DeployOnBuild=True /P:DeployTarget=MSDeployPublish"
$cmd += " /P:AllowUntrustedCertificate=True /P:MSDeployPublishMethod=WMSvc /P:CreatePackageOnPublish=True"
$cmd += ' /fl /flp:logfile=HriDeploy.log;'

# Debug generated command
#Write-Host $cmd
 
Invoke-Expression $cmd

# An example of the full command
#
# C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe .\HRI.Umbraco.sln 
# /P:Configuration=Dev 
# /P:VisualStudioVersion=12.0 /P:DeployOnBuild=True /P:DeployTarget=MSDeployPublish 
# /P:MsDeployServiceUrl=https://192.168.100.105:8172/MsDeploy.axd /P:AllowUntrustedCertificate=True 
# /P:MSDeployPublishMethod=WMSvc /P:CreatePackageOnPublish=True /P:UserName=UserWithDomain
# /P:Password=RealPassword /p:PublishProfile=DEV
