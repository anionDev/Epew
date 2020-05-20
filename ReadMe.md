# General

epew (ExternalProgramExecutionWrapper) is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout.

## Features

- Has a comfortable logging-mechanism: Output will be printed in the console and in a log-file with timestamps (if desired). StdOut and StdErr are of course differentiated.
- Has a timeout-function
- Saves the exit-code of the executed program (if desired)
- Can be executed on Windows and Linux
- Resolves environment-variables
- Has no problems with quotes, slashs, backslashs, etc. in the arguments (The argument for the program can be passed to epew as base64-string)

# Get epew

## Installation via PyPI (Linux and Windows)

'''
pip3 install epew
epew
'''

## Download sourcecode using git (Linux and Windows)

'''
git clone https://github.com/anionDev/externalProgramExecutionWrapper
cd externalProgramExecutionWrapper
dotnet build  ExternalProgramExecutionWrapper.sln
'''

## Installation via WinGet (Windows)

'''
winget install epew
'''

## Usage

epew is a commandline-tool. There is no gui. The main-intention of epew is to be used in scripts with unattended execution.

```
>epew
  -p, --Program                     Required. Program which should be executed
  -a, --Argument                    Argument for the program which should be
                                    executed
  -b, --ArgumentIsBase64Encoded     (Default: false) Specifiy whether Argument
                                    is base64-encoded
  -w, --Workingdirectory            Workingdirectory for the program which
                                    should be executed
  -v, --Verbosity                   (Default: Normal) Verbosity of
                                    ExternalProgramExecutionWrapper
  -i, --PrintErrorsAsInformation    (Default: false) Treat errors as information
  -r, --RunAsAdministrator          (Default: false) Run program as
                                    administrator
  -h, --AddLogOverhead              (Default: false) Add log overhead
  -l, --LogFile                     Logfile for ExternalProgramExecutionWrapper
  -o, --StdOutFile                  File for the stdout of the executed program
  -e, --StdErrFile                  File for the stderr of the executed program
  -x, --ExitCodeFile                File for the exitcode of the executed
                                    program
  -d, --TimeoutInMilliseconds       (Default: 2147483647) Maximal duration of
                                    the execution process before it will by
                                    aborted by ExternalProgramExecutionWrapper
  -t, --Title                       Title for the execution-process
  --help                            Display this help screen.
  --version                         Display version information.
```

Exitcodes:
- -1: No program was executed
- -2: A fatal error occurred
- -3: The executed program was aborted due to the given timeout
If the executed program terminated then its exitcode is the exitcode of epew.

# Technical details

## Strong name

The officially released `epew.dll`-file is always signed with the key with the short-id `79cae7246084aa22`. Do not trust any `epew.dll`-file which are not signed with this key.
Only `epew.dll` will be signed with this key. All other files contained in a binary-release of epew are not signed by this key.
You can verify the key using [sn](https://docs.microsoft.com/en-us/dotnet/framework/tools/sn-exe-strong-name-tool) with `sn -T epew.dll`.

# License

epew is licensed under the terms of [MIT](https://raw.githubusercontent.com/anionDev/externalProgramExecutionWrapper/master/License.txt)

