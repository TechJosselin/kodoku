using Kodoku.Lib.Interaction;
using Kodoku.Lib.Inventory;
using Kodoku.Lib.UI;
using Sandbox;

namespace Kodoku.Glue;

[Title( "Player Inventory Interaction Bridge" )]
[Category( "Kodoku/Interaction" )]
[Icon( "touch_app" )]
public sealed class WorldInventoryInteractionBridge : Component
{
	[Property] public CameraComponent ViewCamera { get; set; }
	[Property] public float TraceDistance { get; set; } = 180f;
	[Property] public string InteractAction { get; set; } = "Use";
	[Property] public string InteractKey { get; set; } = "E";
	[Property] public bool HandleInteractInput { get; set; } = true;
	[Property] public bool LogRaycastDebug { get; set; }
	[Property] public bool LogResults { get; set; } = true;
	[Property] public float HudAnchorHeight { get; set; } = 36f;

	GameObject LookedAtObject { get; set; }
	float RaycastLogTimer { get; set; }
	InventoryComponent Inventory { get; set; }
	InventoryPlayerInteractionComponent InventoryInteractor { get; set; }
	GameMenuUI MenuUi { get; set; }
	WorldInteractionHud InteractionHud { get; set; }
	GameObject PromptTarget { get; set; }
	int SelectedActionIndex { get; set; }

	protected override void OnStart()
	{
		ResolveReferences();
	}

	protected override void OnUpdate()
	{
		if ( !ResolveReferences() )
			return;

		ResolveCamera();
		UpdateLookTarget();

		if ( ShouldLogRaycastThisFrame() )
			LogRaycastState();

		var worldItem = WorldInteractionQuery.FindComponentInHierarchy<WorldItemComponent>( LookedAtObject );
		var lootContainer = worldItem is null
			? WorldInteractionQuery.FindComponentInHierarchy<LootContainerComponent>( LookedAtObject )
			: null;

		InventoryInteractor.SetWorldInteractionContext(
			GetRangeCheckOrigin(),
			GetInteractionRange(),
			GetDropTransform() );
		InventoryInteractor.SetLookedAtWorldObjects( worldItem, lootContainer );

		var actions = BuildActions( worldItem, lootContainer );
		UpdateSelectedAction( LookedAtObject, actions.Count );
		UpdateInteractionHud( worldItem, lootContainer, actions );

		if ( HandleInteractInput && WorldInteractionQuery.WasPressed( InteractAction, InteractKey ) )
		{
			var result = ExecuteSelectedAction( worldItem, lootContainer, actions );
			Run( result );

			if ( result.Success && lootContainer is not null && lootContainer.IsValid() )
				OpenInventoryPage();
		}
	}

	bool ResolveReferences()
	{
		if ( Inventory is null || !Inventory.IsValid() )
			Inventory = FindPreferredInventory();

		if ( Inventory is null || !Inventory.IsValid() )
			return false;

		Inventory.EnsureInitialized();

		if ( InventoryInteractor is null || !InventoryInteractor.IsValid() )
		{
			InventoryInteractor = Scene.GetAllComponents<InventoryPlayerInteractionComponent>()
				.FirstOrDefault( candidate => candidate.Inventory == Inventory && candidate.GameObject == Inventory.GameObject );

			if ( InventoryInteractor is null || !InventoryInteractor.IsValid() )
				InventoryInteractor = Scene.GetAllComponents<InventoryPlayerInteractionComponent>()
					.FirstOrDefault( candidate => candidate.Inventory == Inventory );

			if ( InventoryInteractor is null || !InventoryInteractor.IsValid() )
				InventoryInteractor = Inventory.GameObject.Components.Get<InventoryPlayerInteractionComponent>();

			if ( InventoryInteractor is null || !InventoryInteractor.IsValid() )
			{
				Log.Warning( "[WorldInventoryInteractionBridge] InventoryPlayerInteractionComponent not found in scene — creating one automatically. Add it to the Inventory GameObject for explicit configuration." );
				InventoryInteractor = Inventory.GameObject.Components.Create<InventoryPlayerInteractionComponent>();
			}
		}

		if ( InventoryInteractor is null || !InventoryInteractor.IsValid() )
			return false;

		InventoryInteractor.Inventory = Inventory;

		if ( MenuUi is null || !MenuUi.IsValid() )
			MenuUi = Scene.GetAllComponents<GameMenuUI>().FirstOrDefault();

		if ( InteractionHud is null || !InteractionHud.IsValid() )
			InteractionHud = Scene.GetAllComponents<WorldInteractionHud>().FirstOrDefault();

		if ( InteractionHud is null || !InteractionHud.IsValid() )
		{
			var hudParent = MenuUi is not null && MenuUi.IsValid()
				? MenuUi.GameObject
				: GameObject;
			InteractionHud = hudParent.Components.Create<WorldInteractionHud>();
		}

		return true;
	}

