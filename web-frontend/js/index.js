function table() {
  return {
    prs: [],
    prsLoaded: false,

    async loadPrs() {
      const json = await ky.get(`prs.json`).json();
      this.prs = json

      console.log(this.prs)

      this.prsLoaded = true;
    }
  }
}