using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
var service=new PredictionService();bool skip=args.Contains("problem");var builder=WebFixture.Builder();var tokens=new LocalTokens();tokens.Register(builder.Services);
var app=builder.Build();app.UseAuthentication();app.UseAuthorization();
app.MapPost("/predict",(PredictInput input)=>
{
    try{return Results.Ok(new {score=service.Predict(input.X,skip),modelVersion=PredictionService.Version});}
    catch(ArgumentException e){return Results.BadRequest(new {error=e.Message});}
}).RequireAuthorization("prediction.access");
app.MapGet("/monitor",()=>{var state=service.Monitor();return Results.Ok(new {state.Samples,state.MeanShift,state.Alert,modelVersion=PredictionService.Version});}).RequireAuthorization("prediction.access");
await using var server=await WebFixture.Start(app);using var http=server.Client(tokens.Issue());using var anonymous=server.Client();
var denied=await anonymous.PostAsJsonAsync("/predict",new PredictInput {X=5});Check.That((int)denied.StatusCode==401,"anonymous prediction requests are rejected before inference");
var response=await http.PostAsJsonAsync("/predict",new PredictInput {X=5});response.EnsureSuccessStatusCode();
var first=await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
Check.That(first.GetProperty("modelVersion").GetString()==PredictionService.Version&&float.IsFinite(first.GetProperty("score").GetSingle()),"an actual HTTP request receives a finite real-model prediction and version");
var invalid=await http.PostAsJsonAsync("/predict",new PredictInput {X=-1});Check.That((int)invalid.StatusCode==400,"invalid input is rejected before prediction or monitoring");
for(int i=0;i<12;i++){var item=await http.PostAsJsonAsync("/predict",new PredictInput {X=30+i});item.EnsureSuccessStatusCode();}
var state=await http.GetFromJsonAsync<System.Text.Json.JsonElement>("/monitor");
Console.WriteLine($"DRIFT samples={state.GetProperty("samples")} meanShift={state.GetProperty("meanShift")} alert={state.GetProperty("alert")}");
Check.That(state.GetProperty("alert").GetBoolean(),"the actual shifted input window triggers the agreed drift alert","ML drift was hidden by a missing monitor");
Harness.Save("ml-evidence.json",new {passed=true,actualMlNet=true,actualHttp=true,version=PredictionService.Version,prediction=first,monitor=state,invalidStatus=(int)invalid.StatusCode});