	InventoryComponent FindPreferredInventory()
	{
		return Scene.GetAllComponents<InventoryComponent>()
			.FirstOrDefault( candidate => candidate.GameObject?.Name == "Inventory" )
			?? Scene.GetAllComponents<InventoryComponent>().FirstOrDefault();
	}

	void ResolveCamera()
	{
		if ( ViewCamera is null || !ViewCamera.IsValid() )
			ViewCamera = Scene.Camera;
	}

	void UpdateLookTarget()
	{
		LookedAtObject = WorldInteractionQuery.FindLookedAtObject( Scene, GameObject, ViewCamera, TraceDistance );
	}

	List<WorldInteractionPromptAction> BuildActions( WorldItemComponent worldItem, LootContainerComponent lootContainer )
	{
		var actions = new List<WorldInteractionPromptAction>();

		if ( worldItem is not null && worldItem.IsValid() )
		{
			var item = worldItem.PeekItem();
			if ( item is null || !item.IsValid )
				return actions;

			actions.Add( new WorldInteractionPromptAction( WorldInteractionActionType.Pickup, "Pick up" ) );

			if ( Inventory?.FindFirstAvailableEquipmentSlot( item ).HasValue == true )
				actions.Add( new WorldInteractionPromptAction( WorldInteractionActionType.Equip, "Equip" ) );

			return actions;
		}

		if ( lootContainer is not null && lootContainer.IsValid() )
			actions.Add( new WorldInteractionPromptAction( WorldInteractionActionType.OpenLoot, "Open" ) );

		return actions;
	}

	void UpdateSelectedAction( GameObject target, int actionCount )
	{
		if ( target != PromptTarget )
		{
			PromptTarget = target;
			SelectedActionIndex = 0;
		}

		if ( actionCount <= 0 )
		{
			SelectedActionIndex = 0;
			return;
		}

		if ( SelectedActionIndex >= actionCount )
			SelectedActionIndex = 0;

		if ( actionCount <= 1 || Input.MouseWheel.y == 0f )
			return;

		SelectedActionIndex += Input.MouseWheel.y > 0f ? -1 : 1;

		if ( SelectedActionIndex < 0 )
			SelectedActionIndex = actionCount - 1;

		if ( SelectedActionIndex >= actionCount )
			SelectedActionIndex = 0;
	}

	void UpdateInteractionHud( WorldItemComponent worldItem, LootContainerComponent lootContainer, IReadOnlyList<WorldInteractionPromptAction> actions )
	{
		if ( InteractionHud is null || !InteractionHud.IsValid() )
			return;

		if ( MenuUi?.IsOpen == true || actions.Count == 0 )
		{
			InteractionHud.HidePrompt();
			return;
		}

		if ( !TryGetPromptScreenPosition( worldItem, lootContainer, out var screenPosition ) )
			screenPosition = GetFallbackPromptScreenPosition();

		InteractionHud.ShowPrompt( GetPromptTargetName( worldItem, lootContainer ), screenPosition, actions, SelectedActionIndex );
	}

	bool TryGetPromptScreenPosition( WorldItemComponent worldItem, LootContainerComponent lootContainer, out Vector2 screenPosition )
	{
		screenPosition = default;
		ResolveCamera();

		if ( ViewCamera is null || !ViewCamera.IsValid() )
			return false;

		var targetPosition = worldItem is not null && worldItem.IsValid()
			? worldItem.WorldPosition
			: lootContainer is not null && lootContainer.IsValid()
				? lootContainer.WorldPosition
				: Vector3.Zero;

		if ( targetPosition == Vector3.Zero )
			return false;

		var bounds = BBox.FromPositionAndSize(
			targetPosition + Vector3.Up * (HudAnchorHeight * 0.5f),
			new Vector3( 32f, 32f, HudAnchorHeight ) );
		var screenRect = ViewCamera.BBoxToScreenPixels( bounds, out var isOffscreen );
		screenPosition = new Vector2( screenRect.Center.x, screenRect.Top - 12f );

		if ( !IsFinite( screenPosition ) )
			return false;

		if ( isOffscreen )
			return false;

		screenPosition = ClampToScreen( screenPosition );
		return true;
	}

	Vector2 GetFallbackPromptScreenPosition()
	{
		return new Vector2( Screen.Width * 0.5f, Screen.Height * 0.54f );
	}

