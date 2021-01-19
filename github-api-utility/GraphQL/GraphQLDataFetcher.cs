using GodotGithubOverview.DTOs;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GodotGithubOverview.GraphQL
{
	public class GraphQLDataFetcher
	{
        private readonly GraphQLHttpClient _client;
		public int ResultsPerPage { get; set; } = 100;

		public GraphQLDataFetcher(string accessToken)
		{
            // Setup the HTTP client to always pass the authorization header, as per the API documentation.
            HttpClient http = new HttpClient();
            http.DefaultRequestHeaders.Add("Authorization", $"bearer {accessToken}");
            GraphQLHttpClientOptions graphql = new GraphQLHttpClientOptions()
            {
                EndPoint = new Uri("https://api.github.com/graphql"),
            };
            _client = new GraphQLHttpClient(graphql, new SystemTextJsonSerializer(), http);
        }

		public async Task<IEnumerable<PullRequestDTO>> GetPullRequestData()
		{
            // Create the request
            var req = new GraphQLRequest
            {
                Query = GraphQLQueries.GetPullRequests,
                Variables = new GraphQLRequestVariables
                {
                    resultsPerPage = ResultsPerPage
                }
            };

            // Send the request
            var nodes = new List<PullRequestNode>();
            await GetPullRequestData(req, nodes);

            // Map the results to the DTO for better usability on the client side.
            return nodes.Select(prn => new PullRequestDTO(prn));
		}

        /// <summary>
        /// Method called recursively to get all PullRequests from all pages of a request.
        /// </summary>
        /// <param name="req">Request to be executed.</param>
        /// <param name="nodes">The nodes list which will be added to with the results.</param>
        /// <param name="failCount">The number of times the request has failed.</param>
        /// <returns>The result of the query.</returns>
        private async Task GetPullRequestData(GraphQLRequest req, List<PullRequestNode> nodes, int failCount = 0)
		{
			try
			{
                Console.WriteLine($"Getting Pull Requests, got {nodes.Count} so far.");
                var res = await _client.SendQueryAsync<GraphQLData>(req);

                if (res.Data.repository.pullRequests.edges.Count > 0)
                {
                    var nextCursor = res.Data.repository.pullRequests.edges.Last().cursor;
                    nodes.AddRange(res.Data.repository.pullRequests.edges.Select(e => e.node));
                    req.Variables = new GraphQLRequestVariables
                    {
                        resultsPerPage = ResultsPerPage,
                        cursor = nextCursor
                    };

                    await GetPullRequestData(req, nodes);
                }
                else
                {
                    Console.WriteLine("Thats all of them!");
                    return;
                }
            }
			catch (GraphQLHttpRequestException e)
			{
                failCount++;
                Console.WriteLine("Getting Data Failed with exception: " + e.Message);

				if (failCount < 100)
				{
                    ResultsPerPage = Math.Max(ResultsPerPage - 10, 10);
                    ((GraphQLRequestVariables)req.Variables).resultsPerPage = ResultsPerPage;
                    Console.WriteLine($"Failed {failCount} times. Retrying with {ResultsPerPage} results per page in 5 seconds.");

                    Thread.Sleep(5000);
                    await GetPullRequestData(req, nodes, failCount);
                }
				else
				{
                    Console.WriteLine("Failed 100 times, aborting.");
                    nodes.Clear();
                    return;
				}
			}
        }
	}
}
