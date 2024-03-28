function page() {
	return {
		allPrs: [],
		prs: [],
		numPrsToRender: 100,
		prHistorical: [],
		metadata: {},
		labels: [],
		currentSortBy: null,
		currentSortAsc: true,
		dataLoaded: false,
		currentPage: 0,

		async loadPrs() {
			const prsJson = await ky.get(`data/prs.json`).json();
			const prHistorical = await ky.get("data/pr_historical.json").json();
			const metadataJson = await ky.get(`data/metadata.json`).json();
			this.allPrs = prsJson;
			this.prs = prsJson;
			this.prHistorical = prHistorical;
			this.metadata = metadataJson;

			this.sortPullRequests("changes");
			this.getLabelsFromPullRequests();
			this.dataLoaded = true;
		},

		sortPullRequests(sortBy) {
			if (this.currentSortBy === sortBy) {
				this.currentSortAsc = !this.currentSortAsc;
			} else {
				this.currentSortBy = sortBy;
				this.currentSortAsc = true;
			}

			this.prs.sort((a, b) =>
				this.pullRequestCompareFunction(
					a,
					b,
					this.currentSortBy,
					this.currentSortAsc
				)
			);
		},

		getLabelsFromPullRequests() {
			let localLabels = this.allPrs.reduce((acc, curr, idx, arr) => {
				// Loop through each label in the current pull request
				curr.labels.forEach((label) => {
					// If it does not exist in the accumulator, add it.
					let exists = acc.filter((l) => {
						return l.name == label.name;
					});

					if (exists.length == 0) {
						label.prefix = toTitleCase(
							label.name.includes(":")
								? label.name.substring(0, label.name.indexOf(":"))
								: ""
						);
						label.suffix = toTitleCase(
							label.name.includes(":")
								? label.name.substring(label.name.indexOf(":") + 1)
								: label.name
						);
						acc.push(label);
					}
				});

				return acc;
			}, []);

			localLabels.sort(
				(a, b) =>
					a.prefix.localeCompare(b.prefix) || a.suffix.localeCompare(b.suffix)
			);

			this.labels = localLabels;
		},

		onLabelFilterToggled(labelName) {
			this.prs = this.allPrs.filter(
				(pr) =>
					pr.labels.map((l) => l.name).includes(labelName) || labelName === ""
			);
		},

		/**
		 * Sorting/Compare function for pull requests
		 * @param { PullRequest } a First Element
		 * @param { PullRequest } b Second Element
		 * @param { string } by Property of PullRequest to sort by, e.g. Changes, Additions, Deletions, etc
		 * @param { boolean } asc True if should be sorted ascending, False if descending
		 */
		pullRequestCompareFunction(a, b, by, asc) {
			// ret 0 if eqaul
			if (a[by] === b[by]) {
				return 0;
			}

			// if a should be placed first, return -1, otherwise 1
			let aFirst = asc ? a[by] < b[by] : a[by] > b[by];
			return aFirst ? -1 : 1;
		},

		getAge(utcMs) {
			const diffSeconds = Math.floor((new Date().getTime() - utcMs) / 1000);
			const diffHours = diffSeconds / 60 / 60;

			if (diffSeconds < 3600) {
				// 1 Hour
				return `${Math.floor(diffSeconds / 60)} Minute(s)`;
			} else if (diffSeconds < 86400) {
				// 1 Day
				return `${Math.floor(diffHours)} Hour(s)`;
			} else if (diffHours < 730) {
				// 1 Month
				return `${Math.floor(diffHours / 24)} Days(s)`;
			} else if (diffHours < 8760) {
				// 1 Year
				return `${Math.floor(diffHours / 730)} Month(s)`;
			} else {
				return `${Math.floor(diffHours / 8760)} Year(s)`;
			}
		},

		sortingIconClasses(sortBy) {
			const self = this;

			return {
				[":class"]() {
					const currentlySorted = sortBy === self.currentSortBy;
					return {
						// Sorted on this column
						"ri-arrow-up-line": currentlySorted && !self.currentSortAsc,
						"ri-arrow-down-line": currentlySorted && self.currentSortAsc,
						// Not sorted on this column
						"text-gray-400": !currentlySorted,
						"ri-arrow-up-down-line": !currentlySorted,
						"text-black": currentlySorted,
					};
				},
			};
		},

		titleClasses(isDraft, reviewDecision) {
			return {
				[":class"]() {
					if (!isDraft && reviewDecision === "APPROVED") {
						return "text-green-600";
					} else if (!isDraft) {
						return "text-blue-600";
					} else {
						return "text-gray-600";
					}
				},
			};
		},

		generateBurndownPage() {
			setTimeout(() => {
				this.buildChart();
			}, 100);
		},

		buildChart() {
			let ctx = document.getElementById("chart").getContext("2d");

			let labels = this.prHistorical.map((prh) => prh.date);
			let data = this.prHistorical.map((prh) => prh.count);

			let chart = new Chart(ctx, {
				type: "line",
				data: {
					labels: labels,
					datasets: [
						{
							label: "Open Pull Requests",
							data: data,
							elements: {
								point: {
									radius: 2,
									backgroundColor: "#000000",
								},
								line: {
									fill: true,
									borderWidth: 2,
									backgroundColor: "#99c0f07d",
									borderColor: "#1f1f1f",
								},
							},
						},
					],
				},
				options: {
					plugins: {
						title: {
							text: "Open Pull Requests Over Time",
							display: true,
						},
						annotation: {
							annotations: GodotReleaseAnnotations,
						},
					},
					scales: {
						x: {
							type: "time",
							time: {
								// Luxon format string
								tooltipFormat: "DD",
								displayFormats: {
									day: "DD",
									week: "DD",
									month: "MMM yyyy",
									quarter: "MMM yyyy",
									year: "yyyy",
								},
							},
							title: {
								display: true,
								text: "Date",
							},
						},
						y: {
							title: {
								display: true,
								text: "Open Pull Requests",
							},
						},
					},
				},
			});
		},
	};
}

