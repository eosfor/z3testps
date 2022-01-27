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
            //var SourceVMIndex = MakeSourceVmDictionary();
            //var TargetVMIndex = MakeTargetVmDictionary();

            string[] sourceVmNames = MakeSourceVMNamesArray();
            SourceVMRecord[] sourceVMs = MakeSourceVMsArray();
            
            string[] targetVmNames = MakeTargetVMNamesArray();
            TargetVMRecord[] targetVMs = MakeTargetVMsArray();

            var ctx = new Context();
            var zero = ctx.MkNumeral(0, ctx.MkIntSort());
            var one = ctx.MkNumeral(1, ctx.MkIntSort());

            var existingVMs = ctx.MkEnumSort("existingVMs", sourceVmNames);
            var vmSizes = ctx.MkEnumSort("vmSizes", targetVmNames);


            #region Populate Data Arrays from input data
            
            ArraySort existingVMSort = ctx.MkArraySort(ctx.IntSort, ctx.IntSort);
            ArraySort vmSizeSort = ctx.MkArraySort(ctx.IntSort, ctx.IntSort);

            #region source-data
            ArrayExpr vmCPU = (ArrayExpr) ctx.MkConst(ctx.MkSymbol("vmCPU"), existingVMSort);
            for (int i = 0; i < sourceVMs.Length; i++)
            {
                var x = existingVMs.Consts[i];
                ctx.MkStore(vmCPU, ctx.MkInt(i), ctx.MkInt(sourceVMs[i].cpu));
            }

            ArrayExpr vmRAM = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmRAM"), existingVMSort);

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                var x = existingVMs.Consts[i];
                ctx.MkStore(vmRAM, ctx.MkInt(i), ctx.MkInt(sourceVMs[i].ram));
            }
            #endregion source-data

            #region target-data
            ArrayExpr vmSizeCPU = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeCPU"), vmSizeSort);

            for (int i = 0; i < targetVMs.Length; i++)
            {
                var x = vmSizes.Consts[i];
                ctx.MkStore(vmSizeCPU, ctx.MkInt(i), ctx.MkInt(targetVMs[i].vCPUs));
            }

            ArrayExpr vmSizeRAM = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeRAM"), vmSizeSort);

            for (int i = 0; i < targetVMs.Length; i++)
            {
                var x = vmSizes.Consts[i];
                ctx.MkStore(vmSizeRAM, ctx.MkInt(i), ctx.MkInt((int)double.Parse(targetVMs[i].MemoryGB)));
            }

            ArrayExpr vmSizePrice = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizePrice"), vmSizeSort);

            for (int i = 0; i < targetVMs.Length; i++)
            {
                var x = vmSizes.Consts[i];
                ctx.MkStore(vmSizePrice, ctx.MkInt(i), ctx.MkInt((int)double.Parse(targetVMs[i].retailPriceFlattened)));
            }

            ArrayExpr vmSizeACU = (ArrayExpr)ctx.MkConst(ctx.MkSymbol("vmSizeACU"), vmSizeSort);
            

            for (int i = 0; i < targetVMs.Length; i++)
            {
                var x = vmSizes.Consts[i];
                ctx.MkStore(vmSizeACU, ctx.MkInt(i), ctx.MkInt((int)double.Parse(targetVMs[i].ACUs)));
            }
            #endregion target-data
            
            #endregion

            // decision variables
            Expr[] selectedSizeArr = new Expr[sourceVMs.Length];
            string[] selectedSizeNames = new string[sourceVMs.Length];

            for (int i = 0; i < sourceVMs.Length; i++)
            {
                string n = sourceVMs[i].vmid;
                selectedSizeNames[i] = n;
                selectedSizeArr[i] = ctx.MkConst(n, ctx.IntSort);
            }

            // optimization objectives
            var totalPrice = ctx.MkConst("totalPrice", ctx.IntSort);
            var totalACU = ctx.MkConst("totalPrice", ctx.IntSort);

            var s = ctx.MkOptimize(); //ctx.MkSolver();


            for (int i = 0; i < selectedSizeArr.Length; i++) //for each variable
            {
                var constraint = ctx.MkFalse();
                var decisionVar = selectedSizeArr[i];

                var sourceCpu = ctx.MkSelect(vmCPU, ctx.MkInt(i));
                var sourceRam = ctx.MkSelect(vmRAM, ctx.MkInt(i));

                for (int j = 0; j < targetVMs.Length; j++)
                {
                    var targetCpu = ctx.MkSelect(vmSizeCPU, decisionVar);
                    var targetRam = ctx.MkSelect(vmSizeRAM, decisionVar);


                    var c = ctx.MkGt((ArithExpr)targetCpu, (ArithExpr)sourceCpu);
                    var r = ctx.MkGt((ArithExpr)targetRam, (ArithExpr)sourceRam);

                    var v = ctx.MkAnd(c, r);
                    constraint = ctx.MkOr( v, constraint);
                }
                s.Assert(constraint);
            }

            for (int i = 0; i < selectedSizeArr.Length; i++)
            {
                var decisionVar = selectedSizeArr[i];
                
                var targetPrice = ctx.MkSelect(vmSizePrice, decisionVar);
                var targetACU = ctx.MkSelect(vmSizeACU, decisionVar);

                totalPrice = ctx.MkAdd((ArithExpr)targetPrice, (ArithExpr)totalPrice);
                totalACU = ctx.MkAdd((ArithExpr)targetACU, (ArithExpr)totalACU);
            }

            var totalPriceHandle = s.MkMinimize(totalPrice); // minimize total price
            var totalAcuHandle = s.MkMaximize(totalACU);     // 



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