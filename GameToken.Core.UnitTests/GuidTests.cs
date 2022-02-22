using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameToken.Core.UnitTests;

[TestClass]
public class GuidTests
{
	[TestMethod]
	public void GuidConvertToToken()
	{
		var guid = Guid.Parse("94DFD7BE-C2E3-4960-BA45-88AF42635E81");

		var expected = "94dfd7bec2e34960ba4588af42635e81";

		var actual = guid.ToString("N");

		Assert.AreEqual(expected, actual);
	}
}
