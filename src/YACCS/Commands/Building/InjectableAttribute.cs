using System;
using System.Linq.Expressions;

namespace YACCS.Commands.Building
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public abstract class InjectableAttribute : Attribute
	{
		public abstract Expression CreateInjection(
			ParameterExpression context,
			MemberExpression member);
	}
}