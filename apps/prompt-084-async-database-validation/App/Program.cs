using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
Directory.CreateDirectory("artifacts"); string file=Path.GetFullPath("artifacts/users.db");
var gate=new ValidationGate();
await using var api=await ApiHarness.StartAsync(b =>
{
    b.Services.AddSingleton(gate);
    b.Services.AddDbContextFactory<CourseDb>(o=>o.UseSqlite("Data Source="+file));
    b.Services.AddValidation();
},RegistrationApi.Map);
var factory=api.App.Services.GetRequiredService<IDbContextFactory<CourseDb>>();
await using(var db=await factory.CreateDbContextAsync()) { await db.Database.EnsureDeletedAsync(); await db.Database.EnsureCreatedAsync(); }
using var first=await api.Client.PostAsJsonAsync("/users",new RegisterUser {Email="learner@example.invalid"});
using var duplicate=await api.Client.PostAsJsonAsync("/users",new RegisterUser {Email="learner@example.invalid"});
Console.WriteLine($"api: first registration {(int)first.StatusCode}; duplicate validation {(int)duplicate.StatusCode}");
if(args.FirstOrDefault()=="duplicate") Check.That((int)duplicate.StatusCode==201,"the asynchronous database validator must reject an existing email");
string problem=await duplicate.Content.ReadAsStringAsync();
Check.That((int)first.StatusCode==201 && (int)duplicate.StatusCode==400 && problem.Contains("Email is already registered"),"awaited database validation returns a field-level error before insertion");
var racing=await Task.WhenAll(Enumerable.Range(0,2).Select(_=>api.Client.PostAsJsonAsync("/users",new RegisterUser {Email="race@example.invalid"})));
int[] statuses=racing.Select(r=>(int)r.StatusCode).Order().ToArray(); foreach(var response in racing) response.Dispose();
Console.WriteLine("database: concurrent registrations "+string.Join(", ",statuses));
if(args.FirstOrDefault()=="race") Check.That(statuses.All(s=>s==201),"a preflight uniqueness lookup cannot replace the database constraint");
Check.That(statuses.SequenceEqual(new[]{201,409}),"the unique index resolves a controlled check-then-insert race");
await using(var db=await factory.CreateDbContextAsync()) Check.That(await db.Users.CountAsync()==2,"only one row is stored for each unique email");
using var cancellation=new CancellationTokenSource();
Task<HttpResponseMessage> waiting=api.Client.PostAsJsonAsync("/users",new RegisterUser {Email="wait@example.invalid"},cancellation.Token);
await gate.Entered.Task.WaitAsync(TimeSpan.FromSeconds(10)); cancellation.Cancel();
try { using var ignored=await waiting; throw new Exception("Expected request cancellation"); } catch(OperationCanceledException) { }
await gate.Cancelled.Task.WaitAsync(TimeSpan.FromSeconds(10));
Check.That(gate.Cancelled.Task.IsCompletedSuccessfully,"request cancellation reaches the asynchronous validation rule");
