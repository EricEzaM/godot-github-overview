# Godot Github Overview

For Github information for [Godot](https://github.com/godotengine/godot) in a convenient way 

- Pull Requests - The aim is to give an overview of PRs which can be quickly reviewed and merged in order to clear out the backlog of PRs faster.
- More features to come...

## How it works

- `build.sh` restores, builds and runs the .NET Core 3.1 application which uses the Github GraphQL API
  to download all Pull Requests and related information in the godot repository. The results
  are saved into a JSON file which is then copied to the directory of the static web page.
  The build script can exit with code 1 if the dotnet application fails. This is so that the github action step fails.
- Github Pages is used to serve the web static content.
- The website uses [Alpine.js](https://github.com/alpinejs/alpine) and
  [Ky](https://github.com/sindresorhus/ky) to fetch the JSON from GitHub Pages
  and display it dynamically. This requires JavaScript to be enabled on the
  client. (This stack was inspired by Calinou's [Godot Proposals Viewer](https://github.com/godot-proposals-viewer/godot-proposals-viewer.github.io) - thanks for introducing me to Alpine.js!)

Github Actions execute the build.sh and deploys the website to Github Pages every day.

## Development

- .NET Core 3.1 SDK Required for the data fetching application
- To run the .NET application, you must have a Github Personal Access Token since more than 60 Github REST API requests are made to fetch the information. You can put this directly in the code for development purposes (do NOT commit this information to the repo. If you do, revoke the token immediately), or use an Environment Variable, like the Github Actions do.
- For the website, start a local web server in the `./web-frontend` directory then browse to the local port where the server started; `localhost:port` (for example, use `python -m http.server` or the Live Server VS Code Extension)

## License

Copyright © 2021 Eric "EricEzaM" M

Unless otherwise specified, files in this repository are licensed under the
MIT license. See [LICENSE.md](LICENSE.md) for more information.