using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleEntry>> GetMyScheduleAsync(int studentId);
        Task<SubjectResponse> AddScheduleEntryAsync(int studentId, AddScheduleRequest request);
        Task<SubjectResponse> DeleteScheduleEntryAsync(int entryId, int studentId);
    }
}
