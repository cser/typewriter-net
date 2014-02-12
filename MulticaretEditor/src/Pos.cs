using System;

namespace MulticaretEditor
{
	public struct Pos
	{
		public int ix;
        public int iy;

        public Pos(int ix, int iy)
        {
            this.ix = ix;
            this.iy = iy;
        }

        public bool Equals(Pos other)
        {
            return ix == other.ix && iy == other.iy;
        }

        public override bool Equals(object obj)
        {
            return (obj is Pos) && Equals((Pos)obj);
        }

        public override int GetHashCode()
        {
            return ix.GetHashCode() ^ iy.GetHashCode();
        }

        public static bool operator !=(Pos p1, Pos p2)
        {
            return !p1.Equals(p2);
        }

        public static bool operator ==(Pos p1, Pos p2)
        {
            return p1.Equals(p2);
        }

        public static bool operator <(Pos p1, Pos p2)
        {
            if (p1.iy < p2.iy) return true;
            if (p1.iy > p2.iy) return false;
            if (p1.ix < p2.ix) return true;
            return false;
        }

        public static bool operator <=(Pos p1, Pos p2)
        {
            if (p1.Equals(p2)) return true;
            if (p1.iy < p2.iy) return true;
            if (p1.iy > p2.iy) return false;
            if (p1.ix < p2.ix) return true;
            return false;
        }

        public static bool operator >(Pos p1, Pos p2)
        {
            if (p1.iy > p2.iy) return true;
            if (p1.iy < p2.iy) return false;
            if (p1.ix > p2.ix) return true;
            return false;
        }

        public static bool operator >=(Pos p1, Pos p2)
        {
            if (p1.Equals(p2)) return true;
            if (p1.iy > p2.iy) return true;
            if (p1.iy < p2.iy) return false;
            if (p1.ix > p2.ix) return true;
            return false;
        }

        public override string ToString()
        {
            return "(" + ix + "," + iy + ")";
        }
	}
}
