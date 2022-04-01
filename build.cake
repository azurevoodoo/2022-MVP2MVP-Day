public record BuildData(
    DirectoryPath ArtifactsPath,
    DirectoryPath SourcePath,
    string Configuration,
    string Version
)
{
     public DotNetMSBuildSettings MSBuildSettings = new DotNetMSBuildSettings {
                    Version = Version
                }.
                SetConfiguration(Configuration);
}

Setup(
    context => new BuildData(
        MakeAbsolute(Directory("./artifacts")),
        MakeAbsolute(Directory("./src")),
        "Release",
        FormattableString.Invariant($"{DateTime.UtcNow:yyyy.MM.dd}.{GitHubActions.Environment.Workflow.RunNumber}")
    )
);


Task("Clean")
    .Does<BuildData>((context, data) => {
        CleanDirectory(data.ArtifactsPath);
        CleanDirectories($"{data.SourcePath}/**/{{obj,bin}}*/{data.Configuration}");
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does<BuildData>((context, data) => {
        DotNetRestore(
            data.SourcePath.FullPath,
            new DotNetRestoreSettings {
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does<BuildData>((context, data) => {
        DotNetBuild(
            data.SourcePath.FullPath,
            new DotNetBuildSettings {
                NoRestore = true,
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });

Task("Test")
    .IsDependentOn("Build")
     .Does<BuildData>((context, data) => {
        DotNetTest(
            data.SourcePath.FullPath,
            new DotNetTestSettings {
                NoRestore = true,
                NoBuild = true,
                Configuration = data.Configuration
            }
            );
    });

Task("Integration-Test")
    .IsDependentOn("Build")
     .Does<BuildData>((context, data) => {
        DotNetRun(
            data.SourcePath.Combine("HelloWorld").CombineWithFilePath("HelloWorld.csproj").FullPath,
            "MVP2MVP",
            new DotNetRunSettings {
                NoRestore = true,
                NoBuild = true,
                Configuration = data.Configuration
            }
            );
    });

Task("Pack")
    .IsDependentOn("Test")
    .IsDependentOn("Integration-Test")
    .Does<BuildData>((context, data) => {
        DotNetPack(
            data.SourcePath.FullPath,
            new DotNetPackSettings {
                NoRestore = true,
                NoBuild = true,
                OutputDirectory = data.ArtifactsPath,
                MSBuildSettings = data.MSBuildSettings
            }
            );
    });


Task("Upload-Artifact")
    .IsDependentOn("Pack")
    .Does<BuildData>(async (context, data) => {
        await GitHubActions.Commands.UploadArtifact(
            data.ArtifactsPath,
            $"MVP2MVP{context.Environment.Platform.Family}"
        );
    });

Task("Default")
    .IsDependentOn("Pack");


Task("GitHubActions")
    .IsDependentOn("Upload-Artifact");



RunTarget(Argument("target", "Default"));