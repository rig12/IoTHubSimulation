using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatedDevice
{
    public static class MakersData
    {
        public static IDictionary<string, string> devicekeys = new Dictionary<string, string>()
        {
            {"Maker1","mYZX4/bKTxKzk3pOgx42S4Ly9qSENgkaK7EwD/IfX70=" },
            {"Maker2","O4/VyeYcNuUvc+pmdp+lYlr0xlqw17VfPrQNx+B3/Wc=" },
            {"Maker3","rDi3DVcvrm1rOgJTY+z4gKLbUKd9XwNISN4u7L4iWkw=" },
            {"Maker4","nb0Ye3fGF6uqvUVlPcIdVix84dak/ix4AkpuRTyMSHk=" }
        };

        
        public readonly static string s_iotHubUri = "TobaccoIoTHub.azure-devices.net";

        public readonly static string[] reasons = { "KARTONER","KDF","MAKER","PACKER","Run","SOLARIS","Planned"};
    }
}
