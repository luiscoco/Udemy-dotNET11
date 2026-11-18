using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
public static class DigitModel
{
    public static (int Digit,float[] Scores,double Confidence) Infer(InferenceSession session,byte[] pixels,bool wrongRank=false)
    {
        if(pixels.Length!=784)throw new ArgumentException("Exactly one 28 by 28 cell is required");
        var values=pixels.Select(b=>b/255f).ToArray();
        var tensor=new DenseTensor<float>(values,wrongRank?new[]{1,28,28}:new[]{1,1,28,28});
        using var output=session.Run(new[]{NamedOnnxValue.CreateFromTensor(session.InputMetadata.Keys.Single(),tensor)});
        var scores=output.First().AsEnumerable<float>().ToArray();int digit=Array.IndexOf(scores,scores.Max());
        double sum=scores.Sum(s=>Math.Exp(s-scores.Max()));
        return (digit,scores,1/sum);
    }
}
