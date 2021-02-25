using System.Device.Gpio;

namespace gRPCpi
{
    internal class ConsolePin
    {
        public int PinNumber { get; set; }
        public PinMode? Mode { get; set; }
        public PinValue Value { get; set; }        
    }
}
