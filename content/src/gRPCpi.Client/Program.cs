using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using System.Device.Gpio;
using System.Threading;
using System.Runtime.InteropServices;

namespace gRPCpi
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Include port of the gRPC server as an application argument
            var port = args.Length > 0 ? args[0] : "50051";

            //CHANGE THIS TO YOUR SERVER ADDRESS
            var serverIpAddress = "192.168.1.244";

            var channel = new Channel($"{serverIpAddress}:" + port, ChannelCredentials.Insecure);
            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });

            Console.WriteLine("Greeting: " + reply.Message);

            await channel.ShutdownAsync();

            PulseReply(reply);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static System.Device.Gpio.GpioController GetController()
        {
            if (RuntimeInformation.OSArchitecture == Architecture.Arm) //Assume we're on the pi
                return new GpioController(PinNumberingScheme.Board);
            else
                return new GpioController(PinNumberingScheme.Board, new ConsoleGPIODriver());
        }

        /// <summary>
        /// Pulses the light a number of times corresponding to the character's alphabet index
        /// </summary>
        /// <param name="reply"></param>
        private static void PulseReply(HelloReply reply)
        {
            using (GpioController controller = GetController())
            {
                var onTime = 100;
                var offTime = 50;
                var pin = 40;
                controller.OpenPin(pin, PinMode.Output);

                try
                {
                    foreach (char c in reply.Message)
                    {
                        var pulseCount = Math.Max(c.ToString().ToUpper()[0] - 64, 0);
                        Console.Write(c);
                        for (int i=0;i<pulseCount;i++)
                        {
                            controller.Write(pin, PinValue.High);
                            Console.Write(".");
                            Thread.Sleep(onTime);
                            controller.Write(pin, PinValue.Low);
                            Thread.Sleep(offTime);
                        }
                        Thread.Sleep(500);
                        Console.WriteLine();

                        }
                    }
                finally
                {
                    controller.ClosePin(pin);
                }
            }
        }
    }
}
