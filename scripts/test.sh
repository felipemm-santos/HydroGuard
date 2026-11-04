#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."
dotnet test tests/HydroGuard.Tests/HydroGuard.Tests.csproj -c Release --logger 'console;verbosity=normal' "$@"
