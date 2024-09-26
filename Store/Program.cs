using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

List<string> hashes = new();
List<string> emptyPrefixes = new();
List<string> loadedPartitions = new();
ParallelOptions parallelOptions = new(){};

string pathForPartitionFolders = "./datafiles/";
string partitionConfigArbiter = Environment.GetEnvironmentVariable("ARBITER_ENDPOINT") ?? "https://localhost:5000";
string fixedPartitionConfig = Environment.GetEnvironmentVariable("FIXED_PARTITION") ?? "ALL"; 
string partitionEndpoint = Environment.GetEnvironmentVariable("PARTITION_ENDPOINT") ?? "https://localhost:6000";
string arbiterKey = Environment.GetEnvironmentVariable("ARBITER_KEY") ?? "SecretKey123456";

IHttpClientFactory htcf = app.Services.GetRequiredService<IHttpClientFactory>();
Console.WriteLine("PartitionConfig @ " + fixedPartitionConfig);

if (fixedPartitionConfig.ToUpper() == "ALL")
{
    // otherwise parse the folder and load all relevant partitions IN THE pathForPartitionFolders FOLDER
    foreach (var file in Directory.GetFiles(pathForPartitionFolders))
    {
        await LoadPartitionDataFile(hashes, pathForPartitionFolders, file.Split("/").Last().Split(".").First());
    }   
}
else if (fixedPartitionConfig.Contains(":"))
{
    Console.WriteLine("Fixed partition config contains a range");
    // Contains a range 
    var range = fixedPartitionConfig.Split(":");
    int rangeStart = int.Parse(range[0]);
    int rangeEnd = int.Parse(range[1]);

    for (int i = rangeStart; i <= rangeEnd; i++)
    {
        Console.WriteLine("Fixed partition config -- loading partition " + i + " from disk as hex " + i.ToString("X2"));
        // Convert the decimal number to a 2 digit hex string and load it
        await LoadPartitionDataFile(hashes, pathForPartitionFolders, i.ToString("X2"));           
    }
}
else
{
    // Single partition mode
    await LoadPartitionDataFile(hashes, pathForPartitionFolders, fixedPartitionConfig);
}

// Every 30 seconds re-register with the arbiter
Task.Run(async () => {
    while (true)
    {
        try
        {
            await Task.Delay(3000);
            await RegisterPartitionOwnershipAsync(htcf);
            await Task.Delay(30000);
            Console.WriteLine("Background: Re-registered ownership with arbiter");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Background: Error re-registering with arbiter: {e.Message}");
        }
        
    }
});



// Now register the partition ownership with the remote arbiter via the arbiter endpoint
async Task RegisterPartitionOwnershipAsync(IHttpClientFactory httpClientF)
{
    PartitionList pl = new PartitionList(){EndpointAddress = partitionEndpoint, PartitionsOwned = loadedPartitions};

    HttpClient httpClient = httpClientF.CreateClient();
    httpClient.DefaultRequestHeaders.Add("X-Arbiter-Key", arbiterKey);

    Console.WriteLine("REGISTERING PARTITIONS WITH ARBITER: " + partitionConfigArbiter + "/registerPartitionOwnership");
    HttpResponseMessage htrm = await httpClient.PostAsJsonAsync(partitionConfigArbiter + "/registerPartitionOwnership", pl);

    if (!htrm.IsSuccessStatusCode)
    {
        Console.WriteLine($"Error registering partitions with arbiter: {htrm.StatusCode} -- {await htrm.Content.ReadAsStringAsync()}");
    };

}

app.MapGet("/HashBatchLocal/{hash}", async ([FromRoute]string hash) => {
    
    Console.WriteLine($"Received {hash} to search for");
    return Results.Ok(await SearchPartitionForListHashesAsync(new List<string>(){hash}));

});

app.MapPost("/HashBatchLocal", async ([FromBody]List<string> request) => {
    
    Console.WriteLine($"Received {request.Count} hashes to search for");
    return Results.Ok(await SearchPartitionForListHashesAsync(request));

});

app.Run();

async Task<bool> SearchPartitionForHashAsync(string hashText){

    Console.WriteLine("Searching for " + hashText);

    var prefix = hashText.Substring(0, 2);
    // If its already in empty prefixes then it's defo not on disk or in memory, SKIP IT. 
    if (emptyPrefixes.Contains(prefix))
    {
        // skip it
        return false;
    }
    else
    {
        // At this stage we don't know if it's in memory or on disk, so we need to check memory first
        if (hashes.Contains(hashText))
        {
            // Console.WriteLine($"Found {hashText} in {prefix}");
            return true;
        }
        else
        {
            // not in mem, or in empty prefixes, so we need to check disk
            if (File.Exists(GetPrefixFileAddress(pathForPartitionFolders, prefix)))
            {
                await LoadPartitionDataFile(hashes, pathForPartitionFolders, prefix);

                // now test again and return accordingly
                if (hashes.Contains(hashText))
                {
                    Console.WriteLine($"Found {hashText} in {prefix}");
                    return true;
                }
                else
                {
                    // the prefix exists, but the hash is not in it so return not found
                    return false;
                }
            }
            else
            {
                // it's not in memory, not in empty prefixes, and not on disk, so add it to the empty prefixes list and return not found
                emptyPrefixes.Add(prefix);
                return false;
            }
        }
    }
}

// Multi-search version of the above that accepts a list of strings
async Task<Dictionary<string, bool>> SearchPartitionForListHashesAsync(List<string> hashList)
{
    var results = new Dictionary<string, bool>();
    await Parallel.ForEachAsync(hashList, parallelOptions, async (hash, ct) =>
    {
        results.Add(hash, await SearchPartitionForHashAsync(hash));
    });
    return results;
}

async Task LoadPartitionDataFile(List<string> hashes, string pathForPartitionFolders, string prefix)
{
    // the prefix exists, load it.
    Console.WriteLine($"Loading {prefix} from disk file {GetPrefixFileAddress(pathForPartitionFolders, prefix)}");
    hashes.AddRange((await File.ReadAllLinesAsync(GetPrefixFileAddress(pathForPartitionFolders, prefix))).ToList());
    loadedPartitions.Add(prefix);
}

string GetPrefixFileAddress(string pathForPartitionFolders, string prefix)
{
    return $"{Path.Combine(pathForPartitionFolders, prefix + ".txt")}";
}

public class PartitionList
{
    public string EndpointAddress { get; set; }
    public List<string> PartitionsOwned { get; set; }
}
