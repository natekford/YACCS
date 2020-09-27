using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Name : IName
	{
		public IReadOnlyList<string> Parts { get; }
		private string DebuggerDisplay => $"Name = {ToString()}, Count = {Parts.Count}";

		public Name(IEnumerable<string> parts)
		{
			Parts = parts.ToImmutableArray();
		}

		public override string ToString()
			=> string.Join(' ', Parts);
	}
}