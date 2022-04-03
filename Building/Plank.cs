using Godot;
using System;
using System.Collections.Generic;

public class Plank : RigidBody2D
{
    public const string IS_PLANK_SIGNAL = "IsPlank";
    private const int MAX_CONNECTIONS = 5;
    private const float PIN_SOFTNESS = 1;
    private const float PLAYER_WALK_ANGLE = 30; // in degrees


    public static Node playerNode;

    private static List<Plank> planks = new List<Plank>();
    private static List<Line2D> lines = new List<Line2D>();
    public Position2D left, right;

    private int plankIndex = -1;

    public Plank()
    {
        AddUserSignal(IS_PLANK_SIGNAL);
        plankIndex = planks.Count;
        planks.Add(this);
    }
    public override void _Ready()
    {
        left = GetNode<Position2D>("LeftHandle");
        right = GetNode<Position2D>("RightHandle");

        for(int i=0; i<lines.Count; i++)
        {
            lines[i].QueueFree();
        }
        lines.Clear();
    }

    private bool collidesWithPlayer = true;
    public override void _PhysicsProcess(float delta)
    {
        // Enable and disable collision with the player
        if(playerNode!=null)
        {
            Vector2 l = left.GetGlobalPosition();
            Vector2 r = right.GetGlobalPosition();

            // First, check if we're horizontal
            Vector2 dir = r - l;
            dir = dir / dir.DistanceTo(Vector2.Zero); // normalize the direction vector

            if(Math.Abs(dir.Dot(Vector2.Up)) < (PLAYER_WALK_ANGLE / 90))
            {
                // Check if the player is above us
                Node2D p2D = (Node2D)playerNode;
                Vector2 p = p2D.GetGlobalPosition();
                float m = ((r.y - l.y) / (r.x - l.x));
                float b = l.y - (l.x * m);
                float y = (m * p.x) + b;

                if(p.y < (y + 2f))
                {
                    if(!collidesWithPlayer) 
                    {
                        collidesWithPlayer = true;
                        RemoveCollisionExceptionWith(playerNode);
                        SetMode(RigidBody2D.ModeEnum.Kinematic);
                    }
                    return;
                }

                // Set color based on angle
                Modulate = new Color(0.25f, 0.25f, 0.25f);
            }
            else
            {
                Modulate = new Color(1,1,1);
            }

            if(collidesWithPlayer)
            {
                collidesWithPlayer = false;
                AddCollisionExceptionWith(playerNode);
                SetMode(RigidBody2D.ModeEnum.Rigid);
            }
        }
    }

    void DrawLine(Vector2 l, Vector2 r)
    {
        Node parent = this.GetParent();
        Line2D line = new Line2D();
        parent.AddChild(line);
        line.AddPoint(l);
        line.AddPoint(r);

        lines.Add(line);
    }

    public void ConnectToOtherPlanks()
    {
        Plank otherPlank;
        int currentConnections = 0;
        for(int i=0; i<planks.Count; i++)
        {
            if(plankIndex == i)
            {
                continue;
            }

            otherPlank = planks[i];

            if(otherPlank == null)
            {
                continue;
            }

            Vector2 overlap = Vector2.Zero;
            if(GetOverlapPoint(otherPlank, out overlap))
            {
                Node parent = this.GetParent();
                PinJoint2D pin = new PinJoint2D();
                parent.AddChild(pin);
                pin.SetGlobalPosition(overlap);
                pin.SetSoftness(PIN_SOFTNESS);
                pin.SetNodeA(this.GetPath());
                pin.SetNodeB(otherPlank.GetPath());

                //DrawLine(left.GetGlobalPosition(), right.GetGlobalPosition());
                //DrawLine(otherPlank.left.GetGlobalPosition(), otherPlank.right.GetGlobalPosition());

                currentConnections++;
                if(currentConnections >= MAX_CONNECTIONS)
                {
                    break;
                }
            }
        }
    }

    private bool GetOverlapPoint(Plank otherPlank, out Vector2 overlapPoint)
    {
        Vector2 mL  = left.GetGlobalPosition();
        Vector2 mR = right.GetGlobalPosition();
        Vector2 oL  = otherPlank.left.GetGlobalPosition();
        Vector2 oR = otherPlank.right.GetGlobalPosition();

        float d = ((mL.x - mR.x) * (oL.y - oR.y)) - ((mL.y - mR.y) * (oL.x - oR.x));
        float t = ((mL.x - oL.x) * (oL.y - oR.y)) - ((mL.y - oL.y) * (oL.x - oR.x));
        float u = ((mL.x - oL.x) * (mL.y - mR.y)) - ((mL.y - oL.y) * (mL.x - mR.x));

        bool noOverlap = false;
        if(Math.Abs(d) > 0.0001f)
        {
            t = t/d;
            u = u/d;

            if(t < 0 || t > 1 || u < 0 || u > 1)
            {
                noOverlap = true;
            }
        }
        else
        {
            noOverlap = true;
        }

        if(noOverlap)
        {
            overlapPoint = Vector2.Zero;
            return false;
        }


        overlapPoint.x = ((mL.x * mR.y - mL.y * mR.x) * (oL.x - oR.x)) - ((oL.x * oR.y - oL.y * oR.x) * (mL.x - mR.x));
        overlapPoint.y = ((mL.x * mR.y - mL.y * mR.x) * (oL.y - oR.y)) - ((oL.x * oR.y - oL.y * oR.x) * (mL.y - mR.y));

        overlapPoint /= d;


        return true;
    }
}
