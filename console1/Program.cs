﻿using System;
using Iot.Device.CpuTemperature;
using System.Device.Gpio;
using System.Threading;

namespace dotnet.core.iot
{
    class Program
    {
        static CpuTemperature temperature = new CpuTemperature();
        static GpioController controller = new GpioController();
        static int redPin = 18;
        static int[] pins = new int[] {redPin, 24, 25};
        static int loopTime = 2000;
        static int lightTime = 1000;
        static int dimTime = 200;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                // turn off all pins when the program is terminated, with CTRL-C
                foreach (var pin in pins)
                {
                    Console.WriteLine($"Dim pin {pin}");
                    controller.Write(pin, PinValue.Low);
                }
            };

            while (true)
            {

                doTemperature();
                doRedLED();
                doLEDs();
                Thread.Sleep(loopTime); // sleep for 2000 milliseconds, 2 seconds
            }
        }

        static void doTemperature()
        {
            if (temperature.IsAvailable)
            {
                Console.WriteLine($"The CPU temperature is {temperature.Temperature.DegreesFahrenheit}");
            }
        }

        static void doLEDs()
        {
            foreach (var pin in pins)
            {
                {
                    Console.WriteLine($"Light LED at pin {pin} for {lightTime}ms");
                    controller.Write(pin, PinValue.High);
                    Thread.Sleep(lightTime);

                    Console.WriteLine($"Dim LED at pin {pin} for {dimTime}ms");
                    controller.Write(pin, PinValue.Low);
                    Thread.Sleep(dimTime);
                }            
            }
        }

        static void doRedLED()
        {
            Console.WriteLine($"Light for {lightTime}ms");
            controller.Write(redPin, PinValue.High);
            Thread.Sleep(lightTime);
            
            Console.WriteLine($"Dim for {dimTime}ms");
            controller.Write(redPin, PinValue.Low);
            Thread.Sleep(dimTime); 
        }
    }
}
