using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using YACCS.Commands.Models;

namespace YACCS.Commands
{

	public interface IContextConstraint
	{
		bool DoesTypeSatisfy(Type type);
	}
}