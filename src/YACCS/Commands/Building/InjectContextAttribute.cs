using System.Linq.Expressions;

namespace YACCS.Commands.Building;

/// <summary>
/// Specifies that the property/field is injected by casting the context.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class InjectContextAttribute : InjectableAttribute
{
	/// <inheritdoc />
	public override Expression CreateInjection(
		ParameterExpression context,
		MemberExpression member)
	{
		// Cast context parameter to property type and assign
		var contextCast = Expression.Convert(context, member.Type);
		return Expression.Assign(member, contextCast);
	}
}