using FeladatRadar.backend.Models;


namespace FeladatRadar.backend.Services
{
    public interface IGroupService
    {
        Task<SubjectResponse> CreateGroupAsync(int userId, string groupName);
        Task<SubjectResponse> InviteToGroupAsync(int groupId, int invitedBy, string invitedEmail);
        Task<SubjectResponse> AcceptInviteAsync(int inviteId, int userId);
        Task<SubjectResponse> DeclineInviteAsync(int inviteId, int userId);
        Task<SubjectResponse> LeaveGroupAsync(int groupId, int userId);
        Task<IEnumerable<Group>> GetMyGroupsAsync(int userId);
        Task<IEnumerable<GroupMember>> GetGroupMembersAsync(int groupId, int userId);
        Task<IEnumerable<GroupSubject>> GetGroupSubjectsAsync(int groupId, int userId);
        Task<IEnumerable<GroupScheduleEntry>> GetGroupScheduleAsync(int groupId, int userId, string userRole = "Student");
        Task<IEnumerable<GroupTask>> GetGroupTasksAsync(int groupId, int userId);
        Task<SubjectResponse> AddGroupTaskAsync(int groupId, int userId, AddGroupTaskRequest request);
        Task<IEnumerable<GroupInvite>> GetMyInvitesAsync(string email);
        Task<SubjectResponse> AddGroupScheduleEntryAsync(int groupId, int userId, AddScheduleRequest request, string userRole = "Teacher");
        Task<bool> CanManageGroupAsync(int groupId, int userId);
        Task<bool> IsGroupMemberAsync(int groupId, int userId);
        Task<SubjectResponse> DeleteGroupTaskAsync(int groupId, int taskId, int userId);
        Task<SubjectResponse> DeleteGroupScheduleEntryAsync(int groupId, int entryId, int userId);
    }


}
