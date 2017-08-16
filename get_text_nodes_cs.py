import sys
import json

lines = []
while True:
	try:
		line = sys.stdin.readline()
		lines.append(line)
	except KeyboardInterrupt:
		break
	if not line:
		break
	
data = None
for i, line in enumerate(lines):
	line = line.strip()
	if not data:
		if "class" in line:
			data = {"name":line, "line":i + 1, "childs":[]}
	else:
		if "private" in line or "public" in line:
			data["childs"].append({"name":line, "line":i + 1});
print json.dumps(data)