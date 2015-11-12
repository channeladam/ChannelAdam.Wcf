SET msbuild="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

%msbuild% ..\src\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.0;OutDir=bin\Release\net40
%msbuild% ..\src\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.5;OutDir=bin\Release\net45

..\tools\nuget\nuget.exe pack ..\src\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core\ChannelAdam.Wcf.Microsoft.Practices.TransientFaultHandling.Core.nuspec -Symbols

pause
