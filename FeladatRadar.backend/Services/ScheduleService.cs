using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeladatRadar.backend.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly string _connectionString;

        public ScheduleService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<IEnumerable<ScheduleEntry>> GetMyScheduleAsync(int studentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@StudentID", studentId);
            return await connection.QueryAsync<ScheduleEntry>(
                "sp_GetMySchedule", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> AddScheduleEntryAsync(int studentId, AddScheduleRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SubjectID", request.SubjectID);
                parameters.Add("@DayOfWeek", request.DayOfWeek);
                parameters.Add("@StartTime", TimeSpan.Parse(request.StartTime));
                parameters.Add("@EndTime", TimeSpan.Parse(request.EndTime));
                parameters.Add("@Location", request.Location);
                parameters.Add("@RecurrenceType", request.RecurrenceType ?? "Weekly");
                parameters.Add("@RecurrenceEndDate", request.RecurrenceEndDate.HasValue
                    ? (object)request.RecurrenceEndDate.Value.Date
                    : null);
                parameters.Add("@StartDate", request.StartDate.HasValue
                    ? (object)request.StartDate.Value.Date
                    : null);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_AddScheduleEntry", parameters, commandType: CommandType.StoredProcedure);

                var dict = (IDictionary<string, object>)result!;
                return new SubjectResponse
                {
                    Status = dict["Status"]?.ToString() ?? "ERROR",
                    Message = dict["Message"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<SubjectResponse> DeleteScheduleEntryAsync(int entryId, int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@EntryID", entryId);
                parameters.Add("@StudentID", studentId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteScheduleEntry", parameters, commandType: CommandType.StoredProcedure);

                var dict = (IDictionary<string, object>)result!;
                return new SubjectResponse
                {
                    Status = dict["Status"]?.ToString() ?? "ERROR",
                    Message = dict["Message"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }
    }
}
