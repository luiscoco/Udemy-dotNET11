using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
public static class RuleMetadata
{
    public static (string Code, int Maximum) Read(string assembly)
    {
        using var stream = File.OpenRead(assembly);
        using var pe = new PEReader(stream);
        MetadataReader reader = pe.GetMetadataReader();
        foreach (var handle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(handle);
            if (reader.GetString(type.Name) != nameof(InvoiceRequest)) continue;
            foreach (var attributeHandle in type.GetCustomAttributes())
            {
                var attribute = reader.GetCustomAttribute(attributeHandle);
                if (attribute.Constructor.Kind != HandleKind.MethodDefinition) continue;
                var method = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);
                var owner = reader.GetTypeDefinition(method.GetDeclaringType());
                if (reader.GetString(owner.Name) != nameof(RuleAttribute)) continue;
                BlobReader blob = reader.GetBlobReader(attribute.Value);
                if (blob.ReadUInt16() != 1) throw new InvalidDataException("Invalid attribute prolog");
                string code = blob.ReadSerializedString() ?? throw new InvalidDataException("Missing rule code");
                int maximum = blob.ReadInt32();
                if (blob.ReadUInt16() != 0) throw new InvalidDataException("Unexpected named arguments");
                return (code, maximum);
            }
        }
        throw new InvalidDataException("Rule metadata was not found");
    }
}
