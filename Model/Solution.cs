using System.Collections.Generic;

namespace GodotStart.Model;

public class Solution
{
    public List<Construction> Constructions = new();
    public int Id { get; set; }
    public int Score { get; set; }
    public int ConstructionCount { get; set; }
}