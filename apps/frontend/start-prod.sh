#!/usr/bin/env bash
export PATH="$HOME/.nvm/versions/node/v22.23.2/bin:$PATH"
cd "$(dirname "$0")"
exec npm run start
