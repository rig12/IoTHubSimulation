// Copyright (c) Microsoft. All rights reserved.
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
        


        static int[] previousStates = { -1, -1, -1, -1 };

        private static IList<DeviceClient> deviceclients = Enumerable.Empty<DeviceClient>().ToList();

        // This is the primary key for the device. This is in the portal. 
        // Find your IoT hub in the portal > IoT devices > select your device > copy the key. 
        //private readonly static string s_deviceKey = "rxO7i9u7ccHMPfBGshWslMPFuonNYOCGQWpv9fEuC5c=";

        private static void Main(string[] args)
        {
            Console.WriteLine("Routing Tutorial: Simulated tobacco maker device\n");
            foreach (var key in MakersData.devicekeys.Keys)
                if (MakersData.devicekeys.TryGetValue(key, out var devicekey))
                    deviceclients.Add(DeviceClient.Create(MakersData.s_iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(key, devicekey), TransportType.Mqtt));
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
                    var currentProductivity = 0D;
                    var currentState = string.Empty;// minState + rand.Next(2);

                    string infoString;
                    string productivityValue;

                    var k = rand.NextDouble();
                    if (k > 0.7)
                    {
                        if (rand.NextDouble() > 0.5)
                        {
                            productivityValue = "low";
                            infoString = "Maker low productivity.";
                            currentState = "run";
                            currentProductivity = rand.NextDouble() * 11;
                        }
                        else
                        {
                            productivityValue = "stopped";
                            infoString = "Maker was stopped";
                            currentState = "idle";
                            
                        }
                    }
                    else
                    {
                        productivityValue = "normal";
                        infoString = "Normal productivity";
                        currentState = "run";
                        currentProductivity = rand.NextDouble() * 30;
                    }

                    dynamic telemetryDataPoint = new ExpandoObject();
                    telemetryDataPoint.deviceId = MakersData.devicekeys.Keys.ToArray()[i];
                    
                    telemetryDataPoint.pointInfo = infoString;
                    telemetryDataPoint.EventTime = DateTime.Now;
                    telemetryDataPoint.productivity = 0;
                    telemetryDataPoint.state = -1;
                    telemetryDataPoint.reason = -1;

                    if (previousStates[i] != currentState)
                    {
                        telemetryDataPoint.state = currentState;
                        if (currentState > 0) telemetryDataPoint.productivity = currentProductivity;
                    }
                    else
                        if (previousStates[i] > 0)
                        telemetryDataPoint.productivity = currentProductivity;


                    if (((IDictionary<String, Object>)telemetryDataPoint).ContainsKey("state") && telemetryDataPoint.state == 0)
                        telemetryDataPoint.reason = MakersData.reasons[rand.Next(MakersData.reasons.Count())];

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