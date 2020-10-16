using System;
using System.Collections.Generic;

namespace YACCS.TypeReaders
{
	public class NullChecker : INullChecker
	{
		public HashSet<string> Values { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"nullptr",
			"null",
			"nil",
			"void",
			"nothing",
		};

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

		public bool IsNull(string value)
			=> value is null || Values.Contains(value);
	}
}