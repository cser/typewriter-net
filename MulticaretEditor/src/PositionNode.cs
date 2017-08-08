using System;

namespace MulticaretEditor
{
	public class PositionNode
	{
		public readonly string file;
        public int position;

        public PositionNode(string file, int position)
        {
	        this.file = file;
	        this.position = position;
        }
        
        public override string ToString()
        {
            return "(" + file + ":" + position + ")";
        }
	}
}
