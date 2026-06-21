using Sandbox;
using System.Linq;
using Kodoku.World;

namespace Kodoku.Core;

[Title( "Scene Loader" )]
[Category( "Kodoku/Core" )]
[Icon( "map" )]
public sealed class SceneLoaderComponent : Component
{
	[Property] public GameObject LoadedWorld { get; set; }

	[Property] public string DefaultScenePath { get; set; } = "scenes/world/base.scene";
	[Property] public string DefaultWorldId { get; set; } = "Base";

	private WorldRootComponent CurrentWorld { get; set; }

	protected override void OnStart()
	{
		LoadWorld( DefaultScenePath, DefaultWorldId );
	}

	public void LoadWorld( string scenePath, string worldId )
	{
		if ( LoadedWorld is null )
		{
			Log.Warning( "SceneLoader: LoadedWorld n'est pas assigné." );
			return;
		}

		// Supprime l'ancienne zone chargée
		LoadedWorld.Clear();

		var options = new SceneLoadOptions();
		options.SetScene( scenePath );
		options.IsAdditive = true;

		var loaded = Scene.Load( options );

		if ( !loaded )
		{
			Log.Warning( $"SceneLoader: impossible de charger la scène {scenePath}" );
			return;
		}

		var worldRoot = Scene.GetAll<WorldRootComponent>()
			.FirstOrDefault( x => x.WorldId == worldId );

		if ( worldRoot is null )
		{
			Log.Warning( $"SceneLoader: aucun WorldRootComponent trouvé pour WorldId = {worldId}" );
			return;
		}

		worldRoot.GameObject.SetParent( LoadedWorld, true );
		CurrentWorld = worldRoot;

		Log.Info( $"SceneLoader: scène chargée = {worldId}" );
	}
}
