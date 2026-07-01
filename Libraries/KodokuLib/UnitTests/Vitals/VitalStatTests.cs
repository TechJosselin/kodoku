using Kodoku.Lib.Vitals;

[TestClass]
public sealed class VitalStatTests
{
	[TestMethod]
	public void NewStat_StartsAtFullHealth()
	{
		var stat = new VitalStat();

		Assert.AreEqual( 100f, stat.Current );
		Assert.AreEqual( 100f, stat.Max );
		Assert.AreEqual( 1f, stat.Normalized );
	}

	[TestMethod]
	public void Normalized_IsZero_WhenMaxIsZero()
	{
		var stat = new VitalStat { Max = 0f, Current = 50f };

		Assert.AreEqual( 0f, stat.Normalized );
	}

	[TestMethod]
	public void Set_ClampsAboveMax()
	{
		var stat = new VitalStat { Max = 100f };
		stat.Set( 150f );

		Assert.AreEqual( 100f, stat.Current );
	}

	[TestMethod]
	public void Set_ClampsBelowZero()
	{
		var stat = new VitalStat { Max = 100f };
		stat.Set( -10f );

		Assert.AreEqual( 0f, stat.Current );
	}

	[TestMethod]
	public void Add_IncreasesCurrentAndClampsAtMax()
	{
		var stat = new VitalStat { Max = 100f };
		stat.Set( 80f );
		stat.Add( 30f );

		Assert.AreEqual( 100f, stat.Current );
	}

	[TestMethod]
	public void Remove_DecreasesCurrentAndClampsAtZero()
	{
		var stat = new VitalStat { Max = 100f };
		stat.Set( 10f );
		stat.Remove( 50f );

		Assert.AreEqual( 0f, stat.Current );
	}

	[TestMethod]
	public void Add_NegativeDelta_DecreasesValue()
	{
		var stat = new VitalStat { Max = 100f };
		stat.Set( 60f );
		stat.Add( -20f );

		Assert.AreEqual( 40f, stat.Current );
	}
}
