public static class SafeFiles
{
    public static string ExistingFile(string root, string relative)
    {
        if (Path.IsPathRooted(relative) || relative.Contains(':'))
            throw new UnauthorizedAccessException("Rooted or drive-qualified input rejected");
        root = Path.GetFullPath(root);
        relative = relative.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        string candidate = Path.GetFullPath(Path.Combine(root, relative));
        string local = Path.GetRelativePath(root, candidate);
        if (local == ".." || local.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || Path.IsPathRooted(local)) throw new UnauthorizedAccessException("Path traversal rejected");
        string current = root;
        RejectLink(current);
        foreach (string component in local.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            RejectLink(current);
        }
        return candidate;
    }
    private static void RejectLink(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new UnauthorizedAccessException("Link boundary rejected");
    }
}
