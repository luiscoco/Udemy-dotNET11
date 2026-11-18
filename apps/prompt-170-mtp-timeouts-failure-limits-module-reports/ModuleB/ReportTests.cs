[TestClass]
public sealed class ReportTests
{
    [TestMethod, TestCategory("Passing")]
    public void Second_module_checks_one_seat()
        => Assert.AreEqual(0,CourseRules.SeatsAfterBooking(1,1));
    [TestMethod, TestCategory("Passing")]
    public void Second_module_checks_remaining_capacity()
        => Assert.AreEqual(4,CourseRules.SeatsAfterBooking(5,1));
}
