using System.Text.Json;
public static class ContainerLab
{
    public static async Task Build(string image,string version="1.0.0")
    {
        await DeploymentTools.Dotnet("publish","Web","-c","Release","-r","linux-x64","--self-contained","false","-p:InformationalVersion="+version,"-o","artifacts/published/web");
        await DeploymentTools.Docker("build","--quiet","-f","Dockerfile","-t",image,"artifacts/published/web");
    }
    public static async Task<string> Start(string image,string name,bool root=false,bool fault=false)
    {
        await DeploymentTools.Docker("run","-d","--name",name,"--user",root?"0":"1654","--read-only","--tmpfs","/tmp","--cap-drop","ALL","--security-opt","no-new-privileges","--memory","256m","--cpus","1","-e","INJECT_FAULT="+fault.ToString().ToLowerInvariant(),"-p","127.0.0.1::8080",image);
        string address="http://"+(await DeploymentTools.Docker("port",name,"8080/tcp")).Trim();
        for(int i=0;i<60;i++)
        {
            var json=DeploymentTools.Json(await DeploymentTools.Docker("inspect",name));
            if(json[0].GetProperty("State").GetProperty("Health").GetProperty("Status").GetString()=="healthy") return address;
            await Task.Delay(250);
        }
        throw new TimeoutException("Owned container did not become healthy");
    }
    public static async Task Cleanup(string name,string image)
    {
        await Harness.Run("docker","rm","-f",name);
        await Harness.Run("docker","image","rm",image);
    }
}
