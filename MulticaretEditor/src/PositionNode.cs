using System;

namespace MulticaretEditor
{
	public class PositionNode
	{
		public readonly PositionFile file;
        public int position;

        public PositionNode(PositionFile file, int position)
        {
	        this.file = file;
	        this.position = position;
        }
        
        public override string ToString()
        {
            return "(" + file.path + ":" + position + ")";
        }
	}
}
