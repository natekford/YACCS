using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="ILengthAttribute" />
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public class LengthAttribute : Attribute, ILengthAttribute, IRuntimeFormattableAttribute
{
	/// <inheritdoc />
	public int? Length { get; }

	/// <summary>
	/// Creates a new <see cref="LengthAttribute"/>
	/// with <see cref="Length"/> set to <see langword="null"/>.
	/// </summary>
	public LengthAttribute()
	{
		Length = null;
	}

	/// <inheritdoc cref="LengthAttribute(int?)"/>
	public LengthAttribute(int length) : this((int?)length)
	{
	}

	/// <summary>
	/// Creates a new <see cref="LengthAttribute"/>
	/// with <see cref="Length"/> set to <paramref name="length"/>.
	/// </summary>
	/// <param name="length">
	/// <inheritdoc cref="Length" path="/summary"/>
	/// </param>
	public LengthAttribute(int? length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(length));
		}
		Length = length;
	}

	/// <inheritdoc />
	public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
	{
		var value = Length ?? (object?)Keys.Remainder;
		return new(formatProvider.Format($"{Keys.Length:key} {value:value}"));
	}
}