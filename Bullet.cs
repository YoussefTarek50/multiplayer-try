using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
	public const float Speed = 500.0f;
	public const float JumpVelocity = -400.0f;

	private Vector2 direction = new Vector2();

	public override void _Ready()
	{
		direction = new Vector2(1, 0).Rotated(Rotation);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		velocity = Speed * direction;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}



		Velocity = velocity;
		MoveAndSlide();
	}

	private void _on_bullet_timer_timeout()
	{
		QueueFree();
	}
}
