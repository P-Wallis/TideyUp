using Godot;
using System;
using Random = TideyUp.Utils.Random;

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
    const float LOCATION_SNAP = 10f;
    const int LOCATION_MAX_INCREMENTS = 3;
    public const string BUTTON_SELECT = "select";
    public const string BUTTON_CANCEL = "cancel";
    public const string BUTTON_LEFT = "left";
    public const string BUTTON_RIGHT = "right";
    public const string BUTTON_JUMP = "jump";
    public const string BUTTON_UP = "up";
    public const string BUTTON_DOWN = "down";
    private const string ANIM_RUN = "Run";
    private const string ANIM_IDLE = "Idle";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_BUILD = "Build";

    // Inspector Variables
    [Export] private float maxPickupDistance = 50f;
    [Export] public float JumpImpulse = -400;
    [Export] private float minDustSpeed = 0.1f;
    [Export] private float minSplashSpeed = 0.1f;
    [Export] private float deathTimerDuration = 5f;

    // Class Variables
    Vector2 velocity;
    public AnimatedSprite animatedSprite;
    public CPUParticles2D dustParticles;
    public CPUParticles2D splashParticles;
    public PlankPreview plankPreview;
    public bool isJumping = false;
    public Timer coyoteTime;
    public AudioStreamPlayer splashSound, jumpSound, pickupSound, hammerSound, pinguSound, pinguCancelSound;
    private Plank closestPlank = null;
    bool wasUnderWater = false;
    public Direction direction = Direction.right;
    public State state = State.holdingNothing;
    public Sprite thoughtBubble;
    public Texture curious = GD.Load<Texture>("res://UI/Icon_Curious.png");
    public Texture fixing = GD.Load<Texture>("res://UI/Icon_Fixing.png");
    public Texture holding = GD.Load<Texture>("res://UI/Icon_Holding.png");


    private PlankSize plankSize = PlankSize.Medium;
    private Vector2 plankPreviewPosition;
    bool deathTimerStarted;
    Timer deathTimer;
    Polygon2D fadePanel;
    AudioStreamPlayer backgroundMusic;

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
        plankPreviewPosition = plankPreview.Position;
        coyoteTime = new Timer();
        coyoteTime.OneShot = true;
        coyoteTime.WaitTime = .5f;
        AddChild(coyoteTime);
        splashSound = GetNode<AudioStreamPlayer>("SplashSFX");
        jumpSound = GetNode<AudioStreamPlayer>("JumpSFX");
        pickupSound = GetNode<AudioStreamPlayer>("PickupSFX");
        hammerSound = GetNode<AudioStreamPlayer>("HammerSFX");
        pinguSound = GetNode<AudioStreamPlayer>("PinguSFX");
        pinguCancelSound = GetNode<AudioStreamPlayer>("PinguCancelSFX");
        thoughtBubble = GetNode<Sprite>("IconCurious");
        deathTimerStarted = false;
    }

    public override void _Process(float delta)
    {
        bool isUnderWater = Water._.IsUnderWater(GlobalPosition.y);
        dustParticles.Emitting = !isUnderWater && IsOnFloor() && Mathf.Abs(velocity.x) > minDustSpeed;

        if (isUnderWater && !wasUnderWater && Mathf.Abs(velocity.y) > minSplashSpeed)
        {
            splashParticles.Emitting = true;
            splashSound.PitchScale = Random.Range(0.9f, 1.25f);
            splashSound.Play();
        }
        wasUnderWater = isUnderWater;

        if(Plank.AreAllPlanksUnderWater() && !deathTimerStarted)
        {
            deathTimerStarted = true;
            deathTimer = TideyUp.Utils.Timers.CreateTimer(this, nameof(OnDeathTimerComplete), deathTimerDuration);
            fadePanel = GetNode<Polygon2D>("FadePanel");
            backgroundMusic = GetParent().GetNode<AudioStreamPlayer>("BackgroundMusic");
        }

        if(deathTimerStarted)
        {
            float fade = (deathTimer.TimeLeft/deathTimer.WaitTime);
            if(fade > 0)
            {
                backgroundMusic.PitchScale = fade;
            }
            fadePanel.Color = new Color(0,0,0, 1 - fade);
        }
    }

    public void OnDeathTimerComplete()
    {
        SceneManager._.LoadScene(SceneID.Outro);
    }

    public override void _PhysicsProcess(float delta)
    {
        switch (state)
        {
            case State.holdingNothing:
                HandleHorizontalInput();
                HandleVerticalInput();

                //highlight closest plank
                if (closestPlank != null)
                {
                    closestPlank.highlight.Hide();
                }
                closestPlank = Plank.GetClosestPlankToPlayer();
                if (closestPlank != null && closestPlank.distance < maxPickupDistance)
                {
                    closestPlank.highlight.Show();
                    thoughtBubble.Show();
                    thoughtBubble.Texture = curious;
                }
                else if (closestPlank != null && closestPlank.distance > maxPickupDistance)
                {
                    thoughtBubble.Hide();
                }

                //pick up plank
                if (Input.IsActionJustPressed(BUTTON_SELECT) &&
                    closestPlank != null &&
                    closestPlank.distance < maxPickupDistance)
                {
                    plankSize = closestPlank.size;
                    plankPreview.Position = plankPreviewPosition;
                    plankPreview.RotationDegrees = Mathf.Round(closestPlank.RotationDegrees/ROTATION_SNAP) * ROTATION_SNAP;
                    plankPreview.SetSize(plankSize);
                    plankPreview.Show();
                    closestPlank.Destroy();
                    closestPlank = null;
                    state = State.holdingPlank;

                    pickupSound.PitchScale = Random.Range(0.9f, 1.25f);
                    pickupSound.Play();
                }
                break;

            case State.holdingPlank:
                HandleHorizontalInput();
                HandleVerticalInput();
                thoughtBubble.Texture = holding;

                // Drop Plank
                if (Input.IsActionJustPressed(BUTTON_CANCEL))
                {
                    PlayPinguSound(false);
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
                thoughtBubble.Texture = fixing;
                if (Input.IsActionJustPressed(BUTTON_CANCEL))
                {;
                    state = State.holdingPlank;
                }
                if (Input.IsActionJustPressed(BUTTON_LEFT))
                {
                    PlayHammerSound();
                    plankPreview.RotationDegrees -= ROTATION_SNAP;
                }
                else if (Input.IsActionJustPressed(BUTTON_RIGHT))
                {
                    PlayHammerSound();
                    plankPreview.RotationDegrees += ROTATION_SNAP;
                }
                else if (Input.IsActionJustPressed(BUTTON_UP))
                {
                    PlayHammerSound();
                    plankPreview.Position -= new Vector2(0, LOCATION_SNAP);
                    if(Mathf.Abs(plankPreview.Position.y - plankPreviewPosition.y) > (LOCATION_MAX_INCREMENTS * LOCATION_SNAP))
                    {
                        plankPreview.Position = plankPreviewPosition - new Vector2(0, (LOCATION_MAX_INCREMENTS * LOCATION_SNAP));
                    }
                }
                else if (Input.IsActionJustPressed(BUTTON_DOWN))
                {
                    PlayHammerSound();
                    plankPreview.Position += new Vector2(0, LOCATION_SNAP);
                    if(Mathf.Abs(plankPreview.Position.y - plankPreviewPosition.y) > (LOCATION_MAX_INCREMENTS * LOCATION_SNAP))
                    {
                        plankPreview.Position = plankPreviewPosition + new Vector2(0, (LOCATION_MAX_INCREMENTS * LOCATION_SNAP));
                    }
                }
                else if (Input.IsActionJustPressed(BUTTON_SELECT))
                {
                    PlayPinguSound();
                    hammerSound.Stop();
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

    void PlayPinguSound(bool select = true)
    {
        if(select)
        {
            pinguSound.PitchScale = Random.Range(0.95f, 1.1f);
            pinguSound.Play();
        }
        else
        {
            pinguCancelSound.PitchScale = Random.Range(0.95f, 1.1f);
            pinguCancelSound.Play();
        }
    }

    void PlayHammerSound()
    {
        hammerSound.PitchScale = Random.Range(0.9f, 1.25f);
        hammerSound.Play();
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

    void HandleVerticalInput()
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
                jumpSound.PitchScale = Random.Range(0.95f, 1.1f);
                jumpSound.Play();
                velocity.y += JumpImpulse;
                if(velocity.y < JumpImpulse)
                {
                    velocity.y = JumpImpulse;
                }
            }
        }

        if (Input.IsActionJustPressed(BUTTON_DOWN))
        {
            if(velocity.y < JumpImpulse*-0.5f)
            {
                velocity.y = JumpImpulse*-0.5f;
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
