using System;
using Sandbox;
using Kodoku.Lib.Items;

namespace Kodoku.Lib.Inventory;

[Title( "World Item" )]
[Category( "Kodoku/Inventory" )]
[Icon( "deployed_code" )]
public sealed class WorldItemComponent : Component, Component.ExecuteInEditor
{
	[Property] public ItemDefinition Definition { get; set; }
	[Property, Range( 1, 999 )] public int Quantity { get; set; } = 1;
	[Property] public bool AutoNameGameObject { get; set; } = true;
	[Property] public bool CreateDebugVisual { get; set; } = true;
	[Property] public bool AutoFitColliderToModel { get; set; } = true;
	[Property] public bool CreateColliderIfMissing { get; set; } = true;
	[Property] public bool OverrideExistingCollider { get; set; } = false;
	[Property] public float ColliderPadding { get; set; } = 2f;
	[Property] public Vector3 FallbackColliderSize { get; set; } = new Vector3( 16f, 16f, 16f );
	[Property] public bool EnablePhysics { get; set; } = true;

	public ItemInstance Item { get; private set; }

	const int MaxFitAttempts = 10;

	bool _colliderFitted;
	int _fitAttempts;
	BoxCollider _ourCollider;

	protected override void OnStart()
	{
		EnsureWorldItemSetup();
		TryFitCollider();
		EnsureRigidbody();
	}

	protected override void OnUpdate()
	{
		TryFitColliderIfNeeded();

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
		ResetColliderFit();
	}

	public void ConsumeWorldItem()
	{
		Item = null;
		GameObject.Enabled = false;
		GameObject.Destroy();
	}

	public static WorldItemComponent SpawnDropped( Scene scene, Transform transform, ItemInstance item )
	{
		if ( TrySpawnFromPrefab( scene, transform, item, out var prefabWorldItem ) )
			return prefabWorldItem;

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

	// Returns true when the item definition has a valid PrefabPath and the prefab could be instantiated.
	// Sets CreateDebugVisual = false on the resulting component because the prefab owns its own visuals.
	static bool TrySpawnFromPrefab( Scene scene, Transform transform, ItemInstance item, out WorldItemComponent worldItem )
	{
		worldItem = null;

		var prefabPath = item?.Definition?.PrefabPath;
		if ( string.IsNullOrWhiteSpace( prefabPath ) )
			return false;

		var prefabFile = ResourceLibrary.Get<PrefabFile>( prefabPath );
		if ( prefabFile is null )
			return false;

		var go = SceneUtility.GetPrefabScene( prefabFile )?.Clone();
		if ( go is null )
			return false;

		// Dropping must not resize the item — only reposition/reorient it.
		// Assigning WorldTransform wholesale would overwrite the prefab's own
		// authored scale with the dropper's (e.g. the player's) world scale.
		var authoredScale = go.LocalScale;
		go.WorldTransform = transform;

		if ( scene is not null && go.Scene != scene )
			go.Parent = scene;

		go.LocalScale = authoredScale;

		worldItem = go.Components.Get<WorldItemComponent>( FindMode.EnabledInSelfAndDescendants )
			?? go.Components.Create<WorldItemComponent>();

		// Prefab provides its own mesh and materials — skip the debug sphere / ModelPath visual.
		worldItem.CreateDebugVisual = false;
		worldItem.SetExistingItem( item );
		return true;
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
			if ( Item is not null )
			{
				Item = null;
				ResetColliderFit();
			}
			return;
		}

		if ( Item is not null && ReferenceEquals( Item.Definition, Definition ) && Item.Quantity == Quantity )
			return;

		var prevDef = Item?.Definition;
		Item = new ItemInstance( Definition, Quantity );

		if ( !ReferenceEquals( prevDef, Definition ) )
			ResetColliderFit();
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

			var model = TryLoadItemModel();
			if ( model is not null )
			{
				renderer.Model = model;
				return;
			}

			renderer.Model = Model.Sphere;
			var itemWidth = Definition?.GetWidth( false ) ?? 1;
			var itemHeight = Definition?.GetHeight( false ) ?? 1;
			var size = Definition?.ItemKind == InventoryItemKind.Backpack ? 0.55f : 0.28f;
			GameObject.LocalScale = new Vector3( size * itemWidth, size * itemHeight, size );
			renderer.Tint = GetDebugTint();
			return;
		}

