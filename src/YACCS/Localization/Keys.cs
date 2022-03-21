#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace YACCS.Localization;

public static class Keys
{
	public static ImmutableArray<NeedsLocalization> AllKeys { get; }
		= typeof(Keys).GetProperties(BindingFlags.Public | BindingFlags.Static)
		.Where(x => x.PropertyType == typeof(NeedsLocalization))
		.Select(x => (NeedsLocalization)x.GetValue(null))
		.ToImmutableArray();

	#region Words
	public static NeedsLocalization And { get; } = Create();
	public static NeedsLocalization Attributes { get; } = Create();
	public static NeedsLocalization Id { get; } = Create();
	public static NeedsLocalization Length { get; } = Create();
	public static NeedsLocalization Names { get; } = Create();
	public static NeedsLocalization Or { get; } = Create();
	public static NeedsLocalization Parameters { get; } = Create();
	public static NeedsLocalization Preconditions { get; } = Create();
	public static NeedsLocalization Priority { get; } = Create();
	public static NeedsLocalization Remainder { get; } = Create();
	public static NeedsLocalization Summary { get; } = Create();
	#endregion Words

	#region Nulls
	public static NeedsLocalization Nil { get; } = Create();
	public static NeedsLocalization Nothing { get; } = Create();
	public static NeedsLocalization Null { get; } = Create();
	public static NeedsLocalization NullPtr { get; } = Create();
	public static NeedsLocalization Void { get; } = Create();
	#endregion Nulls

	#region Types
	public static NeedsLocalization BoolType { get; }
		= Create("true or false");
	public static NeedsLocalization ByteType { get; }
		= Create($"integer ({byte.MinValue} to {byte.MaxValue})");
	public static NeedsLocalization CharType { get; }
		= Create("char");
	public static NeedsLocalization DateTimeType { get; }
		= Create("date");
	public static NeedsLocalization DecimalType { get; }
		= Create($"number ({decimal.MinValue} to {decimal.MaxValue})");
	public static NeedsLocalization DoubleType { get; }
		= Create($"number ({double.MinValue} to {double.MaxValue})");
	public static NeedsLocalization FloatType { get; }
		= Create($"number ({float.MinValue} to {float.MaxValue})");
	public static NeedsLocalization IntType { get; }
		= Create($"integer ({int.MinValue} to {int.MaxValue})");
	public static NeedsLocalization LongType { get; }
		= Create($"integer ({long.MinValue} to {long.MaxValue})");
	public static NeedsLocalization SByteType { get; }
		= Create($"integer ({sbyte.MinValue} to {sbyte.MaxValue})");
	public static NeedsLocalization ShortType { get; }
		= Create($"integer ({short.MinValue} to {short.MaxValue})");
	public static NeedsLocalization StringType { get; }
		= Create("text");
	public static NeedsLocalization TimeSpanType { get; }
		= Create("time");
	public static NeedsLocalization UIntType { get; }
		= Create($"integer ({uint.MinValue} to {uint.MaxValue})");
	public static NeedsLocalization ULongType { get; }
		= Create($"integer ({ulong.MinValue} to {ulong.MaxValue})");
	public static NeedsLocalization UriType { get; }
		= Create("url");
	public static NeedsLocalization UShortType { get; }
		= Create($"integer ({ushort.MinValue} to {ushort.MaxValue})");
	#endregion Types

	#region Results
	public static NeedsLocalization CanceledResult { get; }
		= Create("An operation was canceled.");
	public static NeedsLocalization CommandNotFoundResult { get; }
		= Create("Unable to find a matching command.");
	public static NeedsLocalization ExceptionAfterCommandResult { get; }
		= Create("An exception occurred after a command was executed.");
	public static NeedsLocalization ExceptionDuringCommandResult { get; }
		= Create("An exception occurred while a command was executing.");
	public static NeedsLocalization InteractionEndedResult { get; }
		= Create("Interaction ended.");
	public static NeedsLocalization MustBeGreaterThan { get; }
		= Create("Must be greater than or equal to {0}.");
	public static NeedsLocalization MustBeLessThan { get; }
		= Create("Must be less than or equal to {0}.");
	public static NeedsLocalization MustBeLocked { get; }
		= Create("Unable to find an existing {0} matching the supplied value.");
	public static NeedsLocalization MustBeUnlocked { get; }
		= Create("There is already an existing {0} matching the supplied value.");
	public static NeedsLocalization InvalidContextResult { get; }
		= Create("Invalid context type.");
	public static NeedsLocalization InvalidParameterResult { get; }
		= Create("Invalid parameter type.");
	public static NeedsLocalization MultiMatchHandlingErrorResult { get; }
		= Create("Multiple commands match.");
	public static NeedsLocalization NamedArgBadCountResult { get; }
		= Create("There is not an even number of arguments supplied.");
	public static NeedsLocalization NamedArgDuplicateResult { get; }
		= Create("Duplicate value for named argument {0}.");
	public static NeedsLocalization NamedArgInvalidDictionaryResult { get; }
		= Create("Invalid dictionary supplied.");
	public static NeedsLocalization NamedArgNonExistentResult { get; }
		= Create("Nonexistent named argument {0}.");
	public static NeedsLocalization NotEnoughArgsResult { get; }
		= Create("Not enough arguments provided.");
	public static NeedsLocalization NullParameterResult { get; }
		= Create("Parameter is null.");
	public static NeedsLocalization ParseFailedResult { get; }
		= Create("Failed to parse {0}.");
	public static NeedsLocalization QuoteMismatchResult { get; }
		= Create("There is a quote mismatch.");
	public static NeedsLocalization NamedArgMissingValueResult { get; }
		= Create("Missing a value for argument {0}.");
	public static NeedsLocalization TimedOutResult { get; }
		= Create("An operation timed out.");
	public static NeedsLocalization TooManyArgsResult { get; }
		= Create("Too many arguments provided.");
	#endregion Results

	#region Formats
	public static NeedsLocalization ListNameFormat { get; }
		= Create("{0} list");
	public static NeedsLocalization NullableNameFormat { get; }
		= Create("{0} or null");
	#endregion Formats

	private static NeedsLocalization Create(
		string? fallback = null,
		[CallerMemberName] string key = "")
		=> new(key, fallback);
}