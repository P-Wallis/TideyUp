using Godot;
using System;

public class CharacterController : KinematicBody2D
{
    const float gravity = 200.0f;
    const int walkSpeed = 200;
    const string accept = "ui_accept";
    const string left = "ui_left";
    const string right = "ui_right";
    const string up = "ui_up";
    [Export]
    private float maxPickupDistance = 50f;
    [Export]
    public int JumpImpulse = -400;
    [Export]
    private float minDustSpeed = 0.1f;
    [Export]
    private float minSplashSpeed = 0.1f;
    public float spriteScale = .3f;
    private float rotationSnap = 15f;
    Vector2 velocity;
    public AnimatedSprite _animatedSprite;
    public CPUParticles2D dustParticles;
    public CPUParticles2D splashParticles;
    public Sprite plankAboveHead;
    public bool isJumping = false;
    public Timer coyoteTime;

    private Plank closestPlank = null;
    bool wasUnderWater = false;
    public Directions direction = Directions.right;
    public enum Directions
    {
        left,
        right,
    }
    public StateMachine state = StateMachine.holdingNothing;
    public enum StateMachine
    {
        holdingPlank,
        holdingNothing,
        building,
        finalize,
    }

    public CharacterController()
    {
        Plank.playerNode = this;
    }
    PackedScene PlankScene = GD.Load<PackedScene>("res://Building/Plank.tscn");

    public override void _Ready()
    {
        base._Ready();

        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        dustParticles = GetNode<CPUParticles2D>("Dust");
        splashParticles = GetNode<CPUParticles2D>("Splash");
        plankAboveHead = GetNode<Sprite>("PlankSprite");
        coyoteTime = new Timer();
        coyoteTime.OneShot = true;
        coyoteTime.WaitTime = .5f;
        AddChild(coyoteTime);

    }

    public override void _Process(float delta)
    {
        bool isUnderWater = Water._.IsUnderWater(GlobalPosition.y);
        dustParticles.Emitting = !isUnderWater && IsOnFloor() && Math.Abs(velocity.x) > minDustSpeed;

        if (isUnderWater && !wasUnderWater && Math.Abs(velocity.y) > minSplashSpeed)
        {
            splashParticles.Emitting = true;
        }
        wasUnderWater = isUnderWater;
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity.y += delta * gravity;
        
        if (Input.IsActionPressed(left) && state != StateMachine.building)
        {
            if (direction == Directions.right)
            {
                direction = Directions.left;
                _animatedSprite.Scale = new Vector2(-spriteScale, spriteScale);
            }
            velocity.x = -walkSpeed;
            _animatedSprite.Play("Run");
        }
        else if (Input.IsActionPressed(right) && state != StateMachine.building)
        {
            if (direction == Directions.left)
            {
                direction = Directions.right;
                _animatedSprite.Scale = new Vector2(spriteScale, spriteScale);
            }
            velocity.x = walkSpeed;
            _animatedSprite.Play("Run");
        }
        else
        {
            velocity.x = 0;
            if(IsOnFloor())
            {
                _animatedSprite.Play("Idle");
            }
        }

        // Jumping
        if (IsOnFloor() || !coyoteTime.IsStopped())
        {
            isJumping = false;
            if (Input.IsActionJustPressed(up))
            {
                isJumping = true;
                coyoteTime.Stop();
                _animatedSprite.Play("Jump");
                velocity.y += JumpImpulse;

            }
        }

        bool wasOnFloor = IsOnFloor();
        velocity = MoveAndSlide(velocity, new Vector2(0, -1));
        if (!IsOnFloor() && wasOnFloor && !isJumping)
        {
            coyoteTime.Start();
        }

        switch (state)
        {
            case StateMachine.holdingNothing:
                //highlights closest plank
                if (closestPlank != null)
                {
                    closestPlank.highlight.Hide();
                }
                closestPlank = Plank.GetClosestPlankToPlayer();
                if (closestPlank != null && closestPlank.distance < maxPickupDistance)
                {
                    closestPlank.highlight.Show();
                }
                //pick up plank
                if (Input.IsActionJustPressed(accept) && closestPlank != null && closestPlank.distance < maxPickupDistance)
                {
                    closestPlank.Destroy();
                    closestPlank = null;
                    plankAboveHead.Show();
                    state = StateMachine.holdingPlank;
                }
                break;
            case StateMachine.holdingPlank:
                if (Input.IsActionJustPressed(accept))
                {
                    state = StateMachine.building;
                }
                break;
            case StateMachine.building:
                if (Input.IsActionJustPressed(left))
                {

                    plankAboveHead.RotationDegrees -= rotationSnap;
                }
                else if (Input.IsActionJustPressed(right))
                {

                    plankAboveHead.RotationDegrees += rotationSnap;
                }
                else if (Input.IsActionJustPressed(accept))
                {
                    Plank plank = PlankScene.Instance() as Plank;
                    GetParent().AddChild(plank);
                    plank.GlobalPosition = plankAboveHead.GlobalPosition;
                    plank.RotationDegrees = plankAboveHead.RotationDegrees;
                    plank.ConnectToOtherPlanks();
                    plankAboveHead.Hide();
                    state = StateMachine.holdingNothing;
                }
                break;
        }
    }
}
