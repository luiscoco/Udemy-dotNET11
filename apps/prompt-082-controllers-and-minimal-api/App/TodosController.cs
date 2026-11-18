using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("controllers/todos")]
public sealed class TodosController(TodoStore store) : ControllerBase
{
    [HttpPost]
    public ActionResult<Todo> Create(TodoInput input, CancellationToken ct)
    { var item = store.Add(input,ct); return CreatedAtAction(nameof(Get), new { id=item.Id }, item); }
    [HttpGet("{id:int}")]
    public ActionResult<Todo> Get(int id, CancellationToken ct) =>
        store.Get(id,ct) is { } item ? Ok(item) : NotFound();
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, TodoInput input, CancellationToken ct) =>
        store.Update(id,input,ct) ? NoContent() : NotFound();
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id, CancellationToken ct) =>
        store.Delete(id,ct) ? NoContent() : NotFound();
}
