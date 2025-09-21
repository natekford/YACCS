﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using YACCS.Results;

namespace YACCS.Commands.Models;

/// <summary>
/// A mutable command created from a <see cref="System.Delegate"/>.
/// </summary>
public sealed class DelegateCommand : Command
{
	/// <summary>
	/// The delegate being wrapped.
	/// </summary>
	public Delegate Delegate { get; }

	/// <summary>
	/// Creates a new <see cref="DelegateCommand"/>.
	/// </summary>
	/// <param name="delegate">The delegate to wrap.</param>
	/// <param name="paths">The paths for this command.</param>
	/// <param name="contextType">The required context type.</param>
	public DelegateCommand(
		Delegate @delegate,
		IEnumerable<IReadOnlyList<string>> paths,
		Type? contextType = null)
		: this(@delegate, contextType ?? typeof(IContext), paths)
	{
	}

	private DelegateCommand(
		Delegate @delegate,
		Type contextType,
		IEnumerable<IReadOnlyList<string>> names)
		: base(@delegate.Method, contextType)
	{
		Delegate = @delegate;
		Paths = names.ToList();
		Attributes.Add(new(Delegate.Method, AttributeInfo.ON_METHOD, Delegate));
	}

	/// <inheritdoc />
	public override IImmutableCommand ToImmutable()
		=> new ImmutableDelegateCommand(this);

	private sealed class ImmutableDelegateCommand : ImmutableCommand
	{
		private readonly Delegate _Delegate;
		private readonly Func<IReadOnlyList<object?>, object> _Execute;

		public ImmutableDelegateCommand(DelegateCommand mutable)
			: base(mutable, mutable.Delegate.Method.ReturnType)
		{
			_Delegate = mutable.Delegate;
			_Execute = ReflectionUtils.CreateDelegate(Execute);
		}

		public override ValueTask<IResult> ExecuteAsync(
			IContext context,
			IReadOnlyList<object?> args)
			=> ConvertValueAsync(_Execute.Invoke(args));

		private Func<IReadOnlyList<object?>, object> Execute()
		{
			return ExpressionUtils.InvokeFromList<Func<IReadOnlyList<object?>, object>>(
				_Delegate.Target is null ? null : Expression.Constant(_Delegate.Target),
				_Delegate.Method
			);
		}
	}
}