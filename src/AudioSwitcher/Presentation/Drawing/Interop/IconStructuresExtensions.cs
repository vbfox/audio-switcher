// -----------------------------------------------------------------------
// Copyright (c) David Kean. All rights reserved.
// -----------------------------------------------------------------------
using static PInvoke.Kernel32;

namespace AudioSwitcher.Presentation.Drawing.Interop
{
    internal static class IconStructuresExtensions
    {
        public static GRPICONDIR ToGroupIconDir(this ICONDIR iconDir)
        {
            return new GRPICONDIR
            {
                idReserved = iconDir.idReserved,
                idType = iconDir.idType,
                idCount = iconDir.idCount
            };
        }

        public static GRPICONDIRENTRY ToGroupIconDirEntry(this ICONDIRENTRY iconDirEntry, int id)
        {
            return new GRPICONDIRENTRY
            {
                bWidth = iconDirEntry.bWidth,
                bHeight = iconDirEntry.bHeight,
                bColorCount = iconDirEntry.bColorCount,
                bReserved = iconDirEntry.bReserved,
                wPlanes = iconDirEntry.wPlanes,
                wBitCount = iconDirEntry.wBitCount,
                dwBytesInRes = iconDirEntry.dwBytesInRes,
                nId = (ushort)id
            };
        }

        public static ICONDIRENTRY ToIconDirEntry(this GRPICONDIRENTRY grp, uint imageOffset)
        {
            var entry = new ICONDIRENTRY
            {
                bWidth = grp.bWidth,
                bHeight = grp.bHeight,
                bColorCount = grp.bColorCount,
                bReserved = grp.bReserved,
                wPlanes = grp.wPlanes,
                wBitCount = grp.wBitCount,
                dwBytesInRes = grp.dwBytesInRes,
                dwImageOffset = imageOffset
            };
            return entry;
        }

        public static ICONDIR ToIconDir(this GRPICONDIR grp)
        {
            return new ICONDIR
            {
                idReserved = grp.idReserved,
                idType = grp.idType,
                idCount = grp.idCount
            };
        }
    }
}
