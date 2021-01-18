function page()
{
  return {
    prs: [],
    metadata: {},
    currentSortBy: null,
    currentSortAsc: true,
    dataLoaded: false,

    async loadPrs() {
      const prsJson = await ky.get(`prs.json`).json()
      const metadataJson = await ky.get(`metadata.json`).json()
      this.prs = prsJson
      this.metadata = metadataJson

      this.sortPullRequests('changes')
      this.dataLoaded = true
    },

    sortPullRequests(sortBy)
    {
      if (this.currentSortBy === sortBy) {
        this.currentSortAsc = !this.currentSortAsc
      } else {
        this.currentSortBy = sortBy
        this.currentSortAsc = true
      }

      this.prs.sort((a, b) => this.pullRequestCompareFunction(a, b, this.currentSortBy, this.currentSortAsc))
    },

    /**
     * Sorting/Compare function for pull requests
     * @param { PullRequest } a First Element
     * @param { PullRequest } b Second Element
     * @param { string } by Property of PullRequest to sort by, e.g. Changes, Additions, Deletions, etc
     * @param { boolean } asc True if should be sorted ascending, False if descending
     */
    pullRequestCompareFunction(a, b, by, asc)
    {
      // ret 0 if eqaul
      if (a[by] === b[by]) {
        return 0
      }

      // if a should be placed first, return -1, otherwise 1
      let aFirst = asc ? a[by] < b[by] : a[by] > b[by]
      return aFirst ? -1 : 1
    },

    getAge(utcMs)
    {
      const diffSeconds = Math.floor((new Date().getTime() - utcMs) / 1000)
      const diffHours = diffSeconds/60/60

      if (diffSeconds < 3600) { // 1 Hour
        return `${Math.floor(diffSeconds/60)} Minute(s)`
      }
      else if (diffSeconds < 86400) { // 1 Day
        return `${Math.floor(diffHours)} Hour(s)`
      }
      else if (diffHours < 730) { // 1 Month
        return `${Math.floor(diffHours/24)} Days(s)`
      }
      else if (diffHours < 8760) { // 1 Year
        return `${Math.floor(diffHours/730)} Month(s)`
      }
      else {
        return `${Math.floor(diffHours/8760)} Year(s)`
      }
    },

    iconClasses(sortBy)
    {
      const self = this

      return {
        [':class']()
        {
          const currentlySorted = sortBy === self.currentSortBy
          return {
            // Sorted on this column
            'ri-arrow-up-line': currentlySorted && !self.currentSortAsc,
            'ri-arrow-down-line': currentlySorted && self.currentSortAsc,
            // Not sorted on this column
            'text-gray-400': !currentlySorted,
            'ri-arrow-up-down-line': !currentlySorted,
            'text-black': currentlySorted,
          }
        }
      }
    },

    titleClasses(isDraft, reviewDecision)
    {
      return {
        [':class']()
        {
          if (!isDraft && reviewDecision === "APPROVED") {
            return "text-green-600"
          }
          else if (!isDraft) {
            return "text-blue-600"
          }
          else {
            return "text-gray-600"
          }
        }
      }
    }
  }
}