using Godot;

public class BuildingBase : Node2D
{
	public override void _Ready()
	{
		base._Ready();

		ConnectPlanksInChildrenRecursively(this);
	}

	public override void _ExitTree()
	{
		Plank.ClearPlankList();
	}

	void ConnectPlanksInChildrenRecursively(Node node)
	{
		Godot.Collections.Array children = node.GetChildren();
		Plank plank;
		Node child;
		for (int i = 0; i < children.Count; i++)
		{
			child = (Node)children[i];
			if (child.HasSignal(Plank.IS_SIGNAL))
			{
				plank = (Plank)children[i];
				plank.ConnectToOtherPlanks();
			}
			else
			{
				ConnectPlanksInChildrenRecursively(child);
			}
		}
	}
}
