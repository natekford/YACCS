using System;
using System.Diagnostics.CodeAnalysis;

namespace YACCS
{
	public interface ITypeRegistry<T>
	{
		void Register(Type type, T item);

		bool TryGet(Type type, [MaybeNull, NotNullWhen(true)] out T item);
	}
}