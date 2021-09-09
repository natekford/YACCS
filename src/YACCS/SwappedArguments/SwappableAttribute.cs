using System;

using YACCS.Commands.Models;

namespace YACCS.SwappedArguments
{
	/// <summary>
	/// Specifies that the parameter can be swapped in
	/// <see cref="SwappedArgumentsUtils.GenerateSwappedArgumentsVersions(IImmutableCommand, int)"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class SwappableAttribute : Attribute
	{
	}
}