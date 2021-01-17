using Octokit;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GodotGithubOverview
{
	public class PullRequestFileDTO
	{
		public string Filename { get; set; }
		public int Additions { get; set; }
		public int Deletions { get; set; }

		public PullRequestFileDTO(PullRequestFile prf)
		{
			Filename = prf.FileName;
			Additions = prf.Additions;
			Deletions = prf.Deletions;
		}
	}
}
