using Dapper;
using FeladatRadar.backend.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeladatRadar.backend.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly string _connectionString;

        public SubjectService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        // Felvehető tantárgyak: aktív, van szabad hely, a hallgató még nem vette fel
        public async Task<IEnumerable<Subject>> GetAvailableSubjectsAsync(int studentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@StudentID", studentId);

            return await connection.QueryAsync<Subject>(
                "sp_GetAvailableSubjects",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        // A bejelentkezett Felhasználó által felvett órák
        public async Task<IEnumerable<Enrollment>> GetMyEnrollmentsAsync(int studentId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@StudentID", studentId);

            return await connection.QueryAsync<Enrollment>(
                "sp_GetMyEnrollments",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        // Tantárgy felvétele
        public async Task<SubjectResponse> EnrollAsync(int studentId, int subjectId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SubjectID", subjectId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_EnrollSubject",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba történt." };

                var dict = (IDictionary<string, object>)result;
                return new SubjectResponse
                {
                    Status = dict["Status"]?.ToString() ?? "ERROR",
                    Message = dict["Message"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        // Tantárgy leadása
        public async Task<SubjectResponse> DropAsync(int studentId, int subjectId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SubjectID", subjectId);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DropSubject",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba történt." };

                var dict = (IDictionary<string, object>)result;
                return new SubjectResponse
                {
                    Status = dict["Status"]?.ToString() ?? "ERROR",
                    Message = dict["Message"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }

        // Összes tantárgy (admin/tanár nézethez)
        public async Task<IEnumerable<Subject>> GetAllSubjectsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Subject>(
                "sp_GetAllSubjects",
                commandType: CommandType.StoredProcedure
            );
        }

        // Egyedi (nem listában szereplő) tantárgy felvétele
        // Létrehozza az új tantárgyat az adatbázisban, majd rögtön felveszi.
        public async Task<SubjectResponse> EnrollCustomAsync(int studentId, CustomEnrollRequest request)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@StudentID", studentId);
                parameters.Add("@SubjectName", request.SubjectName.Trim());
                parameters.Add("@SubjectCode", string.IsNullOrWhiteSpace(request.SubjectCode)
                    ? "EGYEDI"
                    : request.SubjectCode.Trim().ToUpper());

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_EnrollCustomSubject",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                if (result == null)
                    return new SubjectResponse { Status = "ERROR", Message = "Ismeretlen hiba történt." };

                var dict = (IDictionary<string, object>)result;
                return new SubjectResponse
                {
                    Status = dict["Status"]?.ToString() ?? "ERROR",
                    Message = dict["Message"]?.ToString() ?? ""
                };
            }
            catch (Exception ex)
            {
                return new SubjectResponse { Status = "ERROR", Message = $"Hiba: {ex.Message}" };
            }
        }
    }
}