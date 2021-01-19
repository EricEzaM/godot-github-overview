using GodotGithubOverview.GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GodotGithubOverview.DTOs
{
	public class ReactionGroupDTO
	{
		public string content { get; set; }
		public List<string> users { get; set; }
	}

	public class ReviewDTO
	{
		public string user { get; set; }
		public string state { get; set; }
		public long submittedAtUtc { get; set; }
	}

	public class PullRequestDTO
	{
		public int number { get; set; }
		public string title { get; set; }
		public int additions { get; set; }
		public int deletions { get; set; }
		public int changes { get; set; }
		public int changedFiles { get; set; }
		public string author { get; set; }
		public long createdAtUtc { get; set; }
		public long updatedAtUtc { get; set; }
		public bool isDraft { get; set; }
		public string mergeable { get; set; }
		public List<ReactionGroupDTO> reactions { get; set; }
		public int totalPositiveReactions { get; set; }
		public int totalNegativeReations { get; set; }
		public string url { get; set; }
		public List<ReviewDTO> reviews { get; set; }
		public string reviewDecision { get; set; }

		public PullRequestDTO(PullRequestNode fromNode)
		{
			number = fromNode.number;
            title = fromNode.title;
            additions = fromNode.additions;
            deletions = fromNode.deletions;
			changes = additions + deletions;
            changedFiles = fromNode.changedFiles;
			author = GetUserName(fromNode.author);
			createdAtUtc = fromNode.createdAt.ToUnixTimeMilliseconds();
            isDraft = fromNode.isDraft;
            mergeable = fromNode.mergeable;
            reactions = fromNode.reactionGroups.Select(rg => new ReactionGroupDTO
			{
				content = rg.content,
				users = rg.users.nodes.Select(n => GetUserName(n)).ToList()
			}).ToList();

			totalPositiveReactions = reactions
				.Where(rg => Enumerable.Contains(new string[] { "THUMBS_UP", "HOORAY", "HEART", "ROCKET" }, rg.content))
				.SelectMany(rg => rg.users) // Get a list of all users who voted for any of these
				.Distinct() // Remove duplicates, so each users vote only counts once, even if they reacted multiple times
				.Count(); // Get the number of users who voted for positive reactions

			totalNegativeReations = reactions
				.Where(rg => Enumerable.Contains(new string[] { "CONFUSED", "THUMBS_DOWN" }, rg.content))
				.SelectMany(rg => rg.users)
				.Distinct()
				.Count();

			updatedAtUtc = fromNode.updatedAt.ToUnixTimeMilliseconds();
            url = fromNode.url;
            reviews = fromNode.reviews.nodes.Select(n => new ReviewDTO
			{
				user = GetUserName(n.author),
				state = n.state,
				submittedAtUtc = n.submittedAt.ToUnixTimeMilliseconds()
			}).ToList();
            reviewDecision = fromNode.reviewDecision;
		}

		private string GetUserName(User user)
		{
			return user == null ? "<deleted account>" : user.login;
		}
	}
}
