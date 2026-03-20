using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface ITeacherService
    {
        // Szavazás
        Task<SubjectResponse> CreatePollAsync(int userId, CreatePollRequest request);
        Task<IEnumerable<Poll>> GetGroupPollsAsync(int groupId, int userId);
        Task<SubjectResponse> VoteAsync(int pollId, int optionId, int userId);
        Task<SubjectResponse> DeletePollAsync(int pollId, int userId);

        // Tanári dolgozat
        Task<SubjectResponse> CreateExamAsync(int userId, CreateTeacherExamRequest request);
        Task<IEnumerable<TeacherExam>> GetGroupExamsAsync(int groupId, int userId);
        Task<SubjectResponse> DeleteExamAsync(int examId, int userId);
    }

}
