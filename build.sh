cd ./github-api-utility
echo "Restoring..."
dotnet restore
echo "Building..."
dotnet build -c Release
echo "Running..."
dotnet run -c Release
cd ../

echo "Copying output..."
cp ./github-api-utility/prs.json ./web-frontend/prs.json
cp ./github-api-utility/files.json ./web-frontend/files.json
cp ./github-api-utility/metadata.json ./web-frontend/metadata.json
echo "Done."