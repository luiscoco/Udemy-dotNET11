using System.Text.Json;
public enum Route { Billing, Technical }
public sealed record RouteChoice(string Route);
public sealed record ServiceRequest(string TaskId,string Tenant,string Actor,string Topic,string BillingFacts,string TechnicalFacts,string UnrelatedSecret);
public sealed record Handoff(string TaskId,string Tenant,string Actor,string Topic,string RelevantFacts);
public static class HandoffPolicy
{
    public static Route Parse(string choice) => choice switch
    {
        "Billing"=>Route.Billing, "Technical"=>Route.Technical,
        _=>throw new ArgumentException("Unknown agent route")
    };
    public static string Build(ServiceRequest request,Route route,bool includeRawTranscript=false)
    {
        if(request.Tenant!="a"||request.Actor!="alice") throw new UnauthorizedAccessException("Handoff actor denied");
        if(includeRawTranscript) return JsonSerializer.Serialize(request);
        return JsonSerializer.Serialize(new Handoff(request.TaskId,request.Tenant,request.Actor,request.Topic,
            route==Route.Billing?request.BillingFacts:request.TechnicalFacts));
    }
    public static void AuthorizeReceiver(Route route,string tenant,string actor)
    {
        if(tenant!="a"||actor!="alice"||!Enum.IsDefined(route)) throw new UnauthorizedAccessException("Receiving agent denied");
    }
}
