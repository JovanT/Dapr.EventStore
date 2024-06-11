using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dapr.EventStore.Web
{
    public class SampleController : ControllerBase
    {
        private readonly DaprEventStore store;
        private readonly ILogger logger;
        private readonly DaprClient _client;
        private string streamName = "customer.";

        public SampleController(DaprEventStore store, ILogger<SampleController> logger, global::Dapr.Client.DaprClient client)
        {
            this.store = store;
            this.logger = logger;
            _client = client;

            streamName += store.Mode;
        }

        [HttpGet("getevents")]
        public async Task<ActionResult> Get()
        {
            this.logger.LogInformation("C# got getvents");
            //var meta = store.MetaProvider(streamName);

            //var obj = new Dictionary<string, object>() {
            //                { "filter",
            //                    new Dictionary<string, object>()
            //                        {
            //                            { "EQ",
            //                                new Dictionary<string, string>()
            //                                    {
            //                                        { "value.StreamName", streamName },
            //                                    }
            //                            }
            //                        }
            //                } 
            //};

            //logger.LogDebug(JsonSerializer.Serialize(obj));

            //var r = await _client.QueryStateAsync<EventData>(DaprEventStore.StoreName, JsonSerializer.Serialize(obj));

            //logger.LogDebug(JsonSerializer.Serialize(r));

            //return Ok(r);
            var streamMeta = await store.GetStreamMetaData(streamName);

            logger.LogInformation(JsonSerializer.Serialize(streamMeta));
            var result = new List<EventData>();

            await foreach (var r in store.LoadEventStreamAsync(streamName, 2))
            {
                result.Add(r);
            }

            return Ok(result);
        }

        [Topic(pubsubName: "pubsub", name: "customer")]
        [HttpPost("sample")]
        public async Task<ActionResult> Post()
        {

            //for integers
            Random r = new Random();
            int rInt = r.Next(0, 100);

            this.logger.LogInformation("C# got event (pub/sub");

            EventData[] events =
            {
                EventData.Create("CREATE-CUSTOMER", new
                {
                    FirstName = $"Jovan{rInt}",
                    LastName = $"Trajkov{rInt}",
                    Random = rInt
                }),
                EventData.Create("CREATE-CUSTOMER", new
                {
                    FirstName = $"Jovan{rInt + 1}",
                    LastName = $"Trajkov{rInt + 1}",
                    Random = rInt + 1
                }),
                EventData.Create("CREATE-CUSTOMER", new
                {
                    FirstName = $"Jovan{rInt + 2}",
                    LastName = $"Trajkov{rInt + 2}",
                    Random = rInt + 2
                }),
            };

            return Ok(await store.AppendToStreamAsync(streamName, events));
        }
    }
    public class SampleEvent
    {
        public string Message { get; set; }
    }
}
