using Godot;
using Random = TideyUp.Utils.Random;

public class Water : Sprite
{
    public static Water _;

    const float DEEP_WATER_UNSCALED_HEIGHT = 160;
    const float SURFACE_WATER_HEIGHT = 160;
    Sprite deepWater;
    Vector2 startPosition;
    public Timer waveTimer;
    PackedScene WarningScene = GD.Load<PackedScene>("res://Scenes/Elements/FloodWarning.tscn");

    Sprite warning;
    public override void _Ready()
    {
        deepWater = GetNode<Sprite>("Deep");
        startPosition = GlobalPosition;

        if (_ == null)
        {
            _ = this;
        }

        waveWait = Random.Range(waveWaitMin, waveWaitMax);
        isWave = false;
        waveTimer = new Timer();
        waveTimer.OneShot = true;
        waveTimer.WaitTime = waveWait;
        waveTimer.Connect("timeout", this, nameof(OnTimerComplete));
        AddChild(waveTimer);
        waveTimer.Start();

        CallDeferred(nameof(AddWarningSprite));
    }
	public override void _ExitTree()
	{
		_ = null;
	}

    private void AddWarningSprite()
    {
        warning = WarningScene.Instance() as Sprite;
        GetParent().AddChild(warning);
        warning.GlobalPosition = new Vector2(958, 384);
    }

    private float depth = 0;
    public float GetWaterDepth() { return depth; }
    public void SetWaterDepth(float newDepth)
    {
        depth = newDepth;
        GlobalPosition = new Vector2(startPosition.x, startPosition.y - depth);
        if (depth > SURFACE_WATER_HEIGHT)
        {
            deepWater.Scale = new Vector2(1, (depth - SURFACE_WATER_HEIGHT) / DEEP_WATER_UNSCALED_HEIGHT);
        }
    }

    public bool IsUnderWater(float globalYPosition)
    {
        return HeightAboveWater(globalYPosition) < 0;
    }

    public float HeightAboveWater(float globalYPosition)
    {
        return (startPosition.y - depth) - globalYPosition;
    }

    [Export(PropertyHint.Range, "0,10,")] float baseWaterFillSpeed = 0;
    [Export(PropertyHint.Range, "0,10,")] float waterFillSpeedIncrement = 1;
    [Export(PropertyHint.Range, "0,20,")] float waveWaterFillSpeed = 10;
    [Export(PropertyHint.Range, "1,60,")] float waveWaitMin = 10;
    [Export(PropertyHint.Range, "1,60,")] float waveWaitMax = 30;
    [Export(PropertyHint.Range, "1,60,")] float waveDuration = 5;

    private bool isWave = false;
    private float waveWait;
    private float waterFillSpeedIncrease = 0;

    public void OnTimerComplete()
    {
        isWave = !isWave;

        if (isWave)
        {
            waveTimer.WaitTime = waveDuration;
        }
        else
        {
            waterFillSpeedIncrease += waterFillSpeedIncrement;
            waveWait = Random.Range(waveWaitMin, waveWaitMax);
            waveTimer.WaitTime = waveWait;
        }
        waveTimer.Start();
    }

    public override void _Process(float delta)
    {
        float currentWaterFillSpeed = (isWave ? waveWaterFillSpeed : baseWaterFillSpeed) + waterFillSpeedIncrease;
        SetWaterDepth(depth + (delta * currentWaterFillSpeed));
    }
}
