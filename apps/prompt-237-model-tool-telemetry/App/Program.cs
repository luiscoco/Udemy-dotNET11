using var client=EvalClient.Open();
var telemetry=await TelemetryRun.Run(client,args.Contains("problem"));client.Save();
Check.That(client.Calls==1,"one real model request supplies the measured response");
EvalData.Save(new {passed=true,telemetry,modelCalls=client.Calls});
