"""Gera .env local sem sobrescrever configuração existente."""
from pathlib import Path
import secrets

def main():
    target = Path(__file__).resolve().parents[1] / ".env"
    # Modo x evita sobrescrita inclusive se outro processo criar o arquivo.
    with target.open("x", encoding="utf-8") as file:
        target.chmod(0o600)
        file.write("MSSQL_SA_PASSWORD=Hg!" + secrets.token_hex(20) + "\n")
        file.write("JWT_KEY=" + secrets.token_hex(32) + "\n")
        file.write("ADMIN_PASSWORD=Hg!" + secrets.token_hex(20) + "\n")
        file.write("APP_ENVIRONMENT=Development\nAPI_PORT=5000\nBIND_ADDRESS=127.0.0.1\nSEED_DEMO_DATA=true\nAPI_IMAGE=hydroguard:local\nAPP_REVISION=local\n")
    print(".env criado. Consulte ADMIN_PASSWORD localmente para entrar como admin.")

if __name__ == "__main__":
    main()
