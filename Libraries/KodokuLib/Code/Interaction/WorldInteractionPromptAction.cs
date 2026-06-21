namespace Kodoku.Lib.Interaction;

public enum WorldInteractionActionType
{
	Pickup,
	Equip,
	OpenLoot
}

public sealed class WorldInteractionPromptAction
{
	public WorldInteractionActionType Type { get; }
	public string Label { get; }

	public WorldInteractionPromptAction( WorldInteractionActionType type, string label )
	{
		Type = type;
		Label = label;
	}
}
