#!/usr/bin/env bash
set -euo pipefail

PREFIX=$(realpath "$(dirname "$0")")
cd "$PREFIX"

python3 - <<'PY'
import urllib.request
url = "https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.401/dotnet-sdk-10.0.401-linux-x64.tar.gz"
urllib.request.urlretrieve(url, "/tmp/dotnet-sdk.tar.gz")
PY

tar -xzf /tmp/dotnet-sdk.tar.gz -C "$PREFIX"
rm -f /tmp/dotnet-sdk.tar.gz
chmod +x "$PREFIX/dotnet"

export DOTNET_CLI_HOME="$PREFIX"
export HOME="$PREFIX"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_GENERATE_ASPNET_CERTIFICATE=false

"$PREFIX/dotnet" new console -o cache_application --force
"$PREFIX/dotnet" new console -lang F# -o fs_cache_application --force
"$PREFIX/dotnet" new console -lang VB -o vb_cache_application --force

rm -rf cache_application fs_cache_application vb_cache_application
