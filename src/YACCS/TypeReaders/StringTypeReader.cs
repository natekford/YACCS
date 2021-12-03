
using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Parsing;

namespace YACCS.TypeReaders;

/// <summary>
/// Parses a string.
/// </summary>
public class StringTypeReader : TypeReader<string>
{
	/// <inheritdoc />
	public override ITask<ITypeReaderResult<string>> ReadAsync(
		IContext context,
		ReadOnlyMemory<string> input)
	{
		var handler = GetHandler(context.Services);

		return Success(handler.Join(input)).AsITask();
	}

	[GetServiceMethod]
	private static IArgumentHandler GetHandler(IServiceProvider services)
		=> services.GetRequiredService<IArgumentHandler>();
}
