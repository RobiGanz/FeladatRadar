using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeladatRadar.backend.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly string _connectionString;

        public TeacherService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        // ──────────────────────────────
        // SZAVAZÁS
        // ──────────────────────────────

        public async Task<SubjectResponse> CreatePollAsync(int userId, CreatePollRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@GroupID", request.GroupID);
                p.Add("@CreatedBy", userId);
                p.Add("@Question", request.Question);
                p.Add("@ExpiresAt", request.ExpiresAt);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_CreatePoll", p, commandType: CommandType.StoredProcedure);

                var dict = (IDictionary<string, object>)result!;
                if (dict["Status"]?.ToString() != "OK")
                    return new SubjectResponse { Status = "ERROR", Message = dict["Message"]?.ToString() ?? "" };

                int pollId = Convert.ToInt32(dict["PollID"]);

                // Opciók hozzáadása
                for (int i = 0; i < request.Options.Count; i++)
                {
                    var op = new DynamicParameters();
                    op.Add("@PollID", pollId);
                    op.Add("@OptionText", request.Options[i]);
                    op.Add("@SortOrder", i);
                    await connection.ExecuteAsync("sp_AddPollOption", op, commandType: CommandType.StoredProcedure);
                }

                return new SubjectResponse { Status = "OK", Message = "Szavazás létrehozva." };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = ex.Message };
            }
        }

        public async Task<IEnumerable<Poll>> GetGroupPollsAsync(int groupId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@GroupID", groupId);
            p.Add("@UserID", userId);

            // A SP egy lapos JOIN-t ad vissza, amit itt mappelünk Poll + Options struktúrába
            var rows = await connection.QueryAsync<dynamic>(
                "sp_GetGroupPolls", p, commandType: CommandType.StoredProcedure);

            var pollDict = new Dictionary<int, Poll>();
            foreach (var row in rows)
            {
                var dict = (IDictionary<string, object>)row;
                int pollId = Convert.ToInt32(dict["PollID"]);

                if (!pollDict.TryGetValue(pollId, out var poll))
                {
                    poll = new Poll
                    {
                        PollID = pollId,
                        GroupID = Convert.ToInt32(dict["GroupID"]),
                        CreatedBy = Convert.ToInt32(dict["CreatedBy"]),
                        CreatedByName = dict["CreatedByName"]?.ToString() ?? "",
                        Question = dict["Question"]?.ToString() ?? "",
                        CreatedAt = Convert.ToDateTime(dict["CreatedAt"]),
                        ExpiresAt = dict["ExpiresAt"] == DBNull.Value ? null : Convert.ToDateTime(dict["ExpiresAt"]),
                        IsActive = Convert.ToBoolean(dict["IsActive"]),
                        MyVoteOptionID = dict["MyVoteOptionID"] == DBNull.Value ? null : Convert.ToInt32(dict["MyVoteOptionID"])
                    };
                    pollDict[pollId] = poll;
                }

                poll.Options.Add(new PollOption
                {
                    OptionID = Convert.ToInt32(dict["OptionID"]),
                    PollID = pollId,
                    OptionText = dict["OptionText"]?.ToString() ?? "",
                    VoteCount = Convert.ToInt32(dict["VoteCount"])
                });
            }

            return pollDict.Values;
        }

        public async Task<SubjectResponse> VoteAsync(int pollId, int optionId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@PollID", pollId);
                p.Add("@OptionID", optionId);
                p.Add("@UserID", userId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_VotePoll", p, commandType: CommandType.StoredProcedure);

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

        public async Task<SubjectResponse> DeletePollAsync(int pollId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@PollID", pollId);
                p.Add("@RequestBy", userId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeletePoll", p, commandType: CommandType.StoredProcedure);

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

        // ──────────────────────────────
        // TANÁRI DOLGOZAT
        // ──────────────────────────────

        public async Task<SubjectResponse> CreateExamAsync(int userId, CreateTeacherExamRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@GroupID", request.GroupID);
                p.Add("@CreatedBy", userId);
                p.Add("@Title", request.Title);
                p.Add("@Description", request.Description);
                p.Add("@ExamDate", request.ExamDate);
                p.Add("@SubjectName", request.SubjectName);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_CreateTeacherExam", p, commandType: CommandType.StoredProcedure);

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

        public async Task<IEnumerable<TeacherExam>> GetGroupExamsAsync(int groupId, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@GroupID", groupId);
            p.Add("@UserID", userId);
            return await connection.QueryAsync<TeacherExam>(
                "sp_GetGroupExams", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<SubjectResponse> DeleteExamAsync(int examId, int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var p = new DynamicParameters();
                p.Add("@ExamID", examId);
                p.Add("@RequestBy", userId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteTeacherExam", p, commandType: CommandType.StoredProcedure);

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
