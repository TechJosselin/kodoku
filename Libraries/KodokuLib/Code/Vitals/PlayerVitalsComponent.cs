using Sandbox;

namespace Kodoku.Lib.Vitals;

[Title( "Player Vitals" )]
[Category( "Kodoku/Vitals" )]
[Icon( "favorite" )]
public sealed class PlayerVitalsComponent : Component
{
	public VitalStat Health { get; } = new();
	public VitalStat Stamina { get; } = new();
	public VitalStat Hunger { get; } = new();
	public VitalStat Thirst { get; } = new();
	public VitalStat Madness { get; } = new();

	// Quand activé, les sliders écrasent les stats à chaque frame.
	// Désactiver pour que les effets d'items persistent.
	[Property] public bool OverrideWithDebugValues { get; set; } = false;

	[Property] public float DebugHealth { get; set; } = 100f;
	[Property] public float DebugStamina { get; set; } = 85f;
	[Property] public float DebugHunger { get; set; } = 70f;
	[Property] public float DebugThirst { get; set; } = 55f;
	[Property] public float DebugMadness { get; set; } = 25f;

	public VitalStat GetStat( VitalStatKind kind ) => kind switch
	{
		VitalStatKind.Health  => Health,
		VitalStatKind.Stamina => Stamina,
		VitalStatKind.Hunger  => Hunger,
		VitalStatKind.Thirst  => Thirst,
		VitalStatKind.Madness => Madness,
		_                     => Health
	};

	// Applique les sliders une seule fois sans activer l'override continu.
	public void ApplyDebugValues()
	{
		Health.Max  = 100f;
		Stamina.Max = 100f;
		Hunger.Max  = 100f;
		Thirst.Max  = 100f;
		Madness.Max = 100f;

		Health.Set( DebugHealth );
		Stamina.Set( DebugStamina );
		Hunger.Set( DebugHunger );
		Thirst.Set( DebugThirst );
		Madness.Set( DebugMadness );
	}

	public void ApplyItemUseEffects(
		float healthDelta,
		float staminaDelta,
		float hungerDelta,
		float thirstDelta,
		float madnessDelta )
	{
		if ( healthDelta  != 0f ) Health.Add( healthDelta );
		if ( staminaDelta != 0f ) Stamina.Add( staminaDelta );
		if ( hungerDelta  != 0f ) Hunger.Add( hungerDelta );
		if ( thirstDelta  != 0f ) Thirst.Add( thirstDelta );
		if ( madnessDelta != 0f ) Madness.Add( madnessDelta );
	}

	protected override void OnStart()
	{
		ApplyDebugValues();
	}

	protected override void OnUpdate()
	{
		if ( !OverrideWithDebugValues )
			return;

		ApplyDebugValues();
	}
}
