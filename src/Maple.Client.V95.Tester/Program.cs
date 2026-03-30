using System.Linq;
using Maple.Client.V95;

Console.WriteLine($"Maple.Client.V95 version: {typeof(ClientStructs).Assembly.GetName().Version}");
Console.WriteLine($"Registry struct count: {ClientStructs.Registry.StructNames.Count()}");
Console.WriteLine("OK");
