﻿using System;
using System.Collections.Generic;

namespace YACCS.Preconditions
{
	public abstract class GroupablePrecondition : IGroupablePrecondition
	{
		public virtual IReadOnlyList<string> Groups { get; } = Array.Empty<string>();
		public virtual GroupOp Op { get; } = GroupOp.And;
	}
}