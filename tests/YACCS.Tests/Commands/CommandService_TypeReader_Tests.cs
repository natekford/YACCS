using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands
{
	[TestClass]
	public class CommandService_TypeReader_Tests
	{
		[TestMethod]
		public async Task ProcessTypeReaderMultipleButNotAllValues_Test()
		{
			var value = new[] { 1, 2, 3, 4 };
			var type = value.GetType();

			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(4),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).Append("joeba").Append("trash").ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(4),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(500),
				},
			}.ToParameter();
			var input = value.Select(x => x.ToString()).ToArray();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
		public async Task ProcessTypeReaderNotRegistered_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(IDictionary<ArgumentNullException, IDictionary<string, char>>), "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { "joeba" };
			const int startIndex = 0;

			await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			{
				var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
					parameter,
					input,
					startIndex
				).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		[TestMethod]
		public async Task ProcessTypeReaderOverridden_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(char), "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
				OverriddenTypeReader = new CoolCharTypeReader(),
			}.ToParameter();
			var input = new[] { "joeba" };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task ProcessTypeReadersCharFailure_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(char), "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { "joeba" };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value.ToString() };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value.ToString(), "joeba", "trash" };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
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
			var parameter = new Parameter(type, "Test")
			{
				Attributes = new List<object>
				{
					new LengthAttribute(1),
				},
			}.ToParameter();
			var input = new[] { value };
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter,
				input,
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, type);
			Assert.AreEqual(value, result.Arg);
		}

		[TestMethod]
		public async Task ProcessTypeReaderZeroLength_Test()
		{
			var commandService = new CommandService(CommandServiceConfig.Default, new TypeReaderRegistry());
			var context = new FakeContext();
			var parameter = new Parameter(typeof(IContext), "Test")
			{
				Attributes = new List<object>
				{
					new ContextAttribute(),
				},
			}.ToParameter();
			const int startIndex = 0;

			var result = await commandService.ProcessTypeReadersAsync(
				new PreconditionCache(context),
				parameter,
				new[] { "doesn't matter" },
				startIndex
			).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Arg, typeof(IContext));
		}

		private class CoolCharTypeReader : TypeReader<char>
		{
			public override Task<ITypeReaderResult<char>> ReadAsync(IContext context, string input)
				=> TypeReaderResult<char>.FromSuccess('z').AsTask();
		}
	}
}