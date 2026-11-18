var app = CourseWeb.Create();
await app.StartAsync();
Console.WriteLine("Open " + app.Urls.Single());
await app.WaitForShutdownAsync();
