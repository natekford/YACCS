using System;

using YACCS.Localization;

namespace YACCS.Results;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public class Canceled() : LocalizedResult(false, Keys.CanceledResult);

public class CommandNotFound() : LocalizedResult(false, Keys.CommandNotFoundResult);

public class ExceptionAfterCommand() : LocalizedResult(false, Keys.ExceptionAfterCommandResult);

public class ExceptionDuringCommand() : LocalizedResult(false, Keys.ExceptionDuringCommandResult);

public class Failure() : Result(false, string.Empty);

public class InteractionEnded() : LocalizedResult(false, Keys.InteractionEndedResult);

public class InvalidContext() : LocalizedResult(false, Keys.InvalidContextResult);

public class InvalidParameter() : LocalizedResult(false, Keys.InvalidParameterResult);

public class MultiMatchHandlingError() : LocalizedResult(false, Keys.MultiMatchHandlingErrorResult);

public class MustBeGreaterThan(int min) : LocalizedResult<int>(false, Keys.MustBeGreaterThan, min);

public class MustBeLessThan(int max) : LocalizedResult<int>(false, Keys.MustBeLessThan, max);

public class MustBeLocked(Type type) : LocalizedResult<Type>(false, Keys.MustBeLocked, type);

public class MustBeUnlocked(Type type) : LocalizedResult<Type>(false, Keys.MustBeUnlocked, type);

public class NamedArgBadCount() : LocalizedResult(false, Keys.NamedArgBadCountResult);

public class NamedArgDuplicate(string name) : LocalizedResult<string>(false, Keys.NamedArgDuplicateResult, name);

public class NamedArgInvalidDictionary() : LocalizedResult(false, Keys.NamedArgInvalidDictionaryResult);

public class NamedArgMissingValue(string name) : LocalizedResult<string>(false, Keys.NamedArgMissingValueResult, name);

public class NamedArgNonExistent(string name) : LocalizedResult<string>(false, Keys.NamedArgNonExistentResult, name);

public class NotEnoughArgs() : LocalizedResult(false, Keys.NotEnoughArgsResult);

public class NotFound(Type type) : LocalizedResult<Type>(false, Keys.NotFoundResult, type);

public class NullParameter() : LocalizedResult(false, Keys.NullParameterResult);

public class ParseFailed(Type type) : LocalizedResult<Type>(false, Keys.ParseFailedResult, type);

public class QuoteMismatch() : LocalizedResult(false, Keys.QuoteMismatchResult);

public class Success() : Result(true, string.Empty);

public class TimedOut() : LocalizedResult(false, Keys.TimedOutResult);

public class TooManyArgs() : LocalizedResult(false, Keys.TooManyArgsResult);

public class TooManyMatches(Type type) : LocalizedResult<Type>(false, Keys.TooManyMatchesResult, type);