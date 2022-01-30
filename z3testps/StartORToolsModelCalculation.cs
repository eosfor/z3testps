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

            IntVar[,] selectedVms;
            List<LinearExpr> tmpSum;
            List<LinearExpr> tmpAcu;
            SolutionPrinter cb;
            CpModel model;
            CpSolver solver;
            CpSolverStatus status;


            model = InitializeModel(sourceVMs, targetVMs, costVector, acuVector, out selectedVms, out tmpSum, out tmpAcu, out cb);
            model.Maximize(LinearExpr.Sum(tmpAcu));
            
            solver = new CpSolver();
            solver.StringParameters += "num_search_workers:4, log_search_progress: true, max_time_in_seconds:90 ";
            status = solver.Solve(model, cb);

            // fix the optimized value by adding it as a constraint
            // recreate and rerun the model
            model = InitializeModel(sourceVMs, targetVMs, costVector, acuVector, out selectedVms, out tmpSum, out tmpAcu, out cb);
            model.Add(LinearExpr.Sum(tmpAcu) >= (int)solver.ObjectiveValue); //objective from the previous run
            model.Minimize(LinearExpr.Sum(tmpSum));

            solver = new CpSolver();
            solver.StringParameters += "num_search_workers:4, log_search_progress: true, max_time_in_seconds:90 ";
            status = solver.Solve(model, cb);
            
            WriteObject(_results);
            WriteObject(status);
        }

        private CpModel InitializeModel(SourceVMRecord[] sourceVMs, TargetVMRecord[] targetVMs, int[] costVector, int[] acuVector,
            out IntVar[,] selectedVms, out List<LinearExpr> tmpSum, out List<LinearExpr> tmpAcu, out SolutionPrinter cb)
        {
            CpModel model;
            selectedVms = new IntVar[sourceVMs.Length, targetVMs.Length];
            tmpSum = new List<LinearExpr>();
            tmpAcu = new List<LinearExpr>();
            cb = new SolutionPrinter(sourceVMs, targetVMs, selectedVms, ref _results, 300);
            model = CreateModel(sourceVMs, targetVMs, selectedVms, tmpSum, costVector, tmpAcu, acuVector);
            return model;
        }

        private CpModel CreateModel(SourceVMRecord[] sourceVMs, TargetVMRecord[] targetVMs, IntVar[,] selectedVms,
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