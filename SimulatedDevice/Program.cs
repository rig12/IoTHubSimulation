﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulatedDevice
{
    class Program
    {
        //private static DeviceClient s_deviceClient;
        private static IDictionary<string, string> devicekeys = new Dictionary<string, string>()
        {
            {"Maker1","mYZX4/bKTxKzk3pOgx42S4Ly9qSENgkaK7EwD/IfX70=" },
            {"Maker2","O4/VyeYcNuUvc+pmdp+lYlr0xlqw17VfPrQNx+B3/Wc=" },
            {"Maker3","rDi3DVcvrm1rOgJTY+z4gKLbUKd9XwNISN4u7L4iWkw=" },
            {"Maker4","nb0Ye3fGF6uqvUVlPcIdVix84dak/ix4AkpuRTyMSHk=" }
        };

        static int[] previousStates = { -1, -1, -1, -1 };

        private static IList<DeviceClient> deviceclients = Enumerable.Empty<DeviceClient>().ToList();
        //private readonly static string s_myDeviceId = "CSharpSimulateDevice";
        private readonly static string s_iotHubUri = "TobaccoIoTHub.azure-devices.net";
        // This is the primary key for the device. This is in the portal. 
        // Find your IoT hub in the portal > IoT devices > select your device > copy the key. 
        //private readonly static string s_deviceKey = "rxO7i9u7ccHMPfBGshWslMPFuonNYOCGQWpv9fEuC5c=";

        private static void Main(string[] args)
        {
            Console.WriteLine("Routing Tutorial: Simulated tobacco maker device\n");
            foreach (var key in devicekeys.Keys)
                if (devicekeys.TryGetValue(key, out var devicekey))
                    deviceclients.Add(DeviceClient.Create(s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(key, devicekey), TransportType.Mqtt));
            //s_deviceClient = DeviceClient.Create(s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(s_myDeviceId, s_deviceKey), TransportType.Mqtt);
            SendDeviceToCloudMessagesAsync();
            Console.WriteLine("Press the Enter key to stop.");
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {

            Random rand = new Random();

            while (true)
            {
                for (int i = 0; i < 4; i++)

                {
                    double currentProductivity = 0;
                    int currentState = -1;// minState + rand.Next(2);

                    string infoString;
                    string productivityValue;

                    var k = rand.NextDouble();
                    if (k > 0.7)
                    {
                        if (rand.NextDouble() > 0.5)
                        {
                            productivityValue = "low";
                            infoString = "Maker low productivity.";
                            currentState = 1;
                            currentProductivity = rand.NextDouble() * 30;
                        }
                        else
                        {
                            productivityValue = "stopped";
                            infoString = "Maker was stopped";
                            currentState = 0;
                            currentProductivity = rand.NextDouble() * 11;
                        }
                    }
                    else
                    {
                        productivityValue = "normal";
                        infoString = "This is a normal message.";
                        currentState = 2;
                    }

                    dynamic telemetryDataPoint = new ExpandoObject();
                    telemetryDataPoint.deviceId = devicekeys.Keys.ToArray()[i];

                    telemetryDataPoint.pointInfo = infoString;
                    telemetryDataPoint.EventTime = DateTime.Now;

                    if (previousStates[i] != currentState)
                    {
                        telemetryDataPoint.state = currentState;
                        if (currentState > 0) telemetryDataPoint.productivity = currentProductivity;
                    }
                    else
                        telemetryDataPoint.productivity = currentProductivity;


                    if (((IDictionary<String, Object>)telemetryDataPoint).ContainsKey("state") && telemetryDataPoint.state == 0)
                        telemetryDataPoint.reason = rand.Next(11) + 1;

                    var telemetryDataString = JsonConvert.SerializeObject(telemetryDataPoint);

                    //set the body of the message to the serialized value of the telemetry data
                    var message = new Message(Encoding.ASCII.GetBytes(telemetryDataString));
                    message.Properties.Add("level", productivityValue);
                    //if (productivityValue == "stopped")
                    //    message.Properties.Add("reason", rand.Next(11).ToString());
                    await deviceclients.ToArray()[i].SendEventAsync(message);
                    Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, telemetryDataString);
                    previousStates[i] = currentState;
                    await Task.Delay(1000);
                }
                await Task.Delay(10000);
            }
        }
    }
}