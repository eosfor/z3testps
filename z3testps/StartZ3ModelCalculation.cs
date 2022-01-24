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
            var SourceVMIndex = MakeSourceVmDictionary();
            var TargetVMIndex = MakeTargetVmDictionary();

            var ctx = new Context();
            var zero = ctx.MkNumeral(0, ctx.MkIntSort());
            var one = ctx.MkNumeral(1, ctx.MkIntSort());

            var existingVMs = ctx.MkEnumSort("existingVMs", SourceVMIndex.Keys.ToArray() );
            var vmSizes = ctx.MkEnumSort("vmSizes", TargetVMIndex.Keys.ToArray());

            var selectedSize = new Dictionary<string, Expr>();


            for (int i = 0; i < SourceVM.Length; i++)
            {
                var name = SourceVM[i].Properties["vmid"].ToString();
                selectedSize.Add(name, ctx.MkConst(name, vmSizes));
            }

            var s = ctx.MkSolver();

            //constraint forall(vm in existingVMs)(
            //    vmSizeRAM[selectedSize[vm]] >= vmRAM[vm]
            //);
            foreach (var el in existingVMs.Consts)
            {
                var sourceObject = SourceVMIndex[el.ToString()];
                var targetObject = selectedSize[el.ToString()];
                s.Add(ctx.MkGt(TargetVMIndex[targetObject].vCPUs, sourceObject.cpu));
            }

            //constraint forall(vm in existingVMs)(
            //    vmSizeCPU[selectedSize[vm]] >= vmCPU[vm] * 0.8
            //);
            foreach (var el in existingVMs.Consts)
            {
                var sourceObject = SourceVMIndex[el.ToString()];
                var targetObject = selectedSize[el.ToString()];
                s.Add(ctx.MkGt(TargetVMIndex[targetObject].MemoryGB, sourceObject.ram));
            }

            if (s.Check() == Status.SATISFIABLE)
            {
                var m = s.Model;
                WriteObject(m);
            }
        }

        private Dictionary<string, TargetVMRecord> MakeTargetVmDictionary()
        {
            Dictionary<string, TargetVMRecord> TargetVMIndex = new Dictionary<string, TargetVMRecord>();
            foreach (var targetVM in TargetVM)
            {
                TargetVMIndex[targetVM.Properties["Name"].Value.ToString()] = new TargetVMRecord()
                {
                    AcceleratedNetworkingEnabled = targetVM.Properties["AcceleratedNetworkingEnabled"].Value.ToString(),
                    ACUs = targetVM.Properties["ACUs"].Value.ToString(),
                    CapacityReservationSupported = targetVM.Properties["CapacityReservationSupported"].Value.ToString(),
                    CombinedTempDiskAndCachedIOPS = targetVM.Properties["CombinedTempDiskAndCachedIOPS"].Value.ToString(),
                    CombinedTempDiskAndCachedReadBytesPerSecond =
                        targetVM.Properties["CombinedTempDiskAndCachedReadBytesPerSecond"].Value.ToString(),
                    CombinedTempDiskAndCachedWriteBytesPerSecond =
                        targetVM.Properties["CombinedTempDiskAndCachedWriteBytesPerSecond"].Value.ToString(),
                    CpuArchitectureType = targetVM.Properties["CpuArchitectureType"].Value.ToString(),
                    cpuToRamRatio = targetVM.Properties["cpuToRamRatio"].Value.ToString(),
                    EncryptionAtHostSupported = targetVM.Properties["EncryptionAtHostSupported"].Value.ToString(),
                    EphemeralOSDiskSupported = targetVM.Properties["EphemeralOSDiskSupported"].Value.ToString(),
                    HyperVGenerations = targetVM.Properties["HyperVGenerations"].Value.ToString(),
                    LowPriorityCapable = targetVM.Properties["LowPriorityCapable"].Value.ToString(),
                    MaxDataDiskCount = targetVM.Properties["MaxDataDiskCount"].Value.ToString(),
                    MaxNetworkInterfaces = targetVM.Properties["MaxNetworkInterfaces"].Value.ToString(),
                    MaxResourceVolumeMB = targetVM.Properties["MaxResourceVolumeMB"].Value.ToString(),
                    MemoryGB = targetVM.Properties["MemoryGB"].Value.ToString(),
                    MemoryGBFlattened = targetVM.Properties["MemoryGBFlattened"].Value.ToString(),
                    MemoryPreservingMaintenanceSupported =
                        targetVM.Properties["MemoryPreservingMaintenanceSupported"].Value.ToString(),
                    Name = targetVM.Properties["Name"].Value.ToString(),
                    OSVhdSizeMB = targetVM.Properties["OSVhdSizeMB"].Value.ToString(),
                    PremiumIO = targetVM.Properties["PremiumIO"].Value.ToString(),
                    RdmaEnabled = targetVM.Properties["RdmaEnabled"].Value.ToString(),
                    retailPrice = targetVM.Properties["retailPrice"].Value.ToString(),
                    retailPriceFlattened = targetVM.Properties["retailPriceFlattened"].Value.ToString(),
                    Size = targetVM.Properties["Size"].Value.ToString(),
                    Tier = targetVM.Properties["Tier"].Value.ToString(),
                    vCPUs = targetVM.Properties["vCPUs"].Value.ToString(),
                    vCPUsAvailable = targetVM.Properties["vCPUsAvailable"].Value.ToString(),
                    vCPUsPerCore = targetVM.Properties["vCPUsPerCore"].Value.ToString(),
                    VMDeploymentTypes = targetVM.Properties["VMDeploymentTypes"].Value.ToString()
                };
            }

            return TargetVMIndex;
        }

        private Dictionary<string, SourceVMRecord> MakeSourceVmDictionary()
        {
            Dictionary<string, SourceVMRecord> SourceVMIndex = new Dictionary<string, SourceVMRecord>();
            foreach (PSObject sourceVM in SourceVM)
            {
                SourceVMIndex[sourceVM.Properties["vmid"].Value.ToString()] = new SourceVMRecord()
                {
                    vmid = sourceVM.Properties["vmid"].Value.ToString(),
                    cpu = int.Parse(sourceVM.Properties["cpu"].Value.ToString()),
                    ram = int.Parse(sourceVM.Properties["ram"].Value.ToString()),
                    datadisk = int.Parse(sourceVM.Properties["datadisk"].Value.ToString())
                };
            }

            return SourceVMIndex;
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