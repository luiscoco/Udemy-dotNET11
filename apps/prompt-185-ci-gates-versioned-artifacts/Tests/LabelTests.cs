using Microsoft.VisualStudio.TestTools.UnitTesting;
[TestClass]
public sealed class LabelTests
{
    [TestMethod] public void Trims_valid_title()=>Assert.AreEqual("Course",CourseLabel.Format(" Course "));
    [TestMethod] public void Rejects_null()=>Assert.ThrowsExactly<ArgumentNullException>(()=>CourseLabel.Format(null));
    [TestMethod] public void Rejects_blank()=>Assert.ThrowsExactly<ArgumentException>(()=>CourseLabel.Format(" "));
}
