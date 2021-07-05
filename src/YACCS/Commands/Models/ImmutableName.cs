using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Localization;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class ImmutableName : IReadOnlyList<string>, IUsesLocalizer
	{
		private readonly ImmutableArray<string> _Keys;
		public int Count => _Keys.Length;
		public virtual ILocalizer? Localizer { get; set; }
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {_Keys.Length}";

		public string this[int index]
		{
			get
			{
				var key = _Keys[index];
				return Localizer?.Get(key) ?? key;
			}
		}

		public ImmutableName(IEnumerable<string> keys)
		{
			if (keys is ImmutableName name)
			{
				_Keys = name._Keys;
			}
			else
			{
				_Keys = keys.ToImmutableArray();
			}
		}

		public IEnumerator<string> GetEnumerator()
			=> ((IEnumerable<string>)_Keys).GetEnumerator();

		public override string ToString()
			=> string.Join(CommandServiceUtils.SPACE, this);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}