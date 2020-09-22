using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

using QCommands = System.Collections.Generic.IEnumerable<YACCS.Commands.Models.IQueryableCommand>;
using QEntities = System.Collections.Generic.IEnumerable<YACCS.Commands.Models.IQueryableEntity>;

namespace YACCS.Commands.Linq
{
	public static class Commands
	{
		public static IEnumerable<TEntity> ByAttribute<TEntity, TAttribute>(
			this IEnumerable<TEntity> entities,
			Func<TAttribute, bool> predicate)
			where TEntity : IQueryableEntity
		{
			foreach (var command in entities)
			{
				foreach (var attribute in command.Attributes)
				{
					if (attribute is TAttribute t && predicate(t))
					{
						yield return command;
					}
				}
			}
		}

		public static QCommands ByDelegate(this QCommands commands, Delegate @delegate)
			=> commands.ByDelegate(@delegate, false);

		public static QCommands ByDelegate(this QCommands commands, Delegate @delegate, bool includeMethod)
		{
			var d = commands.ByAttribute((DelegateCommandAttribute d) => d.Delegate == @delegate);
			if (!includeMethod)
			{
				return d;
			}
			return d.Union(commands.ByMethod(@delegate.Method));
		}

		public static QEntities ById(this QEntities commands, string id)
			=> commands.ByAttribute((IIdAttribute x) => x.Id == id);

		public static QCommands ByLastPartOfName(this QCommands commands, string name)
		{
			return commands.Where(x => x.Names.Any(n =>
			{
				const StringComparison COMPARISON = StringComparison.OrdinalIgnoreCase;
				return n.Parts[^1].Equals(name, COMPARISON);
			}));
		}

		public static QCommands ByMethod(this QCommands commands, MethodInfo method)
			=> commands.ByAttribute((MethodInfoCommandAttribute x) => x.Method == method);

		public static QCommands ByName(this QCommands commands, IEnumerable<string> parts)
		{
			var name = new Name(parts);
			return commands.Where(x => x.Names.Any(n => name == n));
		}
	}
}