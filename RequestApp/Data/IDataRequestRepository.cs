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
        Task<bool> CreateNewRequest(Request request);
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

        public async Task<bool> CreateNewRequest(Request request)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await CreateRequest(request, connection, transaction);

                await SyncRequestDataSets(request, connection, transaction);

                await SyncRequestFormats(request, connection, transaction);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> CreateRequest(Request request, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
            INSERT INTO DataRequests.Requests
            (
                Status,
                RequesterID,
                LatestSigning,
                ExpiryDate,
                AuthorizedBy,
                InternalExternal,
                Notes,
                PartialDataSets
            )
            VALUES
            (
                @Status,
                @RequesterID,
                @LatestSigning,
                @ExpiryDate,
                @AuthorizedBy,
                @InternalExternal,
                @Notes,
                @Partial
            );

            SELECT SCOPE_IDENTITY();";

            var requestId = await connection.ExecuteScalarAsync<long>(sql, new
            {
                request.Status,
                RequesterID = request.Requester.ID,
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes,
                Partial = request.PartialDataSets
            }, transaction);

            // assign ID back to object
            request.ID =(int)requestId;

            return requestId > 0;
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateRequest(Request request)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await UpdateRequestRow(request, connection, transaction);

                await SyncRequestDataSets(request, connection, transaction);

                await SyncRequestFormats(request, connection, transaction);

                transaction.Commit();

                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task UpdateRequestRow(Request request, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                UPDATE DataRequests.Requests
                SET
                    Status = @Status,
                    RequesterID = @RequesterID,
                    LatestSigning = @LatestSigning,
                    ExpiryDate = @ExpiryDate,
                    AuthorizedBy = @AuthorizedBy,
                    InternalExternal = @InternalExternal,
                    Notes = @Notes,
                    PartialDataSets = @Partial
                WHERE ID = @ID";

            await connection.ExecuteAsync(sql, new
            {
                request.ID,
                request.Status,
                RequesterID = request.Requester.ID,
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes,
                Partial = request.PartialDataSets
            }, transaction);
        }

        private async Task SyncRequestDataSets(Request request, IDbConnection connection, IDbTransaction transaction)
        {
            // current rows in DB
            var existingIds = (await connection.QueryAsync<int>(@"
                    SELECT DataSetID
                    FROM DataRequests.DataRequestDataSets
                    WHERE RequestID = @RequestID",
                    new
                    {
                        RequestID = request.ID
                    },
                    transaction))
                    .ToList();

            // current datasets in memory
            var currentIds = request.DataSets
                .Select(x => x.ID)
                .ToList();

            // determine differences
            var addedIds = currentIds.Except(existingIds);

            var removedIds = existingIds.Except(currentIds);

            // insert newly added datasets
            foreach (var dataSetId in addedIds)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO DataRequests.DataRequestDataSets
                    (
                        RequestID,
                        DataSetID
                    )
                    VALUES
                    (
                        @RequestID,
                        @DataSetID
                    )",
                    new
                    {
                        RequestID = request.ID,
                        DataSetID = dataSetId
                    },
                    transaction);
            }

            // remove deleted datasets
            foreach (var dataSetId in removedIds)
            {
                await connection.ExecuteAsync(@"
                    DELETE FROM DataRequests.DataRequestDataSets
                    WHERE RequestID = @RequestID
                    AND DataSetID = @DataSetID",
                    new
                    {
                        RequestID = request.ID,
                        DataSetID = dataSetId
                    },
                    transaction);
            }
        }

        private async Task SyncRequestFormats(Request request, IDbConnection connection, IDbTransaction transaction)
        {
            // current rows in DB
            var existingFormats = (await connection.QueryAsync<string>(@"
                    SELECT DataFormat
                    FROM DataRequests.RequestDataFormats
                    WHERE RequestID = @RequestID",
                    new
                    {
                        RequestID = request.ID
                    },
                    transaction))
                    .ToList();

            // current datasets in memory
            var currentFormats = request.DataFormat
                .Select(x => x)
                .ToList();

            // determine differences
            var addedFormats = currentFormats.Except(existingFormats);

            var removedIds = existingFormats.Except(currentFormats);

            // insert newly added datasets
            foreach (var format in addedFormats)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO DataRequests.RequestDataFormats
                    (
                        RequestID,
                        DataFormat
                    )
                    VALUES
                    (
                        @RequestID,
                        @DataFormat
                    )",
                    new
                    {
                        RequestID = request.ID,
                        DataFormat = format
                    },
                    transaction);
            }

            // remove deleted datasets
            foreach (var format in removedIds)
            {
                await connection.ExecuteAsync(@"
                    DELETE FROM DataRequests.RequestDataFormats
                    WHERE RequestID = @RequestID
                    AND DataFormat = @DataSetID",
                    new
                    {
                        RequestID = request.ID,
                        DataFormat = format
                    },
                    transaction);
            }
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
                r.PartialDataSets,
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
            await PopulateDataFormats(result.ToList());

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

        public async Task PopulateDataFormats(List<Request> requests)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT ID, RequestID, DataFormat FROM DataRequests.RequestDataFormats;";

            var result = await connection.QueryAsync(sql);

            var dictionaries = result
            .Select(r => (IDictionary<string, object>)r)
            .ToList();

            foreach (var d in dictionaries)
            {
                var requestId = (int)d["RequestID"];
                var format = (string)d["DataFormat"];
                
                var request = requests.FirstOrDefault(r => r.ID == requestId);
                if (request != null)
                {
                    request.DataFormat.Add(format);
                    
                }
            }
        }
    }
}
