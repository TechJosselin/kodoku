using Sandbox;

namespace Kodoku.Lib.GameMenu;

[Title( "Game Menu" )]
[Category( "Kodoku" )]
[Icon( "view_sidebar" )]
public sealed class GameMenuComponent : Component
{
	[Property] public bool OpenOnStart { get; set; }
	[Property] public GameMenuTab DefaultTab { get; set; } = GameMenuTab.Inventory;

	public GameMenuState State { get; } = new();
	public bool IsOpen => State.IsOpen;

	protected override void OnStart()
	{
		if ( OpenOnStart )
		{
			State.Open( DefaultTab );
		}
		else
		{
			State.Close();
			State.SetActiveTab( DefaultTab );
		}
	}

	public void Open()
	{
		State.Open( GameMenuTab.Inventory );
	}

	public void Close()
	{
		State.Close();
	}

	public void Toggle()
	{
		State.Toggle( GameMenuTab.Inventory );
	}
}
