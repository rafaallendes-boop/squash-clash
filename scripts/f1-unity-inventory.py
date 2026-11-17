#!/usr/bin/env python3
"""Inventario previo: no inicia Unity si ya hay un Editor en ejecución."""
import datetime,re,shlex,subprocess,sys
from pathlib import Path
rows=[]
for line in subprocess.check_output(['ps','-axo','pid=,command='],text=True).splitlines():
    m=re.match(r'\s*(\d+)\s+(.*)',line)
    if not m or not re.search(r'/Unity\.app/Contents/MacOS/Unity(?:\s|$)',m[2]):continue
    args=shlex.split(m[2]);project=args[args.index('-projectPath')+1] if '-projectPath' in args else '(sin -projectPath)'
    rows.append((m[1],m[2],project))
now=datetime.datetime.now(datetime.timezone.utc).isoformat()
output='utc\tpid\tcommand\tproject\n'+(''.join('\t'.join((now,*r))+'\n' for r in rows) if rows else now+'\tNONE\t\t\n')
p=Path(sys.argv[1]);assert not p.exists(), 'Do not overwrite inventory';p.parent.mkdir(parents=True,exist_ok=True);p.write_text(output)
print(output,end='')
if rows:sys.exit('STOP: ya hay un Editor Unity')