	Vector2 ClampToScreen( Vector2 screenPosition )
	{
		var maxX = System.Math.Max( 64f, Screen.Width - 64f );
		var maxY = System.Math.Max( 64f, Screen.Height - 64f );
		return new Vector2(
			System.Math.Clamp( screenPosition.x, 64f, maxX ),
			System.Math.Clamp( screenPosition.y, 64f, maxY ) );
	}

	static bool IsFinite( Vector2 value )
	{
		return !float.IsNaN( value.x )
			&& !float.IsNaN( value.y )
			&& !float.IsInfinity( value.x )
			&& !float.IsInfinity( value.y );
	}

	string GetPromptTargetName( WorldItemComponent worldItem, LootContainerComponent lootContainer )
	{
		if ( worldItem is not null && worldItem.IsValid() )
			return worldItem.PeekItem()?.DisplayName ?? worldItem.GameObject.Name;

		if ( lootContainer is not null && lootContainer.IsValid() )
			return lootContainer.ContainerName;

		return "";
	}

	InventoryActionResult ExecuteSelectedAction( WorldItemComponent worldItem, LootContainerComponent lootContainer, IReadOnlyList<WorldInteractionPromptAction> actions )
	{
		if ( actions.Count == 0 )
			return InventoryInteractor.RequestUseLookedAtObject();

		var action = actions[SelectedActionIndex.Clamp( 0, actions.Count - 1 )];
		return action.Type switch
		{
			WorldInteractionActionType.Pickup => InventoryInteractor.RequestStoreWorldItem( worldItem ),
			WorldInteractionActionType.Equip => InventoryInteractor.RequestEquipWorldItem( worldItem ),
			WorldInteractionActionType.OpenLoot => InventoryInteractor.RequestOpenLootContainer( lootContainer ),
			_ => InventoryInteractor.RequestUseLookedAtObject()
		};
	}

	Vector3 GetRangeCheckOrigin()
	{
		ResolveCamera();
		return ViewCamera is not null && ViewCamera.IsValid()
			? ViewCamera.WorldPosition
			: WorldPosition;
	}

	float GetInteractionRange()
	{
		var traceRange = TraceDistance > 0f ? TraceDistance : 180f;
		if ( Inventory is not null && Inventory.IsValid() && Inventory.WorldItemPickupRadius > 0f )
			return System.Math.Max( Inventory.WorldItemPickupRadius, traceRange );

		return traceRange;
	}

	Transform GetDropTransform()
	{
		ResolveCamera();

		if ( ViewCamera is not null && ViewCamera.IsValid() )
		{
			var position = ViewCamera.WorldPosition
				+ ViewCamera.WorldRotation.Forward * InventoryInteractor.DropDistance
				+ Vector3.Up * InventoryInteractor.DropUpOffset;

			return new Transform( position, Rotation.Identity, 1f );
		}

		return WorldTransform;
	}

	void Run( InventoryActionResult result )
	{
		if ( LogResults )
			Log.Info( result.ToString() );
	}

	void OpenInventoryPage()
	{
		if ( MenuUi is null || !MenuUi.IsValid() )
			MenuUi = Scene.GetAllComponents<GameMenuUI>().FirstOrDefault();

		MenuUi?.OpenInventory();
	}

	bool ShouldLogRaycastThisFrame()
	{
		if ( !LogRaycastDebug )
			return false;

		RaycastLogTimer -= Time.Delta;
		if ( RaycastLogTimer > 0f )
			return false;

		RaycastLogTimer = 0.5f;
		return true;
	}

	void LogRaycastState()
	{
		ResolveCamera();
		if ( ViewCamera is null || !ViewCamera.IsValid() )
		{
			Log.Info( "[Interaction Raycast] NO CAMERA" );
			return;
		}

		var start = ViewCamera.WorldPosition;
		var forward = ViewCamera.WorldRotation.Forward;
		var end = start + forward * TraceDistance;
		var playerRoot = GameObject.Parent ?? GameObject;

		Log.Info( $"[Interaction Raycast] CamPos={start:F0} Forward={forward:F2} TraceEnd={end:F0}" );
		Log.Info( $"[Interaction Raycast] PlayerRoot={playerRoot.Name} CamGO={ViewCamera.GameObject.Name}" );

		if ( LookedAtObject is null || !LookedAtObject.IsValid() )
		{
			Log.Info( "[Interaction Raycast] -> NO HIT" );
			return;
		}

		Log.Info( $"[Interaction Raycast] -> Hit: '{LookedAtObject.Name}'" );
	}
}
