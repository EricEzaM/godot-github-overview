using GodotGithubOverview.DTOs;
using GodotGithubOverview.GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GodotGithubOverview
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			Console.WriteLine("Getting Data...");

			// Set the ACCESS_TOKEN in your environment variables, or use a temporary debug one in Properties/launchSettings.json (Visual Studio)
			var accessToken = Environment.GetEnvironmentVariable("ACCESS_TOKEN");
			var dataFetcher = new GraphQLDataFetcher(accessToken);

			Console.WriteLine($"Getting Open PRs.");
			var openPrData = await dataFetcher.GetOpenPullRequests();

			Console.WriteLine($"Getting Historical Data for PRs.");
			var historicalData = await dataFetcher.GetPullRequestsHistoricalData();

			if (openPrData.Any())
			{
				Utils.WriteObjectToJsonFile("prs", openPrData);
			}

			if (historicalData.Any())
			{
				Utils.WriteObjectToJsonFile("pr_historical", historicalData);
			}

			if (historicalData.Any() || openPrData.Any())
			{
				Utils.WriteObjectToJsonFile("metadata", new
				{
					LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
				});

				Console.WriteLine($"Done.");
				return 0;
			}
			else
			{
				Console.WriteLine($"Data was empty. No files written.");
				return 1;
			}
		}
	}
}
