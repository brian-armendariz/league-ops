#!/usr/bin/env bash

set -euo pipefail

# -------------------------------------------------------
# LeagueOps - Azure Infrastructure
# -------------------------------------------------------

RESOURCE_GROUP="leagueops-rg"
LOCATION="westus2"

# These must be globally unique.
# Change the suffix if Azure says either name is unavailable.
STORAGE_ACCOUNT="leagueopsstorageba"
FUNCTION_APP="leagueops-brian-001"

RUNTIME="dotnet-isolated"
RUNTIME_VERSION="10.0"


echo "🏈 Creating LeagueOps Azure infrastructure..."
echo ""
echo "Resource Group:  $RESOURCE_GROUP"
echo "Location:        $LOCATION"
echo "Storage Account: $STORAGE_ACCOUNT"
echo "Function App:    $FUNCTION_APP"
echo ""


# -------------------------------------------------------
# Verify Azure CLI
# -------------------------------------------------------

if ! command -v az >/dev/null 2>&1; then
    echo "❌ Azure CLI is not installed."
    echo "Install it with: brew install azure-cli"
    exit 1
fi


# -------------------------------------------------------
# Verify Azure login
# -------------------------------------------------------

echo "🔐 Checking Azure login..."

if ! az account show >/dev/null 2>&1; then
    echo "Not logged into Azure. Opening login..."
    az login
fi

ACCOUNT_NAME=$(az account show --query name -o tsv)

echo "✅ Using Azure subscription: $ACCOUNT_NAME"


# -------------------------------------------------------
# Resource Group
# -------------------------------------------------------

echo ""
echo "📦 Creating resource group..."

az group create \
    --name "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --output none

echo "✅ Resource group ready."


# -------------------------------------------------------
# Storage Account
# -------------------------------------------------------

echo ""
echo "💾 Creating storage account..."

if az storage account show \
    --name "$STORAGE_ACCOUNT" \
    --resource-group "$RESOURCE_GROUP" \
    >/dev/null 2>&1; then

    echo "✅ Storage account already exists."

else

    az storage account create \
        --name "$STORAGE_ACCOUNT" \
        --resource-group "$RESOURCE_GROUP" \
        --location "$LOCATION" \
        --sku Standard_LRS \
        --output none

    echo "✅ Storage account created."

fi


# -------------------------------------------------------
# Azure Function App
# -------------------------------------------------------

echo ""
echo "⚡ Creating Azure Function App..."

if az functionapp show \
    --name "$FUNCTION_APP" \
    --resource-group "$RESOURCE_GROUP" \
    >/dev/null 2>&1; then

    echo "✅ Function App already exists."

else

    az functionapp create \
        --resource-group "$RESOURCE_GROUP" \
        --name "$FUNCTION_APP" \
        --storage-account "$STORAGE_ACCOUNT" \
        --flexconsumption-location "$LOCATION" \
        --runtime "$RUNTIME" \
        --runtime-version "$RUNTIME_VERSION" \
        --https-only true \
        --output none

    echo "✅ Function App created."

fi


# -------------------------------------------------------
# Output
# -------------------------------------------------------

FUNCTION_HOST="https://${FUNCTION_APP}.azurewebsites.net"
YAHOO_REDIRECT="${FUNCTION_HOST}/api/yahoo/callback"

echo ""
echo "🎉 LeagueOps infrastructure is ready!"
echo ""
echo "Function App:"
echo "  $FUNCTION_HOST"
echo ""
echo "Yahoo Redirect URI:"
echo "  $YAHOO_REDIRECT"
echo ""
echo "Next:"
echo "  1. Add the Yahoo redirect URI above to your Yahoo Developer app."
echo "  2. Configure Yahoo secrets in Azure."
echo "  3. Deploy LeagueOps.Functions."
echo ""