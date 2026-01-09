namespace Ateliers.Voice.Engines;

/// <summary>
/// 音声ファイル出力用のディレクトリ管理ヘルパー
/// </summary>
public static class VoiceOutputDirectoryHelper
{
    /// <summary>
    /// タイムスタンプベースの出力ディレクトリを作成します
    /// </summary>
    /// <param name="baseDirectory">ベースディレクトリパス</param>
    /// <returns>作成されたディレクトリのフルパス</returns>
    public static string CreateTimestampedDirectory(string baseDirectory)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        var outputDir = Path.Combine(baseDirectory, timestamp);
        Directory.CreateDirectory(outputDir);
        return outputDir;
    }

    /// <summary>
    /// 出力ファイルのフルパスを構築します
    /// </summary>
    /// <param name="directory">ディレクトリパス</param>
    /// <param name="fileName">ファイル名</param>
    /// <returns>フルパス</returns>
    public static string GetOutputFilePath(string directory, string fileName)
    {
        return Path.Combine(directory, fileName);
    }

    /// <summary>
    /// 出力ベースディレクトリを取得または作成します
    /// </summary>
    /// <param name="outputRootDirectory">ルートディレクトリ（nullの場合はデフォルト）</param>
    /// <param name="subDirectoryName">サブディレクトリ名</param>
    /// <returns>ベースディレクトリのフルパス</returns>
    public static string GetOrCreateBaseDirectory(string? outputRootDirectory, string subDirectoryName)
    {
        var root = outputRootDirectory ?? Path.Combine(AppContext.BaseDirectory, "output");
        var baseDir = Path.Combine(root, subDirectoryName);
        Directory.CreateDirectory(baseDir);
        return baseDir;
    }
}