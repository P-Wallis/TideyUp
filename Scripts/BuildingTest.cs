using Godot;
using Random = TideyUp.Utils.Random;

public class BuildingTest : BuildingBase
{
	PackedScene PlankScene = GD.Load<PackedScene>("res://Scenes/Elements/Planks/Plank.tscn");

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.IsPressed())
			{
				InstantiateRandomlyRotatedPlankAtPosition(GetGlobalMousePosition());
			}
		}
	}

	private void InstantiateRandomlyRotatedPlankAtPosition(Vector2 position)
	{
		Plank plank = PlankScene.Instance() as Plank;
		AddChild(plank);
		plank.GlobalPosition = position;
		plank.RotationDegrees = Random.Range(-12, 12) * 30;
		plank.ConnectToOtherPlanks();
	}
}
