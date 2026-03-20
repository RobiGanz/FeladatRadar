using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeladatRadar.backend.Services
{
    public class AdminService : IAdminService
    {
        private readonly string _connectionString;

        public AdminService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        // ──────────────────────────────────────────
        // FELHASZNÁLÓKEZELÉS
        // ──────────────────────────────────────────

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<AdminUserDto>(
                "sp_Admin_GetAllUsers", commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> ChangeUserRoleAsync(int adminUserId, int targetUserId, string newRole)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@TargetUserID", targetUserId);
                p.Add("@NewRole", newRole);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_ChangeUserRole", p, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> ToggleUserActiveAsync(int adminUserId, int targetUserId, bool isActive)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@TargetUserID", targetUserId);
                p.Add("@IsActive", isActive);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_ToggleUserActive", p, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> DeleteUserAsync(int adminUserId, int targetUserId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@TargetUserID", targetUserId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_DeleteUser", p, commandType: CommandType.StoredProcedure);

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

        // ──────────────────────────────────────────
        // RENDSZER STATISZTIKÁK
        // ──────────────────────────────────────────

        public async Task<SystemStatsDto> GetSystemStatsAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var result = await connection.QueryFirstOrDefaultAsync<SystemStatsDto>(
                    "sp_Admin_GetSystemStats", commandType: CommandType.StoredProcedure);
                return result ?? new SystemStatsDto();
            }
            catch
            {
                return new SystemStatsDto();
            }
        }

        // ──────────────────────────────────────────
        // ÖSSZES CSOPORT
        // ──────────────────────────────────────────

        public async Task<IEnumerable<AdminGroupDto>> GetAllGroupsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<AdminGroupDto>(
                "sp_Admin_GetAllGroups", commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> DeleteGroupAsync(int adminUserId, int groupId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@GroupID", groupId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_DeleteGroup", p, commandType: CommandType.StoredProcedure);

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

        // ──────────────────────────────────────────
        // AUDIT LOG
        // ──────────────────────────────────────────

        public async Task<IEnumerable<AuditLogEntry>> GetAuditLogAsync(int limit = 100)
        {
            using var connection = new SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@Limit", limit);
            return await connection.QueryAsync<AuditLogEntry>(
                "sp_Admin_GetAuditLog", p, commandType: CommandType.StoredProcedure);
        }

        public async Task WriteAuditLogAsync(int userId, string action, string? details = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@UserID", userId);
                p.Add("@Action", action);
                p.Add("@Details", details);
                await connection.ExecuteAsync(
                    "sp_Admin_WriteAuditLog", p, commandType: CommandType.StoredProcedure);
            }
            catch { /* silent — audit log nem blokkolhat műveleteket */ }
        }

        // ──────────────────────────────────────────
        // MODERÁCIÓ
        // ──────────────────────────────────────────

        public async Task<SubjectResponse> AdminDeletePollAsync(int adminUserId, int pollId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@PollID", pollId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_DeletePoll", p, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> AdminDeleteExamAsync(int adminUserId, int examId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@ExamID", examId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_DeleteExam", p, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> AdminDeleteTaskAsync(int adminUserId, int taskId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@AdminUserID", adminUserId);
                p.Add("@TaskID", taskId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_Admin_DeleteTask", p, commandType: CommandType.StoredProcedure);

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
