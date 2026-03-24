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
                var groups = (await connection.QueryAsync<Group>(
                    "sp_GetMyGroups", parameters, commandType: CommandType.StoredProcedure)).ToList();

                // Enrich with OwnerRole from Users table
                if (groups.Any())
                {
                    var ownerIds = groups.Select(g => g.CreatedBy).Distinct().ToList();
                    var roles = (await connection.QueryAsync<(int UserID, string UserRole)>(
                        "SELECT UserID, ISNULL(UserRole, 'Student') AS UserRole FROM Users WHERE UserID IN @Ids",
                        new { Ids = ownerIds })).ToDictionary(r => r.UserID, r => r.UserRole);
                    foreach (var g in groups)
                    {
                        g.OwnerRole = roles.GetValueOrDefault(g.CreatedBy, "Student");
                    }
                }
                return groups;
            }
            catch { return Enumerable.Empty<Group>(); }
        }

        public async Task<IEnumerable<GroupMember>> GetGroupMembersAsync(int groupId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@StudentID", userId);
                return await connection.QueryAsync<GroupMember>(
                    "sp_GetGroupMembers", parameters, commandType: CommandType.StoredProcedure);
            }
            catch { return Enumerable.Empty<GroupMember>(); }
        }

        public async Task<IEnumerable<GroupSubject>> GetGroupSubjectsAsync(int groupId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@StudentID", userId);
                return await connection.QueryAsync<GroupSubject>(
                    "sp_GetGroupSubjects", parameters, commandType: CommandType.StoredProcedure);
            }
            catch { return Enumerable.Empty<GroupSubject>(); }
        }

        public async Task<IEnumerable<GroupScheduleEntry>> GetGroupScheduleAsync(int groupId, int userId, string userRole = "Student")
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@StudentID", userId);
                parameters.Add("@UserRole", userRole);
                return await connection.QueryAsync<GroupScheduleEntry>(
                    "sp_GetGroupSchedule", parameters, commandType: CommandType.StoredProcedure);
            }
            catch { return Enumerable.Empty<GroupScheduleEntry>(); }
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
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@GroupID", groupId);
                parameters.Add("@StudentID", userId);
                return await connection.QueryAsync<GroupTask>(
                    "sp_GetGroupTasks", parameters, commandType: CommandType.StoredProcedure);
            }
            catch { return Enumerable.Empty<GroupTask>(); }
        }

        public async Task<IEnumerable<GroupInvite>> GetMyInvitesAsync(string email)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                return await connection.QueryAsync<GroupInvite>(
                    "sp_GetMyInvites", parameters, commandType: CommandType.StoredProcedure);
            }
            catch { return Enumerable.Empty<GroupInvite>(); }
        }
    }


}
