//------------------------------------------------------------------------------
// <copyright file="RegexReplacement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// The RegexReplacement class represents a substitution string for
// use when using regexs to search/replace, etc. It's logically
// a sequence intermixed (1) constant strings and (2) group numbers.

namespace CharsRegularExpressions {

    using System.Collections;
    using System.Collections.Generic;

    internal sealed class RegexReplacement {
        /*
         * Since RegexReplacement shares the same parser as Regex,
         * the constructor takes a RegexNode which is a concatenation
         * of constant strings and backreferences.
         */
#if SILVERLIGHT
        internal RegexReplacement(string rep, RegexNode concat, Dictionary<int, int> _caps) {
#else
        internal RegexReplacement(string rep, RegexNode concat, Hashtable _caps) {
#endif
            System.Text.StringBuilder sb;
            List<string> strings;
            List<int> rules;
            int slot;

            _rep = rep;

            if (concat.Type() != RegexNode.Concatenate)
                throw new System.ArgumentException(SR.GetString(SR.ReplacementError));

            sb = new System.Text.StringBuilder();
            strings = new List<string>();
            rules = new List<int>();

            for (int i = 0; i < concat.ChildCount(); i++) {
                RegexNode child = concat.Child(i);

                switch (child.Type()) {
                    case RegexNode.Multi:
                        sb.Append(child._str);
                        break;
                    case RegexNode.One:
                        sb.Append(child._ch);
                        break;
                    case RegexNode.Ref:
                        if (sb.Length > 0) {
                            rules.Add(strings.Count);
                            strings.Add(sb.ToString());
                            sb.Length = 0;
                        }
                        slot = child._m;

                        if (_caps != null && slot >= 0)
                            slot = (int)_caps[slot];

                        rules.Add(-Specials - 1 - slot);
                        break;
                    default:
                        throw new System.ArgumentException(SR.GetString(SR.ReplacementError));
                }
            }

            if (sb.Length > 0) {
                rules.Add(strings.Count);
                strings.Add(sb.ToString());
            }

            _strings = strings; 
            _rules = rules; 
        }

        internal string _rep;
        internal List<string>  _strings;          // table of string constants
        internal List<int>  _rules;            // negative -> group #, positive -> string #

        // constants for special insertion patterns

        internal const int Specials       = 4;
        internal const int LeftPortion    = -1;
        internal const int RightPortion   = -2;
        internal const int LastGroup      = -3;
        internal const int WholeString    = -4;

        /*
         * The original pattern string
         */
        internal string Pattern {
            get {
                return _rep;
            }
        }
    }

}