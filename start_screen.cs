using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using GodotStart.Model;

public partial class start_screen : Control
{
	private Button _startSearchBtn;
	private Button _uploadBookingListBtn;
	private FileDialog _fdLoad;
	private FileDialog _fdSave;
	private string _bookingListPath = "/home/niclas/Desktop/GodotStart/BookingLists/Bookinglist_07FEB.csv";
	private string _dispositionPath = "/home/niclas/Desktop/GodotStart/BookingLists/disposition.csv";
	private List<Solution> _solutions = new();
	private VBoxContainer _solutionContainer;
	private Solution _currentSolution;

	public override void _Ready()
	{
		_fdLoad = GetNode<FileDialog>("FDLoad");
		_fdLoad.FileSelected += LoadFile;

		_fdSave = GetNode<FileDialog>("FDSave");
		_fdSave.FileSelected += SaveFile;
		
		_startSearchBtn = GetNode<Button>("VBoxContainer/MarginContainer/HBoxContainer/StartSearchBtn");
		_startSearchBtn.Pressed += StartSearch;

		_uploadBookingListBtn = GetNode<Button>("VBoxContainer/MarginContainer/HBoxContainer/UploadBookingListBtn");
		_uploadBookingListBtn.Pressed += UploadBookingList;
		
		_solutionContainer = GetNode<VBoxContainer>("VBoxContainer/SolutionContainer/MarginContainer/VBoxContainer");

		var global = GetNode<Global>("/root/Global");
		if (global.Solutions != null)
		{
			_solutions = global.Solutions;
			
			foreach (var solution in _solutions)
			{
				MakeSolutionLine(solution);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void StartSearch()
	{
		GD.Print("JUST STARTED SEARCH!!!");
		Solution? solution = PlanStarter.Plan(_bookingListPath);
		if (solution != null)
		{
			_solutions.Add(solution);

			MakeSolutionLine(solution);
			GD.Print(solution.Score);
		}
	}

	public void MakeSolutionLine(Solution solution)
	{
		var scoreLabel = new Label();
		scoreLabel.Text = "Score: " + solution.Score;

		var constructionLabel = new Label();
		constructionLabel.Text = "Number of constructions: " + solution.Constructions.Count;

		var viewSolutionBtn = new Button();
		viewSolutionBtn.Text = "View Solution";
		viewSolutionBtn.Pressed += () => ViewSolution(solution);

		var printDispositionBtn = new Button();
		printDispositionBtn.Text = "Print Disposition";
		printDispositionBtn.Pressed += () => PrintDisposition(solution);

		var hboxContainer = new HBoxContainer();
		hboxContainer.AddThemeConstantOverride("separation", 50);
		hboxContainer.AddChild(scoreLabel);
		scoreLabel.Owner = hboxContainer;
		hboxContainer.AddChild(constructionLabel);
		constructionLabel.Owner = hboxContainer;
		hboxContainer.AddChild(viewSolutionBtn);
		viewSolutionBtn.Owner = hboxContainer;
		hboxContainer.AddChild(printDispositionBtn);
		printDispositionBtn.Owner = hboxContainer;
		
		_solutionContainer.AddChild(hboxContainer);
		hboxContainer.Owner = _solutionContainer;
	}

	public void UploadBookingList()
	{
		_fdLoad.Visible = true;
	}

	public void SaveFile(string file)
	{
		_dispositionPath = file;

		var solution = _currentSolution;
		
		var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
		{
			HasHeaderRecord = true,
			Delimiter = ","
		};

		using var writer = new StreamWriter(_dispositionPath, false, System.Text.Encoding.UTF8);
		GD.Print(_dispositionPath);
		using var csv = new CsvWriter(writer, csvConfiguration);
		csv.WriteField("ULD");
		csv.WriteField("Pieces");
		csv.WriteField("Weight");
		csv.WriteField("Volume");
		csv.NextRecord();
		csv.NextRecord();
		foreach (var construction in solution.Constructions)
		{
			var packages = 
				from package in construction.Packages
				group package by package.ShipmentID into packageGroup
				select packageGroup;

			var destinationFlights =
				from package in construction.Packages
				group package by package.DestinationFlight
				into packageGroup
				select packageGroup;

			csv.WriteField(construction.Container.Type);
			csv.WriteField(construction.Packages.Count);
			csv.WriteField(Math.Round(construction.TotalWeight,2));
			csv.WriteField(Math.Round(construction.TotalVolume,3));
			csv.WriteField(packages.Count());
			csv.WriteField(destinationFlights.Count());
			csv.NextRecord();

			csv.WriteField("Origin");
			csv.WriteField("Shipment ID");
			csv.WriteField("Weight");
			csv.WriteField("Volume");
			csv.WriteField("Dimensions");
			csv.WriteField("Categories");
			csv.WriteField("Destination Flight");
			csv.WriteField("Destination");
			csv.WriteField("Count");
			csv.NextRecord();
			

			foreach (var packageGroup in packages)
			{
				GD.Print(packageGroup.Key);
				var selectedPackage =
					(from package in construction.Packages
					where package.ShipmentID == packageGroup.Key
					select package).First();
				
				csv.WriteField(selectedPackage.Origin);
				csv.WriteField(selectedPackage.ShipmentID);
				csv.WriteField(Math.Round(((double)selectedPackage.Weight)/1000000,2));
				csv.WriteField(Math.Round((double)selectedPackage.Volume/1000000,3));
				csv.WriteField($"{selectedPackage.Length}-{selectedPackage.Height}-{selectedPackage.Width}");
				csv.WriteField(string.Join('-', selectedPackage.Categories));
				csv.WriteField(selectedPackage.DestinationFlight);
				csv.WriteField(selectedPackage.Destination);
				csv.WriteField(packageGroup.Count());
				csv.NextRecord();
			}

			csv.NextRecord();
			csv.NextRecord();
		}
		
		GD.Print("Printed Disposition");

		GD.Print(file);
	}

	public void LoadFile(string file)
	{
		_bookingListPath = file;
		GD.Print(file);
	}

	public void ViewSolution(Solution solution)
	{
		var global = GetNode<Global>("/root/Global");
		global.CurrentSolution = solution;
		global.Solutions = _solutions;
		global.GotoScene("res://solution_screen.tscn");
	}

	public void PrintDisposition(Solution solution)
	{
		_currentSolution = solution;
		_fdSave.Visible = true;
	}
}

