using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lactose.Simulation.Controllers;
using Lactose.Simulation.Dtos.UserCrops;
using Lactose.Client;
using LactoseWebApp.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Lactose.Economy;

public class UserCropsClient(
    HttpClient httpClient,
    IApiAuthHandler authHandler,
    IOptions<SimulationClientOptions> options)
    : IUserCropsController
{
    public async Task<ActionResult<GetUserCropsResponse>> GetCrops(GetUserCropsRequest request)
    {
        // Forward the API's Access Token to the HTTP Client.
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/usercrops"),
            Content = JsonContent.Create(request),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", authHandler.AccessToken?.UnsafeToString()) }
        };
        
        var response = await httpClient.SendFromJson<GetUserCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<GetUserCropsResponse>> GetCropsById(GetUserCropsByIdRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/usercrops/byid"),
            Content = JsonContent.Create(request),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", authHandler.AccessToken?.UnsafeToString()) }
        };
        
        var response = await httpClient.SendFromJson<GetUserCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public async Task<ActionResult<SimulateUserCropsResponse>> SimulateCrops(SimulateUserCropsRequest request)
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{options.Value.Url}/usercrops/simulate"),
            Content = JsonContent.Create(request)
        };
        
        var response = await httpClient.SendFromJson<SimulateUserCropsResponse>(httpRequest);
        return response is not null ? response : new EmptyResult();
    }

    public Task<ActionResult<CreateUserCropResponse>> CreateCrop(CreateUserCropRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<HarvestUserCropsResponse>> HarvestCrops(HarvestUserCropsRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<DestroyUserCropsResponse>> DestroyCrops(DestroyUserCropsRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<FertiliseUserCropsResponse>> FertiliseCrops(FertiliseUserCropsRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<SeedUserCropsResponse>> SeedCrop(SeedUserCropRequest request)
    {
        throw new NotImplementedException();
    }
}