using System.Text.Json;
Notice source = new EmailNotice("N-7", "student@example.invalid", "Welcome");
string json = JsonSerializer.Serialize(source, AppJsonContext.Default.Notice);
Console.WriteLine(json);
if (args.FirstOrDefault() == "unknown")
    json = "{\"kind\":\"push\",\"Id\":\"N-7\"}";
Notice? restored = JsonSerializer.Deserialize(json, AppJsonContext.Default.Notice);
Check.That(restored is EmailNotice { Address: "student@example.invalid", Subject: "Welcome" },
    "declared discriminator restores the permitted derived contract");
Check.That(json.Contains("email"), "wire JSON carries the agreed discriminator");
Notice sms = new SmsNotice("N-8", "+34000000000", "Ready");
string smsJson = JsonSerializer.Serialize(sms, AppJsonContext.Default.Notice);
Check.That(JsonSerializer.Deserialize(smsJson, AppJsonContext.Default.Notice) is SmsNotice { Text: "Ready" },
    "the second explicitly permitted case round-trips");
Console.WriteLine($"Generated contract: {AppJsonContext.Default.Notice.Type.Name}");
