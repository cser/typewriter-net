// from http://edapskov.narod.ru/raznoe/as3soveti100.htm
package {
	import flash.display.Sprite;
	
	public class ClassScope extends Sprite {
	
		public function ClassScope() {
			traceThis(); // "Class Instance"
			
			var obj:Object = new Object();
			obj.traceThis = traceThis;
			obj.traceThis(); // "Class Instance"
			
			traceThis.call(new Sprite()); // "Class Instance"
		}
		
		public override function toString():String {
			return "Class Instance";
		}
		
		public function traceThis():void {
			trace(this);
		}
	}
}