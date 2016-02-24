﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean. All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using static PInvoke.User32;
using static PInvoke.Gdi32;

namespace AudioSwitcher.Presentation.UI
{
    internal static class DpiServices
    {
        private const double LogicalDpi = 96.0;
        private static readonly Lazy<SizeF> _scalingFactor = new Lazy<SizeF>(CalculateScalingFactor);

        public static Size Scale(Size size)
        {
            return new Size(ScaleX(size.Width), ScaleY(size.Height));
        }

        public static int ScaleX(int value)
        {
            return (int)Math.Round(_scalingFactor.Value.Width * (float)value);
        }

        public static int ScaleY(int value)
        {
            return (int)Math.Round(_scalingFactor.Value.Height * (float)value);
        }

        private static SizeF CalculateScalingFactor()
        {
            const int LOGPIXELSX = 88;
            const int LOGPIXELSY = 90;

            using (SafeDCHandle primaryMonitorDC = GetDC(IntPtr.Zero))
            {
                int deviceDpiX = GetDeviceCaps(primaryMonitorDC, LOGPIXELSX);
                int deviceDpiY = GetDeviceCaps(primaryMonitorDC, LOGPIXELSY);

                double scalingFactorX = deviceDpiX/LogicalDpi;
                double scalingFactorY = deviceDpiY/LogicalDpi;

                return new SizeF((float)scalingFactorX, (float)scalingFactorY);
            }
        }
    }
}
