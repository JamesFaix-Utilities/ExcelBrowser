﻿using System.Runtime.InteropServices;
using xlApp = Microsoft.Office.Interop.Excel.Application;
using System.Diagnostics;

namespace ExcelBrowser.Interop {

    public static class ApplicationExtensionMethods {

        public static Session Session(this xlApp app) {
            Requires.NotNull(app, nameof(app));
            using (var process = app.AsProcess()) {
                return new Session(process.SessionId);
            }
        }

        public static bool IsActive(this xlApp app) {
            Requires.NotNull(app, nameof(app));
            return Equals(app, app.Session().TopMost);
        }

        public static void Activate(this xlApp app) {
            Requires.NotNull(app, nameof(app));
            using (var process = app.AsProcess()) {
                NativeMethods.BringToFront(process);
            }
        }

        public static bool IsVisible(this xlApp app) {
            Requires.NotNull(app, nameof(app));
            try {
                using (var process = app.AsProcess()) {
                    return app.Visible && process.IsVisible();
                }
            }
            catch (COMException x)
            when (x.Message.StartsWith("The message filter indicated that the application is busy.")
                || x.Message.StartsWith("Call was rejected by callee.")) {
                //This means the application is in a state that does not permit COM automation.
                //Often, this is due to a dialog window or right-click context menu being open.
                return false;
            }
        }

        public static string VersionName(this xlApp app) {
            Requires.NotNull(app, nameof(app));
            try {
                var version = (int)float.Parse(app.Version);
                switch (version) {
                    case 5: return "Excel 5";
                    case 6: return "Excel 6";
                    case 7: return "Excel 95";
                    case 8: return "Excel 97";
                    case 9: return "Excel 2000";
                    case 10: return "Excel 2002";
                    case 11: return "Excel 2003";
                    case 12: return "Excel 2007";
                    case 14: return "Excel 2010";
                    case 15: return "Excel 2013";
                    case 16: return "Excel 2016";
                    default: return "Excel (Unknown version)";
                }
            }
            catch {
                return "Excel (Unknown version)";
            }
        }

        /// <summary>
        /// Gets the Windows Process associated with the given Excel instance.
        /// </summary>
        /// <param name="app">The application.</param>
        public static Process AsProcess(this xlApp app) {
            Requires.NotNull(app, nameof(app));

            var mainWindowHandle = app.Hwnd;
            var processId = NativeMethods.ProcessIdFromWindowHandle(mainWindowHandle);
            return Process.GetProcessById(processId);
        }
    }
}
