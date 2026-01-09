namespace Ateliers.Voice.Engines.VoicevoxTools;

/// <summary>
/// VoicevoxCoreSharp のネイティブライブラリパスを設定するヘルパークラス
/// </summary>
public static class NativeLibraryPath
{
    /// <summary>
    /// ネイティブライブラリの検索パスを設定します
    /// </summary>
    /// <param name="path">VOICEVOX エンジンのディレクトリパス</param>
    public static void Use(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Specified path does not exist: {path}");
        }

        // DLL検索パスに追加
        if (OperatingSystem.IsWindows())
        {
            SetDllDirectory(path);
        }
        
        // 環境変数PATHに追加
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        if (!currentPath.Contains(path))
        {
            Environment.SetEnvironmentVariable("PATH", $"{path};{currentPath}");
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool SetDllDirectory(string lpPathName);
}
