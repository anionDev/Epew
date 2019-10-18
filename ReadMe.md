# General

ExternalProgramExecutionWrapper is a program which receives a programfile and some arguments via commandline-arguments and executes the given program. This tool has some advantages described in the Featue-section.

## Features

ExternalProgramExecutionWrapper
- has no problems with quotes/backslashs/etc. in the arguments
- has a very comfortable loggin-mechanism (output will be printed in the console and in a log-file if desired)
- has a timeout-function

## Installation

```
git clone <remote-address>
pushd externalProgramExecutionWrapper
Build.bat
popd
```

Now the output of the program can be found in `externalProgramExecutionWrapper\ExternalProgramExecutionWrapper\bin\release`. This folder contains the file `ExternalProgramExecutionWrapper.exe`. This exe-file must be available as commandline-program (due to the `PATH`-environment-variable).

## Usage

Calculate the argument:
```
Commandlineargument=Base64("<ProgramPathAndFile>;~<Arguments>;~<Title>;~<WorkingDirectory>;~<PrintErrorsAsInformation>;~<LogFile>;~<TimeoutInMilliseconds>;~<Verbose>")
```
The arguments PrintErrorsAsInformation and verbose are boolean values. Pass '1' to set them to true or anything else to set them to false.

Now call ExternalProgramExecutionWrapper with the base64-argument:
```
ExternalProgramExecutionWrapper Commandlineargument
```
