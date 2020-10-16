using System;
using System.Collections.Generic;

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
			var registry = new TypeNameRegistry();

			var name = registry.Get(typeof(TimeSpan));
			Assert.AreEqual("time", name);
		}

		[TestMethod]
		public void List_Test()
		{
			var registry = new TypeNameRegistry();

			{
				var name = registry.Get(typeof(TimeSpan[]));
				Assert.AreEqual("time list", name);
			}

			{
				var name = registry.Get(typeof(IEnumerable<TimeSpan>));
				Assert.AreEqual("time list", name);
			}
		}

		[TestMethod]
		public void NonRegistered_Test()
		{
			var registry = new TypeNameRegistry();

			var name = registry.Get(typeof(TypeNameRegistry_Tests));
			Assert.AreEqual(nameof(TypeNameRegistry_Tests), name);
		}

		[TestMethod]
		public void Nullable_Test()
		{
			var registry = new TypeNameRegistry();

			var name = registry.Get(typeof(TimeSpan?));
			Assert.AreEqual("time or null", name);
		}
	}
}