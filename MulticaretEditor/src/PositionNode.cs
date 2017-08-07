using System;

namespace MulticaretEditor
{
	public struct PositionNode
	{
		public string file;
        public int position;

        public PositionNode(string file, int position)
        {
	        this.file = file;
	        this.position = position;
        }
        
        public bool IsEmpty { get { return file == null; } }
        
        public bool Equals(PositionNode other)
        {
            return file == other.file && position == other.position;
        }
        
        public override bool Equals(object obj)
        {
            return (obj is PositionNode) && Equals((PositionNode)obj);
        }

        public override int GetHashCode()
        {
            return file.GetHashCode() ^ position.GetHashCode();
        }

        public static bool operator !=(PositionNode p1, PositionNode p2)
        {
            return !p1.Equals(p2);
        }

        public static bool operator ==(PositionNode p1, PositionNode p2)
        {
            return p1.Equals(p2);
        }

        public override string ToString()
        {
            return "(" + file + ":" + position + ")";
        }
	}
}
