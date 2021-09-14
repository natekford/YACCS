#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.Collections.Immutable;
using System.Reflection;

namespace YACCS.Localization
{
	public static class Keys
	{
		public static ImmutableArray<NeedsLocalization> AllKeys { get; }
			= typeof(Keys).GetProperties(BindingFlags.Public | BindingFlags.Static)
			.Where(x => x.PropertyType == typeof(NeedsLocalization))
			.Select(x => (NeedsLocalization)x.GetValue(null))
			.ToImmutableArray();

		#region Words
		public static NeedsLocalization And { get; } = new(nameof(And));
		public static NeedsLocalization Attributes { get; } = new(nameof(Attributes));
		public static NeedsLocalization Id { get; } = new(nameof(Id));
		public static NeedsLocalization Length { get; } = new(nameof(Length));
		public static NeedsLocalization Names { get; } = new(nameof(Names));
		public static NeedsLocalization Or { get; } = new(nameof(Or));
		public static NeedsLocalization Parameters { get; } = new(nameof(Parameters));
		public static NeedsLocalization Preconditions { get; } = new(nameof(Preconditions));
		public static NeedsLocalization Priority { get; } = new(nameof(Priority));
		public static NeedsLocalization Remainder { get; } = new(nameof(Remainder));
		public static NeedsLocalization Summary { get; } = new(nameof(Summary));
		#endregion Words

		#region Nulls
		public static NeedsLocalization Nil { get; } = new(nameof(Nil));
		public static NeedsLocalization Nothing { get; } = new(nameof(Nothing));
		public static NeedsLocalization Null { get; } = new(nameof(Null));
		public static NeedsLocalization NullPtr { get; } = new(nameof(NullPtr));
		public static NeedsLocalization Void { get; } = new(nameof(Void));
		#endregion Nulls

		#region Types
		public static NeedsLocalization BoolType { get; }
			= new(nameof(BoolType), "true or false");
		public static NeedsLocalization ByteType { get; }
			= new(nameof(ByteType), $"integer ({byte.MinValue} to {byte.MaxValue})");
		public static NeedsLocalization CharType { get; }
			= new(nameof(CharType), "char");
		public static NeedsLocalization DateTimeType { get; }
			= new(nameof(DateTimeType), "date");
		public static NeedsLocalization DecimalType { get; }
			= new(nameof(DecimalType), $"number ({decimal.MinValue} to {decimal.MaxValue})");
		public static NeedsLocalization DoubleType { get; }
			= new(nameof(DoubleType), $"number ({double.MinValue} to {double.MaxValue})");
		public static NeedsLocalization FloatType { get; }
			= new(nameof(FloatType), $"number ({float.MinValue} to {float.MaxValue})");
		public static NeedsLocalization IntType { get; }
			= new(nameof(IntType), $"integer ({int.MinValue} to {int.MaxValue})");
		public static NeedsLocalization LongType { get; }
			= new(nameof(LongType), $"integer ({long.MinValue} to {long.MaxValue})");
		public static NeedsLocalization SByteType { get; }
			= new(nameof(SByteType), $"integer ({sbyte.MinValue} to {sbyte.MaxValue})");
		public static NeedsLocalization ShortType { get; }
			= new(nameof(ShortType), $"integer ({short.MinValue} to {short.MaxValue})");
		public static NeedsLocalization StringType { get; }
			= new(nameof(StringType), "text");
		public static NeedsLocalization TimeSpanType { get; }
			= new(nameof(TimeSpanType), "time");
		public static NeedsLocalization UIntType { get; }
			= new(nameof(UIntType), $"integer ({uint.MinValue} to {uint.MaxValue})");
		public static NeedsLocalization ULongType { get; }
			= new(nameof(ULongType), $"integer ({ulong.MinValue} to {ulong.MaxValue})");
		public static NeedsLocalization UriType { get; }
			= new(nameof(UriType), "url");
		public static NeedsLocalization UShortType { get; }
			= new(nameof(UShortType), $"integer ({ushort.MinValue} to {ushort.MaxValue})");
		#endregion Types

		#region Results
		public static NeedsLocalization CanceledResult { get; }
			= new(nameof(CanceledResult), "An operation was canceled.");
		public static NeedsLocalization CommandNotFoundResult { get; }
			= new(nameof(CommandNotFoundResult), "Unable to find a matching command.");
		public static NeedsLocalization ExceptionAfterCommandResult { get; }
			= new(nameof(ExceptionAfterCommandResult), "An exception occurred after a command was executed.");
		public static NeedsLocalization ExceptionDuringCommandResult { get; }
			= new(nameof(ExceptionDuringCommandResult), "An exception occurred while a command was executing.");
		public static NeedsLocalization InteractionEndedResult { get; }
			= new(nameof(InteractionEndedResult), "Interaction ended.");
		public static NeedsLocalization InUseMustBeInUse { get; }
			= new(nameof(InUseMustBeInUse), "Unable to find an existing {0} matching the supplied value.");
		public static NeedsLocalization InUseMustNotBeInUse { get; }
			= new(nameof(InUseMustNotBeInUse), "There is already an existing {0} matching the supplied value.");
		public static NeedsLocalization InvalidContextResult { get; }
			= new(nameof(InvalidContextResult), "Invalid context type.");
		public static NeedsLocalization InvalidParameterResult { get; }
			= new(nameof(InvalidParameterResult), "Invalid parameter type.");
		public static NeedsLocalization MultiMatchHandlingErrorResult { get; }
			= new(nameof(MultiMatchHandlingErrorResult), "Multiple commands match.");
		public static NeedsLocalization NamedArgBadCountResult { get; }
			= new(nameof(NamedArgBadCountResult), "There is not an even number of arguments supplied.");
		public static NeedsLocalization NamedArgDuplicateResult { get; }
			= new(nameof(NamedArgDuplicateResult), "Duplicate value for named argument {0}.");
		public static NeedsLocalization NamedArgNonExistentResult { get; }
			= new(nameof(NamedArgNonExistentResult), "Nonexistent named argument {0}.");
		public static NeedsLocalization NotEnoughArgsResult { get; }
			= new(nameof(NotEnoughArgsResult), "Not enough arguments provided.");
		public static NeedsLocalization NullParameterResult { get; }
			= new(nameof(NullParameterResult), "Parameter is null.");
		public static NeedsLocalization ParseFailedResult { get; }
			= new(nameof(ParseFailedResult), "Failed to parse {0}.");
		public static NeedsLocalization QuoteMismatchResult { get; }
			= new(nameof(QuoteMismatchResult), "There is a quote mismatch.");
		public static NeedsLocalization StructuredArgMissingValueResult { get; }
			= new(nameof(StructuredArgMissingValueResult), "Missing a value for argument {0}.");
		public static NeedsLocalization TimedOutResult { get; }
			= new(nameof(TimedOutResult), "An operation timed out.");
		public static NeedsLocalization TooManyArgsResult { get; }
			= new(nameof(TooManyArgsResult), "Too many arguments provided.");
		#endregion Results

		#region Formats
		public static NeedsLocalization ListNameFormat { get; }
			= new(nameof(ListNameFormat), "{0} list");
		public static NeedsLocalization NullableNameFormat { get; }
			= new(nameof(NullableNameFormat), "{0} or null");
		#endregion Formats
	}
}