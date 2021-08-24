
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Help;

namespace YACCS.Tests.Help
{
	[TestClass]
	public class TypeNameRegistry_Tests
	{
		[TestMethod]
		public void Get_Test()
		{
			var names = new TypeNameRegistry();
			var name = names[typeof(TimeSpan)];
			Assert.AreEqual("time", name);
		}

		[TestMethod]
		public void List_Test()
		{
			var names = new TypeNameRegistry();

			{
				var name = names[typeof(TimeSpan[])];
				Assert.AreEqual("time list", name);
			}

			{
				var name = names[typeof(IEnumerable<TimeSpan>)];
				Assert.AreEqual("time list", name);
			}
		}

		[TestMethod]
		public void NonRegistered_Test()
		{
			var names = new TypeNameRegistry();
			var name = names[typeof(TypeNameRegistry_Tests)];
			Assert.AreEqual(nameof(TypeNameRegistry_Tests), name);
		}

		[TestMethod]
		public void Nullable_Test()
		{
			var names = new TypeNameRegistry();
			var name = names[typeof(TimeSpan?)];
			Assert.AreEqual("time or null", name);
		}
	}
}