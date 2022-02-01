namespace z3testps
{
    public class SourceVMRecord
    {
        public string vmid;
        public int cpu;
        public int ram;
        public int datadisk;

        public SourceVMRecord(string vmid, int cpu, int ram, int datadisk)
        {
            this.vmid = vmid;
            this.cpu = cpu;
            this.ram = ram;
            this.datadisk = datadisk;
        }
    }

    public class TargetVMRecord
    {
        public string? AcceleratedNetworkingEnabled;
        public string ACUs;
        public string? CapacityReservationSupported;
        public string? CombinedTempDiskAndCachedIOPS;
        public string? CombinedTempDiskAndCachedReadBytesPerSecond;
        public string? CombinedTempDiskAndCachedWriteBytesPerSecond;
        public string? CpuArchitectureType;
        public string? cpuToRamRatio;
        public string? EncryptionAtHostSupported;
        public string? EphemeralOSDiskSupported;
        public string? HyperVGenerations;
        public string? LowPriorityCapable;
        public string? MaxDataDiskCount;
        public string? MaxNetworkInterfaces;
        public string? MaxResourceVolumeMB;
        public string MemoryGB;
        public string? MemoryGBFlattened;
        public string? MemoryPreservingMaintenanceSupported;
        public string Name;
        public string? OSVhdSizeMB;
        public string? PremiumIO;
        public string? RdmaEnabled;
        public string retailPrice;
        public string? retailPriceFlattened;
        public string Size;
        public string? Tier;
        public string vCPUs;
        public string? vCPUsAvailable;
        public string? vCPUsPerCore;
        public string? VMDeploymentTypes;

        public TargetVMRecord(string? acceleratedNetworkingEnabled, string acUs, string? capacityReservationSupported, string? combinedTempDiskAndCachedIops, string? combinedTempDiskAndCachedReadBytesPerSecond,
            string? combinedTempDiskAndCachedWriteBytesPerSecond, string? cpuArchitectureType, string? cpuToRamRatio, string? encryptionAtHostSupported, 
            string? ephemeralOsDiskSupported, string? hyperVGenerations, string? lowPriorityCapable, string? maxDataDiskCount, string? maxNetworkInterfaces, string? maxResourceVolumeMb, 
            string memoryGb, string? memoryGbFlattened, string? memoryPreservingMaintenanceSupported, string name, string? osVhdSizeMb, string? premiumIo, string? rdmaEnabled, 
            string retailPrice, string? retailPriceFlattened, string size, string? tier, string vCpUs, string? vCpUsAvailable, string? vCpUsPerCore, string? vmDeploymentTypes)
        {
            Name = name;
            Size = size;
            vCPUs = vCpUs;
            MemoryGB = memoryGb;
            ACUs = acUs;
            Tier = tier;
            this.retailPrice = retailPrice;
            AcceleratedNetworkingEnabled = acceleratedNetworkingEnabled;
            CapacityReservationSupported = capacityReservationSupported;
            CombinedTempDiskAndCachedIOPS = combinedTempDiskAndCachedIops;
            CombinedTempDiskAndCachedReadBytesPerSecond = combinedTempDiskAndCachedReadBytesPerSecond;
            CombinedTempDiskAndCachedWriteBytesPerSecond = combinedTempDiskAndCachedWriteBytesPerSecond;
            CpuArchitectureType = cpuArchitectureType;
            this.cpuToRamRatio = cpuToRamRatio;
            EncryptionAtHostSupported = encryptionAtHostSupported;
            EphemeralOSDiskSupported = ephemeralOsDiskSupported;
            HyperVGenerations = hyperVGenerations;
            LowPriorityCapable = lowPriorityCapable;
            MaxDataDiskCount = maxDataDiskCount;
            MaxNetworkInterfaces = maxNetworkInterfaces;
            MaxResourceVolumeMB = maxResourceVolumeMb;
            MemoryGBFlattened = memoryGbFlattened;
            MemoryPreservingMaintenanceSupported = memoryPreservingMaintenanceSupported;
            OSVhdSizeMB = osVhdSizeMb;
            PremiumIO = premiumIo;
            RdmaEnabled = rdmaEnabled;
            this.retailPriceFlattened = retailPriceFlattened;
            vCPUsAvailable = vCpUsAvailable;
            vCPUsPerCore = vCpUsPerCore;
            VMDeploymentTypes = vmDeploymentTypes;
        }
    }

    public class VMMappingResultRecord
    {
        public string vmid;
        public string Size;
        public int cpu;
        public int ram;
        public string vCPUs;
        public string MemoryGB;
        public string? vCPUsPerCore;
        public string Name;
        public string retailPrice;
        public string ACUs;
        public string? HyperVGenerations;
        public string? CpuArchitectureType;

        public VMMappingResultRecord(string vmid, int cpu, int ram, string size, string name, string memoryGb, string vCpUs, string? vCpUsPerCore, string acUs, string? cpuArchitectureType, string? hyperVGenerations, string retailPrice)
        {
            this.vmid = vmid;
            this.cpu = cpu;
            this.ram = ram;
            Size = size;
            Name = name;
            MemoryGB = memoryGb;
            vCPUs = vCpUs;
            vCPUsPerCore = vCpUsPerCore;
            ACUs = acUs;
            CpuArchitectureType = cpuArchitectureType;
            HyperVGenerations = hyperVGenerations;
            this.retailPrice = retailPrice;
        }
    }

    public class VMMappingResult
    {
        public VMMappingResultRecord[] VmMappingResult;


        public VMMappingResult(SourceVMRecord[] source, TargetVMRecord[] target)
        {
            VmMappingResult = new VMMappingResultRecord[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                VmMappingResult[i] = new VMMappingResultRecord(vmid: source[i].vmid, cpu: source[i].cpu,
                    ram: source[i].ram, size: target[i].Size, name: target[i].Name, memoryGb: target[i].MemoryGB,
                    vCpUs: target[i].vCPUs, vCpUsPerCore: target[i].vCPUsPerCore, acUs: target[i].ACUs,
                    cpuArchitectureType: target[i].CpuArchitectureType, hyperVGenerations: target[i].HyperVGenerations,
                    retailPrice: target[i].retailPrice);
            }

        }
    }
}

