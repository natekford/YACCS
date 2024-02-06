using Microsoft.VisualStudio.TestTools.UnitTesting;

using YACCS.NamedArguments;
using YACCS.Results;
using YACCS.Tests.TypeReaders;
using YACCS.TypeReaders;

namespace YACCS.Tests.NamedArguments;

[TestClass]
public class NamedTypeReader_Tests : TypeReader_Tests<NamedTypeReader_Tests.NamedClass>
{
	private const int NUM = 1;
	private const string STR = "joe";
	public override ITypeReader<NamedClass> Reader { get; }
		= new NamedArgumentsTypeReader<NamedClass>();

	[TestMethod]
	public async Task DuplicateKey_Test()
	{
		await AssertFailureAsync(new[]
		{
			nameof(NamedClass.Number),
			NUM.ToString(),
			nameof(NamedClass.String),
			STR,
			nameof(NamedClass.String),
			STR
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvalidKey_Test()
	{
		await AssertFailureAsync(new[]
		{
			nameof(NamedClass.Number),
			NUM.ToString(),
			nameof(NamedClass.String),
			STR,
			"test",
			STR
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task Success_Test()
	{
		var value = await AssertSuccessAsync(new[]
		{
			nameof(NamedClass.Number),
			NUM.ToString(),
			nameof(NamedClass.String),
			STR,
			nameof(NamedClass.FieldString),
			STR
		}).ConfigureAwait(false);
		Assert.AreEqual(NUM, value.Number);
		Assert.AreEqual(STR, value.String);
		Assert.AreEqual(STR, value.FieldString);
	}

	public class NamedClass
	{
		public string FieldString = "";
		public int Number { get; set; }
		public string String { get; set; } = "";
	}
}