using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.Sat;

namespace z3testps
{
    public abstract class BaseCMDLet: PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject[] SourceVM;

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public PSObject[] TargetVM;

        protected SourceVMRecord[] MakeSourceVMsArray(PSObject[] SourceVM)
        {
            if (SourceVM == null)
            {
                throw new ArgumentNullException(nameof(SourceVM));
            }


            SourceVMRecord[] ret = new SourceVMRecord[SourceVM.Length];
            for (int i = 0; i < SourceVM.Length; i++)
            {
                string? vmid = SourceVM[i].Properties["vmid"]?.Value?.ToString();
                string? cpu = SourceVM[i].Properties["cpu"]?.Value.ToString();
                string? ram = SourceVM[i].Properties["ram"]?.Value.ToString();
                string? datadisk = SourceVM[i].Properties["datadisk"]?.Value.ToString();

                if (null == vmid || null == cpu || null == ram || null == datadisk)
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(),"parameter error", ErrorCategory.InvalidData, SourceVM[i]));
                }

                ret[i] = new SourceVMRecord(vmid: vmid, cpu: int.Parse(cpu), ram: int.Parse(ram),
                    datadisk: int.Parse(datadisk));
            }

            return ret;
        }

        protected TargetVMRecord[] MakeTargetVMsArray(PSObject[] TargetVM)
        {
            if (TargetVM == null)
            {
                throw new ArgumentNullException(nameof(TargetVM));
            }

            TargetVMRecord[] ret = new TargetVMRecord[TargetVM.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                string? name = TargetVM[i].Properties["Name"]?.Value.ToString();
                string? acus = TargetVM[i].Properties["ACUs"]?.Value.ToString();
                string? memoryGb = TargetVM[i].Properties["MemoryGB"]?.Value.ToString();
                string? retailPrice = TargetVM[i].Properties["retailPrice"].Value.ToString();
                string? vcpu = TargetVM[i].Properties["vCPUs"]?.Value.ToString();
                string? size = TargetVM[i].Properties["Size"]?.Value.ToString();

                if (null == name || null == acus || null == memoryGb || null == retailPrice || null == vcpu || null == size)
                {
                    ThrowTerminatingError(new ErrorRecord(new ArgumentException(), "parameter error", ErrorCategory.InvalidData, TargetVM[i]));
                }

                ret[i] = new TargetVMRecord(
                    name: name,
                    vCpUs: vcpu,
                    memoryGb: memoryGb,
                    acUs: acus,
                    size: size,
                    retailPrice: retailPrice,
                    acceleratedNetworkingEnabled: TargetVM[i]?.Properties["AcceleratedNetworkingEnabled"]?.Value.ToString(),
                    capacityReservationSupported: TargetVM[i].Properties["CapacityReservationSupported"]?.Value.ToString(),
                    combinedTempDiskAndCachedIops: TargetVM[i].Properties["CombinedTempDiskAndCachedIOPS"]?.Value.ToString(),
                    combinedTempDiskAndCachedReadBytesPerSecond: TargetVM[i].Properties["CombinedTempDiskAndCachedReadBytesPerSecond"]?.Value.ToString(),
                    combinedTempDiskAndCachedWriteBytesPerSecond: TargetVM[i].Properties["CombinedTempDiskAndCachedWriteBytesPerSecond"]?.Value.ToString(),
                    cpuArchitectureType: TargetVM[i].Properties["CpuArchitectureType"]?.Value.ToString(),
                    cpuToRamRatio: TargetVM[i].Properties["cpuToRamRatio"]?.Value.ToString(),
                    encryptionAtHostSupported: TargetVM[i].Properties["EncryptionAtHostSupported"]?.Value.ToString(),
                    ephemeralOsDiskSupported: TargetVM[i].Properties["EphemeralOSDiskSupported"]?.Value.ToString(),
                    hyperVGenerations: TargetVM[i].Properties["HyperVGenerations"]?.Value.ToString(),
                    lowPriorityCapable: TargetVM[i].Properties["LowPriorityCapable"]?.Value.ToString(),
                    maxDataDiskCount: TargetVM[i].Properties["MaxDataDiskCount"]?.Value.ToString(),
                    maxNetworkInterfaces: TargetVM[i].Properties["MaxNetworkInterfaces"]?.Value.ToString(),
                    maxResourceVolumeMb: TargetVM[i].Properties["MaxResourceVolumeMB"]?.Value.ToString(),
                    memoryGbFlattened: TargetVM[i].Properties["MemoryGBFlattened"]?.Value.ToString(),
                    memoryPreservingMaintenanceSupported: TargetVM[i].Properties["MemoryPreservingMaintenanceSupported"]?.Value.ToString(),
                    osVhdSizeMb: TargetVM[i].Properties["OSVhdSizeMB"]?.Value.ToString(),
                    premiumIo: TargetVM[i].Properties["PremiumIO"]?.Value.ToString(),
                    rdmaEnabled: TargetVM[i].Properties["RdmaEnabled"]?.Value.ToString(),
                    retailPriceFlattened: TargetVM[i].Properties["retailPriceFlattened"]?.Value.ToString(),
                    tier: TargetVM[i].Properties["Tier"]?.Value.ToString(),
                    vCpUsAvailable: TargetVM[i].Properties["vCPUsAvailable"]?.Value.ToString(),
                    vCpUsPerCore: TargetVM[i].Properties["vCPUsPerCore"]?.Value.ToString(),
                    vmDeploymentTypes: TargetVM[i].Properties["VMDeploymentTypes"]?.Value.ToString());
            }

            return ret;
        }
    }
}
