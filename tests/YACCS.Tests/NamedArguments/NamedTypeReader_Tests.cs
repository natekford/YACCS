using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.NamedArguments;
using YACCS.Results;
using YACCS.Tests.TypeReaders;
using YACCS.TypeReaders;

namespace YACCS.Tests.NamedArguments
{
	[TestClass]
	public class NamedTypeReader_Tests : TypeReader_Tests<NamedTypeReader_Tests.NamedClass>
	{
		public override ITypeReader<NamedClass> Reader { get; }
			= new NamedArgumentTypeReader<NamedClass>();

		[TestMethod]
		public async Task DuplicateKey_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = new[]
			{
				nameof(NamedClass.Number),
				NUM.ToString(),
				nameof(NamedClass.String),
				STR,
				nameof(NamedClass.String),
				STR
			};

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(NamedArgDuplicateResult));
		}

		[TestMethod]
		public async Task InvalidKey_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = new[]
			{
				nameof(NamedClass.Number),
				NUM.ToString(),
				nameof(NamedClass.String),
				STR,
				"test",
				STR
			};

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsFalse(result.InnerResult.IsSuccess);
			Assert.IsInstanceOfType(result.InnerResult, typeof(NamedArgNonExistentResult));
		}

		[TestMethod]
		public async Task Success_Test()
		{
			const int NUM = 1;
			const string STR = "joe";
			var input = new[]
			{
				nameof(NamedClass.Number),
				NUM.ToString(),
				nameof(NamedClass.String),
				STR,
				nameof(NamedClass.FieldString),
				STR
			};

			var context = Create();
			var result = await Reader.ReadAsync(context, input).ConfigureAwait(false);
			Assert.IsTrue(result.InnerResult.IsSuccess);
			if (result.Value is null)
			{
				Assert.Fail();
				return;
			}
			Assert.AreEqual(NUM, result.Value.Number);
			Assert.AreEqual(STR, result.Value.String);
			Assert.AreEqual(STR, result.Value.FieldString);
		}

		private FakeContext Create()
		{
			return new FakeContext()
			{
				Services = Utils.CreateServices(),
			};
		}

		public class NamedClass
		{
			public string FieldString = "";
			public int Number { get; set; }
			public string String { get; set; } = "";
		}
	}
}