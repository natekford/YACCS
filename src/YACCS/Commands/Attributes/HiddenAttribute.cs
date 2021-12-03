namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="IHiddenAttribute" />
[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = false)]
public class HiddenAttribute : Attribute, IHiddenAttribute
{
}
