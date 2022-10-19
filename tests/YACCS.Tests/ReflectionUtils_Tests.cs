using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YACCS.Tests;

[TestClass]
public class ReflectionUtils_Tests
{
	[TestMethod]
	public void CreateDelegate_Test()
	{
		var i = 0;
		var del = ReflectionUtils.CreateDelegate<Action>(() => () => ++i, "inc");
		del();
		Assert.AreEqual(1, i);
	}

	[TestMethod]
	public void CreateDelegateError_Test()
	{
		var ex = Assert.ThrowsException<ArgumentException>(() =>
		{
			_ = ReflectionUtils.CreateDelegate<Action>(() =>
			{
				throw new InvalidOperationException("lol");
			}, "exception");
		});
		Assert.IsNotNull(ex.InnerException);
	}

	[TestMethod]
	public void CreateInstance_Test()
	{
		var instance = ReflectionUtils.CreateInstance<AbstractClass>(typeof(RealClass));
		Assert.IsNotNull(instance);
	}

	[TestMethod]
	public void CreateInstanceNotInstantiable_Test()
	{
		var ex = Assert.ThrowsException<ArgumentException>(() =>
		{
			_ = ReflectionUtils.CreateInstance<AbstractClass>(typeof(AbstractClass));
		});
		Assert.IsInstanceOfType(ex.InnerException, typeof(MissingMethodException));
	}

	[TestMethod]
	public void CreateInstanceWrongType_Test()
	{
		var ex = Assert.ThrowsException<ArgumentException>(() =>
		{
			_ = ReflectionUtils.CreateInstance<Guid>(typeof(RealClass));
		});
		Assert.IsNull(ex.InnerException);
	}

	[TestMethod]
	public void GetWritableMembers_Test()
	{
		var (props, fields) = ReflectionUtils.GetWritableMembers(typeof(RealClass));

		Assert.IsTrue(new HashSet<string>()
		{
			nameof(RealClass.ReadWriteProperty)
		}.SetEquals(props.Select(x => x.Name)));

		Assert.IsTrue(new HashSet<string>()
		{
			nameof(RealClass.ReadWriteField)
		}.SetEquals(fields.Select(x => x.Name)));
	}

	private abstract class AbstractClass
	{
	}

	private class RealClass : AbstractClass
	{
		public static int StaticField;
		public readonly int ReadOnlyField;
		public int ReadWriteField;

		public int ReadOnlyProperty { get; }
		public int ReadProtectedWriteProperty { get; protected set; }
		public int ReadWriteProperty { get; set; }
	}
}