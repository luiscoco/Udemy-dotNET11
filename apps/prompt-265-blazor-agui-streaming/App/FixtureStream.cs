using System.Text.Json;
using Microsoft.AspNetCore.Http;
public sealed class FixtureStream(bool broken)
{
    public int Events{get;private set;}public int ToolCalls{get;private set;}
    public async Task Respond(HttpContext context){
        using var input=await JsonDocument.ParseAsync(context.Request.Body);
        string thread=input.RootElement.TryGetProperty("threadId",out var tid)?tid.GetString()!:"course-thread";
        string run=input.RootElement.TryGetProperty("runId",out var rid)?rid.GetString()!:Guid.NewGuid().ToString();
        context.Response.ContentType="text/event-stream";
        async Task Emit(object data){await context.Response.WriteAsync("data: "+JsonSerializer.Serialize(data)+new string((char)10,2));await context.Response.Body.FlushAsync();Events++;await Task.Delay(80);}
        await Emit(new{type="RUN_STARTED",threadId=thread,runId=run});
        await Emit(new{type="TEXT_MESSAGE_START",messageId="answer",role="assistant"});
        await Emit(new{type="TEXT_MESSAGE_CONTENT",messageId="answer",delta="Checking the course fixture. "});
        if(broken){await Emit(new{type="RUN_ERROR",message="Controlled protocol fixture failure",code="COURSE265"});return;}
        ToolCalls++;
        await Emit(new{type="TOOL_CALL_START",toolCallId="lookup-1",toolCallName="course_lookup",parentMessageId="answer"});
        await Emit(new{type="TOOL_CALL_ARGS",toolCallId="lookup-1",delta=JsonSerializer.Serialize(new{topic="async"})});
        await Emit(new{type="TOOL_CALL_END",toolCallId="lookup-1"});
        await Emit(new{type="TOOL_CALL_RESULT",messageId="result-1",toolCallId="lookup-1",content="Cancellation is preserved.",role="tool"});
        await Emit(new{type="TEXT_MESSAGE_CONTENT",messageId="answer",delta="cancellation verified"});
        await Emit(new{type="TEXT_MESSAGE_END",messageId="answer"});
        await Emit(new{type="RUN_FINISHED",threadId=thread,runId=run});
    }
}
