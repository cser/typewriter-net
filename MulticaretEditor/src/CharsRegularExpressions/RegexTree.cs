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
        internal RegexTree(RegexNode root, Hashtable caps, int[] capnumlist, int captop, Hashtable capnames, string[] capslist, RegexOptions opts)
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
        internal Hashtable _caps;
        internal int[]  _capnumlist;
        internal Hashtable _capnames;
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