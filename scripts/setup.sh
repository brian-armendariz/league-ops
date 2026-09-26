#!/usr/bin/env bash
set -euo pipefail

# Lightweight setup script for LeagueOps workspace
# Derived from setup.txt. This script creates solution, projects,
# and wires basic references. It is idempotent where practical.

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

echo "Creating solution and folders..."
dotnet new sln -n LeagueOps || true
mkdir -p src tests

# Create class libraries and test project if they don't exist
create_project() {
  local name=$1
  local path=$2
  if [ ! -d "$path" ]; then
    echo "Creating project $name at $path"
    mkdir -p "$path"
    case "$name" in
      LeagueOps.Tests)
        dotnet new xunit -n "$name" -o "$path"
        ;;
      *)
        dotnet new classlib -n "$name" -o "$path"
        ;;
    esac
  else
    echo "Project $name already exists at $path"
  fi
}

create_project LeagueOps.Core src/LeagueOps.Core
create_project LeagueOps.Yahoo src/LeagueOps.Yahoo
create_project LeagueOps.Discord src/LeagueOps.Discord
create_project LeagueOps.Tests tests/LeagueOps.Tests

# Initialize Azure Functions project if missing
if [ ! -d "src/LeagueOps.Functions" ]; then
  echo "Creating Functions project in src/LeagueOps.Functions"
  mkdir -p src/LeagueOps.Functions
  pushd src/LeagueOps.Functions >/dev/null
  func init --worker-runtime dotnet-isolated || true
  popd >/dev/null
else
  echo "Functions project already exists"
fi

SOLUTION_FILE="LeagueOps.slnx"
if [ ! -f "$SOLUTION_FILE" ]; then
  echo "Solution file not found, creating $SOLUTION_FILE"
  dotnet new sln -n LeagueOps
fi

# Add projects to solution (ignore errors if already added)
dotnet sln "$SOLUTION_FILE" add src/LeagueOps.Core/LeagueOps.Core.csproj || true
dotnet sln "$SOLUTION_FILE" add src/LeagueOps.Yahoo/LeagueOps.Yahoo.csproj || true
dotnet sln "$SOLUTION_FILE" add src/LeagueOps.Discord/LeagueOps.Discord.csproj || true
dotnet sln "$SOLUTION_FILE" add src/LeagueOps.Functions/LeagueOps.Functions.csproj || true
dotnet sln "$SOLUTION_FILE" add tests/LeagueOps.Tests/LeagueOps.Tests.csproj || true

# Wire project references
dotnet add src/LeagueOps.Functions reference src/LeagueOps.Core || true
dotnet add src/LeagueOps.Functions reference src/LeagueOps.Yahoo || true
dotnet add src/LeagueOps.Functions reference src/LeagueOps.Discord || true

dotnet add src/LeagueOps.Yahoo reference src/LeagueOps.Core || true
dotnet add src/LeagueOps.Discord reference src/LeagueOps.Core || true

dotnet add tests/LeagueOps.Tests reference src/LeagueOps.Core || true
dotnet add tests/LeagueOps.Tests reference src/LeagueOps.Yahoo || true
dotnet add tests/LeagueOps.Tests reference src/LeagueOps.Discord || true

cat > ARCHITECTURE.md <<'EOF'
# LeagueOps Architecture (auto-generated snippet)

Functions
   ↓
Core
 ↑  ↑
Yahoo Discord

This workspace contains:
- LeagueOps.Core: business logic, models, services
- LeagueOps.Yahoo: Yahoo API client + OAuth
- LeagueOps.Discord: Discord webhook client
- LeagueOps.Functions: Azure Functions (HTTP triggers, background jobs)
- tests: xUnit tests

Public callback URL example:
https://<your-app>.azurewebsites.net/api/yahoo/callback
EOF

echo "Restoring and building solution (this may take a while)..."
dotnet restore
dotnet build

echo "Setup complete."