function toTitleCase(input) {
	return input
		.toLowerCase()
		.split(" ")
		.map((word) => word.charAt(0).toUpperCase() + word.slice(1))
		.join(" ");
}

const annotationBase = {
	type: "line",
	borderWidth: 2,
	borderColor: "#fcba037d",
};

const labelBase = {
	enabled: true,
	color: "#000",
	position: "start",
	backgroundColor: "#fcba037d",
};

const GodotReleaseAnnotations = {
	gd1_0: {
		...annotationBase,
		xMin: 1417356000000,
		xMax: 1417356000000,
		label: {
			...labelBase,
			content: "v1.0",
		},
	},
	gd1_1: {
		...annotationBase,
		xMin: 1430402400000,
		xMax: 1430402400000,
		label: {
			...labelBase,
			content: "v1.1",
		},
	},
	gd2_0: {
		...annotationBase,
		xMin: 1454248800000,
		xMax: 1454248800000,
		label: {
			...labelBase,
			content: "v2.0",
		},
	},
	gd2_1: {
		...annotationBase,
		xMin: 1467295200000,
		xMax: 1467295200000,
		label: {
			...labelBase,
			content: "v2.1",
		},
	},
	gd3_0: {
		...annotationBase,
		xMin: 1514728800000,
		xMax: 1514728800000,
		label: {
			...labelBase,
			content: "v3.0",
		},
	},
	gd3_1: {
		...annotationBase,
		xMin: 1551362400000,
		xMax: 1551362400000,
		label: {
			...labelBase,
			content: "v3.1",
		},
	},
	gd3_2: {
		...annotationBase,
		xMin: 1577800800000,
		xMax: 1577800800000,
		label: {
			...labelBase,
			content: "v3.2",
		},
	},
	gd3_3: {
		...annotationBase,
		xMin: 1617199200000,
		xMax: 1617199200000,
		label: {
			...labelBase,
			content: "v3.3",
		},
	},
};
