using Maple.Client.V95;

Console.WriteLine($"Maple.Client.V95 version: {typeof(ClientStructs).Assembly.GetName().Version}");
Console.WriteLine($"Registry field count: {ClientStructs.Registry.Fields.Count}");
Console.WriteLine("OK");
