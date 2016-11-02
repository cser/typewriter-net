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
if not 'EXEC : 1) Test error : UnitTests.FailOnTextEditTest.MastNotFail' in out:
	print '!! STATUS CHANGED !!'
else:
	try:
		index = out.index('F....................')
		out = out[:index]
	except:
		pass
for line in out.split('\n'):
	if line.startswith('    '):
		line = line[4:]
	print line