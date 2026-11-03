#!/usr/bin/env bash
# Executado na VM a partir de um diretório privado e exclusivo da execução.
set -Eeuo pipefail
cd "$(dirname "$0")/.."
umask 077
registry_config=""
finish() {
  result=$?
  trap - EXIT
  set +e
  # Retira as credenciais transitórias mesmo se o deploy falhar.
  rm -f .deploy.env
  if [[ -n "$registry_config" ]]; then rm -rf -- "$registry_config"; fi
  unset REGISTRY_TOKEN
  exit "$result"
}
# O arquivo é produzido pelo Bash local com printf %q, nunca por texto externo.
set -a
source .deploy.env
set +a
trap finish EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
rm -f .deploy.env
case "$DEPLOY_ENV" in
  staging) export APP_ENVIRONMENT=Staging ;;
  production) export APP_ENVIRONMENT=Production ;;
  *) exit 1 ;;
esac
export BIND_ADDRESS=127.0.0.1 SEED_DEMO_DATA=false
# Serializa deploys na VM, inclusive se uma conexão anterior foi interrompida.
exec 9> "$HOME/hydroguard/.deploy.lock"
flock -w 600 9
docker version > /dev/null
docker compose version
dotnet --list-sdks
registry_config="$(mktemp -d)"
export DOCKER_CONFIG="$registry_config"
printf '%s' "$REGISTRY_TOKEN" | docker login ghcr.io --username "$REGISTRY_USER" --password-stdin
unset REGISTRY_TOKEN
bash scripts/deploy.sh
# Não há down, remoção de volumes ou limpeza de imagens da aplicação na VM.
