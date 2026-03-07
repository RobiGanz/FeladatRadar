using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItem>> GetMyTasksAsync(int studentId);
        Task<SubjectResponse> AddTaskAsync(int studentId, AddTaskRequest request);
        Task<SubjectResponse> CompleteTaskAsync(int taskId, int studentId);
        Task<SubjectResponse> UncompleteTaskAsync(int taskId, int studentId);
        Task<SubjectResponse> DeleteTaskAsync(int taskId, int studentId);
    }
}
