import sys
import os
import subprocess

test_file = sys.argv[1] if len(sys.argv) > 1 else None
line_number = int(sys.argv[2]) if len(sys.argv) > 2 else None

if test_file != None:
	proc = subprocess.Popen([
		"c:\\Windows\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe",
		"/p:Configuration=Debug",
		"/target:build-tests",
		"/verbosity:m"], shell=True, stdout=subprocess.PIPE)
	out = proc.stdout.read()
	
	has_errors = False
	for line in out.split('\n'):
		if "): error " in line:
			has_errors = True
			print line
	if has_errors:
		exit(1)
	
	namespace = None
	classname = None
	testname = None
	if test_file != None:
		with open(test_file) as f:
			lines = f.readlines()
			for i, line in enumerate(lines):
				line = line.strip()
				if namespace == None and line.startswith("namespace "):
					namespace = line[len("namespace "):]
				elif classname == None and line.startswith("public class "):
					classname = line[len("public class "):]
					if " " in classname:
						classname = classname[:classname.index(" ")]
			pairs = [x for x in enumerate(lines)]
			pairs.reverse()
			for i, line in pairs:
				line = line.strip()
				if line.startswith("public void "):
					if line_number != None:
						if i <= line_number:
							testname = line[len("public void "):]
							if "(" in testname:
								testname = testname[:testname.index("(")].strip()
							break
	
	args = ["NUnit\\nunit-console-x86.exe", "MulticaretEditorTests\\Bin\\TypewriterNET.exe", "MulticaretEditorTests\\bin\\MulticaretEditorTests.dll"]
	if classname != None:
		name = ((namespace + "." if namespace != None else "") +
			(classname + "." if classname != None else "") +
			(testname if testname != None else ""))
		args.append("/run:" + name)
		if testname != None:
			print "Run test: " + name
	proc = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE)
	out = proc.stdout.read()
	
	if "<<<" in out:
		out = out[out.index("<<<"):]
	elif "ProcessModel: Default    DomainUsage: Multiple" in out:
		out = out[out.index("ProcessModel: Default    DomainUsage: Multiple"):]
	out = out.replace('\r\n', '\n')
	lines = []
	for line in out.split('\n'):
		if line.startswith('    '):
			line = line[4:]
		lines.append(line)
	print str.join("\n", lines).strip()
else:
	proc = subprocess.Popen([
		"c:\\Windows\\Microsoft.NET\\Framework\\v2.0.50727\\MSBuild.exe",
		"/target:run-tests",
		"/p:Configuration=Debug",
		"/verbosity:m"], shell=True, stdout=subprocess.PIPE)
	out = proc.stdout.read()
	
	if "<<<" in out:
		out = out[out.index("<<<"):]
	elif "ProcessModel: Default    DomainUsage: Multiple" in out:
		out = out[out.index("ProcessModel: Default    DomainUsage: Multiple"):]
	pattern = "c:\\workspace\\typewriter-net\\makefile.proj(24,3): error MSB3073"
	if pattern in out:
		out = out[:out.index(pattern)]
	pattern = "Tests Not Run:"
	if pattern in out:
		out = out[:out.index(pattern)]
	out = out.replace('\r\n', '\n')
	lines = []
	for line in out.split('\n'):
		if line.startswith('    '):
			line = line[4:]
		lines.append(line)
	print str.join("\n", lines).strip()
	