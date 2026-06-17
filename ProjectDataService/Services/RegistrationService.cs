using Grpc.Core;
using ProjectDataService.Data.Repositories;
using ProjectDataService.Entities;
using ProjectDataService.Protos;
using ProjectDataService.Services.Exceptions;
using ProjectDataService.Services.MessageValidators;

namespace ProjectDataService.Services;

public class RegistrationService(
    IRegistrationRepository registrationRepository,
    IProjectRepository projectRepository) : Protos.RegistrationService.RegistrationServiceBase
{
    public override async Task<SignUpResponse> SignUp(
        SignUpRequest request, ServerCallContext context)
    {
        RegistrationValidator.ValidateSignUp(request);

        var project = await projectRepository.GetTrackedProject(request.ProjectId);

        if (project is null)
            throw new GazellaNotFoundException($"No project was found for id: {request.ProjectId}");

        if (project.Status != ProjectStatus.Active)
            throw new GazellaInvalidOperationException("You can only sign up for active projects");

        if (project.EnrolledCount >= project.MaxVolunteers)
            throw new GazellaInvalidOperationException("This project has reached its maximum volunteers capacity");

        var existingEnrollment = await registrationRepository.GetTrackedEnrollment(
            request.ProjectId, request.VolunteerId);

        if (existingEnrollment is not null && existingEnrollment.Status == EnrollmentStatus.Confirmed)
            throw new GazellaInvalidOperationException("You are already enrolled in this project");

        if (existingEnrollment is not null && existingEnrollment.Status == EnrollmentStatus.Cancelled)
        {
            existingEnrollment.Status = EnrollmentStatus.Confirmed;
            existingEnrollment.EnrolledAt.GetType();
            await registrationRepository.UpdateEnrollment(existingEnrollment);
            project.EnrolledCount++;
            await projectRepository.UpdateProject(project);

            return new SignUpResponse
            {
                EnrollmentId = existingEnrollment.Id,
                Message = "Successfully signed up for the project"
            };
        }

        var enrollment = new Enrollment
        {
            ProjectId = request.ProjectId,
            VolunteerId = request.VolunteerId,
            VolunteerFullName = request.VolunteerFullName,
            VolunteerEmail = request.VolunteerEmail
        };

        project.EnrolledCount++;
        await projectRepository.UpdateProject(project);
        var enrollmentId = await registrationRepository.CreateEnrollment(enrollment);

        return new SignUpResponse
        {
            EnrollmentId = enrollmentId,
            Message = "Successfully signed up for the project"
        };
    }

    public override async Task<CancelRegistrationResponse> CancelRegistration(
        CancelRegistrationRequest request, ServerCallContext context)
    {
        RegistrationValidator.ValidateCancelRegistration(request);

        var enrollment = await registrationRepository.GetTrackedEnrollment(
            request.ProjectId, request.VolunteerId);

        if (enrollment is null)
            throw new GazellaNotFoundException("No enrollment was found for this project and volunteer");

        if (enrollment.Status == EnrollmentStatus.Cancelled)
            throw new GazellaInvalidOperationException("This enrollment is already cancelled");

        enrollment.Status = EnrollmentStatus.Cancelled;
        await registrationRepository.UpdateEnrollment(enrollment);

        var project = await projectRepository.GetTrackedProject(request.ProjectId);
        if (project is not null && project.EnrolledCount > 0)
        {
            project.EnrolledCount--;
            await projectRepository.UpdateProject(project);
        }

        return new CancelRegistrationResponse
        {
            EnrollmentStatus = enrollment.Status.ToString(),
            Message = "Registration cancelled successfully"
        };
    }

    public override async Task<GetMyEnrollmentsResponse> GetMyEnrollments(
        GetMyEnrollmentsRequest request, ServerCallContext context)
    {
        GeneralValidator.ValidateId(request.VolunteerId, "volunteer_id");

        var enrollments = await registrationRepository.GetMyEnrollments(request.VolunteerId);

        var response = new GetMyEnrollmentsResponse();
        response.Enrollments.AddRange(enrollments.Select(e => new MyEnrollment
        {
            ProjectId = e.ProjectId,
            ProjectTitle = e.Project?.Title ?? string.Empty,
            Location = e.Project?.Location ?? string.Empty,
            StartDate = e.Project?.StartDate.ToString("yyyy-MM-dd") ?? string.Empty,
            ProjectStatus = e.Project?.Status.ToString() ?? string.Empty,
            EnrollmentStatus = e.Status.ToString(),
            EnrolledAt = e.EnrolledAt.ToString("o"),
            CoverUri = e.Project?.CoverUri ?? string.Empty
        }));

        return response;
    }
}