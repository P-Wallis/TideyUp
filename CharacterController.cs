using Godot;
using System;

public class CharacterController : KinematicBody2D
{
	const float gravity = 200.0f;
	const int walkSpeed = 200;
	[Export]
	private float maxPickupDistance = 50f;
	[Export]
	public int JumpImpulse =  -400;
	public float spriteScale = .3f;
	Vector2 velocity;
	public AnimatedSprite _animatedSprite;
	public bool isJumping = false;
	public Timer coyoteTime;
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
	}

	public CharacterController()
	{
		Plank.playerNode = this;
	}

	public override void _Ready()
	{
		base._Ready();

		_animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		coyoteTime = new Timer();
		coyoteTime.OneShot = true;
		coyoteTime.WaitTime = .5f;
		AddChild(coyoteTime);

	}

	public override void _PhysicsProcess(float delta)
	{
		if(coyoteTime.IsStopped())
		{
			velocity.y += delta * gravity;
		}
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
		if (IsOnFloor() || !coyoteTime.IsStopped())
		{
			isJumping = false;
			if(Input.IsActionJustPressed("ui_up"))
			{
					isJumping = true;
					coyoteTime.Stop();
					_animatedSprite.Play("Jump");
					velocity.y += JumpImpulse;

			}
		}

		bool wasOnFloor = IsOnFloor();
		velocity = MoveAndSlide(velocity, new Vector2(0, -1));
		if(!IsOnFloor() && wasOnFloor && !isJumping)
		{
			coyoteTime.Start();
			velocity.y = 0;

		}
		//picking up items
		if (Input.IsActionJustPressed("ui_accept"))
		{
			switch (state)
			{
				case StateMachine.holdingNothing:
					Plank closestPlank = Plank.GetClosestPlankToPlayer();
					if (closestPlank != null && closestPlank.distance < maxPickupDistance)
					{
						closestPlank.Hide();
						this.GetNode<Sprite>("PlankSprite").Show();
						state = StateMachine.holdingPlank;
					}
				break;
			}
		}
	}
}
