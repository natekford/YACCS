﻿using System.Collections.Generic;

namespace YACCS.Preconditions
{
	public interface IGroupablePrecondition
	{
		IReadOnlyList<string> Groups { get; }
		GroupOp Op { get; }
	}
}