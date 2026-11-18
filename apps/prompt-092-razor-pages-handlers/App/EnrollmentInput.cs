using System.ComponentModel.DataAnnotations;
public sealed class EnrollmentInput
{
    [Required(ErrorMessage="Name is required.")]
    public string Name {get;set;}="";
    [Range(1,4,ErrorMessage="Seats must be between 1 and 4.")]
    public int Seats {get;set;}=1;
}
