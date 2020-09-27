using System.Collections.Generic;

namespace YACCS.Commands.Models
{
	public interface IName
	{
		public IReadOnlyList<string> Parts { get; }
	}
}