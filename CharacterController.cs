using Godot;
using System;

public class CharacterController : KinematicBody2D
{
    const float gravity = 200.0f;
    const int walkSpeed = 200;
    [Export]
    public int JumpImpulse =  -400;
    public float spriteScale = .3f;
    Vector2 velocity;
    public AnimatedSprite _animatedSprite;
    public Directions direction = Directions.right;
    public enum Directions
    {
        left,
        right,
    }
 
    public CharacterController()
    {
        Plank.playerNode = this;
    }

    public override void _Ready()
    {
        base._Ready();

        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        velocity.y += delta * gravity;

        if (Input.IsActionPressed("ui_left"))
        {
            if (direction == Directions.right)
            {
                direction = Directions.left;
                _animatedSprite.Scale = new Vector2(-spriteScale, spriteScale);
            }
            velocity.x = -walkSpeed;
            _animatedSprite.Play("Run");
        }
        else if (Input.IsActionPressed("ui_right"))
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
            _animatedSprite.Play("Idle");
        }

        // Jumping.
        if (IsOnFloor() && Input.IsActionPressed("ui_select"))
        {
            velocity.y += JumpImpulse;
            _animatedSprite.Play("Jump");
        }
        //Building 
        if (Input.IsMouseButtonPressed(1))
        {
            _animatedSprite.Play("Build");
        }
        // ...
    // We don't need to multiply velocity by delta because "MoveAndSlide" already takes delta time into account.

    // The second parameter of "MoveAndSlide" is the normal pointing up.
    // In the case of a 2D platformer, in Godot, upward is negative y, which translates to -1 as a normal.
    velocity = MoveAndSlide(velocity, new Vector2(0, -1));
    }
}
