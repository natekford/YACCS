using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
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

		public int CompareTo(object obj)
		{
			if (obj is null)
			{
				return 1;
			}
			if (obj is IName other)
			{
				return CompareTo(other);
			}
			throw new ArgumentException($"Object is not a {nameof(IName)}.");
		}

		public int CompareTo(IName other)
		{
			for (var i = 0; i < Parts.Count; ++i)
			{
				// Other has less parts, it occurs before this one
				if (i == other.Parts.Count)
				{
					return 1;
				}

				var part = string.Compare(Parts[i], other.Parts[i], StringComparison.OrdinalIgnoreCase);
				if (part != 0)
				{
					return part;
				}
			}

			return Parts.Count.CompareTo(other.Parts.Count);
		}

		public override bool Equals(object obj)
			=> Equals(obj as IName);

		public bool Equals(IName? other)
		{
			if (other is null)
			{
				return false;
			}
			if (Parts.Count != other.Parts.Count)
			{
				return false;
			}
			for (var i = 0; i < Parts.Count; ++i)
			{
				if (!Parts[i].Equals(other.Parts[i], StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();
			foreach (var part in Parts)
			{
				hash.Add(part, StringComparer.OrdinalIgnoreCase);
			}
			return hash.ToHashCode();
		}

		public override string ToString()
					=> string.Join(' ', Parts);
	}
}