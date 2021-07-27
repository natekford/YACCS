using System;
using System.Collections.Immutable;
using System.Linq;

using YACCS.Commands.Linq;
using YACCS.Commands.Models;

namespace YACCS.StructuredArguments
{
	public static class StructuredArgumentsUtils
	{
		public static ImmutableArray<IImmutableParameter> CreateStructuredParameter<T>(
			this IImmutableCommand source,
			string name,
			Action<IParameter<T>> modifier)
		{
			var parameters = ImmutableArray.CreateBuilder<IImmutableParameter>(1);
			try
			{
				var parameter = Parameters.Create<T>(name);
				modifier(parameter);
				parameters.Add(parameter.ToImmutable());
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Unable to build {typeof(T).Name} " +
					$"parameter for '{source.Names?.FirstOrDefault()}'.", e);
			}
			return parameters.MoveToImmutable();
		}
	}
}