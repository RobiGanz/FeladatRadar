using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeladatRadar.backend.Services
{
    public class TaskService : ITaskService
    {
        private readonly string _connectionString;

        public TaskService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<IEnumerable<TaskItem>> GetMyTasksAsync(int studentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@StudentID", studentId);
            return await connection.QueryAsync<TaskItem>(
                "sp_GetMyTasks", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> AddTaskAsync(int studentId, AddTaskRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SubjectID", request.SubjectID);
                parameters.Add("@Title", request.Title);
                parameters.Add("@Description", request.Description);
                parameters.Add("@DueDate", request.DueDate);
                parameters.Add("@TaskType", request.TaskType);
                parameters.Add("@RecurrenceType", request.RecurrenceType ?? "None");
                parameters.Add("@RecurrenceEndDate", request.RecurrenceEndDate);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_AddTask", parameters, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> CompleteTaskAsync(int taskId, int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@TaskID", taskId);
                parameters.Add("@StudentID", studentId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_CompleteTask", parameters, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> UncompleteTaskAsync(int taskId, int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@TaskID", taskId);
                parameters.Add("@StudentID", studentId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_UncompleteTask", parameters, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> DeleteTaskAsync(int taskId, int studentId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@TaskID", taskId);
                parameters.Add("@StudentID", studentId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteTask", parameters, commandType: CommandType.StoredProcedure);

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
