using System;
using System.Collections.Generic;
using System.Text;

namespace GodotGithubOverview.GraphQL
{
	public class Comments 
	{
		public int totalCount { get; set; }
	}

	public class User
	{
		public string login { get; set; }
	}

	public class Users
	{
		public List<User> nodes { get; set; }
	}

	public class ReactionGroup
	{
		public Users users { get; set; }
		public string content { get; set; }
	}

	public class Review
	{
		public string state { get; set; }
		public DateTimeOffset submittedAt { get; set; }
		public User author { get; set; }
	}

	public class Reviews
	{
		public List<Review> nodes { get; set; }
	}

	public class PullRequestNode
	{
		public int number { get; set; }
		public string title { get; set; }
		public int additions { get; set; }
		public int deletions { get; set; }
		public int changedFiles { get; set; }
		public User author { get; set; }
		public DateTimeOffset createdAt { get; set; }
		public DateTimeOffset updatedAt { get; set; }
		public bool isDraft { get; set; }
		public string mergeable { get; set; }
		public List<ReactionGroup> reactionGroups { get; set; }
		public Comments comments { get; set; }
		public string url { get; set; }
		public Reviews reviews { get; set; }
		public string reviewDecision { get; set; }
	}

	public class PullRequestEdge
	{
		public PullRequestNode node { get; set; }
		public string cursor { get; set; }
	}

	public class PullRequestContainer
	{
		public List<PullRequestEdge> edges { get; set; }
	}

	public class Repository
	{
		public PullRequestContainer pullRequests { get; set; }
	}

	public class GraphQLData
	{
		public Repository repository { get; set; }
	}

	public class GraphQLRequestVariables
	{
		public int resultsPerPage { get; set; }
		public string cursor { get; set; }
	}
}
