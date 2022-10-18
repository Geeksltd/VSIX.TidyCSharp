// Guids.cs
// MUST match guids.h

namespace TidyCSharp.Cli.CommandConstants;

internal static class GuidList
{
    public const string GuidGeeksProductivityToolsPkgString = "c6176957-c61c-4beb-8dd8-e7c0170b0bf3";

    private const string GuidCleanupCmdSetString = "53366ba1-1788-42c8-922a-034d6dc89b12";
    //

    public static readonly Guid GuidCleanupCmdSet = new(GuidCleanupCmdSetString);
};