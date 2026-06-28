using Kodoku.Lib.Items;
using Kodoku.Lib.Inventory;

// Tests covering drop / pickup invariants that can be verified without a live Scene.
//
// Component-level tests (InventoryComponent, WorldItemComponent) require a Sandbox Scene
// context and must be verified manually in the s&box editor:
//
//   [Drop - item not found]        TryDropItemToWorld returns Fail when itemId is unknown.
//   [Drop - spawn failure]         TryDropItemToWorld restores the item to inventory when
//                                  SpawnDropped returns null (cannot be triggered from a unit test
//                                  because the fallback GameObject creation never fails in-engine).
//   [Drop - full round-trip]       Item dropped then picked up reappears in inventory with the
//                                  same Definition and Quantity.
//   [Pickup - full inventory]      TryPickupWorldItem returns Fail and leaves the WorldItem alive
//                                  when no active container has space.
//   [Drop - prefab path]           SpawnDropped instantiates the prefab when PrefabPath is non-empty
//                                  and the asset exists; the resulting WorldItemComponent.Item
//                                  is the exact ItemInstance that was passed.
//   [Drop - no prefab fallback]    SpawnDropped falls back to a bare GameObject+WorldItemComponent
//                                  when PrefabPath is empty.

[TestClass]
public sealed class WorldItemDropTests
{
	static ItemDefinition MakeDef( string name, int w = 1, int h = 1, string prefabPath = "" )
	{
		return new ItemDefinition
		{
			DisplayName = name,
			Width = w,
			Height = h,
			PrefabPath = prefabPath,
		};
	}

	static InventoryContainer MakeContainer( int w = 4, int h = 4 )
		=> new InventoryContainer( "pockets", "Pockets", InventoryContainerKind.Pockets, w, h );

	// ── Drop preconditions ───────────────────────────────────────────────────

	[TestMethod]
	public void DroppedItemInstance_IsValid_BeforeSpawn()
	{
		var def = MakeDef( "Water Bottle", w: 1, h: 2 );
		var item = new ItemInstance( def, 1 );

		Assert.IsTrue( item.IsValid );
		Assert.AreEqual( "Water Bottle", item.DisplayName );
		Assert.AreSame( def, item.Definition );
	}

	[TestMethod]
	public void Container_Remove_ReturnsExactInstance()
	{
		// The item extracted for dropping must be the exact reference stored in the grid,
		// not a copy — SetExistingItem will bind it to the WorldItemComponent.
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Bandage" ), 1 );
		container.TryAddItem( item, false, out _ );

		container.TryRemoveItem( item.InstanceId, out var removed );

		Assert.AreSame( item, removed );
	}

	[TestMethod]
	public void Container_Remove_ItemRemainsValid_AfterExtraction()
	{
		// The item must still be valid after removal so it can be passed to SpawnDropped.
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Bandage" ), 1 );
		container.TryAddItem( item, false, out _ );
		container.TryRemoveItem( item.InstanceId, out var removed );

		Assert.IsTrue( removed.IsValid );
	}

	[TestMethod]
	public void Container_Remove_EmptiesSlot()
	{
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Bandage" ), 1 );
		container.TryAddItem( item, false, out _ );
		container.TryRemoveItem( item.InstanceId, out _ );

		Assert.IsTrue( container.IsEmpty );
	}

	// ── Pickup preconditions ─────────────────────────────────────────────────

	[TestMethod]
	public void Container_Full_RefusesAdd()
	{
		// Simulates inventory-full: TryPickupWorldItem calls TryAddItem which
		// iterates ActiveContainers; if all refuse, pickup fails and WorldItem is NOT consumed.
		var container = MakeContainer( 1, 1 );
		var blocker = new ItemInstance( MakeDef( "Block", 1, 1 ), 1 );
		container.TryAddItem( blocker, false, out _ );

		var incoming = new ItemInstance( MakeDef( "Water", 1, 1 ), 1 );
		var result = container.TryAddItem( incoming, false, out _ );

		Assert.IsFalse( result.Success );
	}

	[TestMethod]
	public void Container_Full_ItemInstanceUnchanged_AfterFailedAdd()
	{
		// The incoming item must remain valid after a failed add so that ConsumeWorldItem
		// is never reached and the WorldItemComponent survives intact.
		var container = MakeContainer( 1, 1 );
		container.TryAddItem( new ItemInstance( MakeDef( "Block", 1, 1 ), 1 ), false, out _ );

		var worldItemData = new ItemInstance( MakeDef( "Water", 1, 1 ), 1 );
		container.TryAddItem( worldItemData, false, out _ );

		Assert.IsTrue( worldItemData.IsValid );
		Assert.AreEqual( 1, worldItemData.Quantity );
	}

	// ── PrefabPath data ──────────────────────────────────────────────────────

	[TestMethod]
	public void ItemDefinition_PrefabPath_DefaultIsEmpty()
	{
		// When PrefabPath is empty, TrySpawnFromPrefab returns false and the
		// bare-GameObject fallback is used — no unexpected prefab load.
		var def = new ItemDefinition();

		Assert.AreEqual( "", def.PrefabPath );
	}

	[TestMethod]
	public void ItemDefinition_PrefabPath_StoredCorrectly()
	{
		const string path = "prefabs/items/consumables/drinks/waterbottle/water_bottle.prefab";
		var def = new ItemDefinition { PrefabPath = path };

		Assert.AreEqual( path, def.PrefabPath );
	}

	// ── Rollback invariant (pure-logic layer) ────────────────────────────────

	[TestMethod]
	public void Container_AddAfterRollback_ReacceptsItem()
	{
		// Mirrors the rollback path in TryDropItemToWorld: if SpawnDropped fails,
		// TryAddItem is called on the extracted item. The container must accept it again.
		var container = MakeContainer();
		var item = new ItemInstance( MakeDef( "Bandage" ), 1 );
		container.TryAddItem( item, false, out _ );

		container.TryRemoveItem( item.InstanceId, out var extracted );

		// Simulate rollback
		var rollback = container.TryAddItem( extracted, false, out _ );

		Assert.IsTrue( rollback.Success );
		Assert.AreEqual( 1, container.Placements.Count );
	}
}
