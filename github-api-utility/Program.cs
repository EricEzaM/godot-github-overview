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
			Console.WriteLine("Getting Data...");
			var pullRequestData = await GraphQLDataFetcher.GetPullRequestData(Environment.GetEnvironmentVariable("ACCESS_TOKEN"));

			WriteObjectToJsonFile("prs", pullRequestData);
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
