#load "./version.cake"

public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }
    public bool IsLocalBuild { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsMainRepo { get; private set; }
    public bool IsMainBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool SkipGitVersion { get; private set; }
    public bool SkipOpenCover { get; private set; }
    public BuildVersion Version { get; private set; }

    public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainRepo
                && IsMainBranch && IsTagged;
        }
    }

    public bool ShouldPublishToMyGet
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMainRepo && (IsTagged || !IsMainBranch);
        }
    }

    public void Initialize(ICakeContext context)
    {
        Version = BuildVersion.Calculate(context, this);
    }

    public static BuildParameters GetParameters(ICakeContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        var target = context.Argument("target", "Default");
        var buildSystem = context.BuildSystem();

        return new BuildParameters {
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsRunningOnUnix = context.IsRunningOnUnix(),
            IsRunningOnWindows = context.IsRunningOnWindows(),
            IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor,
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsMainRepo = StringComparer.OrdinalIgnoreCase.Equals("onovotny/RepoTemplate", buildSystem.AppVeyor.Environment.Repository.Name),
            IsMainBranch = StringComparer.OrdinalIgnoreCase.Equals("master", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsTagged = IsBuildTagged(buildSystem),
            SkipGitVersion = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CAKE_SKIP_GITVERSION")),
            SkipOpenCover = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CAKE_SKIP_OPENCOVER"))
        };
    }

    private static bool IsBuildTagged(BuildSystem buildSystem)
    {
        return buildSystem.AppVeyor.Environment.Repository.Tag.IsTag
            && !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name);
    }
}
