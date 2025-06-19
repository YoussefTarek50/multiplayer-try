using Godot;
using System;

public partial class MultiplayerController : Control
{
	[Export]
	private int port = 8910;

	[Export]
	private string address = "127.0.0.1";
	private const int MAX_CLIENTS = 15;
	private const int HOST_ID = 1;

	private ENetMultiplayerPeer peer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToSever;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}


	/// <summary>
	/// Runs when the connection fails, and it only runs on the clients
	/// </summary>
	private void ConnectionFailed()
	{
		GD.Print("CONNECTION FAILED!");
	}



	/// <summary>
	/// Runs when connection is successful, and only runs on clients
	/// </summary>
	private void ConnectedToSever()
	{
		GD.Print("Connected to Server!");
		RpcId(HOST_ID, "sendPlayerInformation", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}


	/// <summary>
	/// Runs when player disconnects, and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that disconnected</param>
	private void PeerDisconnected(long id)
	{
		GD.Print("Player disconnected with ID: " + id.ToString());
	}


	/// <summary>
	/// Runs when player connects, and runs on all peers
	/// </summary>
	/// <param name="id">id of the player that connected</param>
	private void PeerConnected(long id)
	{
		GD.Print("Player connected with ID: " + id.ToString());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	public void _on_host_button_down()
	{
		peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, MAX_CLIENTS);

		if (error != Error.Ok)
		{
			GD.Print("Error, couldn't host: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		// Set self as peer, to connect to the server
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Waiting for Players...");

		// Host is also a player so send info
		sendPlayerInformation(GetNode<LineEdit>("LineEdit").Text, HOST_ID);
	}

	public void _on_join_button_down()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address, port);

		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game...");
	}

	public void _on_start_game_button_down()
	{
		Rpc("startGame");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame()
	{

		foreach (var item in GameManager.Players)
		{
			GD.Print(item.Name + " is playing!");
		}
		var scene = ResourceLoader.Load<PackedScene>("res://TestScene.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
		this.Hide();
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInformation(string name, int id)
	{
		PlayerInfo playerInfo = new PlayerInfo()
		{
			Name = name,
			Id = id
		};

		if (!GameManager.Players.Contains(playerInfo))
		{
			GameManager.Players.Add(playerInfo);
		}

		if (Multiplayer.IsServer())
		{
			foreach (var item in GameManager.Players)
			{
				Rpc("sendPlayerInformation", item.Name, item.Id);
			}
		}
	}
}
