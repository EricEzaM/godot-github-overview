cd ./github-api-utility
echo "Restoring..."
dotnet restore
echo "Building..."
dotnet build -c Release
echo "Running..."

if dotnet run -c Release
then
  cd ../

  echo "Copying output..."
  mkdir -p ./web-frontend/data/
  cp ./github-api-utility/prs.json ./web-frontend/data/prs.json
  cp ./github-api-utility/pr_historical.json ./web-frontend/data/pr_historical.json
  cp ./github-api-utility/metadata.json ./web-frontend/data/metadata.json
  echo "Done."
else
  echo "Dotnet run failed."
  exit 1
fi