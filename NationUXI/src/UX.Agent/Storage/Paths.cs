namespace UX.Agent.Storage;

public static class Paths
{
    private static string RootDir
    {
        get
        {
            // Cross-platform: %ProgramData%/uxagent or ~/.uxagent
            var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(baseDir, "uxagent");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public static string GetIdentityFilePath() => Path.Combine(RootDir, "identity.json");
    public static string GetStatusFilePath() => Path.Combine(RootDir, "status.json");
}
