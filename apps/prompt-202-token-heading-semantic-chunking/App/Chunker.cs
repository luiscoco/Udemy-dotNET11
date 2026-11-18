using Microsoft.ML.Tokenizers;
public sealed class Chunker
{
    private readonly BertTokenizer tokenizer = BertTokenizer.Create("bert-vocab.txt", new BertOptions { LowerCaseBeforeTokenization = true });
    public int Tokens(string text) => tokenizer.CountTokens(text);
    public List<string> Bounded(string text, int budget = 48)
    {
        var chunks = new List<string>(); string current = "";
        foreach (string word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string next = (current + " " + word).Trim();
            if (Tokens(next) > budget && current.Length > 0) { chunks.Add(current); current = word; }
            else current = next;
            if (Tokens(current) > budget) throw new ArgumentException("An individual word exceeds the token budget.");
        }
        if (current.Length > 0) chunks.Add(current);
        return chunks;
    }
    public static double Cosine(float[] a, float[] b) => a.Zip(b).Sum(p => (double)p.First*p.Second)
        / Math.Sqrt(a.Sum(v => (double)v*v)*b.Sum(v => (double)v*v));
}
