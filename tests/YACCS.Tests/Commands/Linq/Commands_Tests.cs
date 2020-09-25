using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands.Attributes;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Commands.Linq;
using YACCS.Commands;
using System.Threading.Tasks;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq
{
	[TestClass]
	public class Commands_Tests
	{
		private const string DUPE_ID = "dupe_id";
		private const string NORM_ID = "normal_id";
		private readonly List<ICommand> _Commands = new List<ICommand>
		{
			new DelegateCommand((Action<string>)(s => { }), Array.Empty<IName>())
			{
				Attributes = new List<object>
				{
					new IdAttribute(DUPE_ID),
				},
			},
			new DelegateCommand((Action<string>)(s => { }), Array.Empty<IName>())
			{
				Attributes = new List<object>
				{
					new IdAttribute(DUPE_ID),
				},
			},
			new DelegateCommand((Action<string>)(s => { }), Array.Empty<IName>())
			{
				Attributes = new List<object>
				{
					new IdAttribute(NORM_ID),
				},
			},
		};

		[TestMethod]
		public void AddName_Test()
		{
			var command = _Commands.GetCommandById<FakeContext>(NORM_ID);
			Assert.AreEqual(0, command.Names.Count);
			command.AddName(new Name(new[] { "joe", "mama" }));
			Assert.AreEqual(1, command.Names.Count);
		}

		[TestMethod]
		public void AddPrecondition_Test()
		{
			var command = _Commands.GetCommandById<FakeContext>(NORM_ID);
			Assert.AreEqual(0, command.Get<IPrecondition>().Count());
			command.AddPrecondition(new NotSevenPM());
			Assert.AreEqual(1, command.Get<IPrecondition>().Count());
		}

		[TestMethod]
		public void AsCommand_Test()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				var command = default(IQueryableEntity)!.AsCommand();
			});

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var parameter = new Parameter();
				var command = parameter.AsCommand();
			});

			var command = _Commands[0].AsCommand();
			Assert.IsNotNull(command);
		}
	}

	public sealed class NotSevenPM : Precondition<FakeContext>
	{
		public override Task<IResult> CheckAsync(CommandInfo info, FakeContext context)
		{
			if (DateTime.UtcNow.Hour != 19)
			{
				return SuccessResult.InstanceTask;
			}
			return Result.FromError("It's seven PM.").AsTask();
		}
	}
}