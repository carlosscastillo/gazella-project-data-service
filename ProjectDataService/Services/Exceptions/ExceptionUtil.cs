using System.Text;

namespace ProjectDataService.Services.Exceptions;

public static class ExceptionUtil
{
    public static string IssueStringify(List<string> issues)
    {
        StringBuilder bld = new();
        var finalIndex = issues.Count - 1;

        for (var i = 0; i < issues.Count; i++)
        {
            if (i != finalIndex)
                bld.Append($"issue: {issues[i]}, ");
            else
                bld.Append($"issue: {issues[i]}");
        }

        return bld.ToString();
    }
}