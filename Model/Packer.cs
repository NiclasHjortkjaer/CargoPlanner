using System.Collections.Generic;
using System.Linq;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;
using Godot;
using Google.OrTools.Sat;
using Container = CromulentBisgetti.ContainerPacking.Entities.Container;

namespace GodotStart.Model;

public class Packer
{
	public Packer(DataModel dataModel, BoolVar[,] x, CpSolver solver, double limit)
	{
		_dataModel = dataModel;
		_x = x;
		_solver = solver;
		_limit = limit;
	}

	private readonly DataModel _dataModel;
	private readonly BoolVar[,] _x;
	private readonly CpSolver _solver;
	private double _limit;
	private double _highestLimit = 0;

	private static string PackageToArr(Package package)
	{
		var x = new[] { package.X, package.Y, package.Z, package.PackDimX, package.PackDimY, package.PackDimZ };
		return string.Join(',', x);
	}


	public (DataModel, List<Construction> constructions, double _limit) Pack()
	{
		var totalUnpackedItems = new List<Item>();
		var totalUnusedPallets = new List<ULD>();
		var constructions = new List<Construction>();

		while (true)
		{
			var unpackedItems = new List<Item>();
			var unusedPallets = new List<ULD>();
			for (var j = 0; j < _dataModel.Containers.Length; j++)
			{
				BuildConstruction(j, constructions, unpackedItems, unusedPallets, true);
			}
			
			if (constructions.Count > 0)
			{
				totalUnpackedItems = unpackedItems;
				totalUnusedPallets = unusedPallets;
				break;
			}

			_limit = _highestLimit;
		}

		return (new DataModel
		{
			Containers = totalUnusedPallets.ToArray(),
			Items = totalUnpackedItems.Select(item => _dataModel.Items[item.ID]).ToList()
		}, constructions, _limit);
	}

	private void BuildConstruction(int j, List<Construction> constructions, List<Item> unpackedItems, List<ULD> unusedPallets, bool firstTry)
	{
		var construction = new Construction();

		var containers = new List<Container>();
		var pallet = _dataModel.Containers[j];
		pallet.ID = j;
		if (firstTry)
		{
			containers.Add(pallet.ToContainer());
		}
		else
		{
			containers.Add(pallet.ToLiftedContainer());
			construction.IsLifted = true;
		}

		var itemsToPack = new List<Item>();
		var packagesToPack = new Package[_dataModel.Items.Count];
		for (var i = 0; i < _dataModel.Items.Count; i++)
		{
			var b = _x[i, j];
			if (_solver.BooleanValue(b))
			{
				var newItem = _dataModel.Items[i].ToItem(i);
				itemsToPack.Add(newItem);
				newItem.ID = i;
				packagesToPack[i] = (_dataModel.Items[newItem.ID]);
			}
		}

		if (itemsToPack.Count != 0)
		{
			var algorithms = new List<int> { (int)AlgorithmType.EB_AFIT };

			var result = PackingService.Pack(containers, itemsToPack, algorithms);
			
			construction.Container = _dataModel.Containers[j];

			var totalWeight = 0.0;
			foreach (var packedItem in result[0].AlgorithmPackingResults[0].PackedItems)
			{
				var package = packagesToPack[packedItem.ID]; 
				package.AddDimensions(packedItem.CoordX, packedItem.CoordY, packedItem.CoordZ, packedItem.PackDimX, packedItem.PackDimY, packedItem.PackDimZ);
				construction.Packages.Add(package);
				totalWeight += package.Weight;
			}

			construction.TotalWeight = totalWeight/100000;

			var itemString = new List<string>(construction.Packages.Count);

			itemString.AddRange(construction.Packages.Select(PackageToArr));

			construction.ArgumentString = $"\"{_dataModel.Containers[j].Type} {construction.Container.ID}\" {construction.Container.Length} {construction.Container.Width} {construction.Container.Height} {string.Join('.', itemString)}";

			var packedVolume = result[0].AlgorithmPackingResults[0].PackedItems.Sum(s => s.Volume);
			construction.TotalVolume = (double)(packedVolume/1000000);

			foreach (var category in result[0].AlgorithmPackingResults[0].PackedItems.SelectMany(item => _dataModel.Items[item.ID].Categories))
			{
				construction.Categories.Add(category);
			}

			foreach (var item in result[0].AlgorithmPackingResults[0].PackedItems)
			{
				construction.Destinations.Add(_dataModel
					.Items[item.ID].Destination);
				construction.Origins.Add(_dataModel.Items[item.ID].Origin);
			}

			if (construction.Packages.Count > 0 && (double)packedVolume >= pallet.Volume * _limit - 0.001)
			{
				if (construction.IsLifted)
				{
					construction.Container.Length += 20;
					construction.Container.Height -= 10;
				}
				constructions.Add(construction);
			}
			else {
				if (_highestLimit < (double)packedVolume / pallet.Volume)
				{
					_highestLimit = (double)packedVolume / pallet.Volume;
				}

				if (firstTry && construction.Container.Type == PalletEnum.PMC)
				{
					BuildConstruction(j, constructions, unpackedItems, unusedPallets, false);
					return;
				}
				else
				{
					unusedPallets.Add(pallet);
					unpackedItems.AddRange(result[0].AlgorithmPackingResults[0].PackedItems);
				}
					
			}

			unpackedItems.AddRange(result[0].AlgorithmPackingResults[0].UnpackedItems);
		}
		else
		{
			unusedPallets.Add(pallet);
		}
	}
}
