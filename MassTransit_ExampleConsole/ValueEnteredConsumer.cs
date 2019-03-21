using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Context;
using SettlementApiMiddleware.Core.ExchangeId;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransit_ExampleConsole
{

    public class ValueEnteredConsumer : IConsumer<IValueEntered>
    {
        private IExchangeIdHelper _exchangeIdHelper { get; set; }
        public ValueEnteredConsumer(IExchangeIdHelper exchangeIdHelper)
        {
            _exchangeIdHelper = exchangeIdHelper;
        }

        public async Task Consume(ConsumeContext<IValueEntered> context)
        {
            string exchangeId = context.Headers.Get<string>(ExchangeIdHelper.ExchangeHeaderId);

            if (!String.IsNullOrEmpty(exchangeId))
                _exchangeIdHelper.SetExchangeIdValue(exchangeId);

            using (LogContext.PushProperty(ExchangeIdHelper.ExchangeHeaderId, exchangeId))
            {

                Log.Information("Value entered: " + context.Message.Value);

                var simpleHttpProxy = new SimpleHttpProxy(_exchangeIdHelper);

                simpleHttpProxy.GetValue(context.Message.Value);

            }
            
        }        
    }
}
