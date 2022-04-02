using Godot;
using System;
using Random = TideyUp.Utils.Random;

public class BuildingTest : Node2D
{
    PackedScene Plank = GD.Load<PackedScene>("res://Building/Plank.tscn");

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        // Mouse in viewport coordinates.
        if (@event is InputEventMouseButton eventMouseButton)
        {
            InstantiateRandomlyRotatedPlankAtPosition(eventMouseButton.Position);
        }
    }

    private void InstantiateRandomlyRotatedPlankAtPosition(Vector2 position)
    {
        Plank plankInstance = Plank.Instance() as Plank;
        AddChild(plankInstance);
        plankInstance.SetPosition(position);
        plankInstance.SetRotationDegrees(Random.Range(-12,12) * 30);
    }
}
