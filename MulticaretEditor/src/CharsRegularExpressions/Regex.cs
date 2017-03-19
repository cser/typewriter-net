//------------------------------------------------------------------------------
// <copyright file="Regex.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// The Regex class represents a single compiled instance of a regular
// expression.

namespace CharsRegularExpressions {

    using System;
    using System.Threading;
    using System.Collections;
    using System.Reflection;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    ///    <para>
    ///       Represents an immutable, compiled regular expression. Also
    ///       contains static methods that allow use of regular expressions without instantiating
    ///       a Regex explicitly.
    ///    </para>
    /// </devdoc>
    public class Regex
    {
		internal static readonly char[] EmptyChars = new char[0];
        // Fields used by precompiled regexes
        protected internal string pattern;

        protected internal RegexOptions roptions;            // the top-level options from the options string


        // *********** Match timeout fields { ***********

        // We need this because time is queried using Environment.TickCount for performance reasons
        // (Environment.TickCount returns millisecs as an int and cycles):
        private static readonly TimeSpan MaximumMatchTimeout = TimeSpan.FromMilliseconds(Int32.MaxValue - 1);

        // InfiniteMatchTimeout specifies that match timeout is switched OFF. It allows for faster code paths
        // compared to simply having a very large timeout.
        // We do not want to ask users to use System.Threading.Timeout.InfiniteTimeSpan as a parameter because:
        //   (1) We do not want to imply any relation between having using a RegEx timeout and using multi-threading.
        //   (2) We do not want to require users to take ref to a contract assembly for threading just to use RegEx.
        //       There may in theory be a SKU that has RegEx, but no multithreading.
        // We create a public Regex.InfiniteMatchTimeout constant, which for consistency uses the save underlying
        // value as Timeout.InfiniteTimeSpan creating an implementation detail dependency only.
        public static readonly TimeSpan InfiniteMatchTimeout = new TimeSpan (0, 0, 0, 0, Timeout.Infinite);

        // All these protected internal fields in this class really should not be protected. The historic reason
        // for this is that classes extending Regex that are generated via CompileToAssembly rely on the fact that
        // these are accessible as protected in order to initialise them in the generated constructor of the
        // extending class. We should update this initialisation logic to using a protected constructor, but until
        // that is done we stick to the existing pattern however ugly it may be.
        protected internal TimeSpan internalMatchTimeout;   // timeout for the execution of this regex


        // During static initialisation of Regex we check 
        private const String DefaultMatchTimeout_ConfigKeyName = "REGEX_DEFAULT_MATCH_TIMEOUT";


        // FallbackDefaultMatchTimeout specifies the match timeout to use if no other timeout was specified
        // by one means or another. For now it is set to InfiniteMatchTimeout, meaning timeouts are OFF by
        // default (for Dev12 we plan to set a positive value).
        // Having this field is helpful to read the code as it makes it clear when we mean
        // "default that is currently no-timeouts" and when we mean "actually no-timeouts".
        // In Silverlight, DefaultMatchTimeout is always set to FallbackDefaultMatchTimeout,
        // on desktop, DefaultMatchTimeout can be configured via AppDomain and falls back to
        // FallbackDefaultMatchTimeout, if no AppDomain setting is present (see InitDefaultMatchTimeout()).
        internal static readonly TimeSpan FallbackDefaultMatchTimeout = InfiniteMatchTimeout;


        // DefaultMatchTimeout specifies the match timeout to use if no other timeout was specified
        // by one means or another. Typically, it is set to InfiniteMatchTimeout in Dev 11
        // (we plan to set a positive timeout in Dev12).
        // Hosts (e.g.) ASP may set an AppDomain property via SetData to change the default value.        

        internal static readonly TimeSpan DefaultMatchTimeout = FallbackDefaultMatchTimeout;
        
        // *********** } match timeout fields ***********

        // desktop build still uses non-generic collections for AppCompat with .NET Framework 3.5 pre-compiled assemblies
        protected internal Hashtable caps;
        protected internal Hashtable capnames;        
        protected internal String[]  capslist;               // if captures are sparse or named captures are used, this is the sorted list of names
        protected internal int       capsize;                // the size of the capture array

