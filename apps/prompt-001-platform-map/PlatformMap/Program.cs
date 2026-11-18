using System;
using System.Linq;
using System.Text.Json;

int[] values = { 1, 2, 3 };
var doubled = values.Select(n => n * 2);
Console.WriteLine(JsonSerializer.Serialize(doubled));
