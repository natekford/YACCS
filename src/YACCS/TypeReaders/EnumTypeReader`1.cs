using System;

namespace YACCS.TypeReaders
{
	/// <inheritdoc cref="Enum.TryParse{TEnum}(string, bool, out TEnum)"/>
	public delegate bool EnumDelegate<TEnum>(
		string input,
		bool ignoreCase,
		out TEnum result)
		where TEnum : struct, Enum;

	/// <summary>
	/// Parses a <typeparamref name="TEnum"/> via <see cref="EnumDelegate{TEnum}"/>.
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	public class EnumTypeReader<TEnum> : TryParseTypeReader<TEnum> where TEnum : struct, Enum
	{
		/// <inheritdoc cref="EnumTypeReader{TEnum}(EnumDelegate{TEnum})"/>
		public EnumTypeReader() : this(Enum.TryParse)
		{
		}

		/// <summary>
		/// Creates a new <see cref="EnumTypeReader{T}"/>.
		/// </summary>
		/// <param name="delegate">The delegate to use when parsing.</param>
		public EnumTypeReader(EnumDelegate<TEnum> @delegate) : base(Convert(@delegate))
		{
		}

		private static TryParseDelegate<TEnum> Convert(EnumDelegate<TEnum> @delegate)
		{
			return (string input, out TEnum result) =>
			{
				const bool IGNORE_CASE = true;
				return @delegate(input, IGNORE_CASE, out result);
			};
		}
	}
}