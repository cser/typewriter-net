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
	
data = {"name":"asljdflsjadfj"}
print json.dumps(data)