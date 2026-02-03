# Custom NuGet Feed Setup

This guide covers setting up a private NuGet feed for publishing and consuming MCP tools internally.

## Quick Overview

Instead of publishing to public NuGet.org:
- Tools are published to your **private feed** only
- Your infrastructure can access the feed via authentication
- Full control over versions, access, and distribution

## Feed Options

### 1. Azure Artifacts (Recommended for Azure users)

**Pros:**
- Integrated with Azure DevOps
- Simple feed management
- Role-based access control
- Retention policies

**Setup:**

1. Create an Azure DevOps project
2. Go to Artifacts → Create Feed
3. Name it (e.g., `mcp-tools`)
4. Get connection details:
   ```
   Organization: your-org
   Project: your-project
   Feed: mcp-tools
   ```

5. In GitHub, add secrets:
   - `NUGET_FEED_URL`: `https://pkgs.dev.azure.com/{org}/_packaging/mcp-tools/nuget/v3/index.json`
   - `NUGET_FEED_USERNAME`: Your email
   - `NUGET_FEED_PASSWORD`: Personal Access Token (PAT)

6. Create PAT in Azure DevOps (User Settings → Personal Access Tokens):
   - Scope: `Packaging (read & write)`
   - Expiration: 1 year+

### 2. GitHub Packages

**Pros:**
- No extra service needed
- Uses GitHub authentication
- Good for GitHub-centric teams

**Setup:**

1. In your repository, go to Settings → Secrets → Actions
2. Add `NUGET_FEED_URL`: `https://nuget.pkg.github.com/{owner}/index.json`
3. Add `NUGET_FEED_PASSWORD`: Your GitHub token with `write:packages` scope
4. Workflow automatically publishes to GitHub Packages

**Using in projects:**
```bash
dotnet nuget add source \
  --name GitHubPackages \
  --username your-username \
  --password your-github-token \
  https://nuget.pkg.github.com/your-org/index.json

dotnet add package MCPServer.YourTool --source GitHubPackages
```

### 3. On-Premises Server

**Options:**
- **ProGet** - Advanced NuGet server
- **Artifactory** - JFrog's universal repository
- **BaGet** - Open-source NuGet server
- **MyGet** - Hosted private feeds

**Generic Setup:**

```bash
# Add feed
dotnet nuget add source \
  --name PrivateFeed \
  --username your-user \
  --password your-pass \
  https://your-server.com/nuget/v3/index.json

# Verify
dotnet nuget list source

# Remove if needed
dotnet nuget remove source PrivateFeed
```

### 4. BaGet (Open Source Alternative)

**Free, self-hosted option:**

```bash
# Docker deployment
docker run \
  -p 5555:80 \
  -v /path/to/packages:/var/baget/data \
  loicsharma/baget:latest
```

Feed URL: `http://localhost:5555/v3/index.json`

## Workflow Integration

The `.github/workflows/build.yml` automatically:

1. **Builds** all projects
2. **Tests** all packages
3. **Packs** tools as NuGet packages
4. **Publishes** to your custom feed (on merge to main)

**Required Secrets** (GitHub Settings → Secrets):
- `NUGET_FEED_URL` - Your feed endpoint
- `NUGET_FEED_USERNAME` - Feed user (usually email)
- `NUGET_FEED_PASSWORD` - Feed password/token
- `NUGET_FEED_API_KEY` - API key (if required by your feed)

## Local Development

### Add Your Feed Locally

```bash
# Windows
dotnet nuget add source `
  --name MyCPFeed `
  --username your-username `
  --password your-password `
  https://your-feed-url/v3/index.json

# Linux/macOS
dotnet nuget add source \
  --name MCPFeed \
  --username your-username \
  --password your-password \
  https://your-feed-url/v3/index.json
```

### Install Packages

```bash
# List available packages from feed
dotnet package search MCPServer --source MCPFeed

# Add to project
dotnet add package MCPServer.TextProcessing --source MCPFeed

# Restore from feed
dotnet restore
```

### Push Local Packages

```bash
# Build package
dotnet pack -c Release

# Push to feed
dotnet nuget push "./bin/Release/*.nupkg" \
  --api-key YOUR_API_KEY \
  --source https://your-feed-url/v3/index.json
```

## NuGet.Config Management

For teams, store NuGet.Config in your repository:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="MCPFeed" value="https://your-feed-url/v3/index.json" />
    <add key="NuGet" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  
  <packageSourceCredentials>
    <MCPFeed>
      <add key="Username" value="team@company.com" />
      <add key="ClearTextPassword" value="your-password" />
    </MCPFeed>
  </packageSourceCredentials>
</configuration>
```

**Security Note:** Store sensitive credentials in GitHub Secrets or environment variables, not in the config file.

## Security Best Practices

✓ Use Personal Access Tokens (PATs) instead of passwords
✓ Rotate credentials regularly
✓ Restrict token scope to minimum required (e.g., `Packaging`)
✓ Store secrets in GitHub Secrets, not in code
✓ Use HTTPS for all feed URLs
✓ Consider IP whitelisting if possible
✓ Enable audit logging on your feed

## Troubleshooting

**Authentication failed?**
```bash
# Clear cached credentials
dotnet nuget remove source MCPFeed
# Re-add with correct credentials
dotnet nuget add source --name MCPFeed ...
```

**Package not found?**
```bash
# Verify feed is accessible
curl -v https://your-feed-url/v3/index.json

# List all configured sources
dotnet nuget list source
```

**404 on package?**
- Verify package exists in your feed
- Check you're using correct feed URL
- Ensure package version is published

**Permission denied?**
- Verify API key/token has `write:packages` scope
- Check feed permissions for your user
- Ensure token hasn't expired

## Migration from Public NuGet

If migrating from public NuGet:

1. Update build workflow secrets
2. Update client NuGet.config files
3. Tools automatically publish to custom feed on next merge
4. Clients pull from custom feed instead

No code changes needed!

## References

- [Azure Artifacts Docs](https://docs.microsoft.com/en-us/azure/devops/artifacts/)
- [GitHub Packages Docs](https://docs.github.com/en/packages)
- [BaGet Docs](https://loic-sharma.github.io/BaGet/)
- [NuGet.Config Reference](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file)
