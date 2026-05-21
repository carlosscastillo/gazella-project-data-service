using ProjectDataService.Entities;

namespace ProjectDataService.Data.Repositories;

public interface IRegistrationRepository
{
    Task<string> CreateEnrollment(Enrollment enrollment);
    Task<Enrollment?> GetTrackedEnrollment(string projectId, string volunteerId);
    Task UpdateEnrollment(Enrollment enrollment);
    Task<List<Enrollment>> GetMyEnrollments(string volunteerId);
}