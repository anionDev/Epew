pushd ExternalProgramExecutionWrapper
nuget restore -SolutionDirectory ..
msbuild ExternalProgramExecutionWrapper.csproj /t:rebuild /p:Configuration=Release
popd