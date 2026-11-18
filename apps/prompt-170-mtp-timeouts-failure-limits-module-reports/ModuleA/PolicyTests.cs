[TestClass]
public sealed class PolicyTests
{
    [TestMethod, TestCategory("Passing")]
    public void First_module_checks_booking_math()
        => Assert.AreEqual(1,CourseRules.SeatsAfterBooking(3,2));
    [TestMethod, TestCategory("Passing")]
    public void First_module_checks_exact_capacity()
        => Assert.AreEqual(0,CourseRules.SeatsAfterBooking(2,2));
    [TestMethod, TestCategory("Slow")]
    public async Task Deliberately_slow_policy_probe()
        => await Task.Delay(TimeSpan.FromSeconds(30));
    [TestMethod, TestCategory("Failing")]
    public void Deliberate_failure_one() => Assert.Fail("Failure-limit teaching probe one.");
    [TestMethod, TestCategory("Failing")]
    public void Deliberate_failure_two() => Assert.Fail("Failure-limit teaching probe two.");
    [TestMethod, TestCategory("Failing")]
    public void Deliberate_failure_three() => Assert.Fail("Failure-limit teaching probe three.");
}
