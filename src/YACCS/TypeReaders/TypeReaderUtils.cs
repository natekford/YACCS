using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;

using static YACCS.Results.Result;

namespace YACCS.TypeReaders
{
	public static class TypeReaderUtils
	{
		public static ITypeReaderCache EmptyCache { get; } = new EmptyTypeReaderCache();

		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static ITask<ITypeReaderResult> AsITask(this ITypeReaderResult result)
			=> Task.FromResult(result).AsITask();

		public static Task<ITypeReaderResult<T>> AsTask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result);

		public static Task<ITypeReaderResult> AsTask(this ITypeReaderResult result)
			=> Task.FromResult(result);

		public static ResultInstance<T, ITypeReaderResult> AsTypeReaderResultInstance<T>(this T instance) where T : ITypeReaderResult
			=> new ResultInstance<T, ITypeReaderResult>(instance);

		public static ITypeReader<T> Get<T>(this ITypeRegistry<ITypeReader> registry)
		{
			if (registry.Get(typeof(T)) is ITypeReader<T> reader)
			{
				return reader;
			}
			throw new ArgumentException($"Invalid type reader registered for {typeof(T).Name}.", nameof(T));
		}

		public static ITypeReader Get(
			this ITypeRegistry<ITypeReader> registry,
			IImmutableParameter parameter)
			=> parameter.TypeReader ?? registry.Get(parameter.ParameterType);

		public static ITypeReaderCache GetTypeReaderCache(this IContext context)
			=> context.Services.GetService<ITypeReaderCache>() ?? EmptyCache;

		public static IEnumerable<TypeReaderInfo> GetTypeReaders(this Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				var attr = type.GetCustomAttribute<TypeReaderTargetTypesAttribute>();
				if (attr == null)
				{
					continue;
				}

				var typeReader = type.CreateInstance<ITypeReader>();
				yield return new TypeReaderInfo(attr.TargetTypes, typeReader);
			}
		}

		public static ITask<ITypeReaderResult> ReadAsync(
			this ITypeReader reader,
			IContext context,
			string input)
			=> reader.ReadAsync(context, new[] { input });

		public static ITask<ITypeReaderResult<T>> ReadAsync<T>(
			this ITypeReader<T> reader,
			IContext context,
			string input)
			=> reader.ReadAsync(context, new[] { input });

		public static void Register(
			this ITypeRegistry<ITypeReader> registry,
			IEnumerable<TypeReaderInfo> typeReaderInfos)
		{
			foreach (var typeReaderInfo in typeReaderInfos)
			{
				foreach (var type in typeReaderInfo.TargetTypes)
				{
					registry.Register(type, typeReaderInfo.Instance);
				}
			}
		}

		public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
		{
			if (!type.IsAssignableFrom(reader.OutputType))
			{
				throw new ArgumentException(
					$"A type reader with the output type {reader.OutputType.Name} " +
					$"cannot be used for a the type {type.Name}.", nameof(reader));
			}
		}

		private class EmptyTypeReaderCache : ITypeReaderCache
		{
			public ITask<ITypeReaderResult<T>> GetAsync<T>(
				ITypeReader<T> reader,
				IContext context,
				string input,
				TypeReaderCacheDelegate<T> func)
				=> func.Invoke(context, input);

			public void Remove(IContext context)
			{
			}
		}
	}
}