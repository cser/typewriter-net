namespace CharsRegularExpressions {

    public static class SR {
    	public const string UnexpectedOpcode = "Unexpected opcode";
    	public const string BeginIndexNotNegative = "Begin index not negative";
    	public const string CountTooSmall = "Count too small";
    	public const string ReplacementError = "Replacement error";
    	public const string MakeException = "Make exception";
    	public const string TooManyAlternates = "TooMany alternates";
    	public const string IllegalCondition = "Illegal condition";
    	public const string IncompleteSlashP = "Incomplete \\P";
    	public const string MalformedSlashP = "Malformed \\P";
    	public const string UnrecognizedEscape = "Unrecognized escape";
    	public const string UnrecognizedControl = "Unrecognized control";
    	public const string MissingControl = "Missing control";
    	public const string TooFewHex = "Too few hex";
    	public const string CaptureGroupOutOfRange = "Capture group out of range";
    	public const string UndefinedNameRef = "Undefined name ref";
    	public const string UndefinedBackref = "Undefined backref";
    	public const string MalformedNameRef = "Malformed name ref";
    	public const string IllegalEndEscape = "Illegal end escape";
    	public const string UnterminatedComment = "Unterminated comment";
    	public const string AlternationCantCapture = "Alternation can't capture";
    	public const string MalformedReference = "Malformed reference";
    	public const string IllegalDefaultRegexMatchTimeoutInAppDomain = "Illegal default regex match timeout in AppDomain";
    	public const string UnrecognizedGrouping = "Unrecognized grouping";
    	public const string AlternationCantHaveComment = "Alternation can't have comment";
    	public const string UndefinedReference = "Undefined reference";
    	public const string InvalidGroupName = "Invalid group name";
    	public const string CapnumNotZero = "Capnum not zero";
    	public const string UnterminatedBracket = "Unterminated bracket";
    	public const string SubtractionMustBeLast = "Subtraction must be last";
    	public const string ReversedCharRange = "Reversed char range";
    	public const string BadClassInCharRange = "Bad class in char range";
    	public const string NotEnoughParens = "Not enough parens";
    	public const string IllegalRange = "Illegal range";
    	public const string InternalError = "Internal error";
    	public const string QuantifyAfterNothing = "Quantify after nothing";
    	public const string NestedQuantify = "Nested quantify";
    	public const string TooManyParens = "Too many parens";
    	public const string RegexMatchTimeoutException_Occurred = "RegexMatchTimeoutException occurred";
    	public const string EnumNotStarted = "Enum not started";
    	public const string Arg_InvalidArrayType = "Argument - invalid array type";
    	public const string Arg_RankMultiDimNotSupported = "Argument - rank multi dim not supported";
    	public const string UnimplementedState = "Unimplemented state";
    	public const string OnlyAllowedOnce = "Only allowed once";
    	public const string InvalidNullEmptyArgument = "Invalid null or empty argument";
    	public const string UnknownProperty = "Unknown property";
    	public const string LengthNotNegative = "Length not negative";
    	public const string ArgumentNull_ArrayWithNullElements = "Argument is null or array with null elements";
    	public const string NoResultOnFailed = "No result on failed";
    	
    	public static string GetString(string opcode) {
    		return opcode + "";
    	}
    	
    	public static string GetString(string opcode, string message) {
    		return opcode + ": " + message;
    	}
    	
    	public static string GetString(string opcode, string pattern, string message) {
    		return opcode + ": " + pattern + " - " + message;
    	}
    	
    	public static string GetString(string opcode, int index) {
    		return opcode + ": " + index;
    	}
    }

}