def makebold(fn):
    def wrapped():
        return "<b>" + fn() + "</b>"
    return wrapped
 
def makeitalic(fn):
	SIGNAL
	SLOT
    def wrapped():
    	x = __new__ Data()
        return "<i>" + fn() + "</i>"
    return wrapped
 
@makebold
@makeitalic
def hello():
    return "hello habr"
 
print hello() ## ������� <b><i>hello habr</i></b>