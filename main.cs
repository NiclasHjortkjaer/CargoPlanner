using Godot;
using System;
using GodotStart.Model;

public partial class main : Node3D
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var global = GetNode<Global>("/root/Global");
		var solution = global.CurrentSolution;

		for (int i = 0; i < solution.Constructions.Count; i++)
		{

			var construction = solution.Constructions[i];

			var startX = -(construction.Container.Length / 2);
			var startY = 0;
			var startZ = -(construction.Container.Width / 2);
			var random = new Random();

			Color floorColor;
			if (construction.IsLifted)
			{
				floorColor = Colors.Firebrick;
			} 
			else if (construction.Container.Type == PalletEnum.PMC)
			{
				floorColor = Colors.Blue;
			}
			else
			{
				floorColor = Colors.Green;
			}
			
			foreach (var package in construction.Packages)
			{
				var floor = new BoxMesh
				{
					Size = new Vector3(construction.IsLifted ? construction.Container.Length - 20 : construction.Container.Length, 1, construction.Container.Width),
					Material = new StandardMaterial3D
					{
						AlbedoColor = floorColor
					}
				};

				var floorMesh = new MeshInstance3D();
				floorMesh.Mesh = floor;
				floorMesh.Position = new Vector3(i*500, -1, 0);
				
				var pCube = new BoxMesh
				{
					Size = new Vector3((float) package.PackDimX, (float) package.PackDimY, (float) package.PackDimZ),
					Material = new StandardMaterial3D
					{
						Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
						AlbedoColor = new Color((float) random.NextDouble() % 255,(float) random.NextDouble() % 255,(float) random.NextDouble() % 255,0.8f)
					}
				};

				var posX = startX + (float) package.X + ((float) package.PackDimX/2.0f);
				var posY = startY +  (float)package.Y + ((float) package.PackDimY/2.0f);
				var posZ = startZ + (float)package.Z + ((float)package.PackDimZ / 2.0f);
				
				var pMesh = new MeshInstance3D();
				pMesh.Mesh = pCube;
				pMesh.Position = new Vector3(posX, posY, posZ);

				if (construction.IsLifted)
				{
					var liftedFloor = new BoxMesh
					{
						Size = new Vector3(construction.Container.Length, 1, construction.Container.Width),
						Material = new StandardMaterial3D
						{
							AlbedoColor = Colors.Burlywood
						}
					};
					
					var liftedFloorMesh = new MeshInstance3D();
					liftedFloorMesh.Mesh = liftedFloor;
					liftedFloorMesh.Position = new Vector3(0, 10, 0);
					liftedFloorMesh.AddChild(pMesh);
					floorMesh.AddChild(liftedFloorMesh);
				}
				else
				{
					floorMesh.AddChild(pMesh);
				}
				
				AddChild(floorMesh);

				var title = new Label3D
				{
					Text = $"ULD: {construction.Container.Type}\nWeight: {Math.Round(construction.TotalWeight,2)} kg\nVolume: {Math.Round(construction.TotalVolume,2)} m3\nCategories: {construction.CategoryString}\nOrigins: {construction.Originstring}\nDestinations: {construction.DestinationString}",
					Position = new Vector3(i*500, 250, 0),
					FontSize = 3000,
					Billboard = BaseMaterial3D.BillboardModeEnum.Enabled
				};
				AddChild(title);
				
			}
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
