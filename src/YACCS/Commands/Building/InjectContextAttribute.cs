using System;
using System.Linq.Expressions;

namespace YACCS.Commands.Building
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class InjectContextAttribute : InjectableAttribute
	{
		public override Expression CreateInjection(
			ParameterExpression context,
			MemberExpression member)
		{
			// Cast context parameter to property type and assign
			var contextCast = Expression.Convert(context, member.Type);
			return Expression.Assign(member, contextCast);
		}
	}
}