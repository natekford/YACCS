using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands.Linq
{
	public static class Entities
	{
		public static TEntity AddAttribute<TEntity>(this TEntity entity, object attribute)
			where TEntity : IEntityBase
		{
			entity.Attributes.Add(attribute);
			return entity;
		}

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
						// Break after returning once to only return one match for each command
						break;
					}
				}
			}
		}

		public static IEnumerable<T> ByDelegate<T>(this IEnumerable<T> commands, Delegate @delegate, bool includeMethod = false)
			where T : IQueryableCommand
		{
			var delegates = commands.ByAttribute((DelegateCommandAttribute x) => x.Delegate == @delegate);
			if (!includeMethod)
			{
				return delegates;
			}
			return delegates.Union(commands.ByMethod(@delegate.Method));
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

		public static IEnumerable<T> ByName<T>(this IEnumerable<T> commands, IReadOnlyList<string> parts)
			where T : IQueryableCommand
			=> commands.ByName(parts, StringComparison.CurrentCulture);

		public static IEnumerable<T> ByName<T>(this IEnumerable<T> commands, IReadOnlyList<string> parts, StringComparison comparisonType)
			where T : IQueryableCommand
		{
			foreach (var command in commands)
			{
				foreach (var name in command.Names)
				{
					if (name.Parts.Count != parts.Count)
					{
						break;
					}

					var isMatch = true;
					for (var i = 0; i < parts.Count; ++i)
					{
						if (!name.Parts[i].Equals(parts[i], comparisonType))
						{
							isMatch = false;
							break;
						}
					}

					if (isMatch)
					{
						yield return command;
						// Break after returning once to only return one match for each command
						break;
					}
				}
			}
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