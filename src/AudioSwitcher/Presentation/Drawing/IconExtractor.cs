// -----------------------------------------------------------------------
// Copyright (c) David Kean and Abdallah Gomah.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using AudioSwitcher.IO;
using AudioSwitcher.Presentation.Drawing.Interop;
using AudioSwitcher.Win32.InteropServices;
using PInvoke;
using static PInvoke.Kernel32;

namespace AudioSwitcher.Presentation.Drawing
{
    /// <summary>
    /// Get icon resources (RT_GROUP_ICON and RT_ICON) from an executable module (either a .dll or an .exe file).
    /// </summary>
    internal class IconExtractor : IDisposable
    {
        private readonly ReadOnlyCollection<ResourceName> _iconNames;
        private readonly SafeLibraryHandle _moduleHandle;

        public IconExtractor(SafeLibraryHandle moduleHandle, IList<ResourceName> iconNames)
        {
            _moduleHandle = moduleHandle;
            _iconNames = new ReadOnlyCollection<ResourceName>(iconNames);
        }

        public SafeLibraryHandle ModuleHandle
        {
            get { return _moduleHandle; }
        }
        
        /// <summary>
        /// Gets a list of icons resource names RT_GROUP_ICON;
        /// </summary>
        public ReadOnlyCollection<ResourceName> IconNames
        {
            get { return _iconNames; }
        }

        public Icon GetIconByIndex(int index)
        {
            if (index < 0 || index >= IconNames.Count)
            {
                if (IconNames.Count > 0)
                    throw new ArgumentOutOfRangeException("index", index, "Index should be in the range (0-" + IconNames.Count.ToString() + ").");
                else
                    throw new ArgumentOutOfRangeException("index", index, "No icons in the list.");
            }

            return GetIconFromLib(index);
        }

        public static unsafe IconExtractor Open(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            if (fileName.Length == 0)
                throw new ArgumentException(null, "fileName");

            fileName = Path.GetFullPath(fileName);
            fileName = Environment.ExpandEnvironmentVariables(fileName);

            SafeLibraryHandle moduleHandle = LoadLibraryEx(fileName, IntPtr.Zero, Kernel32.LoadLibraryExFlags.LOAD_LIBRARY_AS_DATAFILE);
            if (moduleHandle.IsInvalid)
                throw Win32Marshal.GetExceptionForLastWin32Error(fileName);

            List<ResourceName> iconNames = new List<ResourceName>();
            EnumResourceNames(moduleHandle, RT_GROUP_ICON, (hModule, lpszType, lpszName, lParam) =>
                {
                    if (lpszType == RT_GROUP_ICON)
                        iconNames.Add(new ResourceName(lpszName));

                    return true;
                },
                IntPtr.Zero);


            return new IconExtractor(moduleHandle, iconNames);
        }

        public static Icon ExtractIconByIndex(string fileName, int index)
        {
            using (IconExtractor extractor = IconExtractor.Open(fileName))
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");
            
                if (index >= extractor.IconNames.Count)
                    return null;

                return extractor.GetIconByIndex(index);
            }
        }

        public static Icon ExtractIconById(string fileName, int id)
        {
            using (IconExtractor extractor = IconExtractor.Open(fileName))
            {
                if (id < 0)
                    throw new ArgumentOutOfRangeException("index");

                int count = extractor.IconNames.Count;
                for (int i = 0; i < count; i++)
                {
                    ResourceName name = extractor.IconNames[i];
                    if (name.Id != null && name.Id == id)
                    {
                        return extractor.GetIconByIndex(i);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a System.Drawing.Icon that represents RT_GROUP_ICON at the givin index from the executable module.
        /// </summary>
        /// <param name="index">The index of the RT_GROUP_ICON in the executable module.</param>
        /// <returns>Returns System.Drawing.Icon.</returns>
        private unsafe Icon GetIconFromLib(int index)
        {
            //Convert the resouce into an .ico file image.
            using (UnmanagedMemoryStream inputStream = GetResourceData(this.ModuleHandle, this.IconNames[index], RT_GROUP_ICON))
            using (MemoryStream destStream = new MemoryStream())
            {
                //Read the GroupIconDir header.
                GRPICONDIR grpDir = inputStream.Read<GRPICONDIR>();

                ushort numEntries = grpDir.idCount;
                uint iconImageOffset = (uint)(IconInfo.SizeOfIconDir + numEntries * IconInfo.SizeOfIconDirEntry);

                destStream.Write<ICONDIR>(grpDir.ToIconDir());
                for (int i = 0; i < numEntries; i++)
                {
                    //Read the GroupIconDirEntry.
                    GRPICONDIRENTRY grpEntry = inputStream.Read<GRPICONDIRENTRY>();

                    //Write the IconDirEntry.
                    destStream.Seek(IconInfo.SizeOfIconDir + i * IconInfo.SizeOfIconDirEntry, SeekOrigin.Begin);
                    destStream.Write<ICONDIRENTRY>(grpEntry.ToIconDirEntry(iconImageOffset));

                    //Get the icon image raw data and write it to the stream.
                    using (UnmanagedMemoryStream imgBuf = GetResourceData(this.ModuleHandle, grpEntry.nId, RT_ICON))
                    {
                        destStream.Seek(iconImageOffset, SeekOrigin.Begin);
                        imgBuf.CopyTo(destStream);

                        //Append the iconImageOffset.
                        iconImageOffset += (uint)imgBuf.Length;
                    }
                }
                destStream.Seek(0, SeekOrigin.Begin);
                return new Icon(destStream);
            }
        }
        /// <summary>
        /// Extracts the raw data of the resource from the module.
        /// </summary>
        /// <param name="hModule">The module handle.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <returns>The resource raw data.</returns>
        private static unsafe UnmanagedMemoryStream GetResourceData(SafeLibraryHandle hModule, ResourceName resourceName, char* resourceType)
        {
            //Find the resource in the module.
            IntPtr hResInfo;
            try { hResInfo = FindResource(hModule, resourceName.Value, resourceType); }
            finally { resourceName.Dispose(); }
            if (hResInfo == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Load the resource.
            IntPtr hResData = LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Lock the resource to read data.
            void* hGlobal = LockResource(hResData);
            if (hGlobal == null)
            {
                throw new Win32Exception();
            }
            //Get the resource size.
            int resSize = SizeofResource(hModule, hResInfo);
            if (resSize == 0)
            {
                throw new Win32Exception();
            }
            
            return new UnmanagedMemoryStream((byte*)hGlobal, resSize);
        }
        /// <summary>
        /// Extracts the raw data of the resource from the module.
        /// </summary>
        /// <param name="hModule">The module handle.</param>
        /// <param name="resourceId">The identifier of the resource.</param>
        /// <param name="resourceType">The type of the resource.</param>
        /// <returns>The resource raw data.</returns>
        private static unsafe UnmanagedMemoryStream GetResourceData(SafeLibraryHandle hModule, int resourceId, char* resourceType)
        {
            //Find the resource in the module.
            IntPtr hResInfo = FindResource(hModule, MAKEINTRESOURCE(resourceId), resourceType); 
            if (hResInfo == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Load the resource.
            IntPtr hResData = LoadResource(hModule, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
            //Lock the resource to read data.
            void* hGlobal = LockResource(hResData);
            if (hGlobal == null)
            {
                throw new Win32Exception();
            }
            //Get the resource size.
            int resSize = SizeofResource(hModule, hResInfo);
            if (resSize == 0)
            {
                throw new Win32Exception();
            }
            
            return new UnmanagedMemoryStream((byte*)hGlobal, resSize);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _moduleHandle.Dispose();
            }
        }
    }
}
