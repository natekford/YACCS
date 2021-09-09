using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Commands.Attributes;

namespace YACCS.Localization
{
	/// <inheritdoc />
	[AttributeUsage(AttributeUtils.COMMANDS, AllowMultiple = false, Inherited = true)]
	public class LocalizedCommandAttribute : CommandAttribute
	{
		/// <summary>
		/// The keys for localization.
		/// </summary>
		public IReadOnlyList<string> Keys { get; }
		/// <inheritdoc />
		public override IReadOnlyList<string> Names
		{
			get
			{
				if (Localize.Instance.IsEmpty)
				{
					return base.Names;
				}

				var names = ImmutableArray.CreateBuilder<string>(Keys.Count);
				for (var i = 0; i < names.Count; ++i)
				{
					names.Add(Localize.This(Keys[i]));
				}
				return names.MoveToImmutable();
			}
		}

		/// <inheritdoc cref="LocalizedCommandAttribute(IReadOnlyList{string})"/>
		public LocalizedCommandAttribute(params string[] keys) : this(keys.ToImmutableArray())
		{
		}

		/// <summary>
		/// Creates a new <see cref="LocalizedCommandAttribute"/>.
		/// </summary>
		/// <param name="keys">
		/// <inheritdoc cref="Keys" path="/summary"/>
		/// </param>
		public LocalizedCommandAttribute(IReadOnlyList<string> keys) : base(keys)
		{
			Keys = keys;
		}

		/// <inheritdoc cref="LocalizedCommandAttribute(IReadOnlyList{string})"/>
		public LocalizedCommandAttribute() : this(ImmutableArray<string>.Empty)
		{
		}
	}
}