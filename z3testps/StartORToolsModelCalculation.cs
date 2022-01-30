using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Sat;
using System.Management.Automation;

namespace z3testps
{
    [Cmdlet(VerbsLifecycle.Start, "OrToolsModelCalculation")]
    public class StartOrToolsModelCalculation : BaseCMDLet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public PSObject[] SourceVM;

        [Parameter(Position = 1, Mandatory = true)]
        public PSObject[] TargetVM;

        private List<VMMappingResult> _results = new List<VMMappingResult>();

        protected override void ProcessRecord()
        {
            SourceVMRecord[] sourceVMs = MakeSourceVMsArray(SourceVM); // length = 87
            TargetVMRecord[] targetVMs = MakeTargetVMsArray(TargetVM); // length = 240

            int[] costVector = targetVMs.Select(x => (int)(double.Parse(x.retailPrice) * 10000)).ToArray();
            int[] acuVector = targetVMs.Select(x => int.Parse(x.ACUs)).ToArray();


            IntVar[,] selectedVms = new IntVar[sourceVMs.Length, targetVMs.Length];
            var tmpSum = new List<LinearExpr>();
            var tmpAcu = new List<LinearExpr>();

            SolutionPrinter cb = new SolutionPrinter(sourceVMs, targetVMs, selectedVms, ref _results, 300);
            
            var model = CpModel(sourceVMs, targetVMs, selectedVms, tmpSum, costVector, tmpAcu, acuVector);
            model.Maximize(LinearExpr.Sum(tmpAcu));
            

            CpSolver solver = new CpSolver();
            solver.StringParameters += "num_search_workers:4, log_search_progress: true, max_time_in_seconds:90 ";
            CpSolverStatus status = solver.Solve(model, cb);

            // run again
            selectedVms = new IntVar[sourceVMs.Length, targetVMs.Length];
            tmpSum = new List<LinearExpr>();
            tmpAcu = new List<LinearExpr>();
            cb = new SolutionPrinter(sourceVMs, targetVMs, selectedVms, ref _results, 300);
            model = CpModel(sourceVMs, targetVMs, selectedVms, tmpSum, costVector, tmpAcu, acuVector);

            model.Add(LinearExpr.Sum(tmpAcu) >= (int)solver.ObjectiveValue);
            model.Minimize(LinearExpr.Sum(tmpSum));

            solver = new CpSolver();
            solver.StringParameters += "num_search_workers:4, log_search_progress: true, max_time_in_seconds:90 ";
            status = solver.Solve(model, cb);
            
            WriteObject(_results);
            WriteObject(status);
        }

        private static CpModel CpModel(SourceVMRecord[] sourceVMs, TargetVMRecord[] targetVMs, IntVar[,] selectedVms,
            List<LinearExpr> tmpSum, int[] costVector, List<LinearExpr> tmpAcu, int[] acuVector)
        {
            CpModel model = new CpModel();

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                for (int j = 0; j < targetVMs.Length; j++)
                {
                    selectedVms[i, j] = model.NewBoolVar($"x[{sourceVMs[i].vmid},{targetVMs[j].Name}]");
                }
            }

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                var sourceVm = sourceVMs[i];

                IntVar[] tmp = new IntVar[targetVMs.Length];
                for (int j = 0; j < targetVMs.Length; j++)
                {
                    var targetVm = targetVMs[j];
                    if ((int.Parse(targetVm.vCPUs) >= sourceVm.cpu * 0.8) && (double.Parse(targetVm.MemoryGB) >= sourceVm.ram))
                    {
                        tmp[j] = selectedVms[i, j];
                    }
                }

                tmpSum.Add(LinearExpr.ScalProd(tmp, costVector));
                tmpAcu.Add(LinearExpr.ScalProd(tmp, acuVector));
                model.Add(LinearExpr.Sum(tmp) == 1);
            }

            return model;
        }
    }
}