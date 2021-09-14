
using YACCS.Commands.Models;

namespace YACCS.NamedArguments
{
	/// <summary>
	/// A generated command where arguments must be passed with names.
	/// </summary>
	public sealed class NamedArgumentsCommand : NamedArgumentsCommand<Dictionary<string, object?>>
	{
		/// <inheritdoc cref="GeneratedCommand(IImmutableCommand)"/>
		public NamedArgumentsCommand(IImmutableCommand source) : base(source)
		{
		}
	}
}