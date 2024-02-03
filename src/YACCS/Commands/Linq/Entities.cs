using YACCS.Commands.Attributes;
using YACCS.Commands.Models;

namespace YACCS.Commands.Linq;

/// <summary>
/// Static methods for querying and modifying <see cref="IMutableEntity"/>.
/// </summary>
public static class Entities
{
	/// <summary>
	/// Adds an attribute to <paramref name="entity"/>.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <param name="entity">The entity to modify.</param>
	/// <param name="attribute">The attribute to add.</param>
	/// <returns><paramref name="entity"/> after it has been modified.</returns>
	public static TEntity AddAttribute<TEntity>(this TEntity entity, object attribute)
		where TEntity : IMutableEntity
	{
		entity.Attributes.Add(attribute);
		return entity;
	}

	/// <summary>
	/// Filters <paramref name="entities"/> by attributes that match
	/// <paramref name="predicate"/>.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <typeparam name="TAttribute"></typeparam>
	/// <param name="entities">The entities to query.</param>
	/// <param name="predicate">The predicate to query with.</param>
	/// <returns>
	/// An enumerable of entities that have at least one attribute which satisfies
	/// <paramref name="predicate"/>.
	/// </returns>
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

	/// <summary>
	/// Filters <paramref name="entities"/> by id.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="entities">The entities to query.</param>
	/// <param name="id">The id to search for.</param>
	/// <returns>An enumerable of entities that have the id.</returns>
	public static IEnumerable<T> ById<T>(this IEnumerable<T> entities, string id)
		where T : IQueryableEntity
		=> entities.ByAttribute((IIdAttribute x) => x.Id == id);

	/// <summary>
	/// Gets all attributes of type <typeparamref name="T"/> from <paramref name="entity"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="entity">The entity to query the attributes from.</param>
	/// <returns>An enumerable of attributes of type <typeparamref name="T"/>.</returns>
	public static IEnumerable<T> GetAttributes<T>(this IQueryableEntity entity)
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