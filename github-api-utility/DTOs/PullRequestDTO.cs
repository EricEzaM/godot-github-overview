using Octokit;
using System.Collections.Generic;
using System.Linq;

namespace GodotGithubOverview
{
	public class LabelDTO
	{
		public string Name { get; set; }
		public string Color { get; set; }
	}

	public class PullRequestDTO
	{
		public int Number { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public string Username { get; set; }
		public IEnumerable<LabelDTO> Labels { get; set; }
		public int Additions { get; set; }
		public int Deletions { get; set; }
		public int Changes { get; set; }
		public int ChangedFiles { get; set; }
		public bool Draft { get; set; }
		public bool? Mergable { get; set; }
		public long CreatedAtUtc { get; set; }
		public long UpdatedAtUtc { get; set; }
		public IEnumerable<PullRequestFileDTO> Files { get; set; }

		public PullRequestDTO(PullRequest pr)
		{
			Number = pr.Number;
			Title = pr.Title;
			Url = pr.HtmlUrl;
			Username = pr.User.Login;
			Labels = pr.Labels.Select(l => new LabelDTO { Name = l.Name, Color = l.Color }).ToList();
			Additions = pr.Additions;
			Deletions = pr.Deletions;
			ChangedFiles = pr.ChangedFiles;
			Draft = pr.Draft;
			Mergable = pr.Mergeable;
			CreatedAtUtc = pr.CreatedAt.ToUnixTimeMilliseconds();
			UpdatedAtUtc = pr.UpdatedAt.ToUnixTimeMilliseconds();
		}
	}
}
