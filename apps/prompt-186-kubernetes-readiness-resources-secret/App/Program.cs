string name="course-186-"+Guid.NewGuid().ToString("N")[..10],image=name+":local";
string kind=DeploymentTools.FindTool("kind"),kubectl=DeploymentTools.FindTool("kubectl");
string directory=Path.GetFullPath("artifacts/transient-kubernetes/"+name);
Directory.CreateDirectory(directory);
string kubeconfig=Path.Combine(directory,"kubeconfig");
async Task<string> Kube(params string[] command)=>await DeploymentTools.Run(kubectl,new[]{"--kubeconfig",kubeconfig}.Concat(command).ToArray());
bool broken=args.FirstOrDefault()=="problem";
try
{
    await ContainerLab.Build(image);
    await DeploymentTools.Run(kind,"create","cluster","--name",name,"--image","kindest/node:v1.35.8@sha256:07b2536e30b803ed61d1677a79df6115f798ce64c80f9e22f6ed45afd09323c0","--kubeconfig",kubeconfig,"--wait","90s");
    await DeploymentTools.Run(kind,"load","docker-image",image,"--name",name);
    string token=Path.Combine(directory,"token.txt");
    File.WriteAllText(token,broken?"invalid":"local-teaching-token");
    await Kube("create","secret","generic","runtime-config","--from-file=token="+token);
    string manifest=Path.Combine(directory,"deployment.yaml");
    File.WriteAllText(manifest,File.ReadAllText("Kubernetes/deployment.yaml").Replace("COURSE_IMAGE",image));
    await Kube("apply","-f",manifest);
    await Kube("wait","--for=jsonpath={.status.phase}=Running","pod","-l","app=catalog","--timeout=90s");
    if(!broken) await Kube("rollout","status","deployment/catalog","--timeout=90s");
    else await Task.Delay(5000);
    var pod=DeploymentTools.Json(await Kube("get","pods","-l","app=catalog","-o","json")).GetProperty("items")[0];
    bool ready=pod.GetProperty("status").GetProperty("conditions").EnumerateArray().Any(condition=>condition.GetProperty("type").GetString()=="Ready"&&condition.GetProperty("status").GetString()=="True");
    int endpoints=0;
    for(int attempt=0;attempt<20;attempt++)
    {
        var slices=DeploymentTools.Json(await Kube("get","endpointslices","-l","kubernetes.io/service-name=catalog","-o","json"));
        endpoints=slices.GetProperty("items").EnumerateArray().Sum(slice=>slice.GetProperty("endpoints").EnumerateArray().Count(endpoint=>endpoint.GetProperty("conditions").GetProperty("ready").GetBoolean()));
        if(broken||endpoints==1)break;
        await Task.Delay(250);
    }
    var container=pod.GetProperty("spec").GetProperty("containers")[0];
    Harness.Save("kubernetes-deployment.json",new {running=true,ready,readyEndpoints=endpoints,resources=container.GetProperty("resources"),security=container.GetProperty("securityContext"),runAsNonRoot=pod.GetProperty("spec").GetProperty("securityContext").GetProperty("runAsNonRoot").GetBoolean()});
    Console.WriteLine($"Pod running=True, ready={ready}, ready service endpoints={endpoints}");
    Console.WriteLine("Resources="+container.GetProperty("resources"));
    Check.That(ready&&endpoints==1,"READINESS_DEPLOYMENT_REGRESSION valid configuration makes the deployment eligible for traffic");
    await Kube("exec","deployment/catalog","--","dotnet","Web.dll","--health");
    Check.That(container.GetProperty("securityContext").GetProperty("readOnlyRootFilesystem").GetBoolean(),"deployed pod uses the declared security policy");
}
catch
{
    var diagnostics=new List<string>();
    foreach(var command in new string[][] { ["get","pods","-o","wide"], ["describe","pods","-l","app=catalog"], ["logs","deployment/catalog","--tail=50"] })
    {
        var result=await Harness.Run(kubectl,new[]{"--kubeconfig",kubeconfig,"--request-timeout=10s"}.Concat(command).ToArray());
        diagnostics.Add(result.Text);
    }
    File.WriteAllText("artifacts/kubernetes-diagnostics.txt",string.Join(Environment.NewLine,diagnostics));
    Console.WriteLine(string.Join(Environment.NewLine,diagnostics));
    throw;
}
finally
{
    await Harness.Run(kind,"delete","cluster","--name",name,"--kubeconfig",kubeconfig);
    await Harness.Run("docker","image","rm",image);
}
