namespace Kodoku.Lib.Inventory;

public readonly struct InventoryActionResult
{
	public bool Success { get; }
	public string Reason { get; }

	private InventoryActionResult( bool success, string reason )
	{
		Success = success;
		Reason = reason;
	}

	public static InventoryActionResult Ok( string reason = "Ok" )
	{
		return new InventoryActionResult( true, reason );
	}

	public static InventoryActionResult Fail( string reason )
	{
		return new InventoryActionResult( false, reason );
	}

	public override string ToString()
	{
		return Success ? $"Success: {Reason}" : $"Failed: {Reason}";
	}
}
