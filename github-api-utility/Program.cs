using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
#if DEBUG
			Environment.SetEnvironmentVariable("ACCESS_TOKEN", "your-token-here");
#endif
			var client = new ObservableGitHubClient(
				new ProductHeaderValue("GodotGithubOverview"),
				new InMemoryCredentialStore(
					new Credentials(Environment.GetEnvironmentVariable("ACCESS_TOKEN")))
				);

			var limit = await client.Miscellaneous.GetRateLimits().Do(limit =>
				{
					Console.WriteLine($"Github API Rate Limit: {limit.Resources.Core.Remaining} of {limit.Resources.Core.Limit} remaining");
				});

			Console.WriteLine($"Fetching all Pull Requests for {OWNER}/{REPO}...");

			List<PullRequestDTO> prs = new List<PullRequestDTO>();
			await client.PullRequest
				.GetAllForRepository(OWNER, REPO, new PullRequestRequest { State = ItemStateFilter.Open })
				.Do(pr =>
				{
					Console.WriteLine($"Got PR {pr.Number}: {prs.Count} PRs completed.");
					prs.Add(new PullRequestDTO(pr));
				});

			Console.WriteLine($"All Pull Requests Fetched Successfully.");
			Console.WriteLine($"Fetching Files for all Pull Requests...");

			// Fetch the files for each PR. This must be syncronous to avoid github abuse rate limits. From Github API Best Practices: 
			// "Make requests for a single user or client ID serially. Do not make requests for a single user or client ID concurrently."
			for (int i = 0; i < prs.Count; i++)
			{
				var pr = prs[i];

				Console.WriteLine($"Fetching Files for PR {i + 1}/{prs.Count}");
				pr.Files = await client.PullRequest
					.Files(OWNER, REPO, pr.Number)
					.Select(file => new PullRequestFileDTO(file))
					.ToList();

				// Calculate total additions and deletions based on the files
				pr.Additions = pr.Files.Sum(f => f.Additions);
				pr.Deletions = pr.Files.Sum(f => f.Deletions);
				pr.Changes = pr.Additions + pr.Deletions;
				pr.ChangedFiles = pr.Files.Count();
			}

			Console.WriteLine($"All Files Fetched Successfully.");
			Console.WriteLine($"Organising & Saving data to disk...");

			// Relate each file to PRs which affect it
			var files = prs
				.SelectMany(pr => pr.Files)
				.GroupBy(f => f.Filename)
				.Select(g => g.Key) // GroupBy + Select -> Gets distinct filenames without using Distinct() and an IEqualityComparer.
				.Select(filename => {
					// Get all the "PullRequests" which contain the file.
					var prsWithFile = prs
						.Where(pr => pr.Files.Any(f => f.Filename == filename));

					// Get all the "PullRequestFiles" which have the same name
					var allFilesWithSameFilename = prsWithFile
						.SelectMany(pr => pr.Files)
						.Where(f => f.Filename == filename);

					return new FileDTO
					{
						Filename = filename,
						PullRequestNumbers = prsWithFile.Select(pr => pr.Number),
						Additions = allFilesWithSameFilename.Sum(prf => prf.Additions),
						Deletions = allFilesWithSameFilename.Sum(prf => prf.Deletions)
					};
				});

			// Save to file
			WriteObjectToJsonFile("prs", prs);
			WriteObjectToJsonFile("files", files);

			// Write some metadata to a different file.
			WriteObjectToJsonFile("metadata", new
			{
				LastUpdated = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			});

			Console.WriteLine($"Done.");
		}

		public static void WriteObjectToJsonFile<T>(string filename, T inputObject)
		{
			var prsJson = JsonSerializer.Serialize(inputObject, new JsonSerializerOptions
			{
				WriteIndented = true,
			});

			var filepath = Path.Combine(Environment.CurrentDirectory, filename + ".json");
			File.Delete(filepath);
			File.WriteAllText(filepath, prsJson);

			Console.WriteLine($"JSON File '{filename}' written to path {filepath}");
		}
	}
}
