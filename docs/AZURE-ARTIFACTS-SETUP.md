# Azure Artifacts Setup for Tool Publishing

This guide explains how to set up Azure Artifacts as your NuGet feed and configure GitHub Actions to publish tools.

## Step 1: Create Azure DevOps Organization & Project

1. Go to https://dev.azure.com
2. Create/select your organization
3. Create a new project (or use existing)
4. Note: `Organization name` and `Project name`

## Step 2: Create Feed in Azure Artifacts

1. In your project, go to **Artifacts**
2. Click **Create Feed**
3. Name it: `mcp-tools` (or your preferred name)
4. Visibility: **Organization scoped** (recommended)
5. Click **Create**

## Step 3: Get Feed Details

Once feed is created:

1. Click on the feed name (`mcp-tools`)
2. Click **Connect to feed** (top right)
3. Select **NuGet.exe** from dropdown
4. You'll see connection details like:

```
Organization: your-org
Project: your-project
Feed: mcp-tools
Feed URL: https://pkgs.dev.azure.com/your-org/_packaging/mcp-tools/nuget/v3/index.json
```

**Copy these values** - you'll need them next.

## Step 4: Create Personal Access Token (PAT)

1. Click your profile icon (top-right) → **Personal access tokens**
2. Click **New Token**
3. Fill in:
   - **Name**: GitHub Actions - MCPTools
   - **Organization**: Select your organization
   - **Expiration**: 1 year (or longer)
   - **Scopes**: 
     - ✓ Packaging (read & write)
4. Click **Create**
5. **Copy the token immediately** (you won't see it again!)

## Step 5: Configure GitHub Secrets

Go to your GitHub repo:

1. **Settings** → **Secrets and variables** → **Actions**
2. Create these secrets:

| Secret Name | Value |
|-------------|-------|
| `AZURE_ARTIFACTS_FEED_URL` | `https://pkgs.dev.azure.com/{org}/_packaging/{feed}/nuget/v3/index.json` |
| `AZURE_ARTIFACTS_USERNAME` | Your Azure email (e.g., `your-email@company.com`) |
| `AZURE_ARTIFACTS_PASSWORD` | Your PAT token (paste from Step 4) |
| `MCP_SERVER_WEBHOOK_URL` | (placeholder - fill later) |

**Example:**
```
AZURE_ARTIFACTS_FEED_URL = https://pkgs.dev.azure.com/myorg/_packaging/mcp-tools/nuget/v3/index.json
AZURE_ARTIFACTS_USERNAME = user@company.com
AZURE_ARTIFACTS_PASSWORD = abc123def456ghi789...
MCP_SERVER_WEBHOOK_URL = https://your-server.com/webhook/tools-published
```

## Step 6: Test Publishing

1. Go to your GitHub repo → **Actions**
2. Select **Publish Tools to NuGet Feed** workflow
3. Click **Run workflow**
4. Enter version: `1.0.0`
5. Click **Run workflow**

Wait for it to complete. You should see:
- ✓ All tools packed
- ✓ Published to Azure Artifacts
- ✓ GitHub Release created

## Step 7: Verify in Azure Artifacts

1. Go to your Azure Artifacts feed
2. You should see the published packages:
   - `MCPServer.ToolTemplate`
   - `MCPServer.Examples`
   - Any other tools

## How It Works

```
GitHub Actions Trigger (Manual):
  ✓ Find all tools in src/MCPServer.*
  ✓ Build and pack them
  ✓ Connect to Azure Artifacts
  ✓ Publish .nupkg files
  ✓ Create GitHub Release
  ✓ Call MCP Server webhook (if URL set)
```

## Using Published Tools

To use published tools in your MCP server:

```bash
# Add Azure Artifacts source
dotnet nuget add source \
  --name MCPTools \
  --username your-email@company.com \
  --password YOUR_PAT \
  https://pkgs.dev.azure.com/your-org/_packaging/mcp-tools/nuget/v3/index.json

# Install a tool
dotnet add package MCPServer.Examples

# Verify
dotnet list package
```

## Troubleshooting

**Authentication failed in GitHub Actions?**
- Verify secrets are set correctly in GitHub
- Check Azure email is correct
- Verify PAT token hasn't expired
- PAT scope must include "Packaging (read & write)"

**Package already exists error?**
- Publish workflow has `--skip-duplicate` flag
- Should skip existing versions automatically
- To force new version, increment version in .csproj files

**Can't find feed URL?**
- Go to Azure Artifacts → Your feed → **Connect to feed**
- Select **NuGet.exe** from the dropdown
- Copy the feed URL from there

**PAT expired?**
- Create new PAT in Azure DevOps
- Update `AZURE_ARTIFACTS_PASSWORD` secret in GitHub

## Next Steps

1. Once secrets are configured
2. Go to GitHub Actions → **Publish Tools to NuGet Feed**
3. Click **Run workflow** to publish your first set of tools
4. Check Azure Artifacts to verify packages appear

That's it! Your tools are now published and ready for your MCP server to consume.
