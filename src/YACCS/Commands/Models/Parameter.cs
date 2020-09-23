using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using YACCS.Commands.Attributes;
using YACCS.ParameterPreconditions;

namespace YACCS.Commands.Models
{
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public sealed class Parameter : EntityBase, IMutableParameter
	{
		public bool IsOptional { get; set; }
		public string ParameterName { get; set; }
		public Type ParameterType { get; set; }
		public IList<IParameterPrecondition> Preconditions { get; set; } = new List<IParameterPrecondition>();
		private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

		public Parameter() : base(null)
		{
			ParameterName = "";
			ParameterType = typeof(void);
		}

		public Parameter(ParameterInfo parameter) : base(parameter)
		{
			IsOptional = parameter.IsOptional;
			ParameterName = parameter.Name;
			ParameterType = parameter.ParameterType;
		}

		public IParameter ToParameter()
			=> new ImmutableParameter(this);

		[DebuggerDisplay("{DebuggerDisplay,nq}")]
		private sealed class ImmutableParameter : IParameter
		{
			public IReadOnlyList<object> Attributes { get; }
			public Type? EnumerableType { get; }
			public string Id { get; }
			public bool IsOptional { get; }
			public int Length { get; }
			public string ParameterName { get; }
			public Type ParameterType { get; }
			public IReadOnlyList<IParameterPrecondition> Preconditions { get; }
			IEnumerable<object> IQueryableEntity.Attributes => Attributes;
			private string DebuggerDisplay => $"Name = {ParameterName}, Type = {ParameterType}";

			public ImmutableParameter(Parameter mutable)
			{
				Attributes = mutable.Attributes.ToImmutableArray();
				Id = mutable.Id;
				IsOptional = mutable.IsOptional;
				Length = mutable.Get<ILengthAttribute>().SingleOrDefault()?.Length ?? 1;
				ParameterName = mutable.ParameterName;
				ParameterType = mutable.ParameterType;
				Preconditions = mutable.Get<IParameterPrecondition>().ToImmutableArray();
				EnumerableType = GetEnumerableType(mutable.ParameterType);
			}

			private static Type? GetEnumerableType(Type type)
			{
				if (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return type.GetGenericArguments()[0];
				}
				foreach (var @interface in type.GetInterfaces())
				{
					if (@interface.IsGenericType
						&& @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
					{
						return @interface.GetGenericArguments()[0];
					}
				}
				return null;
			}
		}
	}
}