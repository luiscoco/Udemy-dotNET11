using System.Text.Json.Serialization;
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(EmailNotice), "email")]
[JsonDerivedType(typeof(SmsNotice), "sms")]
public abstract record Notice(string Id);
public sealed record EmailNotice(string Id, string Address, string Subject) : Notice(Id);
public sealed record SmsNotice(string Id, string Phone, string Text) : Notice(Id);
[JsonSerializable(typeof(Notice))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AppJsonContext : JsonSerializerContext;
