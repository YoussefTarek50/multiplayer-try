using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	[Export]
	public PackedScene Bullet;

	public override void _Ready()
	{
		// In Godot, node name is a unique identifier
		GetNode<MultiplayerSynchronizer>("PlayerMultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
    }


	public override void _PhysicsProcess(double delta)
	{
		// This condition checks if player is local player to apply physics only to local player
		if (GetNode<MultiplayerSynchronizer>("PlayerMultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
		{
			Vector2 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
			{
				velocity += GetGravity() * (float)delta;
			}

			// Handle Jump.
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			if (Input.IsActionJustPressed("fire"))
			{
				Node2D b = Bullet.Instantiate<Node2D>();
				b.RotationDegrees = GetNode<Node2D>("GunRotation").RotationDegrees;
				b.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
				GetTree().Root.AddChild(b);
			}


			GetNode<Node2D>("GunRotation").LookAt(GetViewport().GetMousePosition());

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if (direction != Vector2.Zero)
			{
				velocity.X = direction.X * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}
}