        internal  ExclusiveReference runnerref;              // cached runner
        internal  SharedReference    replref;                // cached parsed replacement pattern
        internal  RegexCode          code;                   // if interpreted, this is the code for RegexIntepreter
        internal  bool refsInitialized = false;

        internal static LinkedList<CachedCodeEntry> livecode = new LinkedList<CachedCodeEntry>();// the cached of code and factories that are currently loaded
        internal static int cacheSize = 15;
        
        internal const int MaxOptionShift = 10;       

        protected Regex() {

            // If a compiled-to-assembly RegEx was generated using an earlier version, then internalMatchTimeout will be uninitialised.
            // Let's do it here.
            // In distant future, when RegEx generated using pre Dev11 are not supported any more, we can remove this to aid performance:

            this.internalMatchTimeout = DefaultMatchTimeout;
        }

        /*
         * Compiles and returns a Regex object corresponding to the given pattern
         */
        /// <devdoc>
        ///    <para>
        ///       Creates and compiles a regular expression object for the specified regular
        ///       expression.
        ///    </para>
        /// </devdoc>
        public Regex(String pattern)
            : this(pattern, RegexOptions.None, DefaultMatchTimeout, false) {
        }

        /*
         * Returns a Regex object corresponding to the given pattern, compiled with
         * the specified options.
         */
        /// <devdoc>
        ///    <para>
        ///       Creates and compiles a regular expression object for the
        ///       specified regular expression
        ///       with options that modify the pattern.
        ///    </para>
        /// </devdoc>
        public Regex(String pattern, RegexOptions options)
            : this(pattern, options, DefaultMatchTimeout, false) {
        }

        public Regex(String pattern, RegexOptions options, TimeSpan matchTimeout)
            : this(pattern, options, matchTimeout, false) {
        }

        private Regex(String pattern, RegexOptions options, TimeSpan matchTimeout, bool useCache) {
            RegexTree tree;
            CachedCodeEntry cached = null;
            string cultureKey = null;

            if (pattern == null) 
                throw new ArgumentNullException("pattern");
            if (options < RegexOptions.None || ( ((int) options) >> MaxOptionShift) != 0)
                throw new ArgumentOutOfRangeException("options");
            if ((options &   RegexOptions.ECMAScript) != 0
             && (options & ~(RegexOptions.ECMAScript | 
                             RegexOptions.IgnoreCase | 
                             RegexOptions.Multiline |
                             RegexOptions.CultureInvariant
#if DBG
                           | RegexOptions.Debug
#endif
                                               )) != 0)
                throw new ArgumentOutOfRangeException("options");

            ValidateMatchTimeout(matchTimeout);

            // Try to look up this regex in the cache.  We do this regardless of whether useCache is true since there's
            // really no reason not to. 
            if ((options & RegexOptions.CultureInvariant) != 0)
                cultureKey = CultureInfo.InvariantCulture.ToString(); // "English (United States)"
            else
                cultureKey = CultureInfo.CurrentCulture.ToString();
            
            String key = ((int) options).ToString(NumberFormatInfo.InvariantInfo) + ":" + cultureKey + ":" + pattern;
            cached = LookupCachedAndUpdate(key);

            this.pattern = pattern;
            this.roptions = options;

            this.internalMatchTimeout = matchTimeout;

            if (cached == null) {
                // Parse the input
                tree = RegexParser.Parse(pattern, roptions);

                // Extract the relevant information
                capnames   = tree._capnames;
                capslist   = tree._capslist;
                code       = RegexWriter.Write(tree);
                caps       = code._caps;
                capsize    = code._capsize;

                InitializeReferences();

                tree = null;
                if (useCache)
                    cached = CacheCode(key);
            }
            else {
                caps       = cached._caps;
                capnames   = cached._capnames;
                capslist   = cached._capslist;
                capsize    = cached._capsize;
                code       = cached._code;
                runnerref  = cached._runnerref;
                replref    = cached._replref;
                refsInitialized = true;
            }
        }

