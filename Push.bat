@echo off
echo PUSH PACKAGES TO NUGET
prompt
set nu=C:\Exe\nuget.exe
set src=-Source https://api.nuget.org/v3/index.json

%nu% push .\Gve.Text\bin\Debug\*.nupkg %src% -SkipDuplicate

echo COMPLETED
echo on
