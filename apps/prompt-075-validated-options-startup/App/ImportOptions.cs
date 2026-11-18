using System.ComponentModel.DataAnnotations;
public sealed class ImportOptions
{
    [Required, Url]
    public string Endpoint { get; set; } = "";
    [Range(1, 10)]
    public int BatchSize { get; set; }
}
