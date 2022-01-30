using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Google.OrTools.Sat;

namespace z3testps
{
    public class BaseCMDLet: PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public PSObject[] SourceVM;

        [Parameter(Position = 1, Mandatory = true)]
        public PSObject[] TargetVM;

        //private string[] MakeTargetVMNamesArray()
        //{
        //    string[] ret = new string[TargetVM.Length];
        //    for (int i = 0; i < ret.Length; i++)
        //    {
        //        ret[i] = TargetVM[i].Properties["Name"].Value.ToString();
        //    }

        //    return ret;

        //}

        //private string[] MakeSourceVMNamesArray()
        //{
        //    string[] ret = new string[SourceVM.Length];
        //    for (int i = 0; i < ret.Length; i++)
        //    {
        //        ret[i] = SourceVM[i].Properties["vmid"].Value.ToString();
        //    }

        //    return ret;
        //}


        protected SourceVMRecord[] MakeSourceVMsArray(PSObject[] SourceVM)
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

        protected TargetVMRecord[] MakeTargetVMsArray(PSObject[] TargetVM)
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
    }
}
