using System.CodeDom;
using System.Management.Automation;
using Microsoft.Z3;
using System.Linq;

namespace z3testps
{
    [Cmdlet(VerbsLifecycle.Start, "Z3ModelCalculation")]
    public class StartZ3ModelCalculation: PSCmdlet
    {
        [Parameter(Position=0, Mandatory = true)]
        public PSObject[] SourceVM;
        
        [Parameter(Position = 1, Mandatory = true)]
        public PSObject[] TargetVM;

        protected override void ProcessRecord()
        {
            SourceVMRecord[] sourceVMs = MakeSourceVMsArray(); // length = 87
            TargetVMRecord[] targetVMs = MakeTargetVMsArray(); // length = 240

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
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizeRAM, ctx.MkInt(i)), ctx.MkInt((int)double.Parse(targetVMs[i].MemoryGB))));
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizePrice, ctx.MkInt(i)), ctx.MkInt((int)double.Parse(targetVMs[i].retailPriceFlattened))));
                s.Assert(ctx.MkEq(ctx.MkSelect(vmSizeACU, ctx.MkInt(i)), ctx.MkInt((int)double.Parse(targetVMs[i].ACUs))));
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

        private SourceVMRecord[] MakeSourceVMsArray()
        {
            SourceVMRecord[] ret = new SourceVMRecord[SourceVM.Length];
            for (int i = 0; i < SourceVM.Length; i++)
            {
                ret[i] = new SourceVMRecord()
                {
                    vmid = SourceVM[i].Properties["vmid"].Value.ToString(),
                    cpu = int.Parse(SourceVM[i].Properties["cpu"].Value.ToString()),
                    ram = int.Parse(SourceVM[i].Properties["ram"].Value.ToString()),
                    datadisk = int.Parse(SourceVM[i].Properties["datadisk"].Value.ToString())
                };
            }

            return ret;
        }

        private TargetVMRecord[] MakeTargetVMsArray()
        {
            TargetVMRecord[] ret = new TargetVMRecord[TargetVM.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new TargetVMRecord()
                {
                    AcceleratedNetworkingEnabled = TargetVM[i].Properties["AcceleratedNetworkingEnabled"].Value.ToString(),
                    ACUs = TargetVM[i].Properties["ACUs"].Value.ToString(),
                    CapacityReservationSupported = TargetVM[i].Properties["CapacityReservationSupported"].Value.ToString(),
                    CombinedTempDiskAndCachedIOPS = TargetVM[i].Properties["CombinedTempDiskAndCachedIOPS"].Value.ToString(),
                    CombinedTempDiskAndCachedReadBytesPerSecond =
                        TargetVM[i].Properties["CombinedTempDiskAndCachedReadBytesPerSecond"].Value.ToString(),
                    CombinedTempDiskAndCachedWriteBytesPerSecond =
                        TargetVM[i].Properties["CombinedTempDiskAndCachedWriteBytesPerSecond"].Value.ToString(),
                    CpuArchitectureType = TargetVM[i].Properties["CpuArchitectureType"].Value.ToString(),
                    cpuToRamRatio = TargetVM[i].Properties["cpuToRamRatio"].Value.ToString(),
                    EncryptionAtHostSupported = TargetVM[i].Properties["EncryptionAtHostSupported"].Value.ToString(),
                    EphemeralOSDiskSupported = TargetVM[i].Properties["EphemeralOSDiskSupported"].Value.ToString(),
                    HyperVGenerations = TargetVM[i].Properties["HyperVGenerations"].Value.ToString(),
                    LowPriorityCapable = TargetVM[i].Properties["LowPriorityCapable"].Value.ToString(),
                    MaxDataDiskCount = TargetVM[i].Properties["MaxDataDiskCount"].Value.ToString(),
                    MaxNetworkInterfaces = TargetVM[i].Properties["MaxNetworkInterfaces"].Value.ToString(),
                    MaxResourceVolumeMB = TargetVM[i].Properties["MaxResourceVolumeMB"].Value.ToString(),
                    MemoryGB = TargetVM[i].Properties["MemoryGB"].Value.ToString(),
                    MemoryGBFlattened = TargetVM[i].Properties["MemoryGBFlattened"].Value.ToString(),
                    MemoryPreservingMaintenanceSupported =
                        TargetVM[i].Properties["MemoryPreservingMaintenanceSupported"].Value.ToString(),
                    Name = TargetVM[i].Properties["Name"].Value.ToString(),
                    OSVhdSizeMB = TargetVM[i].Properties["OSVhdSizeMB"].Value.ToString(),
                    PremiumIO = TargetVM[i].Properties["PremiumIO"].Value.ToString(),
                    RdmaEnabled = TargetVM[i].Properties["RdmaEnabled"].Value.ToString(),
                    retailPrice = TargetVM[i].Properties["retailPrice"].Value.ToString(),
                    retailPriceFlattened = TargetVM[i].Properties["retailPriceFlattened"].Value.ToString(),
                    Size = TargetVM[i].Properties["Size"].Value.ToString(),
                    Tier = TargetVM[i].Properties["Tier"].Value.ToString(),
                    vCPUs = TargetVM[i].Properties["vCPUs"].Value.ToString(),
                    vCPUsAvailable = TargetVM[i].Properties["vCPUsAvailable"].Value.ToString(),
                    vCPUsPerCore = TargetVM[i].Properties["vCPUsPerCore"].Value.ToString(),
                    VMDeploymentTypes = TargetVM[i].Properties["VMDeploymentTypes"].Value.ToString()
                };
            }

            return ret;
        }

        private string[] MakeTargetVMNamesArray()
        {
            string[] ret = new string[TargetVM.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = TargetVM[i].Properties["Name"].Value.ToString();
            }

            return ret;

        }

        private string[] MakeSourceVMNamesArray()
        {
            string[] ret = new string[SourceVM.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = SourceVM[i].Properties["vmid"].Value.ToString();
            }

            return ret;
        }
    }

    public class SourceVMRecord
    {
        public string vmid;
        public int cpu;
        public int ram;
        public int datadisk;
    }

    public class TargetVMRecord
    {
        public string AcceleratedNetworkingEnabled;
        public string ACUs;
        public string CapacityReservationSupported;
        public string CombinedTempDiskAndCachedIOPS;
        public string CombinedTempDiskAndCachedReadBytesPerSecond;
        public string CombinedTempDiskAndCachedWriteBytesPerSecond;
        public string CpuArchitectureType;
        public string cpuToRamRatio;
        public string EncryptionAtHostSupported;
        public string EphemeralOSDiskSupported;
        public string HyperVGenerations;
        public string LowPriorityCapable;
        public string MaxDataDiskCount;
        public string MaxNetworkInterfaces;
        public string MaxResourceVolumeMB;
        public string MemoryGB;
        public string MemoryGBFlattened;
        public string MemoryPreservingMaintenanceSupported;
        public string Name;
        public string OSVhdSizeMB;
        public string PremiumIO;
        public string RdmaEnabled;
        public string retailPrice;
        public string retailPriceFlattened;
        public string Size;
        public string Tier;
        public string vCPUs;
        public string vCPUsAvailable;
        public string vCPUsPerCore;
        public string VMDeploymentTypes;
    }
}