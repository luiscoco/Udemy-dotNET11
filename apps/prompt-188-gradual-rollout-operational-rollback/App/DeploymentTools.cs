using System.Text.Json;
public static class DeploymentTools
{
    public static async Task<string> Run(string file, params string[] arguments)
    {
        Directory.CreateDirectory("artifacts");
        var result=await Harness.Run(file,arguments);
        string name=Path.GetFileNameWithoutExtension(file)+"-"+Guid.NewGuid().ToString("N")+".txt";
        File.WriteAllText(Path.Combine("artifacts",name),"$ "+file+" "+string.Join(" ",arguments)+Environment.NewLine+result.Text);
        if(result.Code!=0) Console.WriteLine(result.Text);
        Check.That(result.Code==0,Path.GetFileName(file)+" command completed");
        return result.Text;
    }
    public static Task<string> Dotnet(params string[] arguments)=>Run(Harness.Dotnet,arguments);
    public static Task<string> Docker(params string[] arguments)=>Run("docker",arguments);
    public static string FindTool(string name)
    {
        var directory=new DirectoryInfo(Environment.CurrentDirectory);
        while(directory is not null)
        {
            string file=Path.Combine(directory.FullName,".tools","kubernetes",name+".exe");
            if(File.Exists(file))return file;
            directory=directory.Parent;
        }
        return name;
    }
    public static JsonElement Json(string value)=>JsonDocument.Parse(value).RootElement.Clone();
}
