///------------------------------------------------------------------------------
/// <copyright file="RegexMatchTimeoutException.cs" company="Microsoft">
///     Copyright (c) Microsoft Corporation.  All rights reserved.
/// </copyright>                               
///
/// <owner>gpaperin</owner>
///------------------------------------------------------------------------------

using System;
using System.Security;
using System.Security.Permissions;

namespace CharsRegularExpressions {

/// <summary>
/// This is the exception that is thrown when a RegEx matching timeout occurs.
/// </summary>

public class RegexMatchTimeoutException : TimeoutException {

    private string regexInput = null;

    private string regexPattern = null;

    private TimeSpan matchTimeout = TimeSpan.FromTicks(-1);


    /// <summary>
    /// This is the preferred constructor to use.
    /// The other constructors are provided for compliance to Fx design guidelines.
    /// </summary>
    /// <param name="regexInput">Matching timeout occured during mathing within the specified input.</param>
    /// <param name="regexPattern">Matching timeout occured during mathing to the specified pattern.</param>
    /// <param name="matchTimeout">Matching timeout occured becasue matching took longer than the specified timeout.</param>
    public RegexMatchTimeoutException(string regexInput, string regexPattern, TimeSpan matchTimeout) :
        base(SR.GetString(SR.RegexMatchTimeoutException_Occurred)) {
        Init(regexInput, regexPattern, matchTimeout);
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>    
    public RegexMatchTimeoutException()
        : base() {
        Init();
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RegexMatchTimeoutException(string message)
        : base(message) {
        Init();
    }


    /// <summary>
    /// This constructor is provided in compliance with common NetFx design patterns;
    /// developers should prefer using the constructor
    /// <code>public RegexMatchTimeoutException(string input, string pattern, TimeSpan matchTimeout)</code>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a <code>null</code>.</param>
    public RegexMatchTimeoutException(string message, Exception inner)
        : base(message, inner) {
        Init();
    }

    private void Init() {
        Init("", "", TimeSpan.FromTicks(-1));
    }

    private void Init(string input, string pattern, TimeSpan timeout) {
        this.regexInput = input;
        this.regexPattern = pattern;
        this.matchTimeout = timeout;
    }

    public string Pattern {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        get { return regexPattern; }
    }

    public string Input {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        get { return regexInput; }
    }

    public TimeSpan MatchTimeout {
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        get { return matchTimeout; }
    }
} // public class RegexMatchTimeoutException


} // namespace MonoRegularExpressions

// file RegexMatchTimeoutException.cs