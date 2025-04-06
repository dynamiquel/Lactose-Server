using System.Net.Http.Json;
using Lactose.Simulation.Controllers;
using Lactose.Simulation.Dtos.Crops;
using Lactose.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lactose.Economy;

public class CropsClient(
    HttpClient httpClient,
    IOptions<SimulationClientOptions> options)
    : ICropsController
{
    public async Task<ActionResult<QueryCropsResponse>> QueryCrops(QueryCropsRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/crops/query"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<QueryCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<GetCropsResponse>> GetCrops(GetCropsRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/crops"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<GetCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<GetCropResponse>> CreateCrop(CreateCropRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/crops/create"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<GetCropResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<GetCropResponse>> UpdateCrop(UpdateCropRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/crops/update"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<GetCropResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<DeleteCropsResponse>> DeleteCrops(DeleteCropsRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/crops/delete"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<DeleteCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }
}