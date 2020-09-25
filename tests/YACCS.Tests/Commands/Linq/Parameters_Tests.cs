using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.ParameterPreconditions;
using YACCS.Results;

namespace YACCS.Tests.Commands.Linq
{
	public sealed class NotNegative : ParameterPrecondition<FakeContext, int>
	{
		public override Task<IResult> CheckAsync(
			CommandInfo info,
			FakeContext context,
			[MaybeNull] int value)
		{
			if (value >= 0)
			{
				return SuccessResult.InstanceTask;
			}
			return Result.FromError("Is negative.").AsTask();
		}
	}

	[TestClass]
	public class Parameters_Tests
	{
		private const string CHILD_ID = "child_id";
		private const string DUPE_ID = "dupe_id";
		private const string NORM_ID = "normal_id";
		private const string PARENT_ID = "parent_id";
		private readonly List<IParameter> _Parameters = new List<IParameter>
		{
			new Parameter
			{
				ParameterType = typeof(Test_Child),
				Attributes = new List<object>
				{
					new IdAttribute(DUPE_ID),
					new IdAttribute(CHILD_ID),
				},
			},
			new Parameter
			{
				ParameterType = typeof(Test_Parent),
				Attributes = new List<object>
				{
					new IdAttribute(DUPE_ID),
					new IdAttribute(PARENT_ID),
				},
			},
			new Parameter
			{
				ParameterType = typeof(int),
				Attributes = new List<object>
				{
					new IdAttribute(NORM_ID),
				},
			},
			new Parameter
			{
				ParameterType = typeof(int),
			},
		};

		[TestMethod]
		public void AddParameterPrecondition_Test()
		{
			var parameter = _Parameters.GetParameterById<int>(NORM_ID);
			Assert.AreEqual(1, parameter.Attributes.Count);
			parameter.AddParameterPrecondition(new NotNegative());
			Assert.AreEqual(2, parameter.Attributes.Count);
		}

		[TestMethod]
		public void AsParameter_Test()
		{
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				var parameter = default(IQueryableEntity)!.AsParameter();
			});

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var command = new DelegateCommand((Action)(() => { }), new[] { new Name(new[] { "joe" }) });
				var parameter = command.AsParameter();
			});

			var parameter = _Parameters[0].AsParameter();
			Assert.IsNotNull(parameter);
		}

		[TestMethod]
		public void AsType_Test()
		{
			var parent = _Parameters.ById(PARENT_ID).Single();
			var child = _Parameters.ById(CHILD_ID).Single();

			var parent_parent = parent.AsType<Test_Parent>();
			Assert.IsInstanceOfType(parent_parent, typeof(IParameter<Test_Parent>));
			var parent_child = child.AsType<Test_Parent>();
			Assert.IsInstanceOfType(parent_child, typeof(IParameter<Test_Parent>));

			Assert.ThrowsException<ArgumentException>(() =>
			{
				var child_parent = parent.AsType<Test_Child>();
			});
			var child_child = child.AsType<Test_Child>();
			Assert.IsInstanceOfType(parent_child, typeof(IParameter<Test_Child>));
		}
	}

	public class Test_Child : Test_Parent
	{
	}

	public class Test_Parent
	{
	}
}