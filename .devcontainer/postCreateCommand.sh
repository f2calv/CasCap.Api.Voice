#!/usr/bin/env bash
# Installs native voice tooling, reports tool versions, and restores the standalone Release solution.

set -euo pipefail

if ! command -v ffmpeg >/dev/null 2>&1; then
  sudo apt-get update
  sudo apt-get install -y --no-install-recommends ffmpeg
  sudo rm -rf /var/lib/apt/lists/*
fi

dotnet --version
pre-commit --version
ffmpeg -version | head -n 1

if ! ffmpeg -hide_banner -encoders 2>/dev/null | grep -q libopus; then
  echo '::error::The installed ffmpeg build does not provide the libopus encoder.'
  exit 1
fi

dotnet restore ./CasCap.Api.Voice.Release.slnx -p:Configuration=Release
