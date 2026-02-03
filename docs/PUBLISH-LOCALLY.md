# Publishing Tools to Custom Feed

GitHub only validates and builds tools - **no credentials are stored there**. Publishing to your private NuGet feed is done locally.

## Workflow

```
1. PR submitted to GitHub
2. GitHub Actions builds and tests (PUBLIC)
3. Merge approved
4. Clone locally
5. Publish locally to your feed (with credentials)
```

## Local Publishing

### 1. Configure Your Feed

Add your custom feed locally:

```bash
dotnet nuget add source \
  --name MCPFeed \
  --username your-username \
  --password your-password \
  https://your-feed-url/v3/index.json
```

Or edit `~/.nuget/NuGet/NuGet.Config` directly (credentials never in git).

### 2. Build Packages

```bash
cd your-cloned-repo
dotnet build -c Release
dotnet pack -c Release --output ./packages
```

### 3. Publish to Feed

```bash
dotnet nuget push "./packages/*.nupkg" \
  --api-key YOUR_API_KEY \
  --source MCPFeed
```

Or publish individual package:

```bash
dotnet nuget push ./packages/MCPServer.YourTool.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source MCPFeed
```

### 4. Verify

```bash
# List packages in feed
dotnet package search MCPServer --source MCPFeed
```

## Azure Artifacts Publishing

```bash
# Get your feed URL (Azure DevOps → Artifacts)
dotnet nuget add source \
  --name AzureArtifacts \
  --username email@company.com \
  --password YOUR_PAT \
  https://pkgs.dev.azure.com/org/_packaging/feed-name/nuget/v3/index.json

# Build
dotnet pack -c Release --output ./packages

# Publish
dotnet nuget push "./packages/*.nupkg" \
  --api-key AzureDevOps \
  --source AzureArtifacts
```

## GitHub Packages Publishing

```bash
# Get your GitHub token (Settings → Developer settings → Personal access tokens)
dotnet nuget add source \
  --name GitHubPackages \
  --username your-username \
  --password YOUR_GITHUB_TOKEN \
  https://nuget.pkg.github.com/your-org/index.json

# Build
dotnet pack -c Release --output ./packages

# Publish
dotnet nuget push "./packages/*.nupkg" \
  --source GitHubPackages
```

## Automation Options

### Option 1: CI/CD Pipeline (Private)
Keep your publishing pipeline in a **private repository**:
- Clone public repo tools
- Build and test
- Publish to feed with credentials (stored in private CI/CD only)

### Option 2: Local Script
Create a local script with your credentials:

```bash
#!/bin/bash
# publish.sh

FEED_URL="https://your-feed/v3/index.json"
FEED_USER="your-user"
FEED_PASS="your-pass"
API_KEY="your-key"

dotnet nuget add source \
  --name AutoFeed \
  --username $FEED_USER \
  --password $FEED_PASS \
  $FEED_URL

dotnet build -c Release
dotnet pack -c Release --output ./packages
dotnet nuget push "./packages/*.nupkg" \
  --api-key $API_KEY \
  --source AutoFeed

echo "✓ Published to $FEED_URL"
```

**Add to `.gitignore`:**
```
publish.sh
nuget.config
*.nupkg
```

### Option 3: Manual Publishing
Simple and secure - just run the commands locally when ready.

## Best Practice: No Secrets in Public Repo

✓ GitHub repo has NO credentials
✓ Tool template and examples only
✓ Publishing happens in your infrastructure
✓ Credentials stay private

## Troubleshooting

**Package push failed?**
```bash
# Verify feed is accessible
curl https://your-feed/v3/index.json

# List configured sources
dotnet nuget list source

# Remove and re-add if needed
dotnet nuget remove source MCPFeed
dotnet nuget add source ...
```

**Authentication error?**
- Verify API key/token hasn't expired
- Check username and password
- Ensure feed credentials are correct

**Already published version?**
- Increment version in .csproj
- Use `--skip-duplicate` flag to ignore

## See Also

- [CUSTOM-FEED.md](CUSTOM-FEED.md) - Feed setup options
- [PRIVATE-SERVER-SETUP.md](../PRIVATE-SERVER-SETUP.md) - Server configuration
