using System.CodeDom;
using System.Management.Automation;
using Microsoft.Z3;
using System.Linq;

namespace z3testps
{
    [Cmdlet(VerbsLifecycle.Start, "Z3ModelCalculation")]
    public class StartZ3ModelCalculation: BaseCMDLet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public new PSObject[] SourceVM;

        [Parameter(Position = 1, Mandatory = true)]
        public new PSObject[] TargetVM;

        protected override void ProcessRecord()
        {
            var sourceVMs = MakeSourceVMsArray(SourceVM); // length = 87
            var targetVMs = MakeTargetVMsArray(TargetVM); // length = 240

            var costVector = targetVMs.Select(x => (int)(double.Parse(x.retailPrice) * 10000)).ToArray();
            var acuVector = targetVMs.Select(x => int.Parse(x.ACUs)).ToArray();
            var ones = CreateArrayCoefficiensOnes(targetVMs);

            var ctx = new Context();
            var s =  ctx.MkOptimize(); // ctx.MkSolver(); 

            var zero = ctx.MkInt(0);
            var one = ctx.MkInt(1);


            BoolExpr[,] selectedVMs = new BoolExpr[sourceVMs.Length, targetVMs.Length];

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                for (int j = 0; j < targetVMs.Length; j++)
                {
                    selectedVMs[i,j] = ctx.MkBoolConst($"{sourceVMs[i].vmid}-{targetVMs[j].Name}");
                }
            }

            var tmpPrice = new List<ArithExpr>();
            var tmpAcu = new List<ArithExpr>();

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                var sourceVm = sourceVMs[i];
                BoolExpr[] tmp = new BoolExpr[targetVMs.Length];

                for (int j = 0; j < targetVMs.Length; j++)
                {
                    var targetVm = targetVMs[j];
                    if ((int.Parse(targetVm.vCPUs) >= sourceVm.cpu * 0.8) && (double.Parse(targetVm.MemoryGB) >= sourceVm.ram))
                    {
                        tmp[j] = selectedVMs[i, j];
                    }
                    else
                    {
                        tmp[j] = ctx.MkBool(false);
                    }
                }

                tmpPrice.Add(ScalProd(tmp, costVector, ctx));
                tmpAcu.Add(ScalProd(tmp, acuVector, ctx));
                s.Assert(ctx.MkPBEq(ones, tmp, 1));
            }

            var totalACU = ctx.MkAdd(tmpAcu);
            var totalPrice = ctx.MkAdd(tmpPrice);

            int ubCost = (int)(35.814 * 10000);
            int lbCost = (int)(0.065 * 10000);

            s.Assert(ctx.MkLe(totalACU, ctx.MkInt(230 * 87)));
            s.Assert(ctx.MkGe(totalACU, ctx.MkInt(100 * 87)));

            s.Assert(ctx.MkLe(totalPrice, ctx.MkInt(ubCost * 87)));
            s.Assert(ctx.MkGe(totalPrice, ctx.MkInt(lbCost * 87)));

            var totalPriceHandle = s.MkMinimize(ctx.MkAdd(tmpPrice));  // minimize total price
            var totalAcuHandle = s.MkMaximize(ctx.MkAdd(tmpAcu));     // maximize total performance

            var modelText = s.ToString();

            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                WriteObject(m);
                WriteObject(totalAcuHandle);
                WriteObject(totalPriceHandle);
                WriteObject(modelText);
            }
        }

        private static int[] CreateArrayCoefficiensOnes(TargetVMRecord[] targetVMs)
        {
            int[] ones = new int[targetVMs.Length];
            for (int i = 0; i < targetVMs.Length; i++)
            {
                ones[i] = 1;
            }

            return ones;
        }

        private ArithExpr ScalProd(BoolExpr[] left, int[] right, Context ctx)
        {
            // vectors should be of the same length.
            // TODO: verify this

            var zero = ctx.MkInt(0);
            ArithExpr ret = ctx.MkAdd(zero);
            for (int i = 0; i < left.Length; i++)
            {
                var boolToInt = ctx.MkITE(left[i], ctx.MkInt(1), ctx.MkInt(0));
                ret = null == ret ? ctx.MkAdd(ctx.MkMul((ArithExpr)boolToInt, ctx.MkInt(right[i]))) : 
                                    ctx.MkAdd(ret, ctx.MkMul((ArithExpr)boolToInt, ctx.MkInt(right[i])));
                
            }
            return ret;
        }
    }

}