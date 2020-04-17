# General

epew (ExternalProgramExecutionWrapper) is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout.

## Features

ExternalProgramExecutionWrapper
- has no problems with quotes/backslashs/etc. in the arguments
- has a comfortable logging-mechanism: Output will be printed in the console and in a log-file (if desired)
- has a timeout-function
- saves the exit-code of the executed program (if desired)

## Installation

You can download and compile epew on your own machine.
It is planned to release epew via Chocolatey.

## Usage

```
epew
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

