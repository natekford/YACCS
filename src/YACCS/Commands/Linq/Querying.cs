using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands.Linq
{
	public static class Querying
	{
		public static IEnumerable<TEntity> ByAttribute<TEntity, TAttribute>(
			this IEnumerable<TEntity> entities,
			Func<TAttribute, bool> predicate)
			where TEntity : IQueryableEntity
		{
			foreach (var entity in entities)
			{
				foreach (var attribute in entity.Attributes)
				{
					if (attribute is TAttribute t && predicate(t))
					{
						yield return entity;
						// Break after returning once
						// Otherwise if attributes get modified we get an exception
						break;
					}
				}
			}
		}

		public static IEnumerable<T> ByDelegate<T>(this IEnumerable<T> commands, Delegate @delegate)
			where T : IQueryableCommand
			=> commands.ByDelegate(@delegate, false);

		public static IEnumerable<T> ByDelegate<T>(this IEnumerable<T> commands, Delegate @delegate, bool includeMethod)
			where T : IQueryableCommand
		{
			var d = commands.ByAttribute((DelegateCommandAttribute d) => d.Delegate == @delegate);
			if (!includeMethod)
			{
				return d;
			}
			return d.Union(commands.ByMethod(@delegate.Method));
		}

		public static IEnumerable<T> ById<T>(this IEnumerable<T> commands, string id)
			where T : IQueryableEntity
			=> commands.ByAttribute((IIdAttribute x) => x.Id == id);

		public static IEnumerable<T> ByLastPartOfName<T>(this IEnumerable<T> commands, string name)
			where T : IQueryableCommand
		{
			return commands.Where(x => x.Names.Any(n =>
			{
				const StringComparison COMPARISON = StringComparison.OrdinalIgnoreCase;
				return n.Parts[^1].Equals(name, COMPARISON);
			}));
		}

		public static IEnumerable<T> ByMethod<T>(this IEnumerable<T> commands, MethodInfo method)
			where T : IQueryableCommand
			=> commands.ByAttribute((MethodInfoCommandAttribute x) => x.Method == method);

		public static IEnumerable<T> ByName<T>(this IEnumerable<T> commands, IEnumerable<string> parts)
			where T : IQueryableCommand
		{
			var name = new Name(parts);
			return commands.Where(x => x.Names.Any(n => name == n));
		}

		public static IEnumerable<T> Get<T>(this IQueryableEntity entity)
		{
			foreach (var attribute in entity.Attributes)
			{
				if (attribute is T t)
				{
					yield return t;
				}
			}
		}
	}
}