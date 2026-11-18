using Microsoft.Extensions.AI;
public sealed class ToolLedger(bool unsafeApproval)
{
    public List<(string Name,string Sku)> Calls { get; }=[];
    public int Effects { get; private set; }
    public string Read(string sku)
    {
        Calls.Add(("read_stock",sku));
        if(sku!="NET11")throw new ArgumentException("Unknown SKU");
        return "Synthetic NET11 stock: 10";
    }
    public bool Reserve(string sku,int quantity,bool trustedApproval)
    {
        if(sku!="NET11"||quantity is <1 or >5)throw new ArgumentException("Invalid reservation");
        if(!trustedApproval&&!unsafeApproval)return false;
        Effects++;return true;
    }
    public AITool ReadTool()=>AIFunctionFactory.Create(Read,name:"read_stock",description:"Read synthetic stock for SKU NET11");
}
