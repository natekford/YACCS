using System;
using System.Linq.Expressions;
using System.Reflection;

namespace YACCS.Commands.Building
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class InjectServiceAttribute : InjectableAttribute
	{
		private static readonly MethodInfo _GetService = typeof(IServiceProvider)
			.GetMethod(nameof(IServiceProvider.GetService));

		public override Expression CreateInjection(
			ParameterExpression context,
			MemberExpression member)
		{
			// Get services
			var services = Expression.Property(context, nameof(IContext.Services));

			// Create temp variable
			var typeArgument = Expression.Constant(member.Type);
			var getService = Expression.Call(services, _GetService, typeArgument);
			var temp = Expression.Variable(typeof(object), "__var");
			var tempAssign = Expression.Assign(temp, getService);

			// Make sure the temp variable is not null
			var isType = Expression.TypeIs(temp, member.Type);

			// Set member to temp variable
			var serviceCast = Expression.Convert(temp, member.Type);
			var serviceAssign = Expression.Assign(member, serviceCast);

			var ifThen = Expression.IfThen(isType, serviceAssign);
			var body = Expression.Block(new[] { temp }, tempAssign, ifThen);

			// Catch any exceptions and throw a more informative one
			var message = $"Failed setting the service '{member.Member}' for " +
				$"'{member.Member.ReflectedType.FullName}'.";
			return body.AddThrow((Exception e) => new ArgumentException(message, member.Member.Name, e));
		}
	}
}