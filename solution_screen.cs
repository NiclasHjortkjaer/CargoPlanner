using Godot;
using System;

public partial class solution_screen : Control
{
	private Button _backBtn;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_backBtn = GetNode<Button>("VBoxContainer/MarginContainer/HSplitContainer/HBoxContainer/Back");
		_backBtn.Pressed += Back;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Back()
	{
		var global = GetNode<Global>("/root/Global");
		global.GotoScene("res://start_screen.tscn");
	}
}
