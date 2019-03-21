using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http;
using SettlementApiMiddleware.Core.ExchangeId;
using Serilog;

namespace MassTransit_ExampleConsole
{
    public class SimpleHttpProxy
    {
        private IExchangeIdHelper _exchangeIdHelper;
        public SimpleHttpProxy(IExchangeIdHelper exchangeIdHelper)
        {
            _exchangeIdHelper = exchangeIdHelper;
        }

        public void GetValue(string value)
        {
            var http = new HttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5001/api/values?value={value}");

            message.Headers.Add(ExchangeIdHelper.ExchangeHeaderId, _exchangeIdHelper.GetRequestExchangeId());

            Log.Information("Sending request to simple http api...");

            var response = http.SendAsync(message);

            if (response != null)
                Log.Information($"Response recieved: {response.Result.StatusCode}");
            else
                Log.Information("There was a problem...");
            
        }
    }
}
