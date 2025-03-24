dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o ./build

Copy-Item ./config.json build/config.json

Compress-Archive -Path ./build/* -DestinationPath ClickGuardian.zip
