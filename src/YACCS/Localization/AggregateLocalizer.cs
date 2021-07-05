using System;
using System.Collections.Generic;
using System.Globalization;

namespace YACCS.Localization
{
	public class AggregateLocalizer : ILocalizer
	{
		private readonly List<ILocalizer> _NestedLocalizers = new();

		public bool IsEmpty => _NestedLocalizers.Count == 0;

		public event Action<string, CultureInfo>? KeyNotFound;

		public void Append(ILocalizer localizer)
		{
			lock (_NestedLocalizers)
			{
				_NestedLocalizers.Add(localizer);
			}
		}

		public string? Get(string key, CultureInfo? culture = null)
		{
			culture ??= CultureInfo.CurrentUICulture;

			lock (_NestedLocalizers)
			{
				foreach (var localizer in _NestedLocalizers)
				{
					if (localizer.Get(key, culture) is string s)
					{
						return s;
					}
				}
			}

			KeyNotFound?.Invoke(key, culture);
			return null;
		}

		public void Prepend(ILocalizer localizer)
		{
			lock (_NestedLocalizers)
			{
				_NestedLocalizers.Insert(0, localizer);
			}
		}
	}
}