#!/usr/bin/env bash
# WorldFaith — Chuẩn bị client Unity (Shared Library + SignalR DLLs)
# Chạy từ thư mục gốc repo: ./scripts/setup-unity-client.sh

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="$ROOT/shared/WorldFaith.Shared"
DST="$ROOT/client-unity/Assets/WorldFaith/Shared"
SIGNALR_DIR="$ROOT/client-unity/Assets/Plugins/SignalR"
SIGNALR_VERSION="10.0.9"

echo "==> WorldFaith Unity client setup"
echo "    Root: $ROOT"

# ─── 1. Shared Library ───────────────────────────────────
echo "==> Copying Shared Library..."
mkdir -p "$DST"
rm -rf "$DST/Enums" "$DST/Models" "$DST/Contracts"
cp -r "$SRC/Enums"     "$DST/"
cp -r "$SRC/Models"    "$DST/"
cp -r "$SRC/Contracts" "$DST/"
echo "    OK: $DST"

# ─── 2. SignalR DLLs ─────────────────────────────────────
echo "==> Installing SignalR Client $SIGNALR_VERSION..."
mkdir -p "$SIGNALR_DIR"

TEMP="$(mktemp -d)"
trap 'rm -rf "$TEMP"' EXIT

dotnet new console -n sr -o "$TEMP/sr" --force >/dev/null
dotnet add "$TEMP/sr/sr.csproj" package Microsoft.AspNetCore.SignalR.Client --version "$SIGNALR_VERSION" >/dev/null
dotnet publish "$TEMP/sr/sr.csproj" -c Release -o "$TEMP/pub" >/dev/null

for dll in \
  Microsoft.AspNetCore.SignalR.Client.dll \
  Microsoft.AspNetCore.SignalR.Client.Core.dll \
  Microsoft.AspNetCore.SignalR.Common.dll \
  Microsoft.AspNetCore.SignalR.Protocols.Json.dll \
  Microsoft.AspNetCore.Http.Connections.Client.dll \
  Microsoft.AspNetCore.Http.Connections.Common.dll
do
  cp "$TEMP/pub/$dll" "$SIGNALR_DIR/"
done

echo "    OK: 6 DLLs in $SIGNALR_DIR"

echo ""
echo "Done. Mở client-unity/ trong Unity 6.3 LTS, rồi chạy:"
echo "  WorldFaith > Bootstrap > Full Project Setup"
