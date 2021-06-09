using System;
using System.Collections.Generic;

namespace YACCS.Preconditions
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public abstract class GroupablePreconditionAttribute : Attribute, IGroupablePrecondition
	{
		public string[] Groups { get; set; } = Array.Empty<string>();
		public BoolOp Op { get; set; } = BoolOp.And;

		IReadOnlyList<string> IGroupablePrecondition.Groups => Groups;
	}
}