using System.Reflection;

namespace YACCS.Commands.Attributes
{
	public interface IMethodInfoCommandAttribute
	{
		MethodInfo Method { get; }
	}
}