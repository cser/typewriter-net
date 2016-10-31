import os
import subprocess

proc = subprocess.Popen(["c:\\Windows\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe", "/target:tests"], shell=True, stdout=subprocess.PIPE)
out = proc.stdout.read()
out = out.replace('\r\n', '\n')
index = -1
try:
	index = out.index('<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<');
except:
	pass;
if index != -1:
	out = out[index:]
print out