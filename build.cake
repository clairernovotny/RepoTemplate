// Install tools.
#tool "nuget:https://www.nuget.org/api/v2?package=GitVersion.CommandLine&version=3.6.2"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument<string>("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

// Load other scripts.
#load "./build/parameters.cake"

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////

BuildParameters parameters = BuildParameters.GetParameters(Context);
bool publishingError = false;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    parameters.Initialize(ctx);

    // Increase verbosity?
    if(parameters.IsMainBranch && (ctx.Log.Verbosity != Verbosity.Diagnostic)) {
        Information("Increasing verbosity to diagnostic.");
        ctx.Log.Verbosity = Verbosity.Diagnostic;
    }

    Information("Building version {0} of RepoTemplate ({1}, {2}) using version {3} of Cake. (IsTagged: {4})",
        parameters.Version.SemVersion,
        parameters.Configuration,
        parameters.Target,
        parameters.Version.CakeVersion,
        parameters.IsTagged);
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(ctx => {
        CleanDirectories("./src/**/bin/" + configuration);
        CleanDirectories("./src/**/obj/" + configuration);
        CleanDirectories("./artifacts");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(ctx => {
        DotNetCoreRestore("./", new DotNetCoreRestoreSettings {
            Sources = new [] { "https://api.nuget.org/v3/index.json" },
            Verbosity = DotNetCoreRestoreVerbosity.Warning
        });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(ctx => {
        var projects = GetFiles("./**/project.json");
        foreach(var project in projects)
        {
            DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings {
                Configuration = configuration
            });
        }
});

Task("Package")
    .IsDependentOn("Build")
    .Does(() =>
{
        // Build libraries
    var projects = GetFiles("./**/*.csproj");
    foreach(var project in projects)
    {
        var name = project.GetDirectory().FullPath;
        if(name.EndsWith("Cake") || name.EndsWith("Tests")
            || name.EndsWith("Xunit") || name.EndsWith("NuGet"))
        {
            continue;
        }

        DotNetCorePack(project.GetDirectory().FullPath, new DotNetCorePackSettings {
            VersionSuffix = parameters.Version.DotNetAsterix,
            Configuration = parameters.Configuration,
            OutputDirectory = "./artifacts",
            NoBuild = true,
            Verbose = false
        });
    }
});

Task("Default")
    .IsDependentOn("Package");

Task("AppVeyor")
    .IsDependentOn("Package");

Task("Travis")
    .IsDependentOn("Package");

RunTarget(target);