using System.IO;

namespace SellSn.Client.Windows.Common.Utils;

public static class DirectoryUtils
{
    public static void DirectoryNotExistsCreate(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
}