using Sandbox;
using System;
using System.Linq;
using Kodoku.World;

namespace Kodoku.Core;

[Title( "Scene Loader" )]
[Category( "Kodoku/Core" )]
[Icon( "map" )]
public sealed class SceneLoaderComponent : Component
{
	[Property] public GameObject LoadedWorld { get; set; }

	[Property] public SceneFile DefaultScene { get; set; }
	[Property] public string DefaultWorldId { get; set; } = "Base";
	[Property, Range( 1, 20 )] public int MaxLoadAttempts { get; set; } = 10;
	[Property, Range( 0.1f, 5f )] public float LoadRetryDelay { get; set; } = 0.5f;

	private WorldRootComponent CurrentWorld { get; set; }
	SceneFile _pendingScene;
	string _pendingWorldId;
	int _loadAttempt;
	float _retryDelayRemaining;

	protected override void OnStart()
	{
		LoadWorld( DefaultScene, DefaultWorldId );
	}

	protected override void OnUpdate()
	{
		if ( _pendingScene is null )
			return;

		_retryDelayRemaining -= Time.Delta;
		if ( _retryDelayRemaining <= 0f )
			TryLoadPendingWorld();
	}

	public void LoadWorld( SceneFile sceneFile, string worldId )
	{
		if ( LoadedWorld is null )
		{
			Log.Warning( "SceneLoader: LoadedWorld n'est pas assigné." );
			return;
		}

		if ( sceneFile is null )
		{
			Log.Warning( "SceneLoader: aucune scène n'est assignée." );
			return;
		}

		_pendingScene = sceneFile;
		_pendingWorldId = worldId;
		_loadAttempt = 0;
		_retryDelayRemaining = 0f;
		TryLoadPendingWorld();
	}

	void TryLoadPendingWorld()
	{
		_loadAttempt++;
		if ( !_pendingScene.IsValid() )
		{
			ScheduleRetryOrStop( "la scène assignée n'est pas encore disponible" );
			return;
		}

		var existingWorldRoots = Scene.GetAll<WorldRootComponent>().ToHashSet();

		var options = new SceneLoadOptions();
		options.SetScene( _pendingScene );
		options.IsAdditive = true;

		var loaded = Scene.Load( options );

		if ( !loaded )
		{
			ScheduleRetryOrStop( "impossible de charger la scène assignée" );
			return;
		}

		var worldRoot = Scene.GetAll<WorldRootComponent>()
			.FirstOrDefault( x => !existingWorldRoots.Contains( x ) && x.WorldId == _pendingWorldId );

		if ( worldRoot is null )
		{
			Log.Warning( $"SceneLoader: aucun nouveau WorldRootComponent trouvé pour WorldId = {_pendingWorldId}." );
			ClearPendingLoad();
			return;
		}

		LoadedWorld.Clear();
		worldRoot.GameObject.SetParent( LoadedWorld, true );
		CurrentWorld = worldRoot;

		Log.Info( $"SceneLoader: scène chargée = {_pendingWorldId}" );
		ClearPendingLoad();
	}

	void ScheduleRetryOrStop( string reason )
	{
		if ( _loadAttempt >= Math.Max( 1, MaxLoadAttempts ) )
		{
			Log.Warning( $"SceneLoader: {reason} après {_loadAttempt} tentative(s)." );
			ClearPendingLoad();
			return;
		}

		Log.Warning( $"SceneLoader: {reason}; nouvelle tentative {_loadAttempt + 1}/{MaxLoadAttempts}." );
		_retryDelayRemaining = Math.Max( 0.1f, LoadRetryDelay );
	}

	void ClearPendingLoad()
	{
		_pendingScene = null;
		_pendingWorldId = null;
		_loadAttempt = 0;
		_retryDelayRemaining = 0f;
	}

}
