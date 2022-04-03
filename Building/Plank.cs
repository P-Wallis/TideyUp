using Godot;
using System;
using System.Collections.Generic;

public class Plank : RigidBody2D
{
    private static List<Plank> planks = new List<Plank>();
    public Position2D left, right;

    public bool autoConnectOverlapping;

    private Script plankScript;
    public override void _Ready()
    {
        planks.Add(this);
        this.Connect("body_entered", this, "OnBodyEntered");
        left = GetNode<Position2D>("LeftHandle");
        right = GetNode<Position2D>("RightHandle");
        plankScript = (Script)this.GetScript();
        autoConnectOverlapping = true;
    }

    public void OnBodyEntered(Node node)
    {
        if(!autoConnectOverlapping)
        {
            return;
        }

        if(plankScript.InstanceHas(node))
        {
            Plank otherPlank = (Plank)node;
            Vector2 overlap = Vector2.Zero;
            if(GetOverlapPoint(otherPlank, out overlap))
            {
                Node parent = this.GetParent();
                PinJoint2D pin = new PinJoint2D();
                parent.AddChild(pin);
                pin.SetGlobalPosition(overlap);
                pin.SetNodeA(this.GetPath());
                pin.SetNodeB(otherPlank.GetPath());
            }
        }

        autoConnectOverlapping = false;
    }

    private bool GetOverlapPoint(Plank otherPlank, out Vector2 overlapPoint)
    {
        Vector2 mL  = left.GetGlobalPosition();
        Vector2 mR = right.GetGlobalPosition();
        Vector2 oL  = otherPlank.left.GetGlobalPosition();
        Vector2 oR = otherPlank.right.GetGlobalPosition();

        float d = ((mL.x - mR.x) * (oL.y - oR.y)) - ((oL.x - oR.x) * (mL.y - mR.y));
        float t = ((mL.x - oL.x) * (oL.y - oR.y)) - ((mL.y - oL.y) * (oL.x - oR.x));
        float u = ((mL.x - oL.x) * (mL.y - mR.y)) - ((mL.y - oL.y) * (mL.x - mR.x));

        if(Math.Abs(d) < 0.0001f || t < 0 || t > d || u < 0 || u > d)
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
