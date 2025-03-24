Remove-Item ./build -Force -Recurse

# Some libraries seem to break with IncludeNativeLibrariesForSelfExtract, so DLLs have to be present
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o ./build

Copy-Item ./config.json build/config.json

Compress-Archive -Path ./build/* -DestinationPath ClickGuardian.zip -Force
