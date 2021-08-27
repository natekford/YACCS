using System.Collections.Immutable;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.TypeReaders;

namespace YACCS.Tests.TypeReaders
{
	[TestClass]
	public class NullableTypeReader_Tests : TypeReader_Tests<int?>
	{
		public override ITypeReader<int?> Reader { get; }
			= new NullableTypeReader<int>();

		[TestMethod]
		public async Task AddedString_Test()
		{
			const string JOE = "joe";

			var @null = new NullValidator(new[] { JOE }.ToImmutableHashSet());
			var context = new FakeContext
			{
				Services = new ServiceCollection()
					.AddSingleton<INullValidator>(@null)
					.BuildServiceProvider(),
			};
			var result = await Reader.ReadAsync(context, new[] { JOE }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task Null_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { default(string)! }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task NullString_Test()
		{
			var result = await Reader.ReadAsync(Context, new[] { "null" }).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			const int VALUE = 1;
			var value = await AssertSuccessAsync(VALUE.ToString()).ConfigureAwait(false);
			Assert.AreEqual(VALUE, value);
		}
	}
}