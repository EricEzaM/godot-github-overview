using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading;
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
			Environment.SetEnvironmentVariable("GITHUB_TOKEN", "688f11cbbc1a62942edb58467241154a140ed5c7");

			var client = new ObservableGitHubClient(
				new ProductHeaderValue("GodotGithubOverview"),
				new InMemoryCredentialStore(
					new Credentials(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
				);

			var limit = await client.Miscellaneous.GetRateLimits().Do(limit =>
				{
					Console.WriteLine($"Github API Rate Limit: {limit.Resources.Core.Remaining} of {limit.Resources.Core.Limit} remaining");
				});

			// Fetch PRs and Files one after the other (awaited) otherwise Github will slap us with abuse limits
			Console.WriteLine($"Fetching all Pull Requests for {OWNER}/{REPO}...");

			List<PullRequestDTO> prs = new List<PullRequestDTO>();
			await client.PullRequest
				.GetAllForRepository(OWNER, REPO, new PullRequestRequest { State = ItemStateFilter.Open })
				.Do(pr =>
				{
					Console.WriteLine($"Got PR {pr.Number}: {prs.Count} PRs completed.");
					prs.Add(new PullRequestDTO(pr));
				});

			// Fetch the files for each PR
			// This must be syncronous to avoid github abuse rate limits.
			for (int i = 0; i < prs.Count; i++)
			{
				var pr = prs[i];

				Console.WriteLine($"Fetching Files for PR {i}/{prs.Count}");
				var hello = client.PullRequest
					.Files(OWNER, REPO, pr.Number)
					.Select(file => new PullRequestFileDTO(file))
					.ToList();

				// Calculate total additions and deletions based on the files
				pr.Additions = pr.Files.Sum(f => f.Additions);
				pr.Deletions = pr.Files.Sum(f => f.Deletions);
			}

			// Wait for all PRs to be done.
			Console.WriteLine($"Pull Requests & Files Fetched Successfully");

			// Save to file
			var text = JsonSerializer.Serialize(prs, new JsonSerializerOptions
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
