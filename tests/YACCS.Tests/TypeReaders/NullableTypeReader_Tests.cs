using System.Threading.Tasks;

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

			var checker = new NullChecker(new[] { JOE });
			var context = new FakeContext
			{
				Services = new ServiceCollection()
					.AddSingleton<INullChecker>(checker)
					.BuildServiceProvider(),
			};
			var result = await Reader.ReadAsync(context, JOE).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task Null_Test()
		{
			var result = await Reader.ReadAsync(Context, default(string)!).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task NullString_Test()
		{
			var result = await Reader.ReadAsync(Context, "null").ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsNull(result.Value);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await Reader.ReadAsync(Context, "1").ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.Value, typeof(int?));
			Assert.AreEqual(1, result.Value!.Value);
		}
	}
}