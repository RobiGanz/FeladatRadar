using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface IAdminService
    {
        // Felhasználókezelés
        Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
        Task<SubjectResponse> ChangeUserRoleAsync(int adminUserId, int targetUserId, string newRole);
        Task<SubjectResponse> ToggleUserActiveAsync(int adminUserId, int targetUserId, bool isActive);
        Task<SubjectResponse> DeleteUserAsync(int adminUserId, int targetUserId);

        // Rendszer statisztikák
        Task<SystemStatsDto> GetSystemStatsAsync();

        // Összes csoport
        Task<IEnumerable<AdminGroupDto>> GetAllGroupsAsync();
        Task<SubjectResponse> DeleteGroupAsync(int adminUserId, int groupId);

        // Audit log
        Task<IEnumerable<AuditLogEntry>> GetAuditLogAsync(int limit = 100);
        Task WriteAuditLogAsync(int userId, string action, string? details = null);

        // Moderáció
        Task<SubjectResponse> AdminDeletePollAsync(int adminUserId, int pollId);
        Task<SubjectResponse> AdminDeleteExamAsync(int adminUserId, int examId);
        Task<SubjectResponse> AdminDeleteTaskAsync(int adminUserId, int taskId);
    }

}
