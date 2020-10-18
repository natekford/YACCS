using System.ComponentModel;
using System.Threading.Tasks;

namespace YACCS
{
	public interface IAsyncEvent<T> where T : HandledEventArgs
	{
		IAsyncEvent<ExceptionEventArgs<T>> Exception { get; }

		void Add(AsyncEventHandler<T> handler);

		Task InvokeAsync(T e);

		void Remove(AsyncEventHandler<T> handler);
	}
}