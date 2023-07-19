# Squid

Customizable Script Launcher

## Installation and Usage

run debug mode

```powershell
> cd src
> dotnet run -- -d -w "..\sample" -p NamedPipe

> dotnet run -- -d -w "..\sample" -s "viewer.html" -p NamedPipe
```

build

```powershell
> cd src
> dotnet build -c Release
> .\bin\Release\net7.0-windows\Squid.exe -w "..\sample"
```

## note

```powershell
> dotnet new wpf --framework net7.0
```