SET msbuild="C:\Program Files (x86)\MSBuild\12.0\Bin\msbuild.exe"

%msbuild% ..\src\ChannelAdam.Wcf\ChannelAdam.Wcf.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.0;OutDir=bin\Release\net40
%msbuild% ..\src\ChannelAdam.Wcf\ChannelAdam.Wcf.csproj /t:Rebuild /p:Configuration=Release;TargetFrameworkVersion=v4.5;OutDir=bin\Release\net45

..\tools\nuget\nuget.exe pack ..\src\ChannelAdam.Wcf\ChannelAdam.Wcf.nuspec -Symbols

pause
