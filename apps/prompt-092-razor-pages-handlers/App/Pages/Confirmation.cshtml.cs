using Microsoft.AspNetCore.Mvc.RazorPages;
public sealed class ConfirmationModel : PageModel
{
    public string Name {get;private set;}="";
    public int Seats {get;private set;}
    public void OnGet(string name,int seats) {Name=name;Seats=seats;}
}
