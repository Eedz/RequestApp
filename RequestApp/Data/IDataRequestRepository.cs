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

        /// <summary>
        /// Create Request with related datasets and formats in a single transaction to ensure data integrity. 
        /// If any part of the process fails, the entire transaction will be rolled back, preventing partial data from being saved. 
        /// This approach ensures that the Request and its associated datasets and formats are always in sync in the database.
        /// </summary>
        /// <param name="request">The request object to be created.</param>
        /// <returns>True if the request was successfully created; otherwise, false.</returns>
        public async Task<bool> CreateNewRequest(Request request)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await CreateRequest(request, connection, transaction);

                await SyncRequestRequesters(request, connection, transaction);

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
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes,
                Partial = request.PartialDataSets
            }, transaction);

            // get new ID
            request.ID = (int)requestId;

            return requestId > 0;
        }

        /// <summary>
        /// Update Request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRequest(Request request)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await UpdateRequestRow(request, connection, transaction);

                await SyncRequestRequesters(request, connection, transaction);

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
                request.LatestSigning,
                request.ExpiryDate,
                request.AuthorizedBy,
                request.InternalExternal,
                request.Notes,
                Partial = request.PartialDataSets
            }, transaction);
        }

        private async Task SyncRequestRequesters(Request request, IDbConnection connection, IDbTransaction transaction)
        {
            // current rows in DB
            var existingIds = (await connection.QueryAsync<int>(@"
                    SELECT RequesterID
                    FROM DataRequests.DataRequestRequesters
                    WHERE RequestID = @RequestID",
                    new
                    {
                        RequestID = request.ID
                    },
                    transaction))
                    .ToList();

            // current datasets in memory
            var currentIds = request.Requesters
                .Select(x => x.ID)
                .ToList();

            // determine differences
            var addedIds = currentIds.Except(existingIds);

            var removedIds = existingIds.Except(currentIds);

            // insert newly added datasets
            foreach (var requesterId in addedIds)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO DataRequests.DataRequestRequesters
                    (
                        RequestID,
                        RequesterID
                    )
                    VALUES
                    (
                        @RequestID,
                        @RequesterID
                    )",
                    new
                    {
                        RequestID = request.ID,
                        RequesterID = requesterId
                    },
                    transaction);
            }

            // remove deleted datasets
            foreach (var requesterId in removedIds)
            {
                await connection.ExecuteAsync(@"
                    DELETE FROM DataRequests.DataRequestRequesters
                    WHERE RequestID = @RequestID
                    AND RequesterID = @RequesterID",
                    new
                    {
                        RequestID = request.ID,
                        RequesterID = requesterId
                    },
                    transaction);
            }
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

        /// <summary>
        /// Delete Request.
        /// </summary>
        /// <param name="requestId">The ID of the request to delete.</param>
        /// <returns>True if the request was deleted successfully; otherwise, false.</returns>
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
                DataUser,
                StaffMember,
                CoreMember
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

        /// <summary>
        /// Get Requests with related Requester, DataSets, and DataFormats.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Request>> GetRequests()
        {
            using var connection = CreateConnection();
            var sql = @"
            SELECT 
                ID,
                Status,
                LatestSigning,
                ExpiryDate,
                AuthorizedBy,
                InternalExternal,
                Notes,
                PartialDataSets
            FROM DataRequests.Requests            
            ORDER BY ID";
            var result = await connection.QueryAsync<Request>(sql);

            await PopulateRequesters(result.ToList());
            await PopulateDataSets(result.ToList());
            await PopulateDataFormats(result.ToList());

            return result.ToList();
        }

        public async Task PopulateRequesters(List<Request> requests)
        {
            using var connection = CreateConnection();
            var sql = @"SELECT RD.ID, RequestID, RequesterID, 
                FirstName,
                LastName,
                Affiliation,
                Email,
                CountryTeam,
                WebsiteMember,
                DataUser,
                StaffMember,
                CoreMember FROM DataRequests.DataRequestRequesters RD INNER JOIN DataRequests.Requesters P ON RD.RequesterID = P.ID;";

            var result = await connection.QueryAsync(sql);

            var dictionaries = result
            .Select(r => (IDictionary<string, object>)r)
            .ToList();

            foreach (var d in dictionaries)
            {
                var requestId = (int)d["RequestID"];
                var request = requests.FirstOrDefault(r => r.ID == requestId);
                if (request != null)
                {
                    request.Requesters.Add(new Requester
                    {
                        ID = (int)d["RequesterID"],
                        FirstName = (string)d["FirstName"],
                        LastName = (string)d["LastName"],
                        //LatestSigning = (DateTime?)d["LatestSigning"],
                    });
                }
            }
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
