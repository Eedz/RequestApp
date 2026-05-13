using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace RequestApp
{
    public interface IDataRequestService
    {
        Task<List<Requester>> GetRequesters();
        Task<List<ITCDataSet>> GetDataSets();
        Task<List<string>> GetProjects();
        Task<List<Request>> GetRequests();
        Task<bool> CreateRequest(Request request);  
        Task<bool> UpdateRequest(Request request);
        Task<bool> DeleteRequest(int requestId);
    }

    public class RepoDataRequestService : IDataRequestService
    {
        IDataRequestRepository _repository;

        public RepoDataRequestService(IDataRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Requester>> GetRequesters()
        {
            return await _repository.GetRequesters();
        }

        public async Task<bool> CreateRequest(Request request)
        {
            return await _repository.CreateRequest(request);
        }

        public async Task<bool> UpdateRequest(Request request)
        {
            return await _repository.UpdateRequest(request);
        }

        public async Task<bool> DeleteRequest(int requestId)
        {
            return await _repository.DeleteRequest(requestId);
        }

        public async Task<List<ITCDataSet>> GetDataSets()
        {
            return await _repository.GetDataSets();
        }

        public async Task<List<Request>> GetRequests()
        {
            return await _repository.GetRequests();
        }

        public async Task<List<string>> GetProjects()
        {
            var datasets = await _repository.GetDataSets();
            var projects = datasets.Select(ds => ds.ProjectName)
                                   .Distinct().OrderBy(x=>x)
                                   .ToList();

            return projects;
        }
    }

    public class LocalDataRequestService : IDataRequestService
    {
        public LocalDataRequestService()
        {
        }

        public async Task<List<Requester>> GetRequesters()
        {
            var json = await File.ReadAllTextAsync("requesters.json");
            var names = JsonSerializer.Deserialize<List<Requester>>(json)
                           ?? new List<Requester>();

            return names;
        }

        public async Task<bool> CreateRequest(Request request)
        {
            return true;
        }

        public async Task<bool> UpdateRequest(Request request)
        {
            return true;
        }

        public async Task<bool> DeleteRequest(int requestId)
        {
            return true;
        }

        public async Task<List<ITCDataSet>> GetDataSets()
        {
            var json = await File.ReadAllTextAsync("datasets.json");
            var names = JsonSerializer.Deserialize<List<ITCDataSet>>(json)
                           ?? new List<ITCDataSet>();
            return names;
        }

        public async Task<List<Request>> GetRequests()
        {
            var json = await File.ReadAllTextAsync("requests.json");
            var requests = JsonSerializer.Deserialize<List<Request>>(json)
                           ?? new List<Request>();
            return requests;
        }

        public async Task<List<string>> GetProjects()
        {
            var requests = await GetRequests();
            var projects = requests.SelectMany(x => x.DataSets)
                                   .Select(ds => ds.ProjectName)
                                   .Distinct()
                                   .ToList();
            return projects;
        }
    }
}
