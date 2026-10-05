#!/usr/bin/env bash
# Publish this folder to no-stick.uk on TinyHost. Needs your API key in $TW_KEY.
# The previous version goes to TinyHost's trash, so an upload can be rolled back.
set -euo pipefail
cd "$(dirname "$0")"
: "${TW_KEY:?Set TW_KEY to your TinyHost API key first}"
API=https://tiny-web.uk/api
Z=$(mktemp --suffix=.zip); trap 'rm -f "$Z" /tmp/tw-status.$$' EXIT
rm -f "$Z"; zip -qj "$Z" index.html ./*.png ./*.ico
curl -fsS "$API/host-status.php" -H "Authorization: Bearer $TW_KEY" -o /tmp/tw-status.$$
SLUG=$(python3 -c "
import json, sys
d = json.load(open(sys.argv[1]))
hits = [s for s in (d.get('sites') or []) if isinstance(s, dict) and 'no-stick.uk' in json.dumps(s)]
print((hits[0].get('slug') or hits[0].get('site') or hits[0].get('name') or '') if len(hits) == 1 else '')
" /tmp/tw-status.$$)
[ -n "$SLUG" ] || { echo "Couldn't find exactly one site with the no-stick.uk domain on this account." >&2; exit 1; }
echo "Uploading to site '$SLUG'..."
curl -fsS -X POST "$API/host-upload.php" -H "Authorization: Bearer $TW_KEY" -F "site=$SLUG" -F "zip=@$Z"
echo
