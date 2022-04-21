using Godot;
using TideyUp.Utils;
using Random = TideyUp.Utils.Random;

public class Water : Sprite
{
    public static Water _;

    const float DEEP_WATER_UNSCALED_HEIGHT = 160;
    const float SURFACE_WATER_HEIGHT = 160;
    Sprite deepWater;
    Vector2 startPosition;
    public Timer waveTimer, warningTimer, warningFlashTimer;
    PackedScene WarningScene = GD.Load<PackedScene>("res://Scenes/Elements/FloodWarning.tscn");

    private AudioStreamPlayer waveSound;

    Sprite warning;
    public override void _Ready()
    {
        deepWater = GetNode<Sprite>("Deep");
        waveSound = GetNode<AudioStreamPlayer>("WaveSFX");
        startPosition = GlobalPosition;

        if (_ == null)
        {
            _ = this;
        }

        waveWait = Random.Range(waveWaitMin, waveWaitMax);
        isWave = false;
        waveTimer = Timers.CreateTimer(this, nameof(OnWaveTimerComplete), waveWait);
        warningTimer = Timers.CreateTimer(this, nameof(OnWarningTimerComplete), waveWait - warningPredictionRange);
        warningFlashTimer = Timers.CreateTimer(this, nameof(OnWarningFlashTimerComplete), warningFlashSpeed, false);
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
        warning.Hide();
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
    [Export(PropertyHint.Range, "0,60,")] float waveWaterFillSpeed = 10;
    [Export(PropertyHint.Range, "1,120,")] float waveWaitMin = 10;
    [Export(PropertyHint.Range, "1,120,")] float waveWaitMax = 30;
    [Export(PropertyHint.Range, "1,60,")] float waveDuration = 5;
    [Export(PropertyHint.Range, "0,10,")] float warningPredictionRange = 2;
    [Export(PropertyHint.Range, "0,5,")] float warningFlashSpeed = 0.25f;

    private bool isWave = false;
    private float waveWait;
    private float waterFillSpeedIncrease = 0;

    private int waveIndex = -1;

    public void OnWaveTimerComplete()
    {
        isWave = !isWave;

        if (isWave)
        {
            waveSound.Play();
            waveTimer.WaitTime = waveDuration;
            waveIndex++;
            for(int i=0; i<waveIndex; i++)
            {
                AddPlank(PlankSize.Medium);
                AddPlank(Random.Value() > 0.3f ? PlankSize.Large : PlankSize.Small);
            }
        }
        else
        {
            waterFillSpeedIncrease += waterFillSpeedIncrement;
            waveWait = Random.Range(waveWaitMin, waveWaitMax);
            waveTimer.WaitTime = waveWait;

            // warning
            warning.Hide();
            warningTimer.WaitTime = waveWait - warningPredictionRange;
            warningTimer.Start();
        }
        waveTimer.Start();
    }

    public void OnWarningTimerComplete()
    {
        float depthAfterWave = depth + ((waveWaterFillSpeed + waterFillSpeedIncrease) * waveDuration) + ((baseWaterFillSpeed + waterFillSpeedIncrease) * warningPredictionRange);
        warning.GlobalPosition = new Vector2(warning.GlobalPosition.x, startPosition.y - depthAfterWave);
        warning.Show();
        warningOn = true;
        warningFlashTimer.Start();
    }

    bool warningOn = true;
    public void OnWarningFlashTimerComplete()
    {
        if(warningTimer.IsStopped())
        {
            warningOn = !warningOn;
            if(warningOn)
            {
                warning.Show();
            }
            else
            {
                warning.Hide();
            }
            warningFlashTimer.Start();
        }
    }

    private void AddPlank(PlankSize size)
    {
        Plank plank = Plank.InstantiatePlank(size);
        GetParent().AddChild(plank);

        Vector2 pos = Random.InsideUnitCircle() * 300;
        pos.y = (pos.y<0? -pos.y:pos.y) * 0.3f;
        pos += GlobalPosition;
        if(Random.Value() > 0.5f) // Distribute left and right
        {
            pos.x += 900;
            plank.ApplyCentralImpulse(new Vector2(Random.Range(-300,-30), 0));
        }
        else
        {
            pos.x -= 900;
            plank.ApplyCentralImpulse(new Vector2(Random.Range(300,30), 0));
        }
        plank.GlobalPosition = pos;
        //plank.RotationDegrees = Random.Range(-30, 30);
    }

    public override void _Process(float delta)
    {
        float currentWaterFillSpeed = (isWave ? waveWaterFillSpeed : baseWaterFillSpeed) + waterFillSpeedIncrease;
        SetWaterDepth(depth + (delta * currentWaterFillSpeed));
    }
}
