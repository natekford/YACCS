using YACCS.Commands.Models;

namespace YACCS.Commands.Attributes;

/// <summary>
/// An attribute indicating that an <see cref="IImmutableCommand"/> should not
/// be shown in help commands.
/// This does not prevent the <see cref="IImmutableCommand"/> from being executed.
/// </summary>
public interface IHiddenAttribute
{
}
