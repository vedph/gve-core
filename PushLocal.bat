@echo off
echo PRESS ANY KEY TO INSTALL Cadmus Libraries TO LOCAL NUGET FEED
echo Remember to generate the up-to-date package.

c:\exe\nuget add .\Gve.Text\bin\Debug\Gve.Text.3.0.2.nupkg -source C:\Projects\_NuGet

pause
