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
	
data = {"name":"asljdflsjadfj", "childs":[]}
for i, line in enumerate(lines):
	if "class" in line:
		data["name"] = line.strip()
	data["childs"].append({"name":line});
print json.dumps(data)