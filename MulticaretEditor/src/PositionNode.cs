using System;

namespace MulticaretEditor
{
	public class PositionNode
	{
		public readonly string file;
        public int position;

        public PositionNode(string file)
        {
	        this.file = file;
        }
        
        public override string ToString()
        {
            return "(" + file + ":" + position + ")";
        }
	}
}