        //* Note: "&lt;" is the XML entity for smaller ("<").
        /// <summary>
        /// Validates that the specified match timeout value is valid.
        /// The valid range is <code>TimeSpan.Zero &lt; matchTimeout &lt;= Regex.MaximumMatchTimeout</code>.
        /// </summary>
        /// <param name="matchTimeout">The timeout value to validate.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">If the specified timeout is not within a valid range.        
        /// </exception>
        protected internal static void ValidateMatchTimeout(TimeSpan matchTimeout) {

            if (InfiniteMatchTimeout == matchTimeout)
                return;

            // Change this to make sure timeout is not longer then Environment.Ticks cycle length:
            if (TimeSpan.Zero < matchTimeout && matchTimeout <= MaximumMatchTimeout)
                return;

            throw new ArgumentOutOfRangeException("matchTimeout");
        }

        /* 
        * This method is here for perf reasons: if the call to RegexCompiler is NOT in the 
        * Regex constructor, we don't load RegexCompiler and its reflection classes when
        * instantiating a non-compiled regex
        * This method is internal virtual so the jit does not inline it.
        */
        [MethodImplAttribute(MethodImplOptions.NoInlining)]

        /*
         * Escape metacharacters within the string
         */
        /// <devdoc>
        ///    <para>
        ///       Escapes 
        ///          a minimal set of metacharacters (\, *, +, ?, |, {, [, (, ), ^, $, ., #, and
        ///          whitespace) by replacing them with their \ codes. This converts a string so that
        ///          it can be used as a constant within a regular expression safely. (Note that the
        ///          reason # and whitespace must be escaped is so the string can be used safely
        ///          within an expression parsed with x mode. If future Regex features add
        ///          additional metacharacters, developers should depend on Escape to escape those
        ///          characters as well.)
        ///       </para>
        ///    </devdoc>
        public static String Escape(String str) {
            if (str==null)
                throw new ArgumentNullException("str");
            
            return RegexParser.Escape(str);
        }

        /*
         * Unescape character codes within the string
         */
        /// <devdoc>
        ///    <para>
        ///       Unescapes any escaped characters in the input string.
        ///    </para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Unescape", Justification="[....]: already shipped since v1 - can't fix without causing a breaking change")]
        public static String Unescape(String str) {
            if (str==null)
                throw new ArgumentNullException("str");
            
            return RegexParser.Unescape(str);
        }

