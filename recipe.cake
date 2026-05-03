#load nuget:?package=Cake.Recipe&version=4.0.0

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            solutionFilePath: "./src/Cake.Kudu.sln",
                            title: "Cake.Kudu",
                            repositoryOwner: "cake-contrib",
                            repositoryName: "Cake.Kudu",
                            appVeyorAccountName: "cakecontrib",
                            shouldRunInspectCode: false,
                            preferredBuildProviderType: BuildProviderType.GitHubActions);

BuildParameters.PrintParameters(Context);

Build.RunDotNetCore();
