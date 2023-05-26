using Godot;
using System;

public partial class CameraGimbal : Node3D
{
	private float _rotationSpeed = (float) Math.PI / 2;
	private float _zoomSpeed = 20f;
	private float _zoom = 150.0f;
	private float _mouseSensitivity = 0.005f;
	private bool _leftMousePressed;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetInputKeyboard(delta);
		
		var v = Vector3.One;
		v.X *= _zoom;
		v.Y *= _zoom;
		v.Z *= _zoom;
		// Scale = Lerp(Scale, v, _zoomSpeed);
		Scale = new Vector3(_zoom, _zoom, _zoom);
	}
	
	float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat + (secondFloat - firstFloat) * by;
	}
	
	Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float by)
	{
		float retX = Lerp(firstVector.X, secondVector.X, by);
		float retY = Lerp(firstVector.Y, secondVector.Y, by);
		float retZ = Lerp(firstVector.Z, secondVector.Z, by);
		return new Vector3(retX, retY, retZ);
	}
	
	public override void _Input(InputEvent inputEvent)
	{
		if (_leftMousePressed)
		{
			switch (inputEvent)
			{
				case InputEventMouseMotion inputEventMouseMotion:
					if (inputEventMouseMotion.Relative.X != 0)
					{
						RotateObjectLocal(Vector3.Up, -1 * inputEventMouseMotion.Relative.X * _rotationSpeed * _mouseSensitivity);
					}

					if (inputEventMouseMotion.Relative.Y != 0)
					{
						GetNode<Node3D>("InnerGimbal").RotateObjectLocal(Vector3.Right, -1 * inputEventMouseMotion.Relative.Y * _rotationSpeed * _mouseSensitivity);
					}
					break;
			}
		}
	}

	private void GetInputKeyboard(double delta)
	{
		_leftMousePressed = Input.IsMouseButtonPressed(MouseButton.Left);

		if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left))
		{
			Position -= (GlobalTransform.Basis.X.Normalized() * _zoomSpeed);
		}

		if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right))
		{
			Position += (GlobalTransform.Basis.X.Normalized() * _zoomSpeed);
		}

		if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up))
		{
			Position -= GlobalTransform.Basis.Z.Normalized() * _zoomSpeed;
		}
		
		if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down))
		{
			Position += GlobalTransform.Basis.Z.Normalized() * _zoomSpeed;
		}

		if (Input.IsKeyPressed(Key.Q))
		{
			
			var newY = this.Position.Y - _zoomSpeed;
			Position = new Vector3(Position.X, newY, Position.Z);
		}
		
		if (Input.IsKeyPressed(Key.E))
		{
			
			var newY = this.Position.Y + _zoomSpeed;
			Position = new Vector3(Position.X, newY, Position.Z);
		}
	}
}
