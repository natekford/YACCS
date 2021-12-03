using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Help;

namespace YACCS.Tests.Help;

[TestClass]
public sealed class TypeNameRegistry_Tests
{
	private readonly TypeNameRegistry _Names = new();

	[TestMethod]
	public void Get_Test()
		=> Assert.AreEqual("time", _Names[typeof(TimeSpan)]);

	[TestMethod]
	public void List_Test()
	{
		Assert.AreEqual("time list", _Names[typeof(TimeSpan[])]);
		Assert.AreEqual("time list", _Names[typeof(IEnumerable<TimeSpan>)]);
	}

	[TestMethod]
	public void NonRegistered_Test()
		=> Assert.AreEqual(nameof(Attribute), _Names[typeof(Attribute)]);

	[TestMethod]
	public void Nullable_Test()
		=> Assert.AreEqual("time or null", _Names[typeof(TimeSpan?)]);
}
