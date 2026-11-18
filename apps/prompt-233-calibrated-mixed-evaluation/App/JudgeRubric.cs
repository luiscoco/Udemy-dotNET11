using Microsoft.Extensions.AI;
using System.Text.Json;
public sealed record JudgeVote(bool? Pass,bool Valid,string Raw);
public static class JudgeRubric
{
    public static async Task<JudgeVote> Assess(EvalClient client,string candidate)
    {
        ChatMessage[] messages=[new(ChatRole.System,
            "Judge ONLY whether the answer says retention is seven days. Return JSON {\"pass\":true} or {\"pass\":false}. /no_think"),
            new(ChatRole.User,"Reference: Backups are retained for seven days. Answer: "+candidate)];
        var response=await client.GetResponseAsync(messages,new ChatOptions {ResponseFormat=ChatResponseFormat.Json});
        try
        {
            using var json=JsonDocument.Parse(response.Text);
            var value=json.RootElement.GetProperty("pass");
            return new(value.GetBoolean(),true,response.Text);
        }
        catch(JsonException){return new(null,false,response.Text);}
        catch(InvalidOperationException){return new(null,false,response.Text);}
        catch(KeyNotFoundException){return new(null,false,response.Text);}
    }
    public static bool Gate(bool hard,double f1,bool calibrated,bool? judge,bool unsafeOverride)
        =>unsafeOverride?hard||judge==true:hard&&f1>=0.8&&calibrated&&judge==true;
}
