"""Enable this repository's requested GitHub Actions Pages site.

Uses the configured Git credential in memory, only for api.github.com.
No tokens are accepted as arguments, written to disk, or printed.
"""
import json
import os
import subprocess
import urllib.error
import urllib.request

REPOSITORY = 'maabra/solitaire-tri-peaks-unlimited'
env = {**os.environ, 'GIT_TERMINAL_PROMPT': '0', 'GCM_INTERACTIVE': 'never'}
result = subprocess.run(['git','credential','fill'], input='protocol=https\nhost=github.com\n\n', text=True, capture_output=True, env=env)
credential = dict(line.split('=',1) for line in result.stdout.splitlines() if '=' in line)
if not credential.get('password'):
    raise SystemExit('No configured Git credential. Choose GitHub Actions in Settings > Pages.')
headers = {'Authorization':'Bearer '+credential['password'], 'Accept':'application/vnd.github+json', 'X-GitHub-Api-Version':'2022-11-28', 'User-Agent':'MeadowPeaks-Pages-setup'}
url = 'https://api.github.com/repos/'+REPOSITORY+'/pages'
def request(method, payload=None):
    data=json.dumps(payload).encode() if payload is not None else None
    req=urllib.request.Request(url, data=data, headers=headers, method=method)
    with urllib.request.urlopen(req, timeout=30) as response:
        content=response.read()
        return json.loads(content) if content else {}
try:
    try:
        current=request('GET')
    except urllib.error.HTTPError as error:
        if error.code!=404: raise
        current=request('POST', {'build_type':'workflow'})
    if current.get('build_type')!='workflow': request('PUT', {'build_type':'workflow'})
    current=request('GET')
    print(json.dumps({'url':current.get('html_url'), 'build_type':current.get('build_type'), 'status':current.get('status')}))
except urllib.error.HTTPError as error:
    raise SystemExit('GitHub Pages configuration failed (HTTP '+str(error.code)+'). Choose GitHub Actions in Settings > Pages.')
