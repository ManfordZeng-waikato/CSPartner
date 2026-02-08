using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Application.DTOs.Ai;
using Domain.Exceptions;
using FluentAssertions;
using Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Tests.AI;

public class OpenAiVideoServiceTests
{
    [Fact]
    public async Task GenerateVideoMetaAsync_returns_description()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new
            {
                output = new[]
                {
                    new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "output_text",
                                text = "{\"description\":\"Clean clutch on Mirage\"}"
                            }
                        }
                    }
                }
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new OpenAiVideoService(http, config, NullLogger<OpenAiVideoService>.Instance);

        var result = await service.GenerateVideoMetaAsync(
            new VideoAiInputDto("title", null, null, null, null, null),
            CancellationToken.None);

        result.Description.Should().Be("Clean clutch on Mirage");
    }

    [Fact]
    public async Task GenerateVideoMetaAsync_throws_quota_exception()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = "quota",
                    type = "insufficient_quota",
                    code = "insufficient_quota"
                }
            });

            return new HttpResponseMessage((HttpStatusCode)429)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new OpenAiVideoService(http, config, NullLogger<OpenAiVideoService>.Instance);

        var act = async () => await service.GenerateVideoMetaAsync(
            new VideoAiInputDto("title", null, null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<AiServiceQuotaExceededException>();
    }

    [Fact]
    public async Task GenerateVideoMetaAsync_throws_service_exception_on_api_error()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = "bad request",
                    type = "invalid_request_error",
                    code = "invalid_api_key"
                }
            });

            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new OpenAiVideoService(http, config, NullLogger<OpenAiVideoService>.Instance);

        var act = async () => await service.GenerateVideoMetaAsync(
            new VideoAiInputDto("title", null, null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<AiServiceException>();
    }

    [Fact]
    public async Task GenerateVideoMetaAsync_throws_service_exception_on_non_json_error()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("oops", Encoding.UTF8, "text/plain")
            };
        });

        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new OpenAiVideoService(http, config, NullLogger<OpenAiVideoService>.Instance);

        var act = async () => await service.GenerateVideoMetaAsync(
            new VideoAiInputDto("title", null, null, null, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<AiServiceException>();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
