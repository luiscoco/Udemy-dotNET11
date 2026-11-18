using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
public sealed class JoinModel : PageModel
{
    [BindProperty]
    public EnrollmentInput Input {get;set;}=new();
    public void OnGet() { }
    public IActionResult OnPostReserve()
    {
        if(!ModelState.IsValid) return Page();
        return RedirectToPage("Confirmation",new {name=Input.Name,seats=Input.Seats});
    }
}
