using Dapper;
using Microsoft.Data.SqlClient;
using RequestApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RequestApp
{
    public interface IDataRequestRepository
    {
        Task<List<Requester>> GetRequesters();
        Task<List<ITCDataSet>> GetDataSets();
        Task<List<Request>> GetRequests();  
        Task<bool> CreateRequest(Request request);
        Task<bool> UpdateRequest(Request request);
        Task<bool> DeleteRequest(int requestId);
    }


    public class DataRequestRepository : IDataRequestRepository
    {
        private readonly string _connectionString;

        public DataRequestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        // -------------------------
        // CREATE
        // -------------------------
        public async Task<bool> CreateRequest(Request request)
        {
            using var connection = CreateConnection();

            var sql = @"
            INSERT INTO DataRequests.Requests
            (
                Status,
                RequesterID,
                LatestSigning,
                ExpiryDate,
                AuthorizedBy,
                InternalExternal,
                Notes
            )
            VALUES
            (
                @Status,
                @RequesterID,
                @LatestSigning,
                @ExpiryDate,
                @AuthorizedBy,
                @InternalExternal,
                @Notes
            )";

            var rows = await connection.ExecuteAsync(sql, new
            {
                request.Status,
                RequesterID = request.Requester.ID,
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes
            });

            return rows > 0;
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateRequest(Request request)
        {
            using var connection = CreateConnection();

            var sql = @"
            UPDATE DataRequests.Requests
            SET
                Status = @Status,
                RequesterID = @RequesterID,
                LatestSigning = @LatestSigning,
                ExpiryDate = @ExpiryDate,
                AuthorizedBy = @AuthorizedBy,
                InternalExternal = @InternalExternal,
                Notes = @Notes
            WHERE ID = @ID";

            var rows = await connection.ExecuteAsync(sql, new
            {
                request.ID,
                request.Status,
                RequesterID = request.Requester.ID,
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes
            });

            return rows > 0;
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteRequest(int requestId)
        {
            using var connection = CreateConnection();

            var sql = @"DELETE FROM DataRequests.Requests WHERE ID = @Id";

            var rows = await connection.ExecuteAsync(sql, new { Id = requestId });

            return rows > 0;
        }

        // -------------------------
        // GET REQUESTERS
        // -------------------------
        public async Task<List<Requester>> GetRequesters()
        {
            using var connection = CreateConnection();

            var sql = @"
            SELECT 
                ID,
                FirstName,
                LastName,
                Affiliation,
                Email,
                CountryTeam,
                WebsiteMember,
                DataUser
            FROM DataRequests.Requesters
            ORDER BY LastName, FirstName";

            var result = await connection.QueryAsync<Requester>(sql);

            return result.ToList();
        }

        // -------------------------
        // GET Data Sets
        // -------------------------
        public async Task<List<ITCDataSet>> GetDataSets()
        {
            using var connection = CreateConnection();

            var sql = @"
            SELECT 
                ID,
               Name
            FROM DataRequests.DataSets
            ORDER BY Name";

            var result = await connection.QueryAsync<ITCDataSet>(sql);

            return result.ToList();
        }

        public async Task<List<Request>> GetRequests()
        {
            using var connection = CreateConnection();
            var sql = @"
            SELECT 
                r.ID,
                r.Status,
                r.LatestSigning,
                r.ExpiryDate,
                r.AuthorizedBy,
                r.InternalExternal,
                r.Notes,
                req.ID as RequesterID,
                req.ID,
                req.FirstName,
                req.LastName,
                req.Affiliation,
                req.Email,
                req.CountryTeam,
                req.WebsiteMember,
                req.DataUser
            FROM DataRequests.Requests r
            INNER JOIN DataRequests.Requesters req ON r.RequesterID = req.ID
            ORDER BY r.ID";
            var result = await connection.QueryAsync<Request, Requester, Request>(
                sql,
                (request, requester) =>
                {
                    request.Requester = requester;
                    return request;
                },
                splitOn: "RequesterID"
            );

            await PopulateDataSets(result.ToList());

            return result.ToList();
        }

        public async Task PopulateDataSets(List<Request> requests)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT RD.ID, RequestID, DataSetID, S.Name FROM DataRequests.DataRequestDataSets RD INNER JOIN DataRequests.DataSets S ON RD.DataSetID = S.ID;";

            var result = await connection.QueryAsync(sql);

            var dictionaries = result
            .Select(r => (IDictionary<string, object>)r)
            .ToList();

            foreach (var d in dictionaries)
            {
                var requestId = (int)d["RequestID"];
                var dataSetId = (int)d["DataSetID"];
                var dataSetName = (string)d["Name"];
                var request = requests.FirstOrDefault(r => r.ID == requestId);
                if (request != null)
                {
                    request.DataSets.Add(new ITCDataSet
                    {
                        ID = dataSetId,
                        Name = dataSetName
                    });
                }
            }
        }
    }
}
