﻿using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class TimeSpanTypeReader_Tests : TypeReader_Tests<TimeSpan>
	{
		public override TypeReader<TimeSpan> Reader { get; }
			= new TimeSpanTypeReader<TimeSpan>(TimeSpan.TryParse);

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, "00:00:01").ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(TimeSpan));
		}
	}
}