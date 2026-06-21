using System;
using System.Collections.Generic;

namespace Kodoku.Lib.GameMenu;

public sealed class GameMenuState
{
	public NavigationMenuState Navigation { get; } = new();
	public InventoryMenuState Inventory { get; } = new();

	public static IReadOnlyList<GameMenuTab> DefaultTabs => NavigationMenuState.DefaultTabs;

	public event Action ActiveTabChanged
	{
		add => Navigation.ActiveTabChanged += value;
		remove => Navigation.ActiveTabChanged -= value;
	}

	public event Action OpenStateChanged
	{
		add => Navigation.OpenStateChanged += value;
		remove => Navigation.OpenStateChanged -= value;
	}

	public GameMenuTab ActiveTab => Navigation.ActiveTab;
	public bool IsOpen => Navigation.IsOpen;

	public void SetActiveTab( GameMenuTab tab )
	{
		Navigation.SetActiveTab( tab );
	}

	public void Open( GameMenuTab tab = GameMenuTab.Inventory )
	{
		Navigation.Open( tab );
	}

	public void Close()
	{
		Navigation.Close();
	}

	public void Toggle( GameMenuTab openTab = GameMenuTab.Inventory )
	{
		Navigation.Toggle( openTab );
	}

	public static string GetTabTitle( GameMenuTab tab )
	{
		return NavigationMenuState.GetTabTitle( tab );
	}
}
