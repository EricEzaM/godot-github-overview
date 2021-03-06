﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GodotGithubOverview.GraphQL
{
	public class GraphQLQueries
	{
        public const string GetPullRequests =
            @"
			query ($cursor: String, $resultsPerPage: Int) {
                repository(owner: ""godotengine"", name: ""godot"") {
                    url
                    pullRequests(first: $resultsPerPage, after: $cursor, states: OPEN) {
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
                                    content
                                    users(first: 100) {
                                        nodes {
                                            login
                                        }
                                    }
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
            }";
    }
}
