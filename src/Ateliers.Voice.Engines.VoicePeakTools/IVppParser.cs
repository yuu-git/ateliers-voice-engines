
namespace Ateliers.Voice.Engins.VoicePeakTools;

// VPPファイルを解析するためのインターフェース
public interface IVppParser
{
    VppFile ParseVppFile(string filePath); // VPPファイルを解析してオブジェクトに変換
    bool ValidateVppFile(string filePath);   // VPPファイルの構造を検証
}