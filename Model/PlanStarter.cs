using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Godot;
using Google.OrTools.Sat;

namespace GodotStart.Model;

public class PlanStarter
{
    private static CargoPlanner? _cargoPlanner;

    private static string BookingListPath { get; set; } =
        "/home/niclas/Documents/bachelorprojekt/Bachelor/CargoPlanner/BookingLists/Bookinglist_07FEB.csv";

    private static List<Solution> _oldSolutions = new List<Solution>();

    public static Solution? Plan(string bookingListPath)
    {
        BookingListPath = bookingListPath;
        var dataModel = LoadData();

        
        
        _cargoPlanner = new CargoPlanner();
        
        var isComplete = false;
        var constructions = new List<Construction>();
        var limit = 0.95;

        DataModel newModel = dataModel;
        var packagessNotTogether = GenerateNewSolutionConstraint(newModel);
        
        while (!isComplete && limit >= 0)
        {

            GD.Print($"trying again, limit: {limit}");
            var noNewConstructions = false;
            while (!noNewConstructions)
            {
                (newModel, var newConstructions) = _cargoPlanner.Plan(newModel, limit, packagessNotTogether);

                if (newModel == null && newConstructions == null)
                {
                    limit = 0;
                    break;
                }
                
                constructions.AddRange(newConstructions);
                GD.Print($"created constructions: {newConstructions.Count}");
                GD.Print($"new items: {newModel.Items.Count}");
                
                if (newModel.Items.Count == 0)
                {
                    isComplete = true;
                }
                else
                {
                    GD.Print("Not complete yet");
                    packagessNotTogether = GenerateNewSolutionConstraint(newModel);
                    GD.Print($"length: {newModel.Items[0].Length}, width: {newModel.Items[0].Width}, height: {newModel.Items[0].Height}, volume: {newModel.Items[0].Volume}");
                }

                if (newConstructions.Count == 0)
                {
                    noNewConstructions = true;
                }
            }

            limit -= 0.05;
        }

        if (isComplete)
        {
            var score = 18 - constructions.Count;
            GD.Print("Valid solution found");
            if (score > 0)
            {
                GD.Print($"Score: {score}");
            }
            var solution = new Solution
            {
                Id = _oldSolutions.Count,
                Constructions = constructions,
                Score = score,
                ConstructionCount = constructions.Count
            };
            
            var count = constructions.Sum(construction => construction.Packages.Count);
            GD.Print($"number of packages in total: {count}");
            _oldSolutions.Add(solution);
            return solution;
        }
        else
        {
            GD.Print($"no solution found, limit: {limit}");
        }

        return null;
    }

    private static List<(int,int)> GenerateNewSolutionConstraint(DataModel newModel)
    {
        var packagesNotTogether = new List<(int, int)>();
        foreach (var solution in _oldSolutions)
        {
            for (var i = 0; i < solution.Constructions.Count; i++)
            {
                var construction = solution.Constructions[i];
                if (construction.Packages.Count > 1)
                {
                    var p1 = construction.Packages[0];
                    var p2 = construction.Packages[1];
                    var i1 = newModel.Items.FindIndex(i => i.ID == p1.ID);
                    var i2 = newModel.Items.FindIndex(i => i.ID == p2.ID);
                    if (i1 != -1 && i2 != -1) packagesNotTogether.Add((i1, i2));
                }
            }
        }
        
        return packagesNotTogether;
    }



    private static DataModel LoadData()
    {
        var dataModel = new DataModel();
        var packageId = 0;
        
        dataModel.Containers = new Pallet[18];

        for (int j = 0; j < 8; j++)
        {
            dataModel.Containers[j] = new Pallet(j,PalletEnum.PMC, (long) 5201 * 10000, (long) (337.5*243.8*152.6), (long) 317.5, (long) 243.8, (long) 162.6);
        }

        for (int i = 8; i < 18; i++)
        {
            dataModel.Containers[i] = new Pallet(i, PalletEnum.AKE, 1587 * 10000, (long) (156.2*153.4*162.6), (long) 156.2, (long) 153.4, (long) 162.6);
        }
        
        var packages = new List<Package>();
        
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = false
        };

        using var streamReader = File.OpenText(BookingListPath);
        using var csvReader = new CsvReader(streamReader, csvConfig);

        // remove first row
        csvReader.Read();
        while (csvReader.Read())
        {
            csvReader.TryGetField(0, out string? origin);
            csvReader.TryGetField(2, out string? shipmentId);
            csvReader.TryGetField(3, out int count);
            csvReader.TryGetField(4, out double weight);
            csvReader.TryGetField(5, out double volume);
            csvReader.TryGetField(6, out string? dimensionString);
            csvReader.TryGetField(7, out string? categories);
            csvReader.TryGetField(8, out string? destinationFlight);
            csvReader.TryGetField(10, out string? destination);
            

            var dimensionList = new List<(double, double, double, int)>();
            char[] delimiterChars = {'-', '/'};
            if (dimensionString != null && dimensionString.Trim() != "")
            {
                dimensionString = dimensionString.Trim();
                var dimensionsArr = dimensionString.Split('\n');
                dimensionList.AddRange(dimensionsArr.Select(dimensionsForPackage => dimensionsForPackage.Split(delimiterChars)).Select(dimensions => (Convert.ToDouble(dimensions[0]), Convert.ToDouble(dimensions[1]), Convert.ToDouble(dimensions[2]), Convert.ToInt32(dimensions[3]))));
            }
            
            List<CategoryEnum> categoryEnums = new List<CategoryEnum>();

            if (categories != null)
            {
                string[] categoryArr = categories.Split('-');

                foreach (var category in categoryArr)
                {
                    Enum.TryParse(category, out CategoryEnum categoryEnum);
                    categoryEnums.Add(categoryEnum);
                }
            }

            if (destination == null || destination.Trim() == "")
            {
                destination = "BKK";
            }
            
            var longWeight = Convert.ToInt64(weight / count)*10000;
            var totalVolume = volume * 1000000;

            var packagesLeft = count;
            if (dimensionList.Count != 0)
            {
                foreach (var dimensionsTuple in dimensionList)
                {
                    for (int i = 0; i < dimensionsTuple.Item4; i++)
                    {
                        var volumeInPackage = Convert.ToInt64(dimensionsTuple.Item1 * dimensionsTuple.Item2 * dimensionsTuple.Item3);
                        
                        var package = new Package(packageId, origin, shipmentId, longWeight, volumeInPackage, (long) dimensionsTuple.Item1, (long) dimensionsTuple.Item2,
                            (long) dimensionsTuple.Item3, categoryEnums, destinationFlight, destination);
                        packages.Add(package);
                
                        totalVolume -= volumeInPackage;
                        packagesLeft--;
                        packageId++;
                    }
                }
            }
            
            if (packagesLeft != 0)
            {
                var volumePrPackage = Convert.ToInt64(totalVolume / packagesLeft);
                
                var sides = Convert.ToInt64(Math.Ceiling(Math.Pow(volumePrPackage, (double) 1 / 3)));
                for (var i = 0; i < packagesLeft; i++)
                {
                    packages.Add(new Package(packageId, origin, shipmentId, longWeight, volumePrPackage, sides, categoryEnums, destinationFlight, destination));
                    packageId++;
                }
            }

        }

        dataModel.Items = packages;

        return dataModel;
    }
}
