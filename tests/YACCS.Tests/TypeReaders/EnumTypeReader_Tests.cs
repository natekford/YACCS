﻿using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class EnumTypeReader_Tests : TypeReader_Tests<BindingFlags>
	{
		public override TypeReader<BindingFlags> Reader { get; }
			= new EnumTypeReader<BindingFlags>();

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, nameof(BindingFlags.CreateInstance)).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(BindingFlags));
		}
	}
}