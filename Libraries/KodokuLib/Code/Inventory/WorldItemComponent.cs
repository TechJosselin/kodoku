using System;
using Sandbox;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

[Title( "World Item" )]
[Category( "Inventory" )]
[Icon( "deployed_code" )]
public sealed class WorldItemComponent : Component, Component.ExecuteInEditor
{
	[Property] public ItemDefinition Definition { get; set; }
	[Property, Range( 1, 999 )] public int Quantity { get; set; } = 1;
	[Property] public bool AutoNameGameObject { get; set; } = true;
	[Property] public bool CreateDebugVisual { get; set; } = true;

	public ItemInstance Item { get; private set; }

	protected override void OnStart()
	{
		EnsureWorldItemSetup();
	}

	protected override void OnUpdate()
	{
		if ( Game.IsPlaying )
			return;

		EnsureWorldItemSetup();
	}

	public ItemInstance PeekItem()
	{
		EnsureWorldItemSetup();
		return Item;
	}

	public void SetExistingItem( ItemInstance item )
	{
		Item = item;
		Definition = item?.Definition;
		Quantity = item?.Quantity ?? 1;
		ApplyGameObjectName();
		EnsureDebugVisual();
	}

	public void ConsumeWorldItem()
	{
		Item = null;
		GameObject.Enabled = false;
		GameObject.Destroy();
	}

	public static WorldItemComponent SpawnDropped( Scene scene, Transform transform, ItemInstance item )
	{
		var gameObject = new GameObject( true, item?.DisplayName ?? "World Item" );
		gameObject.WorldTransform = transform;

		var worldItem = gameObject.Components.Create<WorldItemComponent>();
		worldItem.SetExistingItem( item );

		if ( scene is not null && gameObject.Scene != scene )
			gameObject.Parent = scene;

		return worldItem;
	}

	public static WorldItemComponent SpawnDebug( Scene scene, Vector3 position, ItemInstance item )
	{
		var transform = new Transform( position, Rotation.Identity, 1f );
		return SpawnDropped( scene, transform, item );
	}

	void EnsureWorldItemSetup()
	{
		EnsureItemInstance();
		ApplyGameObjectName();
		EnsureDebugVisual();
	}

	void EnsureItemInstance()
	{
		Quantity = GetClampedQuantity();

		if ( Definition is null )
		{
			Item = null;
			return;
		}

		if ( Item is not null && ReferenceEquals( Item.Definition, Definition ) && Item.Quantity == Quantity )
			return;

		Item = new ItemInstance( Definition, Quantity );
	}

	void ApplyGameObjectName()
	{
		if ( !AutoNameGameObject )
			return;

		GameObject.Name = Definition is null
			? "World Item (Missing Definition)"
			: $"World Item - {Definition.DisplayName}";
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

			var itemWidth = Definition?.GetWidth( false ) ?? 1;
			var itemHeight = Definition?.GetHeight( false ) ?? 1;
			var size = Definition?.ItemKind == InventoryItemKind.Backpack ? 0.55f : 0.28f;
			GameObject.LocalScale = new Vector3( size * itemWidth, size * itemHeight, size );
		}

		renderer.Tint = GetDebugTint();
	}

	int GetClampedQuantity()
	{
		if ( Definition is null )
			return Math.Max( 1, Quantity );

		return Math.Clamp( Quantity, 1, Definition.GetMaxStack() );
	}

	Color GetDebugTint()
	{
		if ( Definition is null )
			return Color.White;

		if ( Definition.ItemKind == InventoryItemKind.Backpack )
			return new Color( 0.25f, 0.55f, 0.95f );

		return Definition.IsStackable
			? new Color( 0.45f, 0.85f, 0.55f )
			: new Color( 0.95f, 0.75f, 0.35f );
	}
}
