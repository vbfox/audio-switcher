// -----------------------------------------------------------------------
// Copyright (c) David Kean and Abdallah Gomah.
// -----------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using PInvoke;
using static PInvoke.Kernel32;

namespace AudioSwitcher.Presentation.Drawing
{
    /// <summary>
    /// Represents a resource name (either integer resource or string resource).
    /// </summary>
    internal unsafe class ResourceName : IDisposable
    {
        private readonly int? _id;
        private readonly string _name;
        private char* _value;

        /// <summary>
        /// Initializes a new AudioSwitcher.Presentation.Drawing.ResourceName object.
        /// </summary>
        /// <param name="lpName">Specifies the resource name. For more ifnormation, see the Remarks section.</param>
        /// <remarks>
        /// If the high bit of lpszName is not set (=0), lpszName specifies the integer identifier of the givin resource.
        /// Otherwise, it is a pointer to a null terminated string.
        /// If the first character of the string is a pound sign (#), the remaining characters represent a decimal number that specifies the integer identifier of the resource. For example, the string "#258" represents the identifier 258.
        /// #define IS_INTRESOURCE(_r) ((((ULONG_PTR)(_r)) >> 16) == 0).
        /// </remarks>
        public ResourceName(char* lpName)
        {
            if (IS_INTRESOURCE(lpName))  //Integer resource
            {
                _id = (int)lpName;
                _name = null;
            }
            else
            {
                _id = null;
                _name = Marshal.PtrToStringAuto((IntPtr)lpName);
            }
        }

        /// <summary>
        /// Gets the resource identifier, returns null if the resource is not an integer resource.
        /// </summary>
        public int? Id
        {
            get { return _id; }
        }

        
        /// <summary>
        /// Gets the resource name, returns null if the resource is not a string resource.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        
        /// <summary>
        /// Gets a pointer to resource name that can be used in FindResource function.
        /// </summary>
        public char* Value
        {
            get
            {
                if (Id != null)
                    return MAKEINTRESOURCE(Id.Value);

                if (_value == null)
                    _value = (char*)Marshal.StringToHGlobalAuto(Name);

                return _value;
            }
        }

        /// <summary>
        /// Gets whether the resource is an integer resource.
        /// </summary>
        public bool IsIntResource
        {
            get { return (Id != null); }
        }


        /// <summary>
        /// Destructs the ResourceName object.
        /// </summary>
        ~ResourceName()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns a System.String that represents the current AudioSwitcher.Presentation.Drawing.ResourceName.
        /// </summary>
        /// <returns>Returns a System.String that represents the current AudioSwitcher.Presentation.Drawing.ResourceName.</returns>
        public override string ToString()
        {
            if (IsIntResource)
                return "#" + Id.ToString();

            return Name;
        }
        
        /// <summary>
        /// Release the pointer to the resource name.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_value != null)
            {
                try 
                { 
                    Marshal.FreeHGlobal((IntPtr)_value); 
                }
                finally
                {
                    _value = null;
                }
            }
        }
    }
}
