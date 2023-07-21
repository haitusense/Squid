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
R> remotes::install_github("haitusense/rsquid@20230721")
```


## Description


## note

```powershell
> dotnet new wpf --framework net7.0
```