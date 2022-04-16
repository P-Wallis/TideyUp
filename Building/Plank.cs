using Godot;
using System;
using System.Collections.Generic;

public enum PlankSize
{
	Small,
	Medium,
	Large
}
public class Plank : RigidBody2D
{
	public const string IS_SIGNAL = "IsPlank";
	private const int MAX_CONNECTIONS = 5;
	private const float PIN_SOFTNESS = 1;
	private const float PLAYER_WALK_ANGLE = 30; // in degrees
	private const float PLAYER_WALK_DISTANCE = 10;


	public static Node playerNode;
	private static List<Plank> planks = new List<Plank>();

	[Export] public PlankSize size = PlankSize.Medium; 

	public float distance = float.MaxValue;

	public Position2D left, right;
	public Sprite highlight;

	private int plankIndex = -1;
	private List<PlankConnection> connections = new List<PlankConnection>();

	public Plank()
	{
		AddUserSignal(IS_SIGNAL);
		plankIndex = planks.Count;
		planks.Add(this);
	}
	public override void _Ready()
	{
		left = GetNode<Position2D>("LeftHandle");
		right = GetNode<Position2D>("RightHandle");
		highlight = GetNode<Sprite>("Highlight");
	}

	public bool collidesWithPlayer = true;
	public override void _PhysicsProcess(float delta)
	{
		// Enable and disable collision with the player
		if (playerNode != null)
		{
			Vector2 l = left.GlobalPosition;
			Vector2 r = right.GlobalPosition;
			//distance calculation section
			Node2D p2D = (Node2D)playerNode;
			Vector2 p = p2D.GlobalPosition;
			float m = ((r.y - l.y) / (r.x - l.x));
			float b = l.y - (l.x * m);
			float y = (m * p.x) + b;
			distance = float.MaxValue;
			float l_to_p = (r - l).Dot(p - l);
			float r_to_p = (l - r).Dot(p - r);

			if (l_to_p < 0 || r_to_p < 0)
			{
				distance = Math.Min(l.DistanceTo(p), r.DistanceTo(p));
			}
			else
			{
				distance = Math.Abs(y - p.y) / (float)Math.Sqrt((double)(m * m + 1));
			}

			// First, check if we're horizontal
			Vector2 dir = r - l;
			dir = dir / dir.DistanceTo(Vector2.Zero); // normalize the direction vector

			Modulate = new Color(0.5f, 0.5f, 0.5f);

			if (Math.Abs(dir.Dot(Vector2.Up)) < (PLAYER_WALK_ANGLE / 90))
			{
				// Check if the player is above us

				if (p.y < (y + 2f))
				{
					// check if the player is close

					if (distance < PLAYER_WALK_DISTANCE)
					{
						if (!collidesWithPlayer)
						{
							collidesWithPlayer = true;
							RemoveCollisionExceptionWith(playerNode);
							Mode = RigidBody2D.ModeEnum.Kinematic;
						}
						return;
					}
				}
			}
			else
			{
				Modulate = new Color(1, 1, 1);
			}

			if (collidesWithPlayer)
			{
				collidesWithPlayer = false;
				AddCollisionExceptionWith(playerNode);
				Mode = RigidBody2D.ModeEnum.Rigid;
			}
		}
	}

	public void ConnectToOtherPlanks()
	{
		Plank otherPlank;
		int currentConnections = 0;
		for (int i = 0; i < planks.Count; i++)
		{
			if (plankIndex == i)
			{
				continue;
			}

			otherPlank = planks[i];

			if (otherPlank == null)
			{
				continue;
			}

			Vector2 overlap = Vector2.Zero;
			if (GetOverlapPoint(otherPlank, out overlap))
			{
				Node parent = this.GetParent();
				PinJoint2D pin = new PinJoint2D();
				parent.AddChild(pin);
				pin.GlobalPosition = overlap;
				pin.Softness = PIN_SOFTNESS;
				pin.NodeA = this.GetPath();
				pin.NodeB = otherPlank.GetPath();

				PlankConnection pc = new PlankConnection();
				pc.plankA = this;
				pc.plankB = otherPlank;
				pc.pin = pin;

				connections.Add(pc);
				otherPlank.connections.Add(pc);

				currentConnections++;
				if (currentConnections >= MAX_CONNECTIONS)
				{
					break;
				}
			}
		}
	}
	public void Destroy()
	{
		planks[plankIndex] = null;
		foreach (PlankConnection pc in connections)
		{
			pc.pin.Free();
			if (pc.plankA.plankIndex != plankIndex)
			{
				pc.plankA.RemoveAllConnectionsWith(plankIndex);
			}
			else
			{
				pc.plankB.RemoveAllConnectionsWith(plankIndex);
			}
		}
		connections.Clear();
		Free();
	}

	private void RemoveAllConnectionsWith(int index)
	{
		PlankConnection pc;
		for (int i = 0; i < connections.Count; i++)
		{
			pc = connections[i];
			if (pc.plankA.plankIndex == index || pc.plankB.plankIndex == index)
			{
				connections.RemoveAt(i);
				i--;
			}
		}
	}

	private bool GetOverlapPoint(Plank otherPlank, out Vector2 overlapPoint)
	{
		Vector2 mL = left.GlobalPosition;
		Vector2 mR = right.GlobalPosition;
		Vector2 oL = otherPlank.left.GlobalPosition;
		Vector2 oR = otherPlank.right.GlobalPosition;

		float d = ((mL.x - mR.x) * (oL.y - oR.y)) - ((mL.y - mR.y) * (oL.x - oR.x));
		float t = ((mL.x - oL.x) * (oL.y - oR.y)) - ((mL.y - oL.y) * (oL.x - oR.x));
		float u = ((mL.x - oL.x) * (mL.y - mR.y)) - ((mL.y - oL.y) * (mL.x - mR.x));

		bool noOverlap = false;
		if (Math.Abs(d) > 0.0001f)
		{
			t = t / d;
			u = u / d;

			if (t < 0 || t > 1 || u < 0 || u > 1)
			{
				noOverlap = true;
			}
		}
		else
		{
			noOverlap = true;
		}

		if (noOverlap)
		{
			overlapPoint = Vector2.Zero;
			return false;
		}


		overlapPoint.x = ((mL.x * mR.y - mL.y * mR.x) * (oL.x - oR.x)) - ((oL.x * oR.y - oL.y * oR.x) * (mL.x - mR.x));
		overlapPoint.y = ((mL.x * mR.y - mL.y * mR.x) * (oL.y - oR.y)) - ((oL.x * oR.y - oL.y * oR.x) * (mL.y - mR.y));

		overlapPoint /= d;


		return true;
	}

	private bool IsUnderWater()
	{
		return Water._.IsUnderWater(left.GlobalPosition.y) &&
			Water._.IsUnderWater(right.GlobalPosition.y);

	}
	public static bool AreAllPlanksUnderWater()
	{
		foreach (Plank plank in planks)
		{
			if (plank == null)
			{
				continue;
			}

			if (!plank.IsUnderWater())
			{
				return false;
			}
		}

		return true;
	}
	public static Plank GetClosestPlankToPlayer()
	{
		Plank smallestDistancePlank = null;
		foreach (Plank plank in planks)
		{
			if (smallestDistancePlank == null)
			{
				smallestDistancePlank = plank;
			}
			else if (plank != null)
			{
				if (plank.distance < smallestDistancePlank.distance)
				{
					smallestDistancePlank = plank;
				}
			}

		}
		return smallestDistancePlank;
	}
}
