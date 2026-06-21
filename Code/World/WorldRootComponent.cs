using Sandbox;

namespace Kodoku.World;

[Title( "World Root" )]
[Category( "Kodoku/Core" )]
[Icon( "public" )]
public sealed class WorldRootComponent : Component
{
	[Property] public string WorldId { get; set; } = "Base";
}
