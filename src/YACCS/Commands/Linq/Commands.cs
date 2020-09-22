using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands.Linq
{
	public static class Commands
	{
		public static IEnumerable<IQueryableCommand> ByDelegate(
			this IEnumerable<IQueryableCommand> commands,
			Delegate @delegate)
			=> commands.ByDelegate(@delegate, false);

		public static IEnumerable<IQueryableCommand> ByDelegate(
			this IEnumerable<IQueryableCommand> commands,
			Delegate @delegate,
			bool includeMethod)
		{
			foreach (var command in commands)
			{
				foreach (var attribute in command.Attributes)
				{
					if (attribute is DelegateCommandAttribute d && d.Delegate == @delegate)
					{
						yield return command;
					}
					else if (includeMethod && attribute is MethodInfoCommandAttribute m && m.Method == @delegate.Method)
					{
						yield return command;
					}
				}
			}
		}

		public static IEnumerable<IQueryableEntity> ById(
			this IEnumerable<IQueryableEntity> commands,
			string id)
			=> commands.ByAttribute((IIdAttribute x) => x.Id == id);

		public static IEnumerable<IQueryableCommand> ByLastPartOfName(
			this IEnumerable<IQueryableCommand> commands,
			string name)
		{
			return commands.Where(x => x.Names.Any(n =>
			{
				const StringComparison COMPARISON = StringComparison.OrdinalIgnoreCase;
				return n.Parts[^1].Equals(name, COMPARISON);
			}));
		}

		public static IEnumerable<IQueryableCommand> ByMethod(
			this IEnumerable<IQueryableCommand> commands,
			MethodInfo method)
			=> commands.ByAttribute((MethodInfoCommandAttribute x) => x.Method == method);

		public static IEnumerable<IQueryableCommand> ByName(
			this IEnumerable<IQueryableCommand> commands,
			IEnumerable<string> parts)
		{
			var name = new Name(parts);
			return commands.Where(x => x.Names.Any(n => n == name));
		}

		private static IEnumerable<TEntity> ByAttribute<TEntity, TAttribute>(
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
	}
}