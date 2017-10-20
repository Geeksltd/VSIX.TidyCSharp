// Guids.cs
// MUST match guids.h
using System;

namespace Geeks.GeeksProductivityTools
{
    static class GuidList
    {
        public const string GuidGeeksProductivityToolsPkgString = "c6176957-c61c-4beb-8dd8-e7c0170b0bf3";

        const string guidCleanupCmdSetString = "53366ba1-1788-42c8-922a-034d6dc89b12";
        //

        public static readonly Guid GuidCleanupCmdSet = new Guid(guidCleanupCmdSetString);
    };
}