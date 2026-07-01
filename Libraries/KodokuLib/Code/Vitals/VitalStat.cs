using System;

namespace Kodoku.Lib.Vitals;

public sealed class VitalStat
{
	public float Current { get; set; } = 100f;
	public float Max { get; set; } = 100f;

	public float Normalized => Max <= 0f ? 0f : Math.Clamp( Current / Max, 0f, 1f );

	public void Set( float value ) => Current = Math.Clamp( value, 0f, Max );
	public void Add( float amount ) => Current = Math.Clamp( Current + amount, 0f, Max );
	public void Remove( float amount ) => Current = Math.Clamp( Current - amount, 0f, Max );
}
