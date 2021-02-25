using System;
using System.Linq;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;

namespace gRPCpi
{
    public class ConsoleGPIODriver : GpioDriver
    {
        readonly List<ConsolePin> pins = new List<ConsolePin>();
        public bool ConsoleOut { get; set; }

        protected override int PinCount => 32;

        public ConsoleGPIODriver(bool consoleOut = false) { ConsoleOut = consoleOut; }
        private ConsolePin FindPin(int pinNumber)
        {
            if (!pins.Any(p => p.PinNumber == pinNumber))
                throw new InvalidOperationException($"so sorry, can't do that - pin number {pinNumber} is not open.");

            var match = pins.First(p => p.PinNumber == pinNumber);
            return match;
        }

        void WriteLine(string message)
        {
            if (ConsoleOut)
                Console.WriteLine(message);
        }

        void Write(string message)
        {
            if (ConsoleOut)
                Console.Write(message);
        }

        protected override void ClosePin(int pinNumber)
        {
            var match = FindPin(pinNumber);

            pins.Remove(match);
            WriteLine($"{nameof(ClosePin)}: {pinNumber}");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WriteLine(nameof(Dispose));
        }

        protected override PinValue Read(int pinNumber)
        {
            var val = FindPin(pinNumber).Value;
            WriteLine($"{nameof(Read)}: {pinNumber}, value={val}");
            return val;

        }

        public void Read(Span<PinValuePair> pinValuePairs)
        {
            WriteLine($"{nameof(Read)}(pinValuePairs):");
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                var pair = pinValuePairs[i];                
                pinValuePairs[i] = new PinValuePair(pair.PinNumber, Read(pair.PinNumber));
            }
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            var match = FindPin(pinNumber);

            match.Mode = mode;
            WriteLine($"{nameof(SetPinMode)}: {pinNumber}, mode={mode}");
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            var match = FindPin(pinNumber);
            match.Value = value;
            WriteLine($"{nameof(Write)}: {pinNumber}, value={value}");
        }

        public void Write(ReadOnlySpan<PinValuePair> pinValuePairs)
        {
            WriteLine($"{nameof(Write)}(pinValuePairs):");
            for (int i = 0; i < pinValuePairs.Length; i++)
            {
                var pair = pinValuePairs[i];
                Write(pair.PinNumber, pair.PinValue);
            }
        }

        readonly Dictionary<int, List<PinChangeEventHandler>> PinNone = new Dictionary<int, List<PinChangeEventHandler>>();
        readonly Dictionary<int, List<PinChangeEventHandler>> PinRising = new Dictionary<int, List<PinChangeEventHandler>>();
        readonly Dictionary<int, List<PinChangeEventHandler>> PinFalling = new Dictionary<int, List<PinChangeEventHandler>>();

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            if ((eventTypes | PinEventTypes.None) == PinEventTypes.None)
            {
                if (!PinNone.ContainsKey(pinNumber))
                    PinNone[pinNumber] = new List<PinChangeEventHandler>();

                PinNone[pinNumber].Add(callback);
            }

            if ((eventTypes | PinEventTypes.Rising) == PinEventTypes.Rising)
            {
                if (!PinRising.ContainsKey(pinNumber))
                    PinRising[pinNumber] = new List<PinChangeEventHandler>();

                PinRising[pinNumber].Add(callback);
            }

            if ((eventTypes | PinEventTypes.Falling) == PinEventTypes.Falling)
            {
                if (!PinFalling.ContainsKey(pinNumber))
                    PinFalling[pinNumber] = new List<PinChangeEventHandler>();

                PinFalling[pinNumber].Add(callback);
            }
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            return pins.FirstOrDefault(p => p.PinNumber == pinNumber).Mode ?? throw new Exception($"Pin modenot set for pin {pinNumber}");
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            return true;
        }

        protected override void OpenPin(int pinNumber)
        {
            if (!pins.Any(p => p.PinNumber == pinNumber))
            {
                pins.Add(new ConsolePin 
                { 
                    PinNumber = pinNumber,
                    Value = PinValue.Low
                });
                WriteLine($"{nameof(OpenPin)}: {pinNumber}");
            }
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            if (PinNone.ContainsKey(pinNumber))
            {
                PinNone[pinNumber].Remove(callback);
            }

            if (PinRising.ContainsKey(pinNumber))
            {
                PinRising[pinNumber].Remove(callback);
            }

            if (PinFalling.ContainsKey(pinNumber))
            {
                PinFalling[pinNumber].Remove(callback);
            }
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