        [SuppressMessage("Microsoft.Concurrency", "CA8001", Justification = "Reviewed for thread-safety")]
        public static int CacheSize {
            get {
                return cacheSize;
            }
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                cacheSize = value;
                if (livecode.Count > cacheSize) {
                    lock (livecode) {
                        while (livecode.Count > cacheSize)
                            livecode.RemoveLast();
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the options passed into the constructor
        ///    </para>
        /// </devdoc>
        public RegexOptions Options {
            get { return roptions;}
        }


        /// <summary>
        /// The match timeout used by this Regex instance.
        /// </summary>
        public TimeSpan MatchTimeout {
            get { return internalMatchTimeout; }
        }


        /*
         * True if the regex is leftward
         */
        /// <devdoc>
        ///    <para>
        ///       Indicates whether the regular expression matches from right to
        ///       left.
        ///    </para>
        /// </devdoc>
        public bool RightToLeft {
            get {
                return UseOptionR();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the regular expression pattern passed into the constructor
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            return pattern;
        }

        /*
         * Returns an array of the group names that are used to capture groups
         * in the regular expression. Only needed if the regex is not known until
         * runtime, and one wants to extract captured groups. (Probably unusual,
         * but supplied for completeness.)
         */
        /// <devdoc>
        ///    Returns 
        ///       the GroupNameCollection for the regular expression. This collection contains the
        ///       set of strings used to name capturing groups in the expression. 
        ///    </devdoc>
        public String[] GetGroupNames() {
            String[] result;

            if (capslist == null) {
                int max = capsize;
                result = new String[max];

                for (int i = 0; i < max; i++) {
                    result[i] = Convert.ToString(i, CultureInfo.InvariantCulture);
                }
            }
            else {
                result = new String[capslist.Length];

                System.Array.Copy(capslist, 0, result, 0, capslist.Length);
            }

            return result;
        }

        /*
         * Returns an array of the group numbers that are used to capture groups
         * in the regular expression. Only needed if the regex is not known until
         * runtime, and one wants to extract captured groups. (Probably unusual,
         * but supplied for completeness.)
         */
        /// <devdoc>
        ///    returns 
        ///       the integer group number corresponding to a group name. 
        ///    </devdoc>
        public int[] GetGroupNumbers() {
            int[] result;

            if (caps == null) {
                int max = capsize;
                result = new int[max];

                for (int i = 0; i < max; i++) {
                    result[i] = i;
                }
            }
            else {
                result = new int[caps.Count];

                IDictionaryEnumerator de = caps.GetEnumerator();
                while (de.MoveNext()) {
                    result[(int)de.Value] = (int)de.Key;
                }
            }

            return result;
        }

        /*
         * Given a group number, maps it to a group name. Note that nubmered
         * groups automatically get a group name that is the decimal string
         * equivalent of its number.
         *
         * Returns null if the number is not a recognized group number.
         */
        /// <devdoc>
        ///    <para>
        ///       Retrieves a group name that corresponds to a group number.
        ///    </para>
        /// </devdoc>
        public String GroupNameFromNumber(int i) {
            if (capslist == null) {
                if (i >= 0 && i < capsize)
                    return i.ToString(CultureInfo.InvariantCulture);

                return String.Empty;
            }
            else {
                if (caps != null) {
                    Object obj = caps[i];
                    if (obj == null)
                        return String.Empty;

                    i = (int)obj;
                }

                if (i >= 0 && i < capslist.Length)
                    return capslist[i];

                return String.Empty;
            }
        }

        /*
         * Given a group name, maps it to a group number. Note that nubmered
         * groups automatically get a group name that is the decimal string
         * equivalent of its number.
         *
         * Returns -1 if the name is not a recognized group name.
         */
        /// <devdoc>
        ///    <para>
        ///       Returns a group number that corresponds to a group name.
        ///    </para>
        /// </devdoc>
        public int GroupNumberFromName(String name) {
            int result = -1;

            if (name == null)
                throw new ArgumentNullException("name");

            // look up name if we have a hashtable of names
            if (capnames != null) {
                Object ret = capnames[name];
                if (ret == null)
                    return -1;

                return(int)ret;
            }

            // convert to an int if it looks like a number
            result = 0;
            for (int i = 0; i < name.Length; i++) {
                char ch = name[i];

                if (ch > '9' || ch < '0')
                    return -1;

                result *= 10;
                result += (ch - '0');
            }

            // return int if it's in range
            if (result >= 0 && result < capsize)
                return result;

            return -1;
        }

        /*
         * Static version of simple IsMatch call
         */
        ///    <devdoc>
        ///       <para>
        ///          Searches the input 
        ///             string for one or more occurrences of the text supplied in the pattern
        ///             parameter.
        ///       </para>
        ///    </devdoc>
        public static bool IsMatch(char[] input, String pattern) {
            return IsMatch(input, pattern, RegexOptions.None, DefaultMatchTimeout);
        }        

        /*
         * Static version of simple IsMatch call
         */
        /// <devdoc>
        ///    <para>
        ///       Searches the input string for one or more occurrences of the text 
        ///          supplied in the pattern parameter with matching options supplied in the options
        ///          parameter.
        ///       </para>
        ///    </devdoc>
        public static bool IsMatch(char[] input, String pattern, RegexOptions options) {
            return IsMatch(input, pattern, options, DefaultMatchTimeout);
        }

        public static bool IsMatch(char[] input, String pattern, RegexOptions options, TimeSpan matchTimeout) {
            return new Regex(pattern, options, matchTimeout, true).IsMatch(input);
        }

        /*
         * Returns true if the regex finds a match within the specified string
         */
        /// <devdoc>
        ///    <para>
        ///       Searches the input string for one or 
        ///          more matches using the previous pattern, options, and starting
        ///          position.
        ///       </para>
        ///    </devdoc>
        public bool IsMatch(char[] input) {

            if (input == null)
                throw new ArgumentNullException("input");

            return IsMatch(input, UseOptionR() ? input.Length : 0);            
        }

        /*
         * Returns true if the regex finds a match after the specified position
         * (proceeding leftward if the regex is leftward and rightward otherwise)
         */
        /// <devdoc>
        ///    <para>
        ///       Searches the input 
        ///          string for one or more matches using the previous pattern and options, with
        ///          a new starting position.
        ///    </para>
        /// </devdoc>
        public bool IsMatch(char[] input, int startat) {

            if (input == null)
                throw new ArgumentNullException("input");

            return (null == Run(true, -1, input, 0, input.Length, startat));
        }

        /*
         * Static version of simple Match call
         */
        ///    <devdoc>
        ///       <para>
        ///          Searches the input string for one or more occurrences of the text 
        ///             supplied in the pattern parameter.
        ///       </para>
        ///    </devdoc>
        public static Match Match(char[] input, String pattern) {
            return Match(input, pattern, RegexOptions.None, DefaultMatchTimeout);
        }

        /*
         * Static version of simple Match call
         */
        /// <devdoc>
        ///    <para>
        ///       Searches the input string for one or more occurrences of the text 
        ///          supplied in the pattern parameter. Matching is modified with an option
        ///          string.
        ///       </para>
        ///    </devdoc>
        public static Match Match(char[] input, String pattern, RegexOptions options) {
            return Match(input, pattern, options, DefaultMatchTimeout);
        }

        public static Match Match(char[] input, String pattern, RegexOptions options, TimeSpan matchTimeout) {
            return new Regex(pattern, options, matchTimeout, true).Match(input);
        }

        /*
         * Finds the first match for the regular expression starting at the beginning
         * of the string (or at the end of the string if the regex is leftward)
         */
        /// <devdoc>
        ///    <para>
        ///       Matches a regular expression with a string and returns
        ///       the precise result as a RegexMatch object.
        ///    </para>
        /// </devdoc>
        public Match Match(char[] input) {

            if (input == null)
                throw new ArgumentNullException("input");

            return Match(input, UseOptionR() ? input.Length : 0);
        }

        /*
         * Finds the first match, starting at the specified position
         */
        /// <devdoc>
        ///    Matches a regular expression with a string and returns
        ///    the precise result as a RegexMatch object.
        /// </devdoc>
        public Match Match(char[] input, int startat) {

            if (input == null)
                throw new ArgumentNullException("input");

            return Run(false, -1, input, 0, input.Length, startat);
        }

        /*
         * Finds the first match, restricting the search to the specified interval of
         * the char array.
         */
        /// <devdoc>
        ///    <para>
        ///       Matches a
        ///       regular expression with a string and returns the precise result as a
        ///       RegexMatch object.
        ///    </para>
        /// </devdoc>
        public Match Match(char[] input, int beginning, int length) {
            if (input == null)
                throw new ArgumentNullException("input");

            return Run(false, -1, input, beginning, length, UseOptionR() ? beginning + length : beginning);
        }

        /*
         * Static version of simple Matches call
         */
        ///    <devdoc>
        ///       <para>
        ///          Returns all the successful matches as if Match were
        ///          called iteratively numerous times.
        ///       </para>
        ///    </devdoc>
        public static MatchCollection Matches(char[] input, String pattern) {
            return Matches(input, pattern, RegexOptions.None, DefaultMatchTimeout);
        }

        /*
         * Static version of simple Matches call
         */
        /// <devdoc>
        ///    <para>
        ///       Returns all the successful matches as if Match were called iteratively
        ///       numerous times.
        ///    </para>
        /// </devdoc>
        public static MatchCollection Matches(char[] input, String pattern, RegexOptions options) {
            return Matches(input, pattern, options, DefaultMatchTimeout);
        }

        public static MatchCollection Matches(char[] input, String pattern, RegexOptions options, TimeSpan matchTimeout) {
            return new Regex(pattern, options, matchTimeout, true).Matches(input);
        }

        /*
         * Finds the first match for the regular expression starting at the beginning
         * of the string Enumerator(or at the end of the string if the regex is leftward)
         */
        /// <devdoc>
        ///    <para>
        ///       Returns
        ///       all the successful matches as if Match was called iteratively numerous
        ///       times.
        ///    </para>
        /// </devdoc>
        public MatchCollection Matches(char[] input) {

            if (input == null)
                throw new ArgumentNullException("input");

            return Matches(input, UseOptionR() ? input.Length : 0);
        }

        /*
         * Finds the first match, starting at the specified position
         */
        /// <devdoc>
        ///    <para>
        ///       Returns
        ///       all the successful matches as if Match was called iteratively numerous
        ///       times.
        ///    </para>
        /// </devdoc>
        public MatchCollection Matches(char[] input, int startat) {

            if (input == null)
                throw new ArgumentNullException("input");

            return new MatchCollection(this, input, 0, input.Length, startat);
        }

        /// <devdoc>
        /// </devdoc>
        protected void InitializeReferences() {
            if (refsInitialized)
                throw new NotSupportedException(SR.GetString(SR.OnlyAllowedOnce));
            
            refsInitialized = true;
            runnerref  = new ExclusiveReference();
            replref    = new SharedReference();
        }

        
        /*
         * Internal worker called by all the public APIs
         */
        internal Match Run(bool quick, int prevlen, char[] input, int beginning, int length, int startat) {
            Match match;
            RegexRunner runner = null;

            if (startat < 0 || startat > input.Length)
                throw new ArgumentOutOfRangeException("start", SR.GetString(SR.BeginIndexNotNegative));

            if (length < 0 || length > input.Length)
                throw new ArgumentOutOfRangeException("length", SR.GetString(SR.LengthNotNegative));

            // There may be a cached runner; grab ownership of it if we can.

            runner = (RegexRunner)runnerref.Get();

            // Create a RegexRunner instance if we need to

            if (runner == null) {
                // Use the compiled RegexRunner factory if the code was compiled to MSIL
                runner = new RegexInterpreter(code, UseOptionInvariant() ? CultureInfo.InvariantCulture : CultureInfo.CurrentCulture);
            }

            try {
                // Do the scan starting at the requested position            
                match = runner.Scan(this, input, beginning, beginning + length, startat, prevlen, quick, internalMatchTimeout);
            } finally {
                // Release or fill the cache slot
                runnerref.Release(runner);
            }

#if DBG
            if (Debug && match != null)
                match.Dump();
#endif
            return match;
        }

        /*
         * Find code cache based on options+pattern
         */
        private static CachedCodeEntry LookupCachedAndUpdate(String key) {
            lock (livecode) {
                for (LinkedListNode<CachedCodeEntry> current = livecode.First; current != null; current = current.Next) {
                    if (current.Value._key == key) {
                        // If we find an entry in the cache, move it to the head at the same time. 
                        livecode.Remove(current);
                        livecode.AddFirst(current);
                        return current.Value;
                    }
                }
            }

            return null;
        }

        /*
         * Add current code to the cache
         */
        private CachedCodeEntry CacheCode(String key) {
            CachedCodeEntry newcached = null;

            lock (livecode) {
                // first look for it in the cache and move it to the head
                for (LinkedListNode<CachedCodeEntry> current = livecode.First; current != null; current = current.Next) {
                    if (current.Value._key == key) {
                        livecode.Remove(current);
                        livecode.AddFirst(current);
                        return current.Value;
                    }
                }

                // it wasn't in the cache, so we'll add a new one.  Shortcut out for the case where cacheSize is zero.
                if (cacheSize != 0) {
                    newcached = new CachedCodeEntry(key, capnames, capslist, code, caps, capsize, runnerref, replref);
                    livecode.AddFirst(newcached);
                    if (livecode.Count > cacheSize)
                        livecode.RemoveLast();
                }
            }

            return newcached;
        }

        /*
         * True if the L option was set
         */
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected bool UseOptionR() {
            return(roptions & RegexOptions.RightToLeft) != 0;
        }

        internal bool UseOptionInvariant() {
            return(roptions & RegexOptions.CultureInvariant) != 0;
        }
            

#if DBG
        /*
         * True if the regex has debugging enabled
         */
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        internal bool Debug {
            get {
                return(roptions & RegexOptions.Debug) != 0;
            }
        }

#endif
    }


    /*
     * Callback class
     */
    /// <devdoc>
    /// </devdoc>
    public delegate String MatchEvaluator(Match match);


    /*
     * Used to cache byte codes or compiled factories
     */
    internal sealed class CachedCodeEntry {
        internal string _key;
        internal RegexCode _code;
        internal Hashtable _caps;
        internal Hashtable _capnames;
        internal String[]  _capslist;
        internal int       _capsize;
        internal ExclusiveReference _runnerref;
        internal SharedReference _replref;

        internal CachedCodeEntry(string key, Hashtable capnames, String[] capslist, RegexCode code, Hashtable caps, int capsize, ExclusiveReference runner, SharedReference repl)
        {

            _key        = key;
            _capnames   = capnames;
            _capslist   = capslist;

            _code       = code;
            _caps       = caps;
            _capsize    = capsize;

            _runnerref     = runner;
            _replref       = repl;
        }
    }

    /*
     * Used to cache one exclusive runner reference
     */
    internal sealed class ExclusiveReference {
        RegexRunner _ref;
        Object _obj;
        int _locked;

        /*
         * Return an object and grab an exclusive lock.
         *
         * If the exclusive lock can't be obtained, null is returned;
         * if the object can't be returned, the lock is released.
         *
         */
        internal Object Get() {
            // try to obtain the lock

            if (0 == Interlocked.Exchange(ref _locked, 1)) {
                // grab reference

                   
                Object obj = _ref;

                // release the lock and return null if no reference

                if (obj == null) {
                    _locked = 0;
                    return null;
                }

                // remember the reference and keep the lock

                _obj = obj;
                return obj;
            }

            return null;
        }

        /*
         * Release an object back to the cache
         *
         * If the object is the one that's under lock, the lock
         * is released.
         *
         * If there is no cached object, then the lock is obtained
         * and the object is placed in the cache.
         *
         */
        internal void Release(Object obj) {
            if (obj == null)
                throw new ArgumentNullException("obj");

            // if this reference owns the lock, release it

            if (_obj == obj) {
                _obj = null;
                _locked = 0;
                return;
            }

            // if no reference owns the lock, try to cache this reference

            if (_obj == null) {
                // try to obtain the lock

                if (0 == Interlocked.Exchange(ref _locked, 1)) {
                    // if there's really no reference, cache this reference

                    if (_ref == null)
                        _ref = (RegexRunner) obj;

                    // release the lock

                    _locked = 0;
                    return;
                }
            }
        }
    }

    /*
     * Used to cache a weak reference in a threadsafe way
     */
    internal sealed class SharedReference {
        WeakReference _ref = new WeakReference(null);
        int _locked;

        /*
         * Return an object from a weakref, protected by a lock.
         *
         * If the exclusive lock can't be obtained, null is returned;
         *
         * Note that _ref.Target is referenced only under the protection
         * of the lock. (Is this necessary?)
         */
        internal  Object Get() {
            if (0 == Interlocked.Exchange(ref _locked, 1)) {
                Object obj = _ref.Target;
                _locked = 0;
                return obj;
            }

            return null;
        }

        /*
         * Suggest an object into a weakref, protected by a lock.
         *
         * Note that _ref.Target is referenced only under the protection
         * of the lock. (Is this necessary?)
         */
        internal void Cache(Object obj) {
            if (0 == Interlocked.Exchange(ref _locked, 1)) {
                _ref.Target = obj;
                _locked = 0;
            }
        }
    }

}