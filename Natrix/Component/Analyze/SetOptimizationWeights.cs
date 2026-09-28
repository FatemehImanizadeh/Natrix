using System;
using System.Linq;
using Grasshopper.Kernel;
using Natrix.Utils;

namespace Natrix.Component.Analyze
{
    public class SetOptimizationWeights : GH_Component
    {
        public SetOptimizationWeights() : base("Set Evaluation Weights", "SEW",
            "Rescores and sorts a collection using nonnegative relative weights; zero disables a criterion. " +
            "Default weights: parking count 0.9 (90%); average path length, average turns, " +
            "path length standard deviation and turns standard deviation each 0.025 (2.5%).",
            "Natrix", "Analyse") { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Parking Collection", "PC", "Collection to rescore.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of Parkings", "NP", "Capacity weight. Default: 0.9 (90%).", GH_ParamAccess.item, Optimization.DefaultCapacityWeight);
            pManager.AddNumberParameter("Average Path Length", "PL", "Mean travel-distance weight. Default: 0.025 (2.5%).", GH_ParamAccess.item, Optimization.DefaultOtherWeight);
            pManager.AddNumberParameter("Average Turns", "T", "Mean turn-count weight. Default: 0.025 (2.5%).", GH_ParamAccess.item, Optimization.DefaultOtherWeight);
            pManager.AddNumberParameter("Path Length StdDev", "SD-L", "Weight for uniform travel distances; lower spread is better. Default: 0.025 (2.5%).", GH_ParamAccess.item, Optimization.DefaultOtherWeight);
            pManager.AddNumberParameter("Turns StdDev", "SD-T", "Weight for uniform turn counts; lower spread is better. Default: 0.025 (2.5%).", GH_ParamAccess.item, Optimization.DefaultOtherWeight);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parking Collection", "PC", "Rescored options, highest score first.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Older GH files may restore the removed socket and its old parameter order.
            // Do not silently interpret the old NC weight as the turns weight.
            if (Params.Input.Count != 6 || Params.Input[3].Name != "Average Turns" ||
                Params.Input[4].Name != "Path Length StdDev" || Params.Input[5].Name != "Turns StdDev")
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "This component uses the old input layout. Replace it with a new Set Evaluation Weights " +
                    "component and reconnect weights by name. Defaults: capacity 0.9; each other criterion 0.025.");
                return;
            }
            GenerationCollection source = null;
            if (!DA.GetData(0, ref source) || source?.parkings == null) return;
            double lots = Optimization.DefaultCapacityWeight, length = Optimization.DefaultOtherWeight,
                turns = Optimization.DefaultOtherWeight, pathSpread = Optimization.DefaultOtherWeight,
                turnSpread = Optimization.DefaultOtherWeight;
            if (!DA.GetData(1, ref lots) || !DA.GetData(2, ref length) || !DA.GetData(3, ref turns) ||
                !DA.GetData(4, ref pathSpread) || !DA.GetData(5, ref turnSpread)) return;
            var weights = new Optimization { LotNumW = lots, PathLenW = length, DirShiftW = turns,
                PathStdDevW = pathSpread, TurnsStdDevW = turnSpread };
            // Keep upstream scores intact; geometry is shared and never modified here.
            var result = new GenerationCollection { parkings = source.parkings.Where(p => p != null)
                .Select(p => p.CopyForScoring()).ToList() };
            try { Optimization.OptimizationFunction(weights, result.parkings); }
            catch (ArgumentException ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message); return; }
            if (result.parkings.Any(p => !p.HasValidScore))
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Some options have invalid or missing metrics and are unscored. Regenerate them to calculate enabled statistics.");
            result.parkings = result.parkings.OrderByDescending(p => p.HasValidScore).ThenByDescending(p => p.Score).ToList();
            DA.SetData(0, result);
        }

        protected override System.Drawing.Bitmap Icon => NatrixLogo.CreateIcon(24, "Natrix.SetOptimizationWeights.png");
        public override Guid ComponentGuid => new Guid("B36E7B5B-3C76-44F8-8C3F-242A5CDF76DF");
    }
}
