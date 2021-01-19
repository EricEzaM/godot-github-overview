using System;
using System.IO;
using System.Text.Json;

namespace GodotGithubOverview
{
	public static class Utils
	{
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
