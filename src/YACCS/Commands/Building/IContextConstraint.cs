using System;

namespace YACCS.Commands.Building
{
	public interface IContextConstraint
	{
		bool DoesTypeSatisfy(Type type);
	}
}