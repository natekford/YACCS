using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Name : IReadOnlyList<string>
	{
		private readonly ImmutableArray<string> _Parts;
		private string? _Joined;
		public int Count => _Parts.Length;
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {_Parts.Length}";

		public string this[int index] => _Parts[index];

		public Name(IEnumerable<string> parts)
		{
			_Parts = parts.ToImmutableArray();
		}

		public IEnumerator<string> GetEnumerator()
			=> ((IEnumerable<string>)_Parts).GetEnumerator();

		public override string ToString()
			=> _Joined ??= string.Join(CommandServiceUtils.InternallyUsedSeparator, _Parts);

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}