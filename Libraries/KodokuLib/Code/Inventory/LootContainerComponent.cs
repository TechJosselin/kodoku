using System;
using Sandbox;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

public sealed class LootContainerInitialItem
{
	[Property] public ItemDefinition Definition { get; set; }
	[Property, Range( 1, 999 )] public int Quantity { get; set; } = 1;
	[Property] public bool Rotated { get; set; }
}

[Title( "Loot Container" )]
[Category( "Kodoku/Inventory" )]
[Icon( "inventory" )]
public sealed class LootContainerComponent : Component, Component.ExecuteInEditor
{
	[Property] public string ContainerName { get; set; } = "Loot Container";
	[Property, Range( 1, 6 )] public int Width { get; set; } = 4;
	[Property, Range( 1, 12 )] public int Height { get; set; } = 4;
	[Property] public List<LootContainerInitialItem> InitialItems { get; set; } = new();
	[Property] public bool SeedInitialItemsOnStart { get; set; } = true;
	[Property] public bool AutoNameGameObject { get; set; } = true;
	[Property] public bool CreateDebugVisual { get; set; } = true;
	[Property] public bool LogSetupResults { get; set; } = true;

	public InventoryContainer Container { get; private set; }

	bool SeededInitialItems { get; set; }

	protected override void OnStart()
	{
		EnsureSetup();

		if ( SeedInitialItemsOnStart && !SeededInitialItems )
			SeedInitialItems();
	}

	protected override void OnUpdate()
	{
		if ( Game.IsPlaying )
			return;

		EnsureSetup();
	}

	public void EnsureInitialized()
	{
		EnsureSetup();
	}

	public InventoryActionResult TryMoveItem( string itemId, int x, int y, bool rotated )
	{
		EnsureSetup();
		return Container.TryMoveItem( itemId, x, y, rotated );
	}

	public InventoryActionResult TryAddItemAt( ItemInstance item, int x, int y, bool rotated, out InventoryItemPlacement placement )
	{
		EnsureSetup();
		return Container.TryAddItemAt( item, x, y, rotated, out placement );
	}

	public InventoryActionResult TryRemoveItem( string itemId, out ItemInstance item )
	{
		EnsureSetup();
		return Container.TryRemoveItem( itemId, out item );
	}

	void EnsureSetup()
	{
		Width = Math.Max( 1, Width );
		Height = Math.Max( 1, Height );

		if ( Container is null || Container.Width != Width || Container.Height != Height || Container.DisplayName != ContainerName )
		{
			Container = new InventoryContainer( BuildContainerId(), ContainerName, InventoryContainerKind.Loot, Width, Height );
			SeededInitialItems = false;
		}

		ApplyGameObjectName();
		EnsureDebugVisual();
	}

	void SeedInitialItems()
	{
		EnsureSetup();
		SeededInitialItems = true;

		foreach ( var entry in InitialItems )
		{
			if ( entry?.Definition is null )
				continue;

			var item = new ItemInstance( entry.Definition, entry.Quantity );
			var result = Container.TryAddItem( item, entry.Rotated, out _ );

			if ( LogSetupResults && !result.Success )
				Log.Warning( $"LootContainer '{ContainerName}' could not seed {entry.Definition.DisplayName}: {result.Reason}" );
		}
	}

	void ApplyGameObjectName()
	{
		if ( AutoNameGameObject )
			GameObject.Name = ContainerName;
	}

	void EnsureDebugVisual()
	{
		if ( !CreateDebugVisual )
			return;

		var renderer = Components.Get<ModelRenderer>();
		if ( !renderer.IsValid() )
		{
			renderer = Components.Create<ModelRenderer>();
			renderer.Model = Model.Sphere;
		}

		renderer.Tint = new Color( 0.55f, 0.45f, 0.25f );
	}

	string BuildContainerId()
	{
		return $"loot_{GetHashCode():x}";
	}
}
