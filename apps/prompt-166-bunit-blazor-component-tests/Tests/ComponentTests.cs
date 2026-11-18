using Bunit;
using CourseComponents;
using Microsoft.Extensions.DependencyInjection;
[TestClass]
public sealed class ComponentTests
{
    [TestMethod]
    public void Add_button_is_disabled_at_capacity()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ISelectionStore>(new SelectionFake());
        var component = context.Render<CoursePicker>(parameters => parameters.Add(p => p.Capacity, 2));
        component.Find("#add").Click();
        component.Find("#add").Click();
        Assert.AreEqual("Selected 2 of 2", component.Find("[data-testid=count]").TextContent);
        Assert.IsTrue(component.Find("#add").HasAttribute("disabled"), "The rendered button must be disabled at capacity.");
    }
    [TestMethod]
    public void Zero_capacity_starts_with_add_disabled()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton<ISelectionStore>(new SelectionFake());
        var component = context.Render<CoursePicker>(parameters => parameters.Add(p => p.Capacity, 0));
        Assert.IsTrue(component.Find("#add").HasAttribute("disabled"));
    }
    [TestMethod]
    public void Save_updates_the_rendered_status_after_the_service_call()
    {
        using var context = new BunitContext();
        var store = new SelectionFake();
        context.Services.AddSingleton<ISelectionStore>(store);
        var component = context.Render<CoursePicker>();
        component.Find("#add").Click();
        component.Find("#save").Click();
        component.WaitForAssertion(() => Assert.AreEqual("Saved 1 seats.",component.Find("[role=status]").TextContent));
        CollectionAssert.AreEqual(new[]{1},store.Saved.ToArray());
    }
}
public sealed class SelectionFake : ISelectionStore
{
    public List<int> Saved { get; } = [];
    public async Task SaveAsync(int count)
    {
        await Task.Yield();
        Saved.Add(count);
    }
}
