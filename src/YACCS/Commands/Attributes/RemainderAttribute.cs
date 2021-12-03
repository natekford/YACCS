namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute indicating the parameter has an unlimited <see cref="LengthAttribute.Length"/>.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public class RemainderAttribute : LengthAttribute
{
	/// <summary>
	/// Creates a new <see cref="RemainderAttribute"/> and sets <see cref="LengthAttribute.Length"/>
	/// to <see langword="null"/>.
	/// </summary>
	public RemainderAttribute() : base(null)
	{
	}
}
