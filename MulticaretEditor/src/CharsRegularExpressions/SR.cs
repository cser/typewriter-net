namespace CharsRegularExpressions {

    public static class SR {
    	public const int UnexpectedOpcode = 1;
    	public const int BeginIndexNotNegative = 2;
    	public const int CountTooSmall = 3;
    	public const int ReplacementError = 4;
    	public const int MakeException = 5;
    	public const int TooManyAlternates = 6;
    	public const int IllegalCondition = 7;
    	public const int IncompleteSlashP = 8;
    	public const int MalformedSlashP = 9;
    	public const int UnrecognizedEscape = 10;
    	public const int UnrecognizedControl = 11;
    	public const int MissingControl = 12;
    	public const int TooFewHex = 13;
    	public const int CaptureGroupOutOfRange = 14;
    	public const int UndefinedNameRef = 15;
    	public const int UndefinedBackref = 16;
    	public const int MalformedNameRef = 17;
    	public const int IllegalEndEscape = 18;
    	public const int UnterminatedComment = 19;
    	public const int AlternationCantCapture = 20;
    	public const int MalformedReference = 21;
    	public const int IllegalDefaultRegexMatchTimeoutInAppDomain = 22;
    	public const int UnrecognizedGrouping = 22;
    	public const int AlternationCantHaveComment = 23;
    	public const int UndefinedReference = 24;
    	public const int InvalidGroupName = 25;
    	public const int CapnumNotZero = 26;
    	public const int UnterminatedBracket = 27;
    	public const int SubtractionMustBeLast = 28;
    	public const int ReversedCharRange = 29;
    	public const int BadClassInCharRange = 30;
    	public const int NotEnoughParens = 31;
    	public const int IllegalRange = 32;
    	public const int InternalError = 33;
    	public const int QuantifyAfterNothing = 34;
    	public const int NestedQuantify = 35;
    	public const int TooManyParens = 36;
    	public const int RegexMatchTimeoutException_Occurred = 37;
    	public const int EnumNotStarted = 38;
    	public const int Arg_InvalidArrayType = 39;
    	public const int Arg_RankMultiDimNotSupported = 40;
    	public const int UnimplementedState = 41;
    	public const int OnlyAllowedOnce = 42;
    	public const int InvalidNullEmptyArgument = 43;
    	public const int UnknownProperty = 44;
    	public const int LengthNotNegative = 45;
    	public const int ArgumentNull_ArrayWithNullElements = 46;
    	public const int NoResultOnFailed = 47;
    	
    	public static string GetString(int opcode) {
    		return opcode + "";
    	}
    	
    	public static string GetString(int opcode, string message) {
    		return opcode + ": " + message;
    	}
    	
    	public static string GetString(int opcode, string pattern, string message) {
    		return opcode + ": " + pattern + " - " + message;
    	}
    	
    	public static string GetString(int opcode, int index) {
    		return opcode + ": " + index;
    	}
    }

}