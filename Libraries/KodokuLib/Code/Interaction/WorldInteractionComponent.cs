using Sandbox;

namespace Kodoku.Lib.Interaction;

[Title( "Generic World Interaction Scanner" )]
[Category( "Kodoku/Interaction" )]
[Icon( "touch_app" )]
public sealed class WorldInteractionComponent : Component
{
	[Property] public CameraComponent ViewCamera { get; set; }
	[Property] public float TraceDistance { get; set; } = 180f;
	[Property] public string InteractAction { get; set; } = "Use";
	[Property] public string InteractKey { get; set; } = "E";
	[Property] public bool HandleInteractInput { get; set; } = true;
	[Property] public bool LogInputResults { get; set; } = true;
	[Property] public bool LogRaycastDebug { get; set; }

	public GameObject LookedAtObject { get; private set; }
	public bool HasLookTarget => LookedAtObject is not null && LookedAtObject.IsValid();
	public bool ShouldLogRaycastThisFrame => LogRaycastDebug && _raycastLogTimer >= 0.49f;

	bool _interactPressed;
	float _raycastLogTimer;

	protected override void OnStart()
	{
		ResolveCamera();
	}

	protected override void OnUpdate()
	{
		RefreshInteractionState();
	}

	public void RefreshInteractionState()
	{
		ResolveCamera();
		UpdateRaycastLogTimer();
		UpdateLookTarget();

		if ( ShouldLogRaycastThisFrame )
			LogRaycastState();

		_interactPressed = HandleInteractInput && WasPressed( InteractAction, InteractKey );
	}

	public bool ConsumeInteractPressed()
	{
		var pressed = _interactPressed;
		_interactPressed = false;
		return pressed;
	}

	public CameraComponent GetViewCamera()
	{
		ResolveCamera();
		return ViewCamera;
	}

	public Vector3 GetRangeCheckOrigin()
	{
		var camera = GetViewCamera();
		return camera is not null && camera.IsValid()
			? camera.WorldPosition
			: WorldPosition;
	}

	public bool IsInRange( Component component, float maxDistance )
	{
		if ( component is null || !component.IsValid() )
			return false;

		var distanceSquared = (component.WorldPosition - GetRangeCheckOrigin()).LengthSquared;
		return distanceSquared <= maxDistance * maxDistance;
	}

	public T FindLookedAtComponent<T>() where T : Component
	{
		return FindLookedAtComponent<T>( false );
	}

	public T FindLookedAtComponent<T>( bool refresh ) where T : Component
	{
		if ( refresh )
			UpdateLookTarget();

		return WorldInteractionQuery.FindComponentInHierarchy<T>( LookedAtObject );
	}

	public void UpdateLookTarget()
	{
		LookedAtObject = WorldInteractionQuery.FindLookedAtObject( Scene, GameObject, GetViewCamera(), TraceDistance );
	}

	void ResolveCamera()
	{
		if ( ViewCamera is null || !ViewCamera.IsValid() )
			ViewCamera = Scene.Camera;
	}

	void UpdateRaycastLogTimer()
	{
		if ( !LogRaycastDebug )
			return;

		_raycastLogTimer -= Time.Delta;
		if ( _raycastLogTimer <= 0f )
			_raycastLogTimer = 0.5f;
	}

	void LogRaycastState()
	{
		var camera = GetViewCamera();
		if ( camera is null || !camera.IsValid() )
		{
			Log.Info( "[Interaction Raycast] NO CAMERA" );
			return;
		}

		var start = camera.WorldPosition;
		var forward = camera.WorldRotation.Forward;
		var end = start + forward * TraceDistance;
		var playerRoot = GameObject.Parent ?? GameObject;

		Log.Info( $"[Interaction Raycast] CamPos={start:F0} Forward={forward:F2} TraceEnd={end:F0}" );
		Log.Info( $"[Interaction Raycast] PlayerRoot={playerRoot.Name} CamGO={camera.GameObject.Name}" );

		var trace = Scene.Trace
			.Ray( start, end )
			.IgnoreGameObjectHierarchy( playerRoot )
			.HitTriggers()
			.Run();

		if ( !trace.Hit )
		{
			Log.Info( "[Interaction Raycast] -> NO HIT" );
			return;
		}

		var hitObject = trace.Collider?.GameObject ?? trace.GameObject;
		Log.Info( $"[Interaction Raycast] -> Hit: '{hitObject?.Name}' HitPos={trace.HitPosition:F0} Dist={trace.Distance:F0}" );
	}

	bool WasPressed( string actionName, string keyName )
	{
		return WorldInteractionQuery.WasPressed( actionName, keyName );
	}
}
