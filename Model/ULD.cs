using CromulentBisgetti.ContainerPacking.Entities;

namespace GodotStart.Model;

public class ULD
{
    public int ID { get; set; } = 1;
    public PalletEnum Type { get; set; }
    public long Weight { get; set; }
    public long Volume { get; set; }
    public long Length { get; set; }
    public long Width { get; set; }
    public long Height { get; set; }
    public ULD(int id, PalletEnum type, long weight, long volume, long length, long width, long height)
    {
        ID = id;
        Type = type;
        Weight = weight;
        Volume = volume;
        Length = length;
        Width = width;
        Height = height;
    }
    

    public Container ToContainer()
    {
        return new Container(ID, Length, Width, Height);
    }

    public Container ToLiftedContainer()
    {
        return new Container(ID, Length + 20, Width, Height - 10);
    }
}