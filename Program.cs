using Octokit;
using System;
using System.Linq;
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

            var upReq = new UpdateFileRequest("Update Badi Statuses", jsonContent, existingFile.First().Sha, "main");

            var res = await github.Repository.Content.UpdateFile("evolvedlight", "ZhBadiStatus", "latest.json", upReq);
            Console.WriteLine(res.Commit.Sha);
        }
    }
}
