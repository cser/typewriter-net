using System;

namespace MulticaretEditor
{
    public struct Char
    {
        public char c;
        public short style;

        public Char(char c)
        {
            this.c = c;
            style = 0;
        }
        
        public Char(char c, short style)
        {
            this.c = c;
            this.style = style;
        }
    }
}
