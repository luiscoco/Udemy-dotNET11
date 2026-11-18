using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using var client=AgentRuntime.Client(); var store=new MemoryRepository();
AIAgent agent=client.AsAIAgent(new ChatClientAgentOptions
{
    Name="MemoryTutor", ChatOptions=AgentRuntime.Options("Reply with a short greeting. Do not repeat memory facts."),
    AIContextProviders=[new ScopedMemory(store,"a","alice",args.Contains("problem"))],
    ChatHistoryProvider=new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions
    {
        StorageInputRequestMessageFilter=messages=>messages.Where(m=>
            m.GetAgentRequestMessageSourceType()!=AgentRequestMessageSourceType.AIContextProvider &&
            m.GetAgentRequestMessageSourceType()!=AgentRequestMessageSourceType.ChatHistory),
        StorageInputResponseMessageFilter=_=>[]
    })
});
var session=await agent.CreateSessionAsync();
await AgentRuntime.Ask(agent,"Hello. What is your role?",session);
string supplied=client.InputText;
bool isolated=!supplied.Contains("BOB-ONLY")&&!supplied.Contains("OTHER-TENANT")&&!supplied.Contains("DELETED-A7");
bool expired=!supplied.Contains("EXPIRED-A7");
Console.WriteLine($"MEMORY allowed={supplied.Contains("ALLOWED-A7")} isolated={isolated} expiryEnforced={expired}");
Check.That(supplied.Contains("ALLOWED-A7")&&isolated,"only the trusted actor's current memory is supplied");
Check.That(expired,"expired memory is excluded","AGENT expired memory entered context");
var serialized=await agent.SerializeSessionAsync(session);
Check.That(!serialized.ToString().Contains("ALLOWED-A7"),"transient memory is excluded from stored history");
store.RevokeAlice();
await AgentRuntime.Ask(agent,"Hello again. Reply briefly.",session);
bool revoked=!client.InputText.Contains("ALLOWED-A7");
Check.That(revoked,"revoked memory does not reappear through conversation history");
Check.That(store.Select("a","bob").Single().Text.StartsWith("BOB-ONLY"),"another owner receives only their own records");
AgentRuntime.Save(new { passed=true, isolated, expired, revoked, transientContext=true, maxItems=2 });
