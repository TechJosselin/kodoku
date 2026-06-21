using Sandbox;

namespace Kodoku.Lib.Interaction;

public static class WorldInteractionQuery
{
	public static GameObject FindLookedAtObject( Scene scene, GameObject owner, CameraComponent camera, float traceDistance )
	{
		if ( scene is null || owner is null || camera is null || !camera.IsValid() )
			return null;

		var start = camera.WorldPosition;
		var end = start + camera.WorldRotation.Forward * traceDistance;
		var playerRoot = owner.Parent ?? owner;

		var trace = scene.Trace
			.Ray( start, end )
			.IgnoreGameObjectHierarchy( playerRoot )
			.HitTriggers()
			.Run();

		if ( !trace.Hit )
			return null;

		var hitObject = trace.Collider?.GameObject ?? trace.GameObject;
		return hitObject is not null && hitObject.IsValid() ? hitObject : null;
	}

	public static T FindComponentInHierarchy<T>( GameObject hitObject ) where T : Component
	{
		if ( hitObject is null || !hitObject.IsValid() )
			return null;

		var component = hitObject.GetComponent<T>();
		if ( component is not null )
			return component;

		component = FindComponentInChildren<T>( hitObject );
		if ( component is not null )
			return component;

		// Only check the parent node itself — never scan parent's children,
		// which would find sibling objects the player isn't looking at.
		var parent = hitObject.Parent;
		while ( parent is not null && parent.IsValid() )
		{
			component = parent.GetComponent<T>();
			if ( component is not null )
				return component;

			parent = parent.Parent;
		}

		return null;
	}

	public static bool WasPressed( string actionName, string keyName )
	{
		if ( !string.IsNullOrWhiteSpace( actionName ) && Input.Pressed( actionName ) )
			return true;

		if ( !string.IsNullOrWhiteSpace( keyName ) && Input.Keyboard.Pressed( keyName ) )
			return true;

		return false;
	}

	static T FindComponentInChildren<T>( GameObject gameObject ) where T : Component
	{
		if ( gameObject is null || !gameObject.IsValid() )
			return null;

		foreach ( var child in gameObject.Children )
		{
			if ( child is null || !child.IsValid() )
				continue;

			var component = child.GetComponent<T>();
			if ( component is not null )
				return component;

			component = FindComponentInChildren<T>( child );
			if ( component is not null )
				return component;
		}

		return null;
	}
}
