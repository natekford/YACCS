using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Localization;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// Determines if input represents null.
	/// </summary>
	public class NullValidator : INullValidator
	{
		/// <summary>
		/// The localized default values of null.
		/// </summary>
		protected virtual Localized<ISet<string>> Localized { get; } = new(_ => CreateSet());

		/// <summary>
		/// The additional values which represent null.
		/// </summary>
		protected IImmutableSet<string> Values { get; }

		/// <inheritdoc cref="NullValidator(IImmutableSet{string})"/>
		public NullValidator() : this(ImmutableHashSet<string>.Empty)
		{
		}

		/// <summary>
		/// Creates a new <see cref="NullValidator"/>.
		/// </summary>
		/// <param name="values">
		/// <inheritdoc cref="Values" path="/summary"/>
		/// </param>
		public NullValidator(IImmutableSet<string> values)
		{
			Values = values;
		}

		/// <inheritdoc />
		public virtual bool IsNull(ReadOnlyMemory<string?> input)
		{
			if (input.Length != 1)
			{
				return false;
			}

			var value = input.Span[0];
			return value is null || Values.Contains(value) || Localized.GetCurrent().Contains(value);
		}

		/// <summary>
		/// The default values which represent null.
		/// </summary>
		/// <returns>A set of strings representing null.</returns>
		protected static ISet<string> CreateSet()
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Keys.Nil,
				Keys.Nothing,
				Keys.Null,
				Keys.NullPtr,
				Keys.Void,
			};
		}
	}
}