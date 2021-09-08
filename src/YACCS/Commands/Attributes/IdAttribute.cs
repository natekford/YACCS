using System;
using System.Threading.Tasks;

using YACCS.Help;
using YACCS.Help.Attributes;
using YACCS.Localization;

namespace YACCS.Commands.Attributes
{
	/// <inheritdoc cref="IIdAttribute"/>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public class IdAttribute : Attribute, IIdAttribute, IRuntimeFormattableAttribute
	{
		/// <inheritdoc />
		public virtual string Id { get; }

		/// <summary>
		/// Creates a new <see cref="IdAttribute"/> and sets <see cref="Id"/>
		/// to <paramref name="id"/>.
		/// </summary>
		/// <param name="id"></param>
		public IdAttribute(string id)
		{
			Id = id;
		}

		/// <inheritdoc />
		public virtual ValueTask<string> FormatAsync(IContext context, IFormatProvider? formatProvider = null)
			=> new(formatProvider.Format($"{Keys.Id:key} {Id:value}"));
	}
}