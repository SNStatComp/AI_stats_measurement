using AI_stats_measurement.Backend.Clients;
using AI_stats_measurement.Backend.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AI_stats_measurement.Tests
{
    public class QuerierTests
    {
        [Fact]
        public async Task AskAsync_Returns_Output_Text_From_Grok_Response()
        {
            var json = """
                {
                  "output": [
                    {
                      "type": "message",
                      "content": [
                        {
                          "type": "output_text",
                          "text": "The answer is 100."
                        }
                      ]
                    }
                  ]
                }
                """;

            var handler = new FakeHttpMessageHandler(json, HttpStatusCode.OK);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.x.ai/")
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["LlmKeys:Grok"] = "fake-key"
                })
                .Build();

            var querier = new GrokWebSearchQuerier(httpClient, config);

            var prompt = new Prompt(
                "CBS",
                "You are helpful.",
                "theme",
                DateTime.UtcNow,
                "subject",
                "What is the answer?",
                100,
                new Source
                {
                    Name = "Test",
                    Url = "https://example.com",
                    Type = "NSI"
                },
                ""
            );

            var result = await querier.AskAsync(prompt);

            Assert.Equal("The answer is 100.", result);
        }
    }
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;
        private readonly HttpStatusCode _statusCode;

        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpMessageHandler(string response, HttpStatusCode statusCode)
        {
            _response = response;
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = _statusCode,
                Content = new StringContent(_response, Encoding.UTF8, "application/json")
            });
        }
    }
}
