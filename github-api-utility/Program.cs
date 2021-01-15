using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GodotGithubOverview
{
	class Program
	{
		public const string OWNER = "godotengine";
		public const string REPO = "godot";

		static async Task Main(string[] args)
		{
			Environment.SetEnvironmentVariable("GITHUB_TOKEN", "your-token-here");

			var client = new GitHubClient(
				new ProductHeaderValue("GodotGithubOverview"),
				new InMemoryCredentialStore(
					new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
				);

			var limit = await client.Miscellaneous.GetRateLimits();
			Console.WriteLine($"Github API Rate Limit: {limit.Resources.Core.Remaining} of {limit.Resources.Core.Limit} remaining");


			// Fetch PRs and Files one after the other (awaited) otherwise Github will slap us with abuse limits
			Console.WriteLine($"Fetching all Pull Requests for {OWNER}/{REPO}...");
			var prs = (await client.PullRequest.GetAllForRepository(OWNER, REPO, new PullRequestRequest { State = ItemStateFilter.Open }))
				.Select(async pr =>
				{
					Console.WriteLine($"Fetching file information for PR #{pr.Number}...");

					// Create DTO Based on returned PR, and fill with files from another API call
					var prDto = new PullRequestDTO(pr)
					{
						Files = (await client.PullRequest
						.Files(OWNER, REPO, pr.Number))
						.Select(file => new PullRequestFileDTO(file))
						.ToList()
					};

					// Calculate total additions and deletions based on the files
					prDto.Additions = prDto.Files.Sum(f => f.Additions);
					prDto.Deletions = prDto.Files.Sum(f => f.Deletions);

					return prDto;
				});

			var allPrs = (await Task.WhenAll(prs)).ToList();
			// Wait for all PRs to be done.
			Console.WriteLine($"Pull Requests & Files Fetched Successfully");

			// Save to file
			var text = JsonSerializer.Serialize(allPrs, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			var jsonFilePath = Path.Combine(Environment.CurrentDirectory, "prs.json");
			File.Delete(jsonFilePath);
			File.WriteAllText(jsonFilePath, text);

			Console.WriteLine($"JSON File written to path {jsonFilePath}");
			Console.WriteLine($"Done.");
		}
	}
}
