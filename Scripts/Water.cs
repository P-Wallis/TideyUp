using Godot;
using System;

public class Water : Sprite
{
    const float DEEP_WATER_UNSCALED_HEIGHT = 160;
    const float SURFACE_WATER_HEIGHT = 160;
    Sprite deepWater;
    Vector2 startPosition;
    public override void _Ready()
    {
        deepWater = GetNode<Sprite>("Deep");
        startPosition = GlobalPosition;
    }

    private float depth = 0;
    public float GetWaterDepth() { return depth; }
    public void SetWaterDepth(float newDepth)
    {
        depth = newDepth;
        GlobalPosition = new Vector2(startPosition.x, startPosition.y - depth);
        if(depth > SURFACE_WATER_HEIGHT)
        {
            deepWater.Scale = new Vector2(1, (depth - SURFACE_WATER_HEIGHT) / DEEP_WATER_UNSCALED_HEIGHT);
        }
    }

    public bool IsUnderWater(float globalYPosition)
    {
        return globalYPosition > (startPosition.y - depth);
    }

float time = 0;
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
     time += delta;

     SetWaterDepth(time * 10);
 }
}
