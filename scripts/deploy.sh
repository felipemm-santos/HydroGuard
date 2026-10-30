#!/usr/bin/env bash
set -Eeuo pipefail
cd "$(dirname "$0")/.."
: "${DEPLOY_ENV:?Defina DEPLOY_ENV}"
: "${API_IMAGE:?Defina API_IMAGE com digest}"
if [[ "$DEPLOY_ENV" != staging && "$DEPLOY_ENV" != production ]]; then
  echo "DEPLOY_ENV deve ser staging ou production" >&2; exit 1
fi
if [[ ! "$API_IMAGE" =~ ^ghcr\.io/[a-z0-9_./-]+@sha256:[a-f0-9]{64}$ ]]; then
  echo "API_IMAGE deve usar digest imutável do GHCR" >&2; exit 1
fi
if [[ "$DEPLOY_ENV" == staging ]]; then
  export APP_ENVIRONMENT=Staging API_PORT="${API_PORT:-5001}"
else
  export APP_ENVIRONMENT=Production API_PORT="${API_PORT:-5000}"
fi
export BIND_ADDRESS="${BIND_ADDRESS:-127.0.0.1}" SEED_DEMO_DATA=false
dotnet run --project tests/HydroGuard.SmokeTests -c Release -- --validate-config
project="hydroguard-$DEPLOY_ENV"
mkdir -p evidence
# Senhas nunca são gravadas em arquivos no runner. Variáveis do job vencem .env.
# Sem --volumes: volumes persistentes são preservados entre releases.
docker compose -p "$project" config --quiet
docker compose -p "$project" pull
docker compose -p "$project" up -d --no-build --wait --wait-timeout 300
dotnet run --project tests/HydroGuard.SmokeTests -c Release --no-build -- "http://127.0.0.1:$API_PORT" \
  --expected-environment "$APP_ENVIRONMENT" --expected-revision "${APP_REVISION:?Defina APP_REVISION}" \
  --output "evidence/$DEPLOY_ENV-smoke.json"
docker compose -p "$project" ps > "evidence/$DEPLOY_ENV-compose.txt"
printf 'Environment: %s\nImage: %s\nRevision: %s\n' "$DEPLOY_ENV" "$API_IMAGE" "$APP_REVISION" > "evidence/$DEPLOY_ENV-release.txt"
