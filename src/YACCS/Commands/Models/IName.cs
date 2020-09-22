using System;
using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IName : IComparable, IComparable<IName>, IEquatable<IName>
	{
		public IReadOnlyList<string> Parts { get; }
	}
}