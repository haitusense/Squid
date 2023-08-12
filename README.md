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


## note

```powershell
> dotnet new wpf --framework net7.0
```

### 命名規則参考

RはGoogle, tidyverse等あるがアバウト  
C#はt_, s_のprefixルールあるが、評判悪し  

|                         | C++           | C#              | Rust(RFC 430) | javascript    | R              |
| :--                     | :--           | :--             | :--           | :--           | :--            |
| const/static variable   | CONSTANT_NAME | ConstantName    | CONSTANT_NAME | CONSTANT_NAME | CONSTANT_NAME  |
|                         | kConstantName |                 |               |               | kConstantName  |
| variable/field/property | my_var        | PublicVar       | local_var     | publicVar     | local_var      |
|                         | strcut_member | this.privateVar |               | privateVar    |                |
|                         | class_member_ | _privateVar     |               | privateVar_   |                |
| method                  | my_function() | DoSometing      | do_something  | doSometing    | do_something   |
|                         |               |                 |               |               | doSometing     |
|                         |               | DoSometingAsync |               |               |                |
| argument                |               | argName         | arg_name      | argName       |                |
|                         |               |                 |               | opt_arg       |                |
|                         |               |                 |               | var_args      |                |
| class/struct/trait      | ClassName     | ClassName       | StrcutName    | ClassName     | class_name     |
|                         |               |                 |               |               | ClassName      |
|                         |               | IInterfaceName  |               |               |                |
|                         |               | InterfaceName   |               |               |                |
| enum                    | EnumName      | EnumName        | EnumName      | EnumName      |                |
| enum member/variant     | ENUM_VAR      | EnumMember      | EnumMember    | EnumMember    |                |
| component               |               |                 |               | MyComponent   |                |
| library/creat           |               |                 | crate_name    |               | rLibraryName   |
| file name               | my_src.cpp    | FeatureCategory | crate_name    | file-name     | file_name.R    |
|                         |               |                 | file-name     | filePath      | file-name.R    |
|                         |               |                 |               |               | 00_file_name.R |
