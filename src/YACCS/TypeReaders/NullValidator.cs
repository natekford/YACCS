using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using YACCS.Localization;

namespace YACCS.TypeReaders
{
	public class NullValidator : INullValidator
	{
		protected virtual Localized<ISet<string>> Localized { get; } = new(_ => CreateSet());

		protected IImmutableSet<string> Values { get; }

		public NullValidator() : this(ImmutableHashSet<string>.Empty)
		{
		}

		public NullValidator(IImmutableSet<string> values)
		{
			Values = values;
		}

		public virtual bool IsNull(ReadOnlyMemory<string?> input)
		{
			if (input.Length != 1)
			{
				return false;
			}

			var value = input.Span[0];
			return value is null || Values.Contains(value) || Localized.GetCurrent().Contains(value);
		}

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