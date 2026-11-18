string name="course-183-"+Guid.NewGuid().ToString("N"),image=name+":local";
try
{
    await ContainerLab.Build(image);
    string address=await ContainerLab.Start(image,name,root:args.FirstOrDefault()=="problem");
    string uid=(await DeploymentTools.Docker("exec",name,"id","-u")).Trim();
    var inspect=DeploymentTools.Json(await DeploymentTools.Docker("inspect",name))[0];
    var limits=inspect.GetProperty("HostConfig");
    string health=inspect.GetProperty("State").GetProperty("Health").GetProperty("Status").GetString()!;
    using var client=new HttpClient();
    string response=await client.GetStringAsync(address);
    Console.WriteLine($"UID={uid}, health={health}, memory={limits.GetProperty("Memory")}, nanoCPUs={limits.GetProperty("NanoCpus")}");
    Console.WriteLine(response);
    Harness.Save("container-policy.json",new {uid,health,memory=limits.GetProperty("Memory").GetInt64(),nanoCpus=limits.GetProperty("NanoCpus").GetInt64(),readOnly=limits.GetProperty("ReadonlyRootfs").GetBoolean(),response});
    Check.That(limits.GetProperty("Memory").GetInt64()==268435456&&limits.GetProperty("NanoCpus").GetInt64()==1000000000,"actual resource limits match the deployment policy");
    Check.That(uid=="1654","NONROOT_POLICY_REGRESSION container executes as the application user");
    Check.That(health=="healthy","actual application health probe passes");
}
finally { await ContainerLab.Cleanup(name,image); }
