using Kodoku.Lib.Inventory;

[TestClass]
public sealed class InventoryActionResultTests
{
	[TestMethod]
	public void Ok_HasSuccessTrue()
	{
		var result = InventoryActionResult.Ok( "done" );

		Assert.IsTrue( result.Success );
		Assert.AreEqual( "done", result.Reason );
	}

	[TestMethod]
	public void Ok_DefaultReasonIsOk()
	{
		var result = InventoryActionResult.Ok();

		Assert.IsTrue( result.Success );
		Assert.AreEqual( "Ok", result.Reason );
	}

	[TestMethod]
	public void Fail_HasSuccessFalse()
	{
		var result = InventoryActionResult.Fail( "no space" );

		Assert.IsFalse( result.Success );
		Assert.AreEqual( "no space", result.Reason );
	}
}
