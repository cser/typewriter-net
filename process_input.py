import sys

while True:
	try:
		line = sys.stdin.readline()
	except KeyboardInterrupt:
		break
	if not line:
		break
	print ">> " + line.replace('\n', '').replace('\r', '')