# General

Epew (ExternalProgramExecutionWrapper) is a tool to wrap program-calls with some useful functions.
It is something like a wrapper or a shim to be able to add timestamps to the output, write output to files, and to add some more functionalities.
Due to the possibility to encode arguments for program-calls in base64 Epew is especially helpful when you want to run a program with some arguments which contain every kind of escape-character which should not be escaped but simply be passed to the executed program.
Adding a timeout is also of course possible.
Epew is available for Windows and Linux.
If you do not know the full path of the program, Epew of course uses the `PATH`-environment-variable to find the desired program, and this also works with some tricky cmd-tools like npm on Windows.

Epew is a commandline-tool.
There is no gui.
The main-advantage of epew is to be used when

- it is difficult to pass special characters (backslashs, quotes, etc.) from commandline to another program (use epew with the `--ArgumentIsBase64Encoded`-switch).
- it is difficult to get stdout, stderr or the exitcode of the program (use epew with the `--StdOutFile`- or `--StdErrFile`- or `--ExitCodeFile`-switch).
- you want to have a simple timeout when running a program (use epew with the `--TimeoutInMilliseconds`-switch).
- you want to print the output to the console but also log the output into a file when running a program. Both (console and logfile) can of course have timestamps and a distinction between stderr and stdout.

Other features:

- epew is available for Linux and Windows.
- epew resolves environment-variables. (For example you can set `git` as program instead of `C:\Program Files\Git\cmd\git.exe`. Both variants are working.)

## Get epew

### Download sourcecode using git (Linux and Windows)

```
git clone https://github.com/anionDev/Epew
cd Epew
dotnet build  Epew.sln
```

### Installation via winget (Windows, planned)

Coming soon.

### Installation via apt (Linux, planned)

Coming soon.

## Usage

### Arguments

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

  -r, --ProcessIdFile               File for the process-id of the executed
                                    program

  -d, --TimeoutInMilliseconds       (Default: 2147483647) Maximal duration of
                                    the execution process before it will by
                                    aborted by epew

  -t, --Title                       Title for the execution-process

  -n, --NotSynchronous              (Default: false) Run the program
                                    asynchronously

  -n, --LogNamespace                (Default: ) Namespace for log

  -c, --WriteOutputToConsole        (Default: false) Write output to console

  --help                            Display this help screen.

  --version                         Display version information.

```

Exitcodes:

2147393801: If no program was executed

2147393802: If a fatal error occurred

2147393803: If the executed program was aborted due to the given timeout

If running synchronously then the exitcode of the executed program will be set as exitcode of epew.

If running asynchronously then the process-id of the executed program will be set as exitcode of epew.

### Verbosity

Currently the following verbosity-levels are available:

- 0 (Quiet)
- 1 (Normal)
- 2 (Verbose)

## Reference

The Epew-reference can be found [here](./Epew/Other/Reference/ReferenceContent).

## Build

This product requires to use `scbuildcodeunits` implemented/provided by [ScriptCollection](https://github.com/anionDev/ScriptCollection) to build the project.

## Changelog

See the [Changelog-folder](./Other/Resources/Changelog).

## Contribue

Contributions are always welcome.

This product has the contribution-requirements defines by [DefaultOpenSourceContributionProcess](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/Contributing/DefaultOpenSourceContributionProcess/DefaultOpenSourceContributionProcess.md).

## Repository-structure

This product uses the [CommonProjectStructure](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/RepositoryStructure/CommonProjectStructure/CommonProjectStructure.md) as repository-structure.

## Branching-system

This product follows the [GitFlowSimplified](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/BranchingSystem/GitFlowSimplified/GitFlowSimplified.md)-branching-system.

## Versioning

This product follows the [SemVerPractise](https://projects.aniondev.de/PublicProjects/Common/ProjectTemplates/-/blob/main/Conventions/Versioning/SemVerPractise/SemVerPractise.md)-versioning-system.

## License

epew is licensed under the terms of MIT. The concrete license-text can be found [here](https://raw.githubusercontent.com/anionDev/Epew/main/License.txt).
