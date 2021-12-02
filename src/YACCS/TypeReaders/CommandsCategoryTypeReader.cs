using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Parses commands which have all the supplied categories.
	/// </summary>
	/// <remarks>Order is NOT guaranteed</remarks>
	public class CommandsCategoryTypeReader :
		TypeReader<IContext, IReadOnlyCollection<IImmutableCommand>>,
		IOverrideTypeReaderAttribute
	{
		ITypeReader IOverrideTypeReaderAttribute.Reader => this;

		/// <inheritdoc />
		public override ITask<ITypeReaderResult<IReadOnlyCollection<IImmutableCommand>>> ReadAsync(
			IContext context,
			ReadOnlyMemory<string> input)
		{
			if (input.Length == 0)
			{
				return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailed.Task;
			}

			var commands = GetCommands(context.Services);

			// Create a hashset to remove duplicates and have a quicker Contains()
			var categories = new HashSet<string>(input.Length, StringComparer.OrdinalIgnoreCase);
			foreach (var category in input.Span)
			{
				categories.Add(category);
			}

			var found = new List<IImmutableCommand>();
			var matchedCategories = new HashSet<string>(categories.Count, categories.Comparer);
			foreach (var command in commands.Commands)
			{
				foreach (var category in command.Attributes.OfType<ICategoryAttribute>())
				{
					if (categories.Contains(category.Category))
					{
						matchedCategories.Add(category.Category);
					}

					// An equal amount of categories found to categories searched for
					// means all have been found so we can stop looking
					if (matchedCategories.Count == categories.Count)
					{
						found.Add(command);
						break;
					}
				}
				matchedCategories.Clear();
			}

			if (found.Count == 0)
			{
				return CachedResults<IReadOnlyCollection<IImmutableCommand>>.ParseFailed.Task;
			}
			return Success(found).AsITask();
		}

		[GetServiceMethod]
		private static ICommandService GetCommands(IServiceProvider services)
			=> services.GetRequiredService<ICommandService>();
	}
}