using Contracts;
using Catalog;
using Enrollment;
using Microsoft.Extensions.DependencyInjection;
var services = new ServiceCollection();
services.AddCatalog();
services.AddTransient<EnrollmentService>();
using var provider = services.BuildServiceProvider();
var enrollments = provider.GetRequiredService<EnrollmentService>();
Check.That(enrollments.Enroll(11, "Luis") == "Luis enrolled in .NET architecture", "enrollment uses the published catalog contract");
Check.That(enrollments.Enroll(99, "Luis") == "Course unavailable", "a missing course has an explicit result");
var references = System.Xml.Linq.XDocument.Load("Enrollment/Enrollment.csproj")
    .Descendants("ProjectReference").Select(x => (string)x.Attribute("Include")!).ToArray();
Check.That(references.SequenceEqual(new[]{"../Contracts/Contracts.csproj"}), "Enrollment depends only on Contracts");
Check.That(typeof(CatalogModule).Assembly.GetType("Catalog.CourseStore")!.IsNotPublic, "catalog storage remains internal to its module");
