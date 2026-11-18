using Microsoft.AspNetCore.Mvc;
[Route("enroll")]
public sealed class EnrollController : Controller
{
    [HttpGet("create")]
    public IActionResult Create() => View(new EnrollmentInput());
    [HttpPost("create"),ValidateAntiForgeryToken]
    public IActionResult Create(EnrollmentInput input)
    {
        if(!ModelState.IsValid) return View(input);
        return RedirectToAction(nameof(Success),new {name=input.Name,seats=input.Seats});
    }
    [HttpGet("success")]
    public IActionResult Success(string name,int seats) => View(new EnrollmentInput {Name=name,Seats=seats});
}
