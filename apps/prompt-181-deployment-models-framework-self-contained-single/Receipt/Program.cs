using System.Text.Json;
using System.Text.Json.Serialization;
if(!int.TryParse(args.FirstOrDefault()??"3",out int quantity)||quantity<1) return 2;
var receipt=new Receipt("NET11",quantity,quantity*25m);
Console.WriteLine(JsonSerializer.Serialize(receipt,ReceiptJson.Default.Receipt));
return 0;
public sealed record Receipt(string Course,int Quantity,decimal Total);
[JsonSerializable(typeof(Receipt))]
internal partial class ReceiptJson:JsonSerializerContext { }
