# General

epew (ExternalProgramExecutionWrapper) is a tool to wrap program-calls with some useful functions.

## Features

epew is a commandline-tool. There is no gui. The main-advantage of epew is to be used when

- it is difficult to pass special characters (backslashs, quotes, etc.) from commandline to another program (use epew with the `--ArgumentIsBase64Encoded`-switch).
- it is difficult to get stdout, stderr or the exitcode of the program (use epew with the `--StdOutFile`- or `--StdErrFile`- or `--ExitCodeFile`-switch).
- you want to have a simple timeout when running a program (use epew with the `--TimeoutInMilliseconds`-switch).
- you want to print the output to the console but also log the output into a file when running a program. Both (console and logfile) can of course have timestamps and a distinction between stderr and stdout.

Other features:

- epew is available for Linux and Windows.
- epew resolves environment-variables (e. g. you can set `git` as program instead of `C:\Program Files\Git\cmd\git.exe` (which would also be possible)).

## Get epew

### Download sourcecode using git (Linux and Windows)

```
git clone https://github.com/anionDev/Epew
cd Epew
dotnet build  Epew.sln
```

### Installation via winget (Windows, planned)

Coming as soon as possible.

### Installation via apt (Linux, planned)

Coming as soon as possible.

### Usage

```
>epew
Copyright (C) 2020 Marius Göcke

  -p, --Program                     Required. Program which should be executed

  -a, --Argument                    Argument for the program which should be
                                    executed

  -b, --ArgumentIsBase64Encoded     (Default: false) Specifiy whether Argument
                                    is base64-encoded

  -w, --Workingdirectory            Workingdirectory for the program which
                                    should be executed

  -v, --Verbosity                   (Default: Normal) Verbosity of epew

  -i, --PrintErrorsAsInformation    (Default: false) Treat errors as information

  -h, --AddLogOverhead              (Default: false) Add log overhead

  -f, --LogFile                     Logfile for epew

  -o, --StdOutFile                  File for the stdout of the executed program

  -e, --StdErrFile                  File for the stderr of the executed program

  -x, --ExitCodeFile                File for the exitcode of the executed
                                    program

  -d, --TimeoutInMilliseconds       (Default: 2147483647) Maximal duration of
                                    the execution process before it will by
                                    aborted by epew

  -t, --Title                       Title for the execution-process

  -n, --NotSynchronous              (Default: false) Run the program
                                    asynchronously

  -n, --LogNamespace                (Default: ) Namespace for log

  --help                            Display this help screen.

  --version                         Display version information.
```

Exitcodes:

2147393801: If no program was executed

2147393802: If a fatal error occurred

2147393803: If the executed program was aborted due to the given timeout

2147393881: If executed on MacOS (applies only to the pip-package)

2147393882: If executed on an unknown OS (applies only to the pip-package)

2147393883: If an (unexpected) exception occurred (applies only to the pip-package)

If running synchronously then the exitcode of the executed program will be set as exitcode of epew.

If running asynchronously then the process-id of the executed program will be set as exitcode of epew.

## Technical details

### The pip-package

Installing epew via pip does really install epew. It does not install another implementation of epew with same behavior. The pip-package of epew takes the binary-files of epew (which are written in C# and compiled for the [runtimes](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) `win-x64` and `linux-x64`), wraps them to a bundle and simple calls the correct one dependent on your OS.

### Strong name

The officially released `epew.dll`-file is always signed with the key with the short-id `79cae7246084aa22`. Do not trust any `epew.dll`-file which are not signed with this key.
Only `epew.dll` will be signed with this key. All other files contained in a binary-release of epew are not signed by this key.
You can verify the key using [sn](https://docs.microsoft.com/en-us/dotnet/framework/tools/sn-exe-strong-name-tool) with `sn -T epew.dll`.

## License

epew is licensed under the terms of MIT. The concrete license-text can be found [here](https://raw.githubusercontent.com/anionDev/externalProgramExecutionWrapper/master/License.txt).
