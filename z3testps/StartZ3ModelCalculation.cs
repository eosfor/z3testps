using System.CodeDom;
using System.Management.Automation;
using Microsoft.Z3;
using System.Linq;

namespace z3testps
{
    [Cmdlet(VerbsLifecycle.Start, "Z3ModelCalculation")]
    public class StartZ3ModelCalculation: BaseCMDLet
    {
        [Parameter(Position=0, Mandatory = true)]
        public PSObject[] SourceVM;
        
        [Parameter(Position = 1, Mandatory = true)]
        public PSObject[] TargetVM;

        protected override void ProcessRecord()
        {
            SourceVMRecord[] sourceVMs = MakeSourceVMsArray(SourceVM); // length = 87
            TargetVMRecord[] targetVMs = MakeTargetVMsArray(TargetVM); // length = 240

            int[] costVector = targetVMs.Select(x => (int)(double.Parse(x.retailPrice) * 10000)).ToArray();
            int[] acuVector = targetVMs.Select(x => int.Parse(x.ACUs)).ToArray();

            var ctx = new Context();
            var s =  ctx.MkOptimize(); // ctx.MkSolver(); 


            var zero = ctx.MkInt(0);
            var one = ctx.MkInt(1);


            IntExpr[, ] selectedVMs = new IntExpr[sourceVMs.Length, targetVMs.Length];

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                for (int j = 0; j < targetVMs.Length; j++)
                {
                    selectedVMs[i,j] = ctx.MkIntConst($"{sourceVMs[i].vmid}-{targetVMs[j].Name}");
                }
            }

            var tmpPrice = new List<ArithExpr>();
            var tmpAcu = new List<ArithExpr>();

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                var sourceVm = sourceVMs[i];
                ArithExpr[] tmp = new ArithExpr[targetVMs.Length];

                for (int j = 0; j < targetVMs.Length; j++)
                {
                    var targetVm = targetVMs[j];
                    if ((int.Parse(targetVm.vCPUs) >= sourceVm.cpu * 0.8) && (double.Parse(targetVm.MemoryGB) >= sourceVm.ram))
                    {
                        tmp[j] = selectedVMs[i, j];
                    }
                    else
                    {
                        tmp[j] = zero;
                    }
                }

                tmpPrice.Add(ScalProd(tmp, costVector, ctx));
                tmpAcu.Add(ScalProd(tmp, acuVector, ctx));
                s.Assert(ctx.MkEq(ctx.MkAdd(tmp), one));
            }

            var totalAcuHandle = s.MkMaximize(ctx.MkAdd(tmpAcu ));     // maximize total performance
            var totalPriceHandle = s.MkMinimize(ctx.MkAdd(tmpPrice));  // minimize total price

            var modelText = s.ToString();
            File.WriteAllText(@"C:\temp\z3testps\smtlib-model.txt", modelText);

            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                WriteObject(m);
                WriteObject(totalAcuHandle);
                WriteObject(totalPriceHandle);
                WriteObject(ctx);
            }
        }

        private ArithExpr ScalProd(ArithExpr[] left, int[] right, Context ctx)
        {
            // vectors should be of the same length.
            // TODO: verify this

            var zero = ctx.MkInt(0);
            ArithExpr ret = ctx.MkAdd(zero);
            for (int i = 0; i < left.Length; i++)
            {
                ret = ctx.MkAdd(ret, ctx.MkMul(left[i], ctx.MkInt(right[i])));
            }
            return ret;
        }
    }

}