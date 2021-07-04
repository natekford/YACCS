using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay(CommandServiceUtils.DEBUGGER_DISPLAY)]
	public class ImmutableName : IReadOnlyList<string>
	{
		private readonly ImmutableArray<string> _Parts;
		private string? _Joined;
		public int Count => _Parts.Length;
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {_Parts.Length}";

		public string this[int index] => _Parts[index];

		public ImmutableName(IEnumerable<string> parts)
		{
			if (parts is ImmutableName name)
			{
				_Parts = name._Parts;
			}
			else
			{
				_Parts = parts.ToImmutableArray();
			}
		}

		public IEnumerator<string> GetEnumerator()
			=> ((IEnumerable<string>)_Parts).GetEnumerator();

		public override string ToString()
			=> _Joined ??= string.Join(CommandServiceUtils.InternallyUsedSeparator, _Parts);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}