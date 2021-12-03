using System.Linq.Expressions;

namespace YACCS.Commands.Building;

/// <summary>
/// The base class for specifying how a property/field is injected.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public abstract class InjectableAttribute : Attribute
{
	/// <summary>
	/// Creates a new <see cref="Expression"/> which sets a value for whichever
	/// property/field this attribute is applied to.
	/// </summary>
	/// <param name="context">
	/// An expression which is context as a parameter:
	/// <code>
	/// (<see cref="IContext"/> Context)
	/// </code>
	/// </param>
	/// <param name="member">
	/// An expression which is the member access of a property/field:
	/// <code>
	/// DeclaringType.Property
	/// </code>
	/// <code>
	/// DeclaringType.Field
	/// </code>
	/// </param>
	/// <returns>A new <see cref="Expression"/>.</returns>
	public abstract Expression CreateInjection(
		ParameterExpression context,
		MemberExpression member);
}
