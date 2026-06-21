using Sandbox;

namespace Kodoku.World;

public sealed class WorldRootComponent : Component
{
	[Property] public string WorldId { get; set; } = "Base";
}
