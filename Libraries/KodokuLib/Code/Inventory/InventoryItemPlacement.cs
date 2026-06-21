using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

public sealed class InventoryItemPlacement
{
	public ItemInstance Item { get; }
	public int X { get; private set; }
	public int Y { get; private set; }
	public bool Rotated { get; private set; }

	public InventoryItemPlacement( ItemInstance item, int x, int y, bool rotated )
	{
		Item = item;
		X = x;
		Y = y;
		Rotated = rotated;
	}

	public int Width => Item?.Definition?.GetWidth( Rotated ) ?? 0;
	public int Height => Item?.Definition?.GetHeight( Rotated ) ?? 0;

	public bool ContainsCell( int x, int y )
	{
		return x >= X
			&& y >= Y
			&& x < X + Width
			&& y < Y + Height;
	}

	internal void MoveTo( int x, int y, bool rotated )
	{
		X = x;
		Y = y;
		Rotated = rotated;
	}
}
