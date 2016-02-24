// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using PInvoke;
using static PInvoke.HResult.Code;
using static PInvoke.Win32ErrorCode;

namespace AudioSwitcher.Interop
{
    internal static class HResults
    {
        public static readonly HResult OK = S_OK;
        public static readonly HResult NotFound = ERROR_NOT_FOUND.ToHResult();
        public static readonly HResult FileNotFound = ERROR_FILE_NOT_FOUND.ToHResult();
    }
}