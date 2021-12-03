namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute indicating the parameter is <see cref="IContext"/> and should not
/// be parsed from the input.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public class ContextAttribute : LengthAttribute
{
	/// <summary>
	/// Creates a new <see cref="ContextAttribute"/> and sets <see cref="LengthAttribute.Length"/>
	/// to 0.
	/// </summary>
	public ContextAttribute() : base(0)
	{
	}
}
