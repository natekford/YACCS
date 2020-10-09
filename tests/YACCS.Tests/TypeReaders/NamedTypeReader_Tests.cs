using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.Commands;
using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class NamedTypeReader_Tests : TypeReader_Tests<NamedTypeReader_Tests.NamedClass>
	{
		public override TypeReader<NamedClass> Reader { get; } = new NamedTypeReader<NamedClass>();

		[TestMethod]
		public async Task ReadAsync_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = $"{nameof(NamedClass.Number)}: {NUM} {nameof(NamedClass.String)}: {STR}";

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.IsSuccess);
			if (result.Value is null)
			{
				Assert.Fail();
				return;
			}
			Assert.AreEqual(NUM, result.Value.Number);
			Assert.AreEqual(STR, result.Value.String);
		}

		private FakeContext Create()
		{
			return new FakeContext()
			{
				Services = new ServiceCollection()
					.AddSingleton<ITypeReaderRegistry, TypeReaderRegistry>()
					.BuildServiceProvider(),
			};
		}

		public class NamedClass
		{
			public int Number { get; set; }
			public string String { get; set; } = "";
		}
	}
}