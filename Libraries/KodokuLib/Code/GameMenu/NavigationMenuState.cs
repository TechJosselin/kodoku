using System;
using System.Collections.Generic;

namespace Kodoku.Lib.GameMenu;

public sealed class NavigationMenuState
{
	public static IReadOnlyList<GameMenuTab> DefaultTabs { get; } =
		new[]
		{
			GameMenuTab.Inventory,
			GameMenuTab.Stats,
			GameMenuTab.Quests,
			GameMenuTab.Map,
			GameMenuTab.Options
		};

	public event Action ActiveTabChanged;
	public event Action OpenStateChanged;

	public GameMenuTab ActiveTab { get; private set; } = GameMenuTab.Inventory;
	public bool IsOpen { get; private set; }

	public void SetActiveTab( GameMenuTab tab )
	{
		if ( ActiveTab == tab )
			return;

		ActiveTab = tab;
		ActiveTabChanged?.Invoke();
	}

	public void Open( GameMenuTab tab = GameMenuTab.Inventory )
	{
		SetActiveTab( tab );

		if ( IsOpen )
			return;

		IsOpen = true;
		OpenStateChanged?.Invoke();
	}

	public void Close()
	{
		if ( !IsOpen )
			return;

		IsOpen = false;
		OpenStateChanged?.Invoke();
	}

	public void Toggle( GameMenuTab openTab = GameMenuTab.Inventory )
	{
		if ( IsOpen )
		{
			Close();
			return;
		}

		Open( openTab );
	}

	public static string GetTabTitle( GameMenuTab tab )
	{
		return tab switch
		{
			GameMenuTab.Inventory => "Inventory",
			GameMenuTab.Stats     => "Stats",
			GameMenuTab.Quests    => "Quests",
			GameMenuTab.Map       => "Map",
			GameMenuTab.Options   => "Options",
			_                     => tab.ToString()
		};
	}
}
