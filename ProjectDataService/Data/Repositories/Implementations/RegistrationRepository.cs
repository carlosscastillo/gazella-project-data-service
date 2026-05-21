using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectDataService.Data.Exceptions;
using ProjectDataService.Entities;
using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Data.Repositories.Implementations;

public class RegistrationRepository(GazellaDbContext context, ILogger<RegistrationRepository> logger) : IRegistrationRepository
{
    public async Task<string> CreateEnrollment(Enrollment enrollment)
    {
        try
        {
            await context.Enrollments.AddAsync(enrollment);
            await context.SaveChangesAsync();
            return enrollment.Id;
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while creating enrollment: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is DbUpdateException)
        {
            logger.LogError(ex, "DbUpdate exception while creating enrollment: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task<Enrollment?> GetTrackedEnrollment(string projectId, string volunteerId)
    {
        try
        {
            return await context.Enrollments
                .FirstOrDefaultAsync(e => e.ProjectId == projectId && e.VolunteerId == volunteerId);
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting enrollment: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task UpdateEnrollment(Enrollment enrollment)
    {
        try
        {
            context.Enrollments.Update(enrollment);
            await context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while updating enrollment: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException)
        {
            logger.LogError(ex, "Concurrency exception while updating enrollment: {Ex}", ex.Message);
            throw new GazellaConcurrencyException(ex.Message);
        }
    }

    public async Task<List<Enrollment>> GetMyEnrollments(string volunteerId)
    {
        try
        {
            return await context.Enrollments
                .Where(e => e.VolunteerId == volunteerId)
                .Include(e => e.Project)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting enrollments: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }
}