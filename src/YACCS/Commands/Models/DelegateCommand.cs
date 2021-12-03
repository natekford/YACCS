using System.Linq.Expressions;

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
		: this(@delegate, contextType ?? typeof(IContext), null, paths)
	{
	}

	/// <summary>
	/// Creates a new <see cref="DelegateCommand"/>.
	/// </summary>
	/// <param name="delegate">The delegate to wrap.</param>
	/// <param name="source">The command this one is being marked as generated from.</param>
	public DelegateCommand(Delegate @delegate, IImmutableCommand source)
		: this(@delegate, source.ContextType, source, source.Paths)
	{
	}

	private DelegateCommand(
		Delegate @delegate,
		Type contextType,
		IImmutableCommand? source,
		IEnumerable<IReadOnlyList<string>> names)
		: base(@delegate.Method, contextType, source)
	{
		Delegate = @delegate;

		foreach (var name in names)
		{
			Paths.Add(name);
		}

		Attributes.Add(@delegate);
	}

	/// <inheritdoc />
	public override IImmutableCommand ToImmutable()
		=> new ImmutableDelegateCommand(this);

	private sealed class ImmutableDelegateCommand : ImmutableCommand
	{
		private readonly Delegate _Delegate;
		private readonly Func<object?[], object> _Execute;

		public ImmutableDelegateCommand(DelegateCommand mutable)
			: base(mutable, mutable.Delegate.Method.ReturnType)
		{
			_Delegate = mutable.Delegate;
			_Execute = ReflectionUtils.CreateDelegate(Execute, "execute");
		}

		public override ValueTask<IResult> ExecuteAsync(IContext context, object?[] args)
			=> ConvertValueAsync(_Execute.Invoke(args));

		private Func<object?[], object> Execute()
		{
			return ExpressionUtils.GetInvokeFromObjectArray<Func<object?[], object>>(
				_Delegate.Target is null ? null : Expression.Constant(_Delegate.Target),
				_Delegate.Method
			);
		}
	}
}