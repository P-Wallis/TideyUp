using Godot;
using System;
using Random = TideyUp.Utils.Random;

public class BuildingTest : Node2D
{
    PackedScene PlankScene = GD.Load<PackedScene>("res://Building/Plank.tscn");

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton)
        {
            if(eventMouseButton.IsPressed())
            {
                InstantiateRandomlyRotatedPlankAtPosition(eventMouseButton.Position);
            }
        }
    }

    private void InstantiateRandomlyRotatedPlankAtPosition(Vector2 position)
    {
        Plank plank = PlankScene.Instance() as Plank;
        AddChild(plank);
        plank.SetPosition(position);
        plank.SetRotationDegrees(Random.Range(-12,12) * 30);
        plank.ConnectToOtherPlanks();
    }
}
