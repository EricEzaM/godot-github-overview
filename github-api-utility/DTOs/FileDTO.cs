using System.Collections.Generic;

namespace GodotGithubOverview
{
	public class FileDTO
	{
		public string Filename { get; set; }
		public int Additions { get; set; }
		public int Deletions { get; set; }
		public IEnumerable<int> PullRequestNumbers { get; set; }
	}
}
