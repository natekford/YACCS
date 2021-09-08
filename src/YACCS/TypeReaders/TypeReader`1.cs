using YACCS.Commands;

namespace YACCS.TypeReaders
{
	/// <summary>
	/// A <see cref="TypeReader{TContext, TValue}"/> which does not care about the type of
	/// the passed in <see cref="IContext"/>. This does NOT mean that <see langword="null"/> is allowed.
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	public abstract class TypeReader<TValue> : TypeReader<IContext, TValue>
	{
	}
}