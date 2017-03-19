//------------------------------------------------------------------------------
// <copyright file="RegexCapture.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// Capture is just a location/length pair that indicates the
// location of a regular expression match. A single regexp
// search may return multiple Capture within each capturing
// RegexGroup.

namespace CharsRegularExpressions {

    /// <devdoc>
    ///    <para> 
    ///       Represents the results from a single subexpression capture. The object represents
    ///       one substring for a single successful capture.</para>
    /// </devdoc>
    public class Capture {
        internal char[] _text;
        internal int _index;
        internal int _length;

        internal Capture(char[] text, int i, int l) {
            _text = text;
            _index = i;
            _length = l;
        }

        /*
         * The index of the beginning of the matched capture
         */
        /// <devdoc>
        ///    <para>Returns the position in the original string where the first character of
        ///       captured substring was found.</para>
        /// </devdoc>
        public int Index {
            get {
                return _index;
            }
        }

        /*
         * The length of the matched capture
         */
        /// <devdoc>
        ///    <para>
        ///       Returns the length of the captured substring.
        ///    </para>
        /// </devdoc>
        public int Length {
            get {
                return _length;
            }
        }
        
        public string Value {
            get {
                return new string(_text, _index, _length);
            }
        }

        /*
         * The original string
         */
        internal char[] GetOriginalString() {
            return _text;
        }

#if DBG
        internal virtual string Description() {
            StringBuilder Sb = new StringBuilder();

            Sb.Append("(I = ");
            Sb.Append(_index);
            Sb.Append(", L = ");
            Sb.Append(_length);
            Sb.Append("): ");
            Sb.Append(_text, _index, _length);

            return Sb.ToString();
        }
#endif
    }



}