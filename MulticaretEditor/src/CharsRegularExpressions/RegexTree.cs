//------------------------------------------------------------------------------
// <copyright file="RegexTree.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// RegexTree is just a wrapper for a node tree with some
// global information attached.

namespace CharsRegularExpressions {

    using System.Collections;
    using System.Collections.Generic;

    internal sealed class RegexTree {
#if SILVERLIGHT
        internal RegexTree(RegexNode root, Dictionary<int, int> caps, int[] capnumlist, int captop, Dictionary<string, int> capnames, string[] capslist, RegexOptions opts)
#else
        internal RegexTree(RegexNode root, Hashtable caps, int[] capnumlist, int captop, Hashtable capnames, string[] capslist, RegexOptions opts)
#endif

        {
            _root = root;
            _caps = caps;
            _capnumlist = capnumlist;
            _capnames = capnames;
            _capslist = capslist;
            _captop = captop;
            _options = opts;
        }

        internal RegexNode _root;
#if SILVERLIGHT
        internal Dictionary<int, int> _caps;
#else
        internal Hashtable _caps;
#endif
        internal int[]  _capnumlist;
#if SILVERLIGHT
        internal Dictionary<string, int> _capnames;
#else
        internal Hashtable _capnames;
#endif
        internal string[]  _capslist;
        internal RegexOptions _options;
        internal int       _captop;

#if DBG
        internal void Dump() {
            _root.Dump();
        }

        internal bool Debug {
            get {
                return(_options & RegexOptions.Debug) != 0;
            }
        }
#endif
    }
}