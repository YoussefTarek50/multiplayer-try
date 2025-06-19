using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	[Export]
	public PackedScene Bullet;

	private Vector2 syncPosition = new Vector2(0, 0);
	private float syncGunRotation = 0;

	private const float LERP_FACTOR = 0.22f;

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
				Rpc("fire");
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

			syncPosition = GlobalPosition;
			syncGunRotation = GetNode<Node2D>("GunRotation").RotationDegrees;
		}
		else
		{
			GlobalPosition = GlobalPosition.Lerp(syncPosition, LERP_FACTOR);
			GetNode<Node2D>("GunRotation").RotationDegrees = Mathf.Lerp(GetNode<Node2D>("GunRotation").RotationDegrees, syncGunRotation, LERP_FACTOR);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void fire()
	{
		Node2D b = Bullet.Instantiate<Node2D>();
		b.RotationDegrees = GetNode<Node2D>("GunRotation").RotationDegrees;
		b.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		GetTree().Root.AddChild(b);
	}

	public void setupPlayer(string name)
	{
		GetNode<Label>("Label").Text = name;
	}
}
