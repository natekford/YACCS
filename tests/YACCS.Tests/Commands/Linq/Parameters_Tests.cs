﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands.Attributes;
using YACCS.Commands.Linq;
using YACCS.Commands.Models;
using YACCS.Preconditions;
using YACCS.Results;
using YACCS.TypeReaders;

namespace YACCS.Tests.Commands.Linq;

[TestClass]
public class Parameters_Tests
{
	private const string CHILD_ID = "child_id";
	private const string DUPE_ID = "dupe_id";
	private const string NORM_ID = "normal_id";
	private const string PARENT_ID = "parent_id";
	private readonly List<IMutableParameter> _Parameters =
	[
		new Parameter(typeof(Child), "", null)
		{
			Attributes =
				[
					new IdAttribute(DUPE_ID),
					new IdAttribute(CHILD_ID),
				],
		},
		new Parameter(typeof(Base), "", null)
		{
			Attributes =
				[
					new IdAttribute(DUPE_ID),
					new IdAttribute(PARENT_ID),
				],
		},
		new Parameter(typeof(int), "", null)
		{
			Attributes =
				[
					new IdAttribute(NORM_ID),
				],
		},
		new Parameter(typeof(int), "", null),
	];

	[TestMethod]
	public void AddParameterPrecondition_Test()
	{
		var parameter = _Parameters.ById(NORM_ID).Single().AsType<int>();
		Assert.AreEqual(1, parameter.Attributes.Count);
		parameter.AddParameterPrecondition(new NotNegative());
		Assert.AreEqual(2, parameter.Attributes.Count);
	}

	[TestMethod]
	public void AsParameter_Test()
	{
		Assert.ThrowsExactly<ArgumentNullException>(static () =>
		{
			var parameter = default(IQueryableEntity)!.AsParameter();
		});

		Assert.ThrowsExactly<ArgumentException>(static () =>
		{
			var command = FakeDelegateCommand.New();
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

		var child_parent = parent.AsType<Child>();
		Assert.IsInstanceOfType<IParameter<Child>>(child_parent);
		var child_child = child.AsType<Child>();
		Assert.IsInstanceOfType<IParameter<Child>>(child_child);

		Assert.ThrowsExactly<ArgumentException>(() =>
		{
			var parent_child = child.AsType<Base>();
		});
		var parent_parent = parent.AsType<Base>();
		Assert.IsInstanceOfType<IParameter<Base>>(parent_parent);
		var parent_child = (IParameter<Child>)parent_parent;
		Assert.IsInstanceOfType<IParameter<Base>>(parent_child);
	}

	[TestMethod]
	public void GetParametersByType_Test()
	{
		{
			var parameters = _Parameters.GetParametersByType<Base>();
			Assert.AreEqual(1, parameters.Count());
		}

		{
			var parameters = _Parameters.GetParametersByType<Child>();
			Assert.AreEqual(2, parameters.Count());
		}

		{
			var parameters = _Parameters.GetParametersByType<int>();
			Assert.AreEqual(2, parameters.Count());
		}

		{
			var parameters = _Parameters.GetParametersByType<string>();
			Assert.AreEqual(0, parameters.Count());
		}
	}

	[TestMethod]
	public void ModifyDefaultValue_Test()
	{
		var parameter = _Parameters.ById(NORM_ID).Single().AsType<int>();
		Assert.IsFalse(parameter.HasDefaultValue);

		parameter.SetDefaultValue(1);
		Assert.IsTrue(parameter.HasDefaultValue);
		Assert.AreEqual(1, parameter.DefaultValue);

		parameter.RemoveDefaultValue();
		Assert.IsFalse(parameter.HasDefaultValue);

		parameter.SetDefaultValue(2);
		Assert.IsTrue(parameter.HasDefaultValue);
		Assert.AreEqual(2, parameter.DefaultValue);
	}

	[TestMethod]
	public void SetOverridenTypeReader_Test()
	{
		var parameter = _Parameters.ById(NORM_ID).Single().AsType<int>();
		Assert.IsNull(parameter.TypeReader);

		parameter.SetTypeReader(new NumberTypeReader<int>(int.TryParse));
		Assert.IsNotNull(parameter.TypeReader);

		parameter.RemoveTypeReader();
		Assert.IsNull(parameter.TypeReader);
	}

	private class Base;

	private class Child : Base;

	private sealed class NotNegative : ParameterPrecondition<FakeContext, int>
	{
		private const string _Message = "Is negative.";
		private static readonly Result _Failure = Result.Failure(_Message);

		protected override ValueTask<IResult> CheckNotNullAsync(
			CommandMeta meta,
			FakeContext context,
			int value)
		{
			if (value >= 0)
			{
				return new(Result.EmptySuccess);
			}
			return new(_Failure);
		}
	}
}