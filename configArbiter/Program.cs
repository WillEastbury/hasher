using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Dictionary<string, string> PartitionList = new();
string arbiterKey = Environment.GetEnvironmentVariable("ARBITER_KEY") ?? "SecretKey123456";

app.MapPost("/registerPartitionOwnership", async ([FromBody] PartitionList partitions, HttpContext httpContext) => 
{
    if (!CheckArbiterKey(httpContext))
    {
        return Results.Unauthorized();
    }
    else
    {
        Console.WriteLine($"Auth OK Registering {partitions.PartitionsOwned.Count} partitions to {partitions.EndpointAddress}");

        if (partitions.PartitionsOwned.Count < 1) {Console.WriteLine("Error - nop partitions registered"); return Results.BadRequest($"There must be more than zero partitions to register in the request batch");}
        if (partitions.PartitionsOwned.Count > 256) {Console.WriteLine("Error - too many"); return Results.BadRequest($"Too many partitions in request, max is {256}");}
        
        foreach (var partition in partitions.PartitionsOwned)
        {
            PartitionList[partition] = partitions.EndpointAddress;
        }

        return Results.Ok($"Registered {partitions.PartitionsOwned.Count} partitions to {partitions.EndpointAddress}");
    }
});

app.MapGet("/getPartitions", async (HttpContext httpContext) => {

    return Results.Ok(PartitionList);

});

bool CheckArbiterKey(HttpContext httpContext)
{
    Console.WriteLine("Checking arbiter key");
    if (httpContext.Request.Headers["X-Arbiter-Key"] != arbiterKey)
    {
        Console.WriteLine("Key Check Failed");
        return false;
    }
    return true;
}

app.Run();

public class PartitionList
{
    public string EndpointAddress { get; set; }
    public List<string> PartitionsOwned { get; set; }
}