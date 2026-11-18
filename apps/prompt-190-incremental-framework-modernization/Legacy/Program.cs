using System;
using System.Web;
using System.Web.Script.Serialization;
public static class Program
{
    public static int Main(string[] args)
    {
        if(args[0]=="report") { Console.WriteLine("Legacy report integration"); return 0; }
        var invoice=new {Title=HttpUtility.HtmlEncode(args[1]),Total=75m};
        Console.WriteLine(new JavaScriptSerializer().Serialize(invoice));
        return 0;
    }
}
