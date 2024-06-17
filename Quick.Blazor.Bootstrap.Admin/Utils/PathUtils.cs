using System;
using System.IO;

namespace Quick.Blazor.Bootstrap.Admin;

public class PathUtils
{
    public const char UNIX_DIR_SEPARATOR_CHAR = '/';

    public static string UseUnixDirectorySeparatorChar(string path)
    {
        if (Path.DirectorySeparatorChar != UNIX_DIR_SEPARATOR_CHAR)
            path = path.Replace(Path.DirectorySeparatorChar, UNIX_DIR_SEPARATOR_CHAR);
        return path;
    }
}
