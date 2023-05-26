using System.Collections.Generic;

namespace GodotStart.Model;

public class Construction
{
    public ULD Container { get; set; }
    public List<Package> Packages { get; set; } = new();
    public string ArgumentString { get; set; }
    public double TotalVolume { get; set; }
    public double TotalWeight { get; set; }
    public HashSet<CategoryEnum> Categories { get; set; } = new();
    public string CategoryString => string.Join('-', Categories);
    public HashSet<string> Destinations { get; set; } = new();
    public string DestinationString => string.Join('-', Destinations);
    public HashSet<string> Origins { get; set; } = new();
    public string Originstring => string.Join('-', Origins);
    public bool IsLifted { get; set; }
}