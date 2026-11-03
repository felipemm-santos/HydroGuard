#!/usr/bin/env bash
# Executado pelo runner GitHub; os testes da aplicação continuam em C#.
set -Eeuo pipefail
cd "$(dirname "$0")/.."
umask 077
for key in SSH_HOST SSH_USER SSH_PRIVATE_KEY SSH_KNOWN_HOSTS DEPLOY_ENV API_IMAGE APP_REVISION MSSQL_SA_PASSWORD JWT_KEY ADMIN_PASSWORD REGISTRY_USER REGISTRY_TOKEN; do
  if [[ -z "${!key:-}" ]]; then echo "Configuração ausente: $key" >&2; exit 1; fi
done
[[ "$SSH_HOST" =~ ^[a-zA-Z0-9][a-zA-Z0-9.-]*$ ]] || { echo 'SSH_HOST deve ser IPv4 ou DNS, sem protocolo/porta' >&2; exit 1; }
[[ "$SSH_USER" =~ ^[a-z_][a-z0-9_-]*$ ]] || { echo 'SSH_USER inválido' >&2; exit 1; }
case "$DEPLOY_ENV" in staging|production) ;; *) exit 1 ;; esac
[[ "$APP_REVISION" =~ ^[a-f0-9]{40}$ ]] || { echo 'APP_REVISION inválida' >&2; exit 1; }
[[ "$API_IMAGE" =~ ^ghcr\.io/[a-z0-9_./-]+@sha256:[a-f0-9]{64}$ ]] || { echo 'Use imagem GHCR por digest' >&2; exit 1; }
run_id="${GITHUB_RUN_ID:-0}"
run_attempt="${GITHUB_RUN_ATTEMPT:-1}"
[[ "$run_id" =~ ^[0-9]+$ && "$run_attempt" =~ ^[0-9]+$ ]] || exit 1
export API_PORT="${API_PORT:-$([[ "$DEPLOY_ENV" == staging ]] && echo 5001 || echo 5000)}"
[[ "$API_PORT" =~ ^[0-9]+$ ]] && (( 10#$API_PORT >= 1 && 10#$API_PORT <= 65535 )) || { echo 'API_PORT inválida' >&2; exit 1; }
work="$(mktemp -d)"
trap 'rm -rf -- "$work"' EXIT
printf '%s\n' "$SSH_PRIVATE_KEY" | tr -d '\r' > "$work/key"
printf '%s\n' "$SSH_KNOWN_HOSTS" | tr -d '\r' > "$work/known_hosts"
chmod 600 "$work/key" "$work/known_hosts"
options=(-i "$work/key" -o "UserKnownHostsFile=$work/known_hosts" -o StrictHostKeyChecking=yes -o IdentitiesOnly=yes -o BatchMode=yes -o ConnectTimeout=20 -o ServerAliveInterval=15 -o ServerAliveCountMax=4)
# Nenhum valor de configuração externo é inserido diretamente no comando remoto.
# O caminho usa somente constantes e IDs validados acima; é relativo ao home da VM.
release="hydroguard/$DEPLOY_ENV/releases/$APP_REVISION-$run_id-$run_attempt"
target="$SSH_USER@$SSH_HOST"
mkdir -p "$work/payload/scripts" "$work/payload/tests"
cp docker-compose.yml Dockerfile "$work/payload/"
cp scripts/deploy.sh scripts/deploy-remote.sh "$work/payload/scripts/"
cp -R tests/HydroGuard.SmokeTests "$work/payload/tests/"
# Não transfere bin/obj que possam existir no checkout local.
rm -rf "$work/payload/tests/HydroGuard.SmokeTests/bin" "$work/payload/tests/HydroGuard.SmokeTests/obj"
for key in DEPLOY_ENV API_IMAGE APP_REVISION API_PORT MSSQL_SA_PASSWORD JWT_KEY ADMIN_PASSWORD REGISTRY_USER REGISTRY_TOKEN; do
  printf '%s=%q\n' "$key" "${!key}" >> "$work/payload/.deploy.env"
done
# Credenciais somente no pacote privado transitório, transmitido pelo canal SSH.
# O pacote e a chave nunca são enviados como artefatos do Actions.
tar -czf "$work/payload.tar.gz" -C "$work/payload" .
if ! ssh "${options[@]}" "$target" "umask 077; mkdir -p '$release' && tar -xzf - -C '$release'" < "$work/payload.tar.gz"; then
  # Se o transporte caiu após criar o arquivo, tenta remover o segredo parcial.
  ssh "${options[@]}" "$target" "rm -f '$release/.deploy.env'" || true
  exit 1
fi
result=0
ssh "${options[@]}" "$target" "cd '$release' && bash scripts/deploy-remote.sh" || result=$?
# O cliente ainda tenta remover o arquivo se houve falha antes de iniciar o script.
ssh "${options[@]}" "$target" "rm -f '$release/.deploy.env'" || true
if (( result == 0 )) && [[ -n "${GITHUB_STEP_SUMMARY:-}" ]]; then
  {
    printf '### Deploy Azure: %s\n\n' "$DEPLOY_ENV"
    printf -- '- Smoke HTTP: aprovado.\n- Imagem: `%s`\n- Commit: `%s`\n' "$API_IMAGE" "$APP_REVISION"
    printf -- '- Containers e volumes mantidos na VM.\n'
  } >> "$GITHUB_STEP_SUMMARY"
fi
exit "$result"
