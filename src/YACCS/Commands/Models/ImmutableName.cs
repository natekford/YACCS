using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using YACCS.Localization;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class ImmutableName : IReadOnlyList<string>
	{
		private readonly ImmutableArray<string> _Keys;
		public int Count => _Keys.Length;
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {_Keys.Length}";

		public string this[int index] => Localize.This(_Keys[index]);

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
		{
			foreach (var key in _Keys)
			{
				yield return Localize.This(key);
			}
		}

		public override string ToString()
			=> string.Join(CommandServiceUtils.SPACE, this);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}