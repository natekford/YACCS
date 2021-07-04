using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using MorseCode.ITask;

using YACCS.Commands;
using YACCS.Commands.Models;

namespace YACCS.TypeReaders
{
	public static class TypeReaderUtils
	{
		public static ITask<ITypeReaderResult<T>> AsITask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result).AsITask();

		public static ITask<ITypeReaderResult> AsITask(this ITypeReaderResult result)
			=> Task.FromResult(result).AsITask();

		public static Task<ITypeReaderResult<T>> AsTask<T>(this ITypeReaderResult<T> result)
			=> Task.FromResult(result);

		public static Task<ITypeReaderResult> AsTask(this ITypeReaderResult result)
			=> Task.FromResult(result);

		public static ITypeReader<T> GetTypeReader<T>(
			this IReadOnlyDictionary<Type, ITypeReader> registry)
		{
			if (registry.TryGetValue(typeof(T), out var temp) && temp is ITypeReader<T> reader)
			{
				return reader;
			}
			throw new ArgumentException(
				$"Invalid type reader registered for {typeof(T).Name}.", nameof(T));
		}

		public static ITypeReader GetTypeReader(
			this IReadOnlyDictionary<Type, ITypeReader> registry,
			IImmutableParameter parameter)
			=> parameter.TypeReader ?? registry[parameter.ParameterType];

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

		public static void RegisterTypeReaders(
			this IDictionary<Type, ITypeReader> registry,
			IEnumerable<TypeReaderInfo> typeReaderInfos)
		{
			foreach (var typeReaderInfo in typeReaderInfos)
			{
				foreach (var type in typeReaderInfo.TargetTypes)
				{
					registry[type] = typeReaderInfo.Instance;
				}
			}
		}

		public static void ThrowIfInvalidTypeReader(this ITypeReader reader, Type type)
		{
			if (!type.IsAssignableFrom(reader.OutputType))
			{
				throw new ArgumentException(
					$"A type reader with the output type {reader.OutputType.Name} " +
					$"cannot be used for the type {type.Name}.", nameof(reader));
			}
		}
	}
}