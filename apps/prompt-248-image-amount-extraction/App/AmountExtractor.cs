using Microsoft.ML.OnnxRuntime;
using SkiaSharp;
public sealed record AmountFields(decimal Total,string Currency,string SourcePage,double[] CellConfidence);
public static class AmountExtractor
{
    public static AmountFields Extract(string file,string trustedTemplateCurrency)
    {
        using var image=SKBitmap.Decode(file)??throw new InvalidDataException("Image decoding failed");
        if(image.Width!=140||image.Height!=28)throw new InvalidDataException("Expected the approved five-cell template");
        using var session=new InferenceSession("assets/mnist-12.onnx");
        var digits=new List<int>();var confidence=new List<double>();
        for(int cell=0;cell<5;cell++)
        {
            var pixels=new byte[784];for(int y=0;y<28;y++)for(int x=0;x<28;x++)pixels[y*28+x]=image.GetPixel(cell*28+x,y).Red;
            var result=DigitModel.Infer(session,pixels);digits.Add(result.Digit);confidence.Add(result.Confidence);
        }
        int cents=digits.Aggregate(0,(value,d)=>value*10+d);
        return new(cents/100m,trustedTemplateCurrency,Path.GetFileName(file),confidence.ToArray());
    }
    public static bool Validate(AmountFields fields)=>fields.Total is >0 and <10000
        &&fields.Currency=="EUR"&&fields.CellConfidence.Length==5&&fields.CellConfidence.All(c=>double.IsFinite(c)&&c>=0.5);
}
