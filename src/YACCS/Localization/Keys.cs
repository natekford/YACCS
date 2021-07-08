using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace YACCS.Localization
{
	public static class Keys
	{
		public static ImmutableArray<NeedsLocalization> AllKeys { get; }
			= typeof(Keys).GetProperties(BindingFlags.Static | BindingFlags.Public)
			.Where(x => x.PropertyType == typeof(NeedsLocalization))
			.Select(x => (NeedsLocalization)x.GetValue(null))
			.ToImmutableArray();

		#region Words
		public static NeedsLocalization Attributes { get; } = nameof(Attributes);
		public static NeedsLocalization Id { get; } = nameof(Id);
		public static NeedsLocalization Length { get; } = nameof(Length);
		public static NeedsLocalization Names { get; } = nameof(Names);
		public static NeedsLocalization Parameters { get; } = nameof(Parameters);
		public static NeedsLocalization Preconditions { get; } = nameof(Preconditions);
		public static NeedsLocalization Priority { get; } = nameof(Priority);
		public static NeedsLocalization Remainder { get; } = nameof(Remainder);
		public static NeedsLocalization Summary { get; } = nameof(Summary);
		#endregion Words

		#region Nulls
		public static NeedsLocalization Nil { get; } = nameof(Nil);
		public static NeedsLocalization Nothing { get; } = nameof(Nothing);
		public static NeedsLocalization Null { get; } = nameof(Null);
		public static NeedsLocalization NullPtr { get; } = nameof(NullPtr);
		public static NeedsLocalization Void { get; } = nameof(Void);
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

		#region Formats
		public static NeedsLocalization ListNameFormat { get; }
			= new(nameof(ListNameFormat), "{0} list");
		public static NeedsLocalization NullableNameFormat { get; }
			= new(nameof(NullableNameFormat), "{0} or null");
		#endregion Formats
	}
}