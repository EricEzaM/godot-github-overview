using GodotGithubOverview.GraphQL;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GodotGithubOverview
{
	public class GraphQLDataFetcher
	{
        public int ResultsPerPage { get; set; } = 100;

		public async Task<IEnumerable<PullRequestDTO>> GetPullRequestData(string accessToken)
		{
            // Setup the HTTP client to always pass the authorization header, as per the API documentation.
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
            GraphQLHttpClientOptions graphql = new GraphQLHttpClientOptions()
            {
                EndPoint = new Uri("https://api.github.com/graphql"),
            };
			var client = new GraphQLHttpClient(graphql, new SystemTextJsonSerializer(), http);

            // Create the request
            var req = new GraphQLRequest
            {
				Query = @"
					query ($cursor: String) {
                      repository(owner: ""godotengine"", name: ""godot"") {
                        url
                        pullRequests(first: 100, after: $cursor, states: OPEN) {
                          edges {
                            node {
                              number
                              title
                              additions
                              deletions
                              changedFiles
                              author {
                                login
                              }
                              comments {
                                totalCount
                              }
                              createdAt
                              updatedAt
                              isDraft
                              mergeable
                              reactionGroups {
                                users(first: 100) {
                                  nodes {
                                    login
                                  }
                                }
                                content
                              }
                              url
                              reviewDecision
                              reviews(first: 100) {
                                nodes {
                                  author {
                                    login
                                  }
                                  state
                                  submittedAt
                                }
                              }
                            }
                            cursor
                          }
                        }
                      }
                    }"
            };

            // Send the request
            var nodes = new List<PullRequestNode>();
            await GetPullRequestData(client, req, nodes);

            // Map the results to the DTO for better usability on the client side.
            return nodes.Select(prn => new PullRequestDTO(prn));
		}

        private async Task<List<PullRequestNode>> GetPullRequestData(GraphQLHttpClient client, GraphQLRequest req, List<PullRequestNode> nodes, int failCount = 0)
		{
			try
			{
                Console.WriteLine($"Getting Pull Requests, got {nodes.Count} so far.");
                var res = await client.SendQueryAsync<GraphQLData>(req);

                if (res.Data.repository.pullRequests.edges.Count > 0)
                {
                    var nextCursor = res.Data.repository.pullRequests.edges.Last().cursor;
                    nodes.AddRange(res.Data.repository.pullRequests.edges.Select(e => e.node));
                    req.Variables = new
                    {
                        cursor = nextCursor
                    };

                    return await GetPullRequestData(client, req, nodes);
                }
                else
                {
                    Console.WriteLine("Thats all of them!");
                    return nodes;
                }
            }
			catch (GraphQLHttpRequestException e)
			{
				if (failCount < 10)
				{
                    failCount++;
                    Console.WriteLine($"Failed {failCount} times.");
                    Console.WriteLine("Getting Data Failed with exception: " + e.Message);
                    ResultsPerPage = Math.Max(ResultsPerPage - 10, 10);
                    Console.WriteLine($"Retrying in 2 seconds with {ResultsPerPage} results per page.");

                    Thread.Sleep(2000);
                    return await GetPullRequestData(client, req, nodes);
                }
				else
				{
                    Console.WriteLine("Failed 10 times, aborting.");
                    throw e;
				}
			}
        }
	}
}
