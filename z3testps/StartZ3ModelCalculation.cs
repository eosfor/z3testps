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

            var existingVMs = ctx.MkEnumSort("existingVMs", SourceVM.Select(x => x.Properties["vmid"].Value.ToString()).ToArray() );
            var vmSizes = ctx.MkEnumSort("vmSizes", TargetVM.Select(x => x.Properties["Name"].Value.ToString()).ToArray());

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

            base.ProcessRecord();
        }

        private Dictionary<string, TargetVMRecord> MakeTargetVmDictionary()
        {
            Dictionary<string, TargetVMRecord> TargetVMIndex = new Dictionary<string, TargetVMRecord>();
            foreach (var targetVM in TargetVM)
            {
                TargetVMIndex[targetVM.Properties["Name"].ToString()] = new TargetVMRecord()
                {
                    AcceleratedNetworkingEnabled = targetVM.Properties["AcceleratedNetworkingEnabled"].ToString(),
                    ACUs = targetVM.Properties["ACUs"].ToString(),
                    CapacityReservationSupported = targetVM.Properties["CapacityReservationSupported"].ToString(),
                    CombinedTempDiskAndCachedIOPS = targetVM.Properties["CombinedTempDiskAndCachedIOPS"].ToString(),
                    CombinedTempDiskAndCachedReadBytesPerSecond =
                        targetVM.Properties["CombinedTempDiskAndCachedReadBytesPerSecond"].ToString(),
                    CombinedTempDiskAndCachedWriteBytesPerSecond =
                        targetVM.Properties["CombinedTempDiskAndCachedWriteBytesPerSecond"].ToString(),
                    CpuArchitectureType = targetVM.Properties["CpuArchitectureType"].ToString(),
                    cpuToRamRatio = targetVM.Properties["cpuToRamRatio"].ToString(),
                    EncryptionAtHostSupported = targetVM.Properties["EncryptionAtHostSupported"].ToString(),
                    EphemeralOSDiskSupported = targetVM.Properties["EphemeralOSDiskSupported"].ToString(),
                    HyperVGenerations = targetVM.Properties["HyperVGenerations"].ToString(),
                    LowPriorityCapable = targetVM.Properties["LowPriorityCapable"].ToString(),
                    MaxDataDiskCount = targetVM.Properties["MaxDataDiskCount"].ToString(),
                    MaxNetworkInterfaces = targetVM.Properties["MaxNetworkInterfaces"].ToString(),
                    MaxResourceVolumeMB = targetVM.Properties["MaxResourceVolumeMB"].ToString(),
                    MemoryGB = targetVM.Properties["MemoryGB"].ToString(),
                    MemoryGBFlattened = targetVM.Properties["MemoryGBFlattened"].ToString(),
                    MemoryPreservingMaintenanceSupported =
                        targetVM.Properties["MemoryPreservingMaintenanceSupported"].ToString(),
                    Name = targetVM.Properties["Name"].ToString(),
                    OSVhdSizeMB = targetVM.Properties["OSVhdSizeMB"].ToString(),
                    PremiumIO = targetVM.Properties["PremiumIO"].ToString(),
                    RdmaEnabled = targetVM.Properties["RdmaEnabled"].ToString(),
                    retailPrice = targetVM.Properties["retailPrice"].ToString(),
                    retailPriceFlattened = targetVM.Properties["retailPriceFlattened"].ToString(),
                    Size = targetVM.Properties["Size"].ToString(),
                    Tier = targetVM.Properties["Tier"].ToString(),
                    vCPUs = targetVM.Properties["vCPUs"].ToString(),
                    vCPUsAvailable = targetVM.Properties["vCPUsAvailable"].ToString(),
                    vCPUsPerCore = targetVM.Properties["vCPUsPerCore"].ToString(),
                    VMDeploymentTypes = targetVM.Properties["VMDeploymentTypes"].ToString()
                };
            }

            return TargetVMIndex;
        }

        private Dictionary<string, SourceVMRecord> MakeSourceVmDictionary()
        {
            Dictionary<string, SourceVMRecord> SourceVMIndex = new Dictionary<string, SourceVMRecord>();
            foreach (PSObject sourceVM in SourceVM)
            {
                SourceVMIndex[sourceVM.Properties["vmid"].ToString()] = new SourceVMRecord()
                {
                    vmid = sourceVM.Properties["vmid"].ToString(),
                    cpu = int.Parse(sourceVM.Properties["cpu"].ToString()),
                    ram = int.Parse(sourceVM.Properties["ram"].ToString()),
                    datadisk = int.Parse(sourceVM.Properties["datadisk"].ToString())
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