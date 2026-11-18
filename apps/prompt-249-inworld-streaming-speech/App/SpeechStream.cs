using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
public static class SpeechStream
{
    public static async Task<(int Chunks,long Bytes)> Generate(string file)
    {
        string key=Environment.GetEnvironmentVariable("INWORLD_API_KEY")??throw new InvalidOperationException("Set INWORLD_API_KEY in the process environment");
        using var http=new HttpClient {Timeout=TimeSpan.FromSeconds(90)};
        using var request=new HttpRequestMessage(HttpMethod.Post,"https://api.inworld.ai/tts/v1/voice:stream");
        request.Headers.Authorization=new AuthenticationHeaderValue("Basic",key);
        request.Content=new StringContent(JsonSerializer.Serialize(new {text="The synthetic lesson is ready.",voiceId="Yash",modelId="inworld-tts-2",
            enhanceGeneration=true,deliveryMode="BALANCED",audioConfig=new {audioEncoding="MP3",sampleRateHertz=48000,speakingRate=1.25}}),Encoding.UTF8,"application/json");
        using var response=await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();using var reader=new StreamReader(await response.Content.ReadAsStreamAsync());
        await using var output=File.Create(file);int chunks=0;
        while(await reader.ReadLineAsync() is string line)
        {
            if(string.IsNullOrWhiteSpace(line))continue;
            using var json=JsonDocument.Parse(line);
            if(json.RootElement.TryGetProperty("error",out _))throw new InvalidDataException("Provider returned a streaming error");
            if(json.RootElement.TryGetProperty("result",out var result)&&result.TryGetProperty("audioContent",out var audio))
            {var bytes=Convert.FromBase64String(audio.GetString()!);await output.WriteAsync(bytes);chunks++;}
        }
        return (chunks,output.Length);
    }
    public static bool HasMp3Header(byte[] bytes)=>bytes.Length>=3&&
        ((bytes[0]==73&&bytes[1]==68&&bytes[2]==51)||(bytes[0]==255&&(bytes[1]&224)==224));
    public static void ValidateLabel(string declaredEncoding,byte[] bytes)
    {
        if(declaredEncoding!="MP3"||!HasMp3Header(bytes))throw new InvalidDataException("SPEECH MP3 was mislabeled as PCM");
    }
}
