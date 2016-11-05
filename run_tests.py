import os
import subprocess

proc = subprocess.Popen(["c:\\Windows\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe", "/target:tests"], shell=True, stdout=subprocess.PIPE)
out = proc.stdout.read()
if "<<<" in out:
	out = out[out.index("<<<"):]
elif "ProcessModel: Default    DomainUsage: Multiple" in out:
	out = out[out.index("ProcessModel: Default    DomainUsage: Multiple"):]
out = out.replace('\r\n', '\n')
for line in out.split('\n'):
	if line.startswith('    '):
		line = line[4:]
	print line