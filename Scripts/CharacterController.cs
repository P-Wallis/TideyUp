using Godot;
using System;

public class CharacterController : KinematicBody2D
{
    // Enums
    public enum State
    {
        holdingPlank,
        holdingNothing,
        building,
    }

    public enum Direction
    {
        left,
        right,
    }


    // Constants
    const float GRAVITY = 200.0f;
    const float WALK_SPEED = 200;
    const float SPRITE_SCALE = .3f;
    const float ROTATION_SNAP = 15f;
    const string BUTTON_SELECT = "select";
    const string BUTTON_CANCEL = "cancel";
    const string BUTTON_LEFT = "left";
    const string BUTTON_RIGHT = "right";
    const string BUTTON_JUMP = "jump";
    private const string ANIM_RUN = "Run";
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_BUILD = "Build";

    // Inspector Variables
    [Export] private float maxPickupDistance = 50f;
    [Export] public float JumpImpulse = -400;
    [Export] private float minDustSpeed = 0.1f;
    [Export] private float minSplashSpeed = 0.1f;

    // Class Variables
    Vector2 velocity;
    public AnimatedSprite animatedSprite;
    public CPUParticles2D dustParticles;
    public CPUParticles2D splashParticles;
    public PlankPreview plankPreview;
    public bool isJumping = false;
    public Timer coyoteTime;
    public AudioStreamPlayer splashSound;

    private Plank closestPlank = null;
    bool wasUnderWater = false;
    public Direction direction = Direction.right;
    public State state = State.holdingNothing;

    private PlankSize plankSize = PlankSize.Medium;

    public CharacterController()
    {
        Plank.playerNode = this;
    }

    public override void _Ready()
    {
        base._Ready();

        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
        dustParticles = GetNode<CPUParticles2D>("Dust");
        splashParticles = GetNode<CPUParticles2D>("Splash");
        plankPreview = GetNode<PlankPreview>("PlankPreview");
        coyoteTime = new Timer();
        coyoteTime.OneShot = true;
        coyoteTime.WaitTime = .5f;
        AddChild(coyoteTime);
        splashSound = GetNode<AudioStreamPlayer>("AudioStreamPlayer");



    }

    public override void _Process(float delta)
    {
        bool isUnderWater = Water._.IsUnderWater(GlobalPosition.y);
        dustParticles.Emitting = !isUnderWater && IsOnFloor() && Math.Abs(velocity.x) > minDustSpeed;

        if (isUnderWater && !wasUnderWater && Math.Abs(velocity.y) > minSplashSpeed)
        {
            splashParticles.Emitting = true;
            splashSound.Play();
        }
        wasUnderWater = isUnderWater;

        if(Plank.AreAllPlanksUnderWater())
        {
            SceneManager._.LoadScene(SceneID.Outro);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case State.holdingNothing:
                HandleHorizontalInput();
                HandleJumpInput();

                //highlight closest plank
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
                if (Input.IsActionJustPressed(BUTTON_SELECT) &&
                    closestPlank != null &&
                    closestPlank.distance < maxPickupDistance)
                {
                    plankSize = closestPlank.size;
                    closestPlank.Destroy();
                    closestPlank = null;
                    plankPreview.RotationDegrees = 0;
                    plankPreview.SetSize(plankSize);
                    plankPreview.Show();
                    state = State.holdingPlank;
                }
                break;

            case State.holdingPlank:
                HandleHorizontalInput();
                HandleJumpInput();

                // Drop Plank
                if (Input.IsActionJustPressed(BUTTON_CANCEL))
                {
                    CreatePlankAboveHead();
                    state = State.holdingNothing;
                }

                // Build with Plank
                if (Input.IsActionJustPressed(BUTTON_SELECT))
                {
                    animatedSprite.Play(ANIM_BUILD);
                    state = State.building;
                }
                break;

            case State.building:
                velocity.x = 0; //don't move in the building state
                if (Input.IsActionJustPressed(BUTTON_CANCEL))
                {
                    state = State.holdingPlank;
                }
                if (Input.IsActionJustPressed(BUTTON_LEFT))
                {
                    plankPreview.RotationDegrees -= ROTATION_SNAP;
                }
                else if (Input.IsActionJustPressed(BUTTON_RIGHT))
                {
                    plankPreview.RotationDegrees += ROTATION_SNAP;
                }
                else if (Input.IsActionJustPressed(BUTTON_SELECT))
                {
                    Plank plank = CreatePlankAboveHead();
                    state = State.holdingNothing;
                }
                break;
        }

        // Move the character
        velocity.y += delta * GRAVITY;
        bool wasOnFloor = IsOnFloor();
        velocity = MoveAndSlide(velocity, new Vector2(0, -1));
        if (!IsOnFloor() && wasOnFloor && !isJumping)
        {
            coyoteTime.Start();
        }
    }

    void HandleHorizontalInput()
    {
        if (Input.IsActionPressed(BUTTON_LEFT))
        {
            if (direction == Direction.right)
            {
                direction = Direction.left;
                animatedSprite.Scale = new Vector2(-SPRITE_SCALE, SPRITE_SCALE);
            }
            velocity.x = -WALK_SPEED;
            if(IsOnFloor())
            {
                animatedSprite.Play(ANIM_RUN);
            }
        }
        else if (Input.IsActionPressed(BUTTON_RIGHT))
        {
            if (direction == Direction.left)
            {
                direction = Direction.right;
                animatedSprite.Scale = new Vector2(SPRITE_SCALE, SPRITE_SCALE);
            }
            velocity.x = WALK_SPEED;
            if(IsOnFloor())
            {
                animatedSprite.Play(ANIM_RUN);
            }
        }
        else
        {
            velocity.x = 0;
            if(IsOnFloor() || animatedSprite.Animation != ANIM_JUMP)
            {
                animatedSprite.Play(ANIM_IDLE);
            }
        }
    }

    void HandleJumpInput()
    {
        if(!IsOnFloor())
        {
            animatedSprite.Play(ANIM_JUMP);
        }

        if ( Water._.IsUnderWater(GlobalPosition.y) || IsOnFloor() || !coyoteTime.IsStopped())
        {
            isJumping = false;
            if (Input.IsActionJustPressed(BUTTON_JUMP))
            {
                isJumping = true;
                coyoteTime.Stop();
                animatedSprite.Play(ANIM_JUMP);
                velocity.y += JumpImpulse;
                if(velocity.y < JumpImpulse)
                {
                    velocity.y = JumpImpulse;
                }
            }
        }
    }

    Plank CreatePlankAboveHead(bool hideAboveHeadPlank = true)
    {
        Plank plank = Plank.InstantiatePlank(plankSize);
        GetParent().AddChild(plank);
        plank.GlobalPosition = plankPreview.GlobalPosition;
        plank.RotationDegrees = plankPreview.RotationDegrees;
        plank.ConnectToOtherPlanks();

        if(hideAboveHeadPlank)
        {
            plankPreview.Hide();
        }

        return plank;
    }
}
