# Squid

Customizable Script Launcher


## Installation and Usage

### Run html on local

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

### Run html on web

run debug mode

```powershell
> cd src
> dotnet run -- -d -w "..\sample" -s "https://github.com/haitusense"
```

### Run with R script

When using R language scripts, install rsquid

```R
# stable
R> remotes::install_github("haitusense/rsquid")

# develop/nightly
R> remotes::install_github("haitusense/rsquid@branch")
```

### Run with OpenCV

If you need to install OpenCV DLLs, you can use chocolatey

```powershell
> Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
> choco install OpenCV
```


## Description

### options

|option name|shorthand|default|Description|
|:--|:--|:--|:--|
|--devtool                |-d|            | Open DevTools (Shift + Ctr + i) |
|--starturl               |-s| index.html | |
|--working-directory      |-w|            | |
|--hostobjects-name       |-o| Squid      | |
|--no-register-javascript |-r|            | |
|--args                   |-a| "{}"       | |
