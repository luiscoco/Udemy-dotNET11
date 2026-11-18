using System.ComponentModel.DataAnnotations;
public sealed class Registration
{
    [Required(ErrorMessage = "Enter a username.")]
    [MinLength(3, ErrorMessage = "Use at least three characters.")]
    public string Username { get; set; } = "";
}
