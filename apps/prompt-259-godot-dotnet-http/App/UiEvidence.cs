using System.Security.Cryptography;
using System.Text.Json;
public static class UiEvidence
{
    public static void Record(string image,object details)
    {
        string hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(image))).ToLowerInvariant();
        File.AppendAllText("artifacts/native-ui-history.jsonl",JsonSerializer.Serialize(new{sha256=hash,details})+Environment.NewLine);
    }
}
