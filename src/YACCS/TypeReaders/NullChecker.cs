using System;
using System.Collections.Generic;
using System.Globalization;

using YACCS.Localization;

namespace YACCS.TypeReaders
{
	public class NullChecker : INullChecker
	{
		protected virtual Localized<ISet<string>> Localized { get; } = new(CreateSet);

		protected ISet<string> Values => Localized.GetCurrent();

		public NullChecker(IEnumerable<string> values)
		{
			foreach (var value in values)
			{
				Values.Add(value);
			}
		}

		public NullChecker()
		{
		}

		public bool IsNull(string? value)
			=> value is null || Values.Contains(value);

		protected static ISet<string> CreateSet(CultureInfo _)
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