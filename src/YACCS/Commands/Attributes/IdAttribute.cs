using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes;

/// <inheritdoc cref="IIdAttribute"/>
/// <summary>
/// Creates a new <see cref="IdAttribute"/>.
/// </summary>
/// <param name="id">
/// <inheritdoc cref="Id" path="/summary"/>
/// </param>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public class IdAttribute(string id)
	: Attribute, IIdAttribute, IRuntimeFormattableAttribute
{
	/// <inheritdoc />
	public virtual string Id { get; } = id;

	/// <inheritdoc />
	public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
		=> new(formatProvider.Format($"{Keys.Id:key} {Id:value}"));
}