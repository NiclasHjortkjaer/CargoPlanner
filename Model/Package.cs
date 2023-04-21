using System.Collections.Generic;
using CromulentBisgetti.ContainerPacking.Entities;

namespace GodotStart.Model;

public class Package
{
    public int ID { get; set; }
    public string Origin { get; set; }
    public string ShipmentID { get; set; }
    public long ShipmentIDNumber { get; set; }
    public long Weight { get; set; }
    public long Volume { get; set; }
    public long Length { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
    public List<CategoryEnum> Categories { get; set; }
    public string DestinationFlight { get; set; }
    public string Destination { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal Z { get; set; }
    public decimal PackDimX { get; set; }
    public decimal PackDimY { get; set; }
    public decimal PackDimZ { get; set; }

    public Package(int id, string origin, string shipmentId, long weight, long volume, long length, long width, long height, List<CategoryEnum> categories, string destinationFlight, string destination)
    {
        ID = id;
        Origin = origin;
        ShipmentID = shipmentId;
        Weight = weight;
        Volume = volume;
        Length = length;
        Width = width;
        Height = height;
        Categories = categories;
        DestinationFlight = destinationFlight;
        Destination = destination;
    }

    public Package(int id, string origin, string shipmentId, long weight, long volume, long sides, List<CategoryEnum> categories, string destinationFlight, string destination)
    {
        ID = id;
        Origin = origin;
        ShipmentID = shipmentId;
        Weight = weight;
        Volume = volume;
        Length = sides;
        Width = sides;
        Height = sides;
        Categories = categories;
        DestinationFlight = destinationFlight;
        Destination = destination;
    }

    public Item ToItem(int id)
    {
        return new Item(id, Length, Width, Height, 1);
    }

    public void AddDimensions(decimal x, decimal y, decimal z, decimal packDimX, decimal packDimY, decimal packDimZ)
    {
        X = x;
        Y = y;
        Z = z;
        PackDimX = packDimX;
        PackDimY = packDimY;
        PackDimZ = packDimZ;
    }
}
