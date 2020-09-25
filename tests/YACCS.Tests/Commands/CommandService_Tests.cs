using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandService_Tests
	{
		[TestMethod]
		public async Task ProcessTypeReaderMultipleButNotAllValues_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(4),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).Append("joeba").Append("trash").ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReaderMultipleValuesAllValues_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(4),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReaderMultipleValuesLongerThanArgs_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(500),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);

			var cast = (int[])result.Arg!;
			for (var i = 0; i < value.Length; ++i)
			{
				Assert.AreEqual(value[i], cast[i]);
			}
		}

		[TestMethod]
		public async Task ProcessTypeReadersCharFailure_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = typeof(char),
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { "joeba" };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task ProcessTypeReaderSingleValue_Test()
		{
			const int value = 2;
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value.ToString() };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);
			Assert.AreEqual(value, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReaderSingleValueWhenMultipleExist_Test()
		{
			const int value = 2;
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value.ToString(), "joeba", "trash" };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);
			Assert.AreEqual(value, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReadersString_Test()
		{
			const string value = "joeba";
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var cache = new PreconditionCache(context);
			var parameter = new Parameter
			{
				ParameterName = "Test",
				ParameterType = type,
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				cache,
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);
			Assert.AreEqual(value, result.Arg);
		}
	}
}