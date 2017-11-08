using Xamarin.Forms;
using Microsoft.Azure.Devices;
using Microsoft.Azure;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Errors;
using Newtonsoft.Json;
using HeartBeat.Models;
using System.Text;
using System;
using Newtonsoft.Json;
using PCLCrypto;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HeartBeat
{
    public partial class HeartBeatPage : ContentPage
    {
        static DeviceClient deviceClient;
        private const string _connectionString = "Endpoint=sb://heartbeatxlbus.servicebus.windows.net/;SharedAccessKeyName=Manager;SharedAccessKey=UQLtOGMLk4ubKNriRIY2Ezi8F4fuQdoULjB7swcJLmY=;EntityPath=heartbeatxlqueue";
        private List<AzureHeartBeat> _highHearts = new List<AzureHeartBeat>()
        {
            new AzureHeartBeat() { HeartBeatRate = 190, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 195, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 193, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 199, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 194, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 190, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 190, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 140, HeartIsFailing = false }
        };
        private List<AzureHeartBeat> _lowHeartRates = new List<AzureHeartBeat>()
        {
            new AzureHeartBeat() { HeartBeatRate = 30, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 20, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 25, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 39, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 40, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 15, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 23, HeartIsFailing = true },
            new AzureHeartBeat() { HeartBeatRate = 70, HeartIsFailing = false }
        };

        public HeartBeatPage()
        {
            InitializeComponent();
        }

        async void Handle_Temp_High_Failure(object sender, System.EventArgs e)
        {   
            try{
                foreach(AzureHeartBeat beat in _highHearts)
                {
                    var response = await PostOrderDtoToQueue(beat, "heartbeatxlbus", "Manager", "UQLtOGMLk4ubKNriRIY2Ezi8F4fuQdoULjB7swcJLmY=", "heartbeatxlqueue");
                    System.Diagnostics.Debug.WriteLine(response);
                }
            }catch(Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        async void Handle_Temp_Low_Failure(object sender, System.EventArgs e)
        {
            try
            {
                foreach (AzureHeartBeat beat in _lowHeartRates)
                {
                    var response = await PostOrderDtoToQueue(beat, "heartbeatxlbus", "Manager", "UQLtOGMLk4ubKNriRIY2Ezi8F4fuQdoULjB7swcJLmY=", "heartbeatxlqueue");
                    System.Diagnostics.Debug.WriteLine(response);
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }        
        }

        public const string ServiceBusNamespace = "heartbeatxlbus";

        public const string BaseServiceBusAddress = "https://" + ServiceBusNamespace + ".servicebus.windows.net/";

        /// <summary>
        /// The get shared access signature token.
        /// </summary>
        /// <param name="sasKeyName">
        /// The shared access signature key name.
        /// </param>
        /// <param name="sasKeyValue">
        /// The shared access signature key value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetSasToken(string sasKeyName, string sasKeyValue)
        {
            var expiry = GetExpiry();
            var stringToSign = WebUtility.UrlEncode(BaseServiceBusAddress) + "\n" + expiry;

            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha256);
            var hasher = algorithm.CreateHash(Encoding.UTF8.GetBytes(sasKeyValue));
            hasher.Append(Encoding.UTF8.GetBytes(stringToSign));
            var mac = hasher.GetValueAndReset();
            var signature = Convert.ToBase64String(mac);

            var sasToken = string.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", WebUtility.UrlEncode(BaseServiceBusAddress), WebUtility.UrlEncode(signature), expiry, sasKeyName);
            return sasToken;
        }

        /// <summary>
        /// Posts an order data transfer object to queue.
        /// </summary>
        /// <param name="orderDto">
        /// The order data transfer object.
        /// </param>
        /// <param name="serviceBusNamespace">
        /// The service bus namespace.
        /// </param>
        /// <param name="sasKeyName">
        /// The shared access signature key name.
        /// </param>
        /// <param name="sasKey">
        /// The shared access signature key.
        /// </param>
        /// <param name="queue">
        /// The queue.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<HttpResponseMessage> PostOrderDtoToQueue(AzureHeartBeat orderDto, string serviceBusNamespace, string sasKeyName, string sasKey, string queue)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseServiceBusAddress);
                client.DefaultRequestHeaders.Accept.Clear();

                var token = GetSasToken(sasKeyName, sasKey);
                client.DefaultRequestHeaders.Add("Authorization", token);

                HttpContent content = new StringContent(JsonConvert.SerializeObject(orderDto), Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var path = BaseServiceBusAddress + queue + "/messages";

                return await client.PostAsync(path, content);
            }
        }

        /// <summary>
        ///     Gets the expiry for a shared access signature token
        /// </summary>
        /// <returns>
        ///     The <see cref="string" /> expiry.
        /// </returns>
        private static string GetExpiry()
        {
            var sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToString((int)sinceEpoch.TotalSeconds + 3600);
        }
    }
}
