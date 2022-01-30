namespace z3testps
{
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

    public class VMMappingResultRecord
    {
        public string vmid;
        public string Size;
        public int cpu;
        public int ram;
        public string vCPUs;
        public string MemoryGB;
        public string vCPUsPerCore;
        public string Name;
        public string retailPrice;
        public string ACUs;
        public string HyperVGenerations;
        public string CpuArchitectureType;

    }

    public class VMMappingResult
    {
        private SourceVMRecord[]? _source = null;
        private TargetVMRecord[]? _target= null;
        public VMMappingResultRecord[] VmMappingResult;


        public VMMappingResult(SourceVMRecord[]? source, TargetVMRecord[]? target)
        {
            _source = source;
            _target = target;
            VmMappingResult = new VMMappingResultRecord[_source.Length];

            for (int i = 0; i < _source.Length; i++)
            {
                VmMappingResult[i] = new VMMappingResultRecord() { vmid = _source[i].vmid, cpu = _source[i].cpu, ram = _source[i].ram, 
                                                                   Size = _target[i].Size, Name = _target[i].Name, MemoryGB = _target[i].MemoryGB, 
                                                                   vCPUs = _target[i].vCPUs, vCPUsPerCore = _target[i].vCPUsPerCore,
                                                                   ACUs = _target[i].ACUs, CpuArchitectureType = _target[i].CpuArchitectureType, HyperVGenerations = _target[i].HyperVGenerations, retailPrice = _target[i].retailPrice};
            }

        }
    }
}

