@echo off
echo BUILD packages

del .\Gve.Text\bin\Debug\*.*nupkg

cd .\Gve.Text
dotnet pack -c Debug -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
cd..

pause
