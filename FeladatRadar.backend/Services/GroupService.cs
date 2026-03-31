using Dapper;
using FeladatRadar.backend.Models;
using FeladatRadar.backend.Services;
using Microsoft.Data.SqlClient;
using System.Data;


namespace FeladatRadar.backend.Services
{
    public class GroupService : IGroupService
    {
        private readonly string _connectionString;

        public GroupService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        private SubjectResponse ParseResponse(dynamic result)
        {
            if (result == null)
                return new SubjectResponse { Status = "ERROR", Message = "Nem érkezett válasz az adatbázistól." };
            var dict = (IDictionary<string, object>)result;
            return new SubjectResponse
            {
                Status = dict.ContainsKey("Status") ? dict["Status"]?.ToString() ?? "ERROR" : "OK",
                Message = dict.ContainsKey("Message") ? dict["Message"]?.ToString() ?? "" : ""
            };
        }

        public async Task<SubjectResponse> CreateGroupAsync(int userId, string groupName)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@UserID", userId);
                parameters.Add("@GroupName", groupName);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_CreateGroup", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<SubjectResponse> InviteToGroupAsync(int groupId, int invitedBy, string invitedEmail)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@InvitedBy", invitedBy);
                parameters.Add("@InvitedEmail", invitedEmail);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_InviteToGroup", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<SubjectResponse> AcceptInviteAsync(int inviteId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@InviteID", inviteId);
                parameters.Add("@StudentID", userId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_AcceptGroupInvite", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<SubjectResponse> DeclineInviteAsync(int inviteId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@InviteID", inviteId);
                parameters.Add("@StudentID", userId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeclineGroupInvite", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<SubjectResponse> LeaveGroupAsync(int groupId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@StudentID", userId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_LeaveGroup", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<IEnumerable<Group>> GetMyGroupsAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", userId);
                // sp_GetMyGroups már tartalmazza az OwnerRole-t – nincs szükség második lekérdezésre
                return (await connection.QueryAsync<Group>(
                    "sp_GetMyGroups", parameters, commandType: CommandType.StoredProcedure)).ToList();
            }
            catch (Exception ex)
            {
                // Nem nyeljük le csendesen – logolható / buborékoltatható
                throw new InvalidOperationException($"Csoportok lekérése sikertelen: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<GroupMember>> GetGroupMembersAsync(int groupId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupID", groupId);
            parameters.Add("@StudentID", userId);
            return await connection.QueryAsync<GroupMember>(
                "sp_GetGroupMembers", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<GroupSubject>> GetGroupSubjectsAsync(int groupId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupID", groupId);
            parameters.Add("@StudentID", userId);
            return await connection.QueryAsync<GroupSubject>(
                "sp_GetGroupSubjects", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<GroupScheduleEntry>> GetGroupScheduleAsync(int groupId, int userId, string userRole = "Student")
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupID", groupId);
            parameters.Add("@StudentID", userId);
            parameters.Add("@UserRole", userRole);
            return await connection.QueryAsync<GroupScheduleEntry>(
                "sp_GetGroupSchedule", parameters, commandType: CommandType.StoredProcedure);
        }
        public async Task<SubjectResponse> AddGroupScheduleEntryAsync(int groupId, int userId, AddScheduleRequest request, string userRole = "Teacher")
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@AddedBy", userId);
                parameters.Add("@SubjectID", request.SubjectID);
                parameters.Add("@DayOfWeek", request.DayOfWeek);
                parameters.Add("@StartTime", TimeSpan.Parse(request.StartTime));
                parameters.Add("@EndTime", TimeSpan.Parse(request.EndTime));
                parameters.Add("@Location", request.Location);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_AddGroupScheduleEntry", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }
        public async Task<IEnumerable<GroupTask>> GetGroupTasksAsync(int groupId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@GroupID", groupId);
            parameters.Add("@StudentID", userId);
            return await connection.QueryAsync<GroupTask>(
                "sp_GetGroupTasks", parameters, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Egyetlen DB lekérdezéssel ellenőrzi, hogy a user kezelheti-e a csoportot.
        /// Korábban mindkét controllerben az összes csoport lekérésével oldották meg (N+1 jellegű).
        /// </summary>
        public async Task<bool> CanManageGroupAsync(int groupId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var row = await connection.QueryFirstOrDefaultAsync<(int CreatedBy, string OwnerRole, bool IsMember)>(
                    @"SELECT g.CreatedBy,
                             ISNULL(u.UserRole, 'Student') AS OwnerRole,
                             CASE WHEN gm.StudentID IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsMember
                      FROM Groups g
                      INNER JOIN Users u ON u.UserID = g.CreatedBy
                      LEFT JOIN GroupMembers gm ON gm.GroupID = g.GroupID AND gm.StudentID = @UserID
                      WHERE g.GroupID = @GroupID",
                    new { GroupID = groupId, UserID = userId });

                if (!row.IsMember) return false;
                if (row.OwnerRole == "Student") return true;   // diák-csoport: mindenki kezelhet
                return row.CreatedBy == userId;                // tanári csoport: csak a tulajdonos
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsGroupMemberAsync(int groupId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM GroupMembers WHERE GroupID = @GroupID AND StudentID = @UserID",
                    new { GroupID = groupId, UserID = userId }) > 0;
            }
            catch { return false; }
        }

        public async Task<SubjectResponse> AddGroupTaskAsync(int groupId, int userId, AddGroupTaskRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@AddedBy", userId);
                parameters.Add("@SubjectID", request.SubjectID);
                parameters.Add("@Title", request.Title);
                parameters.Add("@Description", request.Description);
                parameters.Add("@DueDate", request.DueDate);
                parameters.Add("@TaskType", request.TaskType ?? "Exam");
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_AddGroupTask", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<IEnumerable<GroupInvite>> GetMyInvitesAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            return await connection.QueryAsync<GroupInvite>(
                "sp_GetMyInvites", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> DeleteGroupTaskAsync(int groupId, int taskId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@TaskID", taskId);
                parameters.Add("@UserID", userId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteGroupTask", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }

        public async Task<SubjectResponse> DeleteGroupScheduleEntryAsync(int groupId, int entryId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@EntryID", entryId);
                parameters.Add("@UserID", userId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteGroupScheduleEntry", parameters, commandType: CommandType.StoredProcedure);
                return ParseResponse(result);
            }
            catch (Exception ex) { return new SubjectResponse { Status = "ERROR", Message = ex.Message }; }
        }
    }


}
