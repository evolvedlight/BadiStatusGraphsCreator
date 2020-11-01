using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YetAnotherConsoleTables;

namespace BadiStatusGraphsCreator
{
    class Program
    {
        static async Task Main()
        {
            var badiInfoScraper = new BadiInfoScraper();
            var results = await badiInfoScraper.GetBadiInfo();

            ConsoleTable.From(results).Write();

            var github = new GitHubClient(new ProductHeaderValue("BadiInfoUpdater"));
            var tokenAuth = new Credentials(Environment.GetEnvironmentVariable("GH_TOKEN"));

            github.Credentials = tokenAuth;

            var jsonContent = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            var existingFile = await github.Repository.Content.GetAllContents("evolvedlight", "ZhBadiStatus", "latest.json");


            var existingBadis = JsonSerializer.Deserialize<List<BadiInfo>>(existingFile.Single().Content);
            var commitStrings = new List<String>();
            var changedBadis = new List<String>();
            foreach (var existingBadi in existingBadis)
            {
                var newBadi = results.Single(x => x.Name == existingBadi.Name);
                if (newBadi.LastUpdate != existingBadi.LastUpdate)
                {
                    commitStrings.Add($"- {newBadi.Name} changed status at {newBadi.LastUpdate} from {existingBadi.Status} to {newBadi.Status}");
                    changedBadis.Add(existingBadi.Name);
                }
            }

            if (commitStrings.Any())
            {
                foreach (var commitString in commitStrings)
                {
                    Console.WriteLine(commitString);
                }

                var upReq = new UpdateFileRequest($"{string.Join(", ", changedBadis)} changed: \n\n{string.Join(Environment.NewLine, commitStrings)}", jsonContent, existingFile.First().Sha, "main");

                var res = await github.Repository.Content.UpdateFile("evolvedlight", "ZhBadiStatus", "latest.json", upReq);

                Console.WriteLine($"Updated Badis: {res.Commit.Sha}");
            }
            else
            {
                Console.WriteLine($"No change for badis, no commit");
            }
        }
    }
}
