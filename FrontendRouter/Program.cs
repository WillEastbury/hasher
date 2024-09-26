using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

int maxRequestHashes = 100;
int arbiterRefreshRate = 5000;

IHttpClientFactory clientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
ParallelOptions parallelOptions = new(){};

string partitionConfigArbiter = Environment.GetEnvironmentVariable("ARBITER_ENDPOINT") ?? "https://localhost:5000";
string arbiterKey = Environment.GetEnvironmentVariable("ARBITER_KEY") ?? "SecretKey123456";

// We need a bucket to store the partitions handled by the addresses to route to (key == 2 digit partition id, value = address)
Dictionary<string, string> partitionAddresses = new();

// and we need to periodically download them, on a background thread from the arbiter, say every second for now
Task.Run(async () => {
    while (true)
    {
        partitionAddresses = await GetPartitionedLoadBalancerAddressesAsync(clientFactory);
        Console.WriteLine("Background: Refreshed partition addresses from arbiter");
        Console.WriteLine($"Partitions {System.Text.Json.JsonSerializer.Serialize(partitionAddresses)}");
        await Task.Delay(arbiterRefreshRate);
    }
});

app.MapGet("partitionAddresses", () => partitionAddresses);

// SEARCH A single value
app.MapGet("/search/{hash}", async ([FromRoute]string hash, IHttpClientFactory clientFactory) => {

    Dictionary<string, List<string>> queries = new();
    queries[hash.Substring(0, 2)] = new List<string>(){hash};
    return Results.Ok(await SearchRemotePartitionSetForListOfHashesAsync(queries, clientFactory));
});

// SEARCH A BATCH OF Values (Sort and divide up the searched hashes and send them to the lb endpoint)
app.MapPost("/search", async ([FromBody]List<string> request, IHttpClientFactory clientFactory) => {

    if (request.Count < 1) return Results.BadRequest($"There must be more than zero hashes in the request batch");
    if (request.Count > maxRequestHashes) return Results.BadRequest($"Too many hashes in request, max {maxRequestHashes}");

    // Get the list of requests for hashes (each pair of first 2 chars of hash and the list of matching hashes)
    Dictionary<string, List<string>> queries = request.GroupBy(x => x.Substring(0, 2)).ToDictionary(x => x.Key, x => x.ToList());
    return Results.Ok(await SearchRemotePartitionSetForListOfHashesAsync(queries, clientFactory));

});

async Task<Dictionary<string,string>> GetPartitionedLoadBalancerAddressesAsync(IHttpClientFactory clientFactory)
{
    var client = clientFactory.CreateClient();
    client.DefaultRequestHeaders.Add("X-Arbiter-Key", arbiterKey);
    var response = await client.GetAsync(partitionConfigArbiter + "/getPartitions");
    Dictionary<string,string> partitionAddresses = await response.Content.ReadFromJsonAsync<Dictionary<string,string>>();
    return partitionAddresses;
}

async Task<Dictionary<string, Dictionary<string, bool>>> SearchRemotePartitionSetForListOfHashesAsync(Dictionary<string, List<string>> hashList, IHttpClientFactory clientFactory)
{
    Console.WriteLine("Searching -- Remote across " + hashList.Count + " partitions");

    var results = new Dictionary<string, Dictionary<string, bool>>();
    await Parallel.ForEachAsync(hashList, parallelOptions, async (hashGroup, ct) =>
    {
        string queryUrl = partitionAddresses[hashGroup.Key] + "/HashBatchLocal";
        Console.WriteLine("Searching for " + hashGroup.Key + " at " + queryUrl);
        HttpClient client = clientFactory.CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync(queryUrl, hashGroup.Value);
        string responseContent = await response.Content.ReadAsStringAsync();
        results.Add(hashGroup.Key, System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(responseContent));
    });

    return results;
}

app.Run();
