# Squid

Customizable Script Launcher

## Installation and Usage

### 

run debug mode

```powershell
> cd src
> dotnet run -- -d -w "..\sample"

> dotnet run -- -d -w "..\sample" -s "viewer.html"
```

build

```powershell
> cd src
> dotnet build -c Release
> .\bin\Release\net7.0-windows\Squid.exe -w "..\sample"
```

When using R language scripts

```R
R> remotes::install_github("haitusense/rsquid")
```

If you need to install OpenCV DLLs, you can use chocolatey

```powershell
> Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
> choco install OpenCV
```

## Description


## note

```powershell
> dotnet new wpf --framework net7.0
```