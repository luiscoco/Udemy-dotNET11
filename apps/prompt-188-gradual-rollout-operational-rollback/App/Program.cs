using Microsoft.AspNetCore.Builder;
string prefix="course-188-"+Guid.NewGuid().ToString("N"),stableImage=prefix+":v1",candidateImage=prefix+":v2";
string stableName=prefix+"-stable",candidateName=prefix+"-candidate";
try
{
    await ContainerLab.Build(stableImage,"1.0.0");
    await ContainerLab.Build(candidateImage,"2.0.0");
    string stable=await ContainerLab.Start(stableImage,stableName);
    string candidate=await ContainerLab.Start(candidateImage,candidateName,fault:args.FirstOrDefault()=="problem");
    using var router=new CanaryRouter(stable,candidate);
    string stableImageId=DeploymentTools.Json(await DeploymentTools.Docker("inspect",stableName))[0].GetProperty("Image").GetString()!;
    string candidateImageId=DeploymentTools.Json(await DeploymentTools.Docker("inspect",candidateName))[0].GetProperty("Image").GetString()!;
    Check.That(stableImageId!=candidateImageId,"retained stable and candidate images have distinct identities");
    await using var gateway=await ApiHarness.StartAsync(_=>{},app=>app.MapGet("/",router.Forward));
    async Task<object> Sample(string phase,int weight)
    {
        router.SetWeight(weight);
        int errors=0,stableResponses=0,candidateResponses=0;
        for(int index=0;index<100;index++)
        {
            using var response=await gateway.Client.GetAsync("/");
            string body=await response.Content.ReadAsStringAsync();
            if(!response.IsSuccessStatusCode) errors++;
            else if(body.Contains("1.0.0"))stableResponses++;
            else if(body.Contains("2.0.0"))candidateResponses++;
        }
        Console.WriteLine($"{phase} candidateWeight={weight}, stable={stableResponses}, candidate={candidateResponses}, errors={errors}");
        return new {phase,weight,stableResponses,candidateResponses,errors};
    }
    var baseline=DeploymentTools.Json(System.Text.Json.JsonSerializer.Serialize(await Sample("baseline",0)));
    var canary=DeploymentTools.Json(System.Text.Json.JsonSerializer.Serialize(await Sample("canary",20)));
    bool accepted=canary.GetProperty("errors").GetInt32()==0&&canary.GetProperty("candidateResponses").GetInt32()==20;
    var result=DeploymentTools.Json(System.Text.Json.JsonSerializer.Serialize(await Sample(accepted?"promoted":"rollback",accepted?100:0)));
    bool recovered=result.GetProperty("errors").GetInt32()==0;
    Harness.Save("rollout-observations.json",new {stableImageId,candidateImageId,baseline,canary,result,accepted,recovered,rollbackVerified=!accepted&&result.GetProperty("stableResponses").GetInt32()==100});
    Check.That(recovered,"operational sample verifies the selected release after the gate");
    Check.That(accepted,"ROLLOUT_GATE_REGRESSION candidate must pass the error and traffic gate before promotion");
}
finally
{
    await ContainerLab.Cleanup(stableName,stableImage);
    await ContainerLab.Cleanup(candidateName,candidateImage);
}
