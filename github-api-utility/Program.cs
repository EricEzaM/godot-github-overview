using GodotGithubOverview.GraphQL;
using System;
using System.Threading.Tasks;

namespace GodotGithubOverview
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Getting Data...");

			// Set the ACCESS_TOKEN in your environment variables, or use a temporary debug one in Properties/launchSettings.json (Visual Studio)
			var accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
			var dataFetcher = new GraphQLDataFetcher(accessToken);
			var data = await dataFetcher.GetPullRequestData();

			Utils.WriteObjectToJsonFile("prs", data);
			Utils.WriteObjectToJsonFile("metadata", new
			{
				LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			});

			Console.WriteLine($"Done.");
		}
	}
}
