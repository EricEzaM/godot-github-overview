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
  cp ./github-api-utility/prs.json ./web-frontend/prs.json
  cp ./github-api-utility/metadata.json ./web-frontend/metadata.json
  echo "Done."
else
  echo "Dotnet run failed."
  exit 1
fi