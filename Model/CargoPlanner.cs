using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.Sat;

namespace GodotStart.Model;

public class CargoPlanner
{
    private CpSolver _solver;
    
    public (DataModel?, List<Construction>?) Plan(DataModel dataModel, double limit, List<(int,int)> packagesNotTogether)
    {
        GD.Print($"Containers: {dataModel.Containers.Length}, Items: {dataModel.Items.Count}");
        var model = new CpModel();
        
        // VARIABLES
        var x = new BoolVar[dataModel.Items.Count, dataModel.Containers.Length];
        for (int i = 0; i < dataModel.Items.Count; i++)
        {
            for (int j = 0; j < dataModel.Containers.Length; j++)
            {
                x[i,j] = model.NewBoolVar($"x{i}_{j}");
            }
        }
        
        var y = new BoolVar[dataModel.Containers.Length];
        for (int j = 0; j < dataModel.Containers.Length; j++)
        {
            y[j] = model.NewBoolVar($"y_{j}");
        }
        
        // CONSTRAINTS
        for (int i = 0; i < dataModel.Items.Count; ++i)
        {
            var isInBin = new List<BoolVar>();

            for (int j = 0; j < dataModel.Containers.Length; ++j)
            {
                isInBin.Add(x[i,j]);
            }

            model.AddExactlyOne(isInBin);
        }
        
        for (int j = 0; j < dataModel.Containers.Length; ++j)
        {
            List<BoolVar> items = new List<BoolVar>();
            for (int i = 0; i < dataModel.Items.Count; i++)
            {
                items.Add(x[i, j]);
            }

            for (int i = 0; i < dataModel.Items.Count; i++)
            {
                model.Add(dataModel.Items[i].Length <= LinearExpr.Term(y[j], dataModel.Containers[j].Length)).OnlyEnforceIf(x[i,j]);
                model.Add(dataModel.Items[i].Width <= LinearExpr.Term(y[j], dataModel.Containers[j].Width)).OnlyEnforceIf(x[i,j]);
                model.Add(dataModel.Items[i].Height <= LinearExpr.Term(y[j], dataModel.Containers[j].Height)).OnlyEnforceIf(x[i,j]);

                if (dataModel.Containers[j].Type == PalletEnum.PMC &&
                    dataModel.Items[i].Categories.Contains(CategoryEnum.VAL))
                {
                    model.Add(x[i, j] == 0);
                }
            }

            model.Add(LinearExpr.WeightedSum(items, dataModel.Items.Select(i => i.Weight)) <= LinearExpr.Term(y[j], dataModel.Containers[j].Weight));
            model.Add(LinearExpr.WeightedSum(items, dataModel.Items.Select(i => i.Volume)) <= LinearExpr.Term(y[j], dataModel.Containers[j].Volume));


            for (int m = 0; m < dataModel.Items.Count; m++)
            {
                for (int n = 0; n < dataModel.Items.Count; n++)
                {
                    var aviM = dataModel.Items[m].Categories.Contains(CategoryEnum.AVI);
                    var aviN = dataModel.Items[n].Categories.Contains(CategoryEnum.AVI);;
                    var iceN = dataModel.Items[n].Categories.Contains(CategoryEnum.ICE);
                    var eatN = dataModel.Items[n].Categories.Contains(CategoryEnum.EAT);
                    var pilM = dataModel.Items[m].Categories.Contains(CategoryEnum.PIL);
                    var humM = dataModel.Items[m].Categories.Contains(CategoryEnum.HUM);
                    var lhoN = dataModel.Items[n].Categories.Contains(CategoryEnum.LHO);
                    var rclM = dataModel.Items[m].Categories.Contains(CategoryEnum.RCL);
                    var hegN = dataModel.Items[n].Categories.Contains(CategoryEnum.HEG);
                    var rflN = dataModel.Items[n].Categories.Contains(CategoryEnum.RFL);
                    var roxM = dataModel.Items[m].Categories.Contains(CategoryEnum.ROX);
                    var rscN = dataModel.Items[n].Categories.Contains(CategoryEnum.RFL);
                    var rfwN = dataModel.Items[n].Categories.Contains(CategoryEnum.RFW);
                    var rcmM = dataModel.Items[m].Categories.Contains(CategoryEnum.RCM);
                    
                    
                    if (rfwN && rcmM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }
                    
                    if (rscN && roxM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }
                    
                    if (aviM && iceN)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());                        
                    }

                    if (eatN && pilM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (eatN && aviM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (eatN && humM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (aviN && humM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (lhoN && humM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (lhoN && rclM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }
                    
                    if (hegN && rclM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }
                    
                    if (rflN && roxM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }

                    if (hegN && rclM)
                    {
                        // model.AddAtMostOne(new[] { x[m, j], x[n, j] });
                        var eitherOr = model.NewBoolVar("eitherOr");
                        model.Add(x[m,j] == 0).OnlyEnforceIf(eitherOr);
                        model.Add(x[n,j] == 0).OnlyEnforceIf(eitherOr.Not());   
                    }
                }
            }
        }

        foreach (var (i1, i2) in packagesNotTogether)
        {
            for (var j = 0; j < dataModel.Containers.Length; j++)
            {
                model.AddImplication(x[i1, j], x[i2, j].Not());
                model.AddImplication(x[i2, j], x[i1, j].Not());
            }
        }

        model.Minimize(LinearExpr.Sum(y));

        _solver = new CpSolver();
        var result = _solver.Solve(model);

        if (result is CpSolverStatus.Feasible or CpSolverStatus.Optimal)
        {
            var packer = new Packer(dataModel, x, _solver, limit);
            return packer.Pack();
        }

        return (null, null);

    }

    public async void StopSearch()
    {
        _solver.StopSearch();
    }
}