#!/usr/bin/env bash
set -euo pipefail

# register-microsoft-web.sh
# Registers the Microsoft.Web resource provider for a subscription.
# Usage:
#   SUBSCRIPTION_ID=xxx ./scripts/register-microsoft-web.sh
#   or
#   ./scripts/register-microsoft-web.sh --subscription xxx

usage() {
  cat <<EOF
Usage: $(basename "$0") [--subscription SUBSCRIPTION_ID]

You can also set SUBSCRIPTION_ID in the environment.
EOF
  exit 1
}

SUBSCRIPTION_ID=""

while [[ ${#} -gt 0 ]]; do
  case "$1" in
    -s|--subscription)
      SUBSCRIPTION_ID="$2"
      shift 2
      ;;
    -h|--help)
      usage
      ;;
    *)
      # positional subscription id
      if [[ -z "$SUBSCRIPTION_ID" ]]; then
        SUBSCRIPTION_ID="$1"
        shift
      else
        echo "Unknown argument: $1" >&2
        usage
      fi
      ;;
  esac
done

# allow environment override
: "${SUBSCRIPTION_ID:=${SUBSCRIPTION_ID:-}}"
SUBSCRIPTION_ID="${SUBSCRIPTION_ID:-}"

if [[ -z "$SUBSCRIPTION_ID" ]]; then
  echo "Error: SUBSCRIPTION_ID must be provided via --subscription or environment." >&2
  usage
fi

if ! command -v az >/dev/null 2>&1; then
  echo "az CLI not found. Install with: brew install azure-cli" >&2
  exit 1
fi

echo "Using subscription: $SUBSCRIPTION_ID"
echo "+ az account set --subscription \"$SUBSCRIPTION_ID\""
az account set --subscription "$SUBSCRIPTION_ID"

echo "Checking Microsoft.Web provider registration state..."
set +e
state=$(az provider show --namespace Microsoft.Web --subscription "$SUBSCRIPTION_ID" --query "registrationState" -o tsv 2>/dev/null)
rc=$?
set -e

if [[ $rc -ne 0 ]]; then
  echo "Failed to query provider state. You may not have permission to view providers for this subscription." >&2
  echo "Try running this from an account with Owner or Provider Registration permissions." >&2
  exit $rc
fi

echo "Current registrationState: ${state:-Unknown}"

if [[ "$state" == "Registered" ]]; then
  echo "Microsoft.Web is already registered for subscription $SUBSCRIPTION_ID"
  exit 0
fi

echo "Registering Microsoft.Web provider (this may take a few minutes)..."
az provider register --namespace Microsoft.Web --subscription "$SUBSCRIPTION_ID" --wait

echo "Verifying registration state..."
az provider show --namespace Microsoft.Web --subscription "$SUBSCRIPTION_ID" --query "registrationState" -o table

echo "Done. If registration failed due to permissions, ask a subscription admin to register the provider or use the Azure Portal: Subscriptions → Resource providers → Microsoft.Web → Register."
