namespace ProjectDataService.Data;

public static class EntitySizeConstraints
{
    public static readonly int IdLength = 36;

    public static readonly int ProjectTitleMaxLength = 128;
    public static readonly int ProjectDescriptionMaxLength = 2000;
    public static readonly int ProjectCoverUriMaxLength = 256;
    public static readonly int ProjectLocationMaxLength = 256;
    public static readonly int ProjectCategoryMaxLength = 64;

    public static readonly int OrganizerIdMaxLength = 36;
    public static readonly int OrganizerNameMaxLength = 128;
    public static readonly int OrganizerPfpUriMaxLength = 256;

    public static readonly int VolunteerIdMaxLength = 36;
    public static readonly int VolunteerFullNameMaxLength = 128;
    public static readonly int VolunteerEmailMaxLength = 256;

    public static readonly int CategoryNameMaxLength = 64;
}