		// Only the placeholder sphere (no real model available) gets the debug color.
		// A renderer already showing a real item model keeps its authored Tint.
		if ( renderer.Model == Model.Sphere )
			renderer.Tint = GetDebugTint();
	}

	Model TryLoadItemModel()
	{
		var path = Definition?.ModelPath;
		return string.IsNullOrWhiteSpace( path ) ? null : Model.Load( path );
	}

	void TryFitColliderIfNeeded()
	{
		if ( _colliderFitted || _fitAttempts >= MaxFitAttempts )
			return;

		TryFitCollider();
	}

	void TryFitCollider()
	{
		_fitAttempts++;

		// On the first attempt, resolve which collider we are working with.
		// Subsequent retries skip this phase and reuse _ourCollider directly.
		if ( _fitAttempts == 1 )
		{
			var existing = Components.Get<BoxCollider>();

			// Respect a pre-existing collider we did not create when override is disabled.
			if ( existing is not null && _ourCollider is null && !OverrideExistingCollider )
			{
				_colliderFitted = true;
				return;
			}

			if ( existing is not null )
			{
				_ourCollider = existing;
			}
			else if ( CreateColliderIfMissing )
			{
				_ourCollider = Components.Create<BoxCollider>();
				_ourCollider.Scale = FallbackColliderSize; // immediate fallback so raycast can hit the item
			}
			else
			{
				_colliderFitted = true;
				return;
			}

			if ( !AutoFitColliderToModel )
			{
				_ourCollider.Scale = FallbackColliderSize;
				_ourCollider.Center = Vector3.Zero;
				_colliderFitted = true;
				return;
			}
		}

		if ( _ourCollider is null || !_ourCollider.IsValid() )
		{
			_colliderFitted = true;
			return;
		}

		if ( TryComputeModelBounds( out var worldBounds ) )
		{
			ApplyBoundsToCollider( _ourCollider, worldBounds );
			_colliderFitted = true;
			return;
		}

		// Bounds not ready yet — retry next frame up to MaxFitAttempts.
		// Fallback size is already applied; the item remains detectable during retries.
		if ( _fitAttempts >= MaxFitAttempts )
			_colliderFitted = true;
	}

	bool TryComputeModelBounds( out BBox worldBounds )
	{
		worldBounds = default;
		var found = false;

		foreach ( var renderer in Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants ) )
		{
			if ( !renderer.IsValid() )
				continue;

			var b = renderer.Bounds;
			if ( b.Size.LengthSquared < 0.001f )
				continue;

			worldBounds = found ? EncapsulateBBox( worldBounds, b ) : b;
			found = true;
		}

		foreach ( var renderer in Components.GetAll<SkinnedModelRenderer>( FindMode.EnabledInSelfAndDescendants ) )
		{
			if ( !renderer.IsValid() )
				continue;

			var b = renderer.Bounds;
			if ( b.Size.LengthSquared < 0.001f )
				continue;

			worldBounds = found ? EncapsulateBBox( worldBounds, b ) : b;
			found = true;
		}

		return found;
	}

	void ApplyBoundsToCollider( BoxCollider collider, BBox worldBounds )
	{
		var worldScale = GameObject.WorldScale;
		var safeScale = new Vector3(
			Math.Abs( worldScale.x ) > 0.0001f ? worldScale.x : 1f,
			Math.Abs( worldScale.y ) > 0.0001f ? worldScale.y : 1f,
			Math.Abs( worldScale.z ) > 0.0001f ? worldScale.z : 1f
		);

		collider.Scale = (worldBounds.Size + Vector3.One * ColliderPadding) / safeScale;
		collider.Center = WorldTransform.PointToLocal( worldBounds.Center );
	}

	void ResetColliderFit()
	{
		_colliderFitted = false;
		_fitAttempts = 0;
		// _ourCollider is kept: if we already own a collider it will be reused on next fit.
	}

	static BBox EncapsulateBBox( BBox a, BBox b )
	{
		return new BBox(
			new Vector3(
				Math.Min( a.Mins.x, b.Mins.x ),
				Math.Min( a.Mins.y, b.Mins.y ),
				Math.Min( a.Mins.z, b.Mins.z )
			),
			new Vector3(
				Math.Max( a.Maxs.x, b.Maxs.x ),
				Math.Max( a.Maxs.y, b.Maxs.y ),
				Math.Max( a.Maxs.z, b.Maxs.z )
			)
		);
	}

	void EnsureRigidbody()
	{
		if ( !EnablePhysics )
			return;

		if ( !Components.Get<Rigidbody>().IsValid() )
			Components.Create<Rigidbody>();
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
