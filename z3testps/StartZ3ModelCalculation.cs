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
            TargetVMRecord[] targetVMs = MakeTargetVMsArray(SourceVM); // length = 240

            var ctx = new Context();
            var s = ctx.MkOptimize(); // ctx.MkSolver();

            #region Populate Data Arrays from input data

            ArraySort existingVmSort = ctx.MkArraySort(ctx.IntSort, ctx.IntSort);
            ArraySort vmSizeSort = ctx.MkArraySort(ctx.IntSort, ctx.IntSort);

            #region source-data
            
            ArrayExpr vmCPU = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmCPU"), existingVmSort);
            ArrayExpr vmRAM = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmRAM"), existingVmSort);
            for (int i = 0; i < sourceVMs.Length; i++)
            {
                s.Assert(ctx.MkEq(ctx.MkSelect(vmCPU, ctx.MkInt(i)), ctx.MkInt(sourceVMs[i].cpu)));
                s.Assert(ctx.MkEq(ctx.MkSelect(vmRAM, ctx.MkInt(i)), ctx.MkInt(sourceVMs[i].ram)));

            }

            #endregion source-data

            #region target-data

            //ArrayExpr vmSizeCPU =  ctx.MkArrayConst("vmSizeCPU", ctx.IntSort, ctx.IntSort);
            ArrayExpr vmSizeCPU = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeCPU"), vmSizeSort);
            ArrayExpr vmSizeRAM = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeRAM"), vmSizeSort);
            ArrayExpr vmSizePrice = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizePrice"), vmSizeSort);
            ArrayExpr vmSizeACU = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeACU"), vmSizeSort);
            for (int i = 0; i < targetVMs.Length; i++)
            {
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizeCPU, ctx.MkInt(i)), ctx.MkInt(targetVMs[i].vCPUs)));
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizeRAM, ctx.MkInt(i)), ctx.MkInt((int)double.Parse(targetVMs[i].MemoryGB) * 10000))); //a few hacks to get rid of doubles
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizePrice, ctx.MkInt(i)), ctx.MkInt( (int)double.Parse(targetVMs[i].retailPrice) * 10000 ))); //a few hacks to get rid of doubles
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizeACU, ctx.MkInt(i)), ctx.MkInt(int.Parse(targetVMs[i].ACUs))));
            }

            #endregion target-data
            
            #endregion

            // decision variables
            Expr[] selectedSizeArr = new Expr[sourceVMs.Length];

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                string n = sourceVMs[i].vmid;
                selectedSizeArr[i] = ctx.MkConst(n, ctx.IntSort);
                var constraint = ctx.MkAnd(ctx.MkGe((ArithExpr) selectedSizeArr[i], ctx.MkInt(0)), ctx.MkLt((ArithExpr) selectedSizeArr[i], ctx.MkInt(targetVMs.Length)));
                s.Assert(constraint);
            }

            // constraints
            for (int i = 0; i < selectedSizeArr.Length; i++) //for each variable
            {
                BoolExpr? constraint = null;
                var decisionVar = selectedSizeArr[i];


                var sourceCpu = ctx.MkSelect(vmCPU, ctx.MkInt(i));
                var sourceRam = ctx.MkSelect(vmRAM, ctx.MkInt(i));

                var targetCpu = ctx.MkSelect(vmSizeCPU, decisionVar);
                var targetRam = ctx.MkSelect(vmSizeRAM, decisionVar);

                var c = ctx.MkGe((ArithExpr)targetCpu, (ArithExpr)sourceCpu);
                var r = ctx.MkGe((ArithExpr)targetRam, (ArithExpr)sourceRam);

                var v = ctx.MkAnd(c, r);
                constraint = constraint == null ? ctx.MkOr(v) : ctx.MkOr(v, constraint);
                
                
                s.Assert(constraint);
            }


            // optimization objectives

            Expr? totalPrice = null;
            Expr? totalAcu = null;


            for (int i = 0; i < selectedSizeArr.Length; i++)
            {
                var decisionVar = selectedSizeArr[i];
                
                var targetPrice = ctx.MkSelect(vmSizePrice, decisionVar);
                var targetACU = ctx.MkSelect(vmSizeACU, decisionVar);


                totalPrice = totalPrice == null? ctx.MkAdd((ArithExpr)targetPrice) : ctx.MkAdd((ArithExpr)targetPrice, (ArithExpr)totalPrice);
                totalAcu = totalAcu == null? ctx.MkAdd((ArithExpr)targetACU) : ctx.MkAdd((ArithExpr)targetACU, (ArithExpr)totalAcu);
            }

            var totalPriceHandle = s.MkMinimize(totalPrice); // minimize total price
            var totalAcuHandle = s.MkMaximize(totalAcu);     // maximize total performance



            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                WriteObject(m);
                WriteObject(totalPriceHandle);
                WriteObject(totalAcuHandle);
            }
        }
    }

}