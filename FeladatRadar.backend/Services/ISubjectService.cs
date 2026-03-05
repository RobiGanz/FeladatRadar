using FeladatRadar.backend.Models;

namespace FeladatRadar.backend.Services
{
    public interface ISubjectService
    {
        // Összes felvehető tantárgy listázása (aktív, van szabad hely)
        Task<IEnumerable<Subject>> GetAvailableSubjectsAsync(int studentId);

        // Egy hallgató által már felvett tantárgyak
        Task<IEnumerable<Enrollment>> GetMyEnrollmentsAsync(int studentId);

        // Tantárgy felvétele
        Task<SubjectResponse> EnrollAsync(int studentId, int subjectId);

        // Tantárgy leadása
        Task<SubjectResponse> DropAsync(int studentId, int subjectId);

        // Egyedi tantárgy felvétele (ami nem szerepel a listában)
        Task<SubjectResponse> EnrollCustomAsync(int studentId, CustomEnrollRequest request);

        // Összes tantárgy (tanároknak / adminnak)
        Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    }
}
