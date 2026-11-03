#!/usr/bin/env bash
set -euo pipefail

PREFIX=$(realpath "$(dirname "$0")")
cd "$PREFIX"

python3 - <<'PY'
import urllib.request
url = "https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.416/dotnet-sdk-8.0.416-linux-x64.tar.gz"
urllib.request.urlretrieve(url, "/tmp/dotnet-sdk-8.tar.gz")
PY

tar -xzf /tmp/dotnet-sdk-8.tar.gz -C "$PREFIX"
rm -f /tmp/dotnet-sdk-8.tar.gz
chmod +x "$PREFIX/dotnet"

export DOTNET_CLI_HOME="$PREFIX"
export HOME="$PREFIX"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

"$PREFIX/dotnet" --version
