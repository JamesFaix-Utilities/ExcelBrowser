﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using xlApp = Microsoft.Office.Interop.Excel.Application;

namespace ExcelBrowser.Interop {

    public static class AppFactory {

        /// <summary>
        /// Gets the Windows Process associated with the given Excel instance.
        /// </summary>
        /// <param name="app">The application.</param>
        public static Process AsProcess(this xlApp app) {
            Requires.NotNull(app, nameof(app));

            var mainWindowHandle = app.Hwnd;
            var processId = NativeMethods.ProcessIdFromWindowHandle(mainWindowHandle);
            var process = Process.GetProcessById(processId);

            return process;
        }

        /// <summary>
        /// Gets the Excel instance running in the given process, or null if none exists.
        /// </summary>
        /// <param name="process">The process.</param>
        public static xlApp AsExcelApp(this Process process) {
            Requires.NotNull(process, nameof(process));

            var handle = process.MainWindowHandle.ToInt32();
            var result = FromMainWindowHandle(handle);
            //Debug.Assert(result != null);
            return result;
        }

        /// <summary>
        /// Gets the Excel instance running in the process that has the given ID, or null if none exists.
        /// </summary>
        /// <param name="processId">The process identifier.</param>
        public static xlApp FromProcessId(int processId) =>
            Process.GetProcessById(processId)?.AsExcelApp();

        /// <summary>
        /// Gets the Excel instance whose main window has the given handle, or null if none exists.
        /// </summary>
        /// <param name="handle">The handle.</param>
        public static xlApp FromMainWindowHandle(int handle) =>
            NativeMethods.AppFromMainWindowHandle(handle);

        public static xlApp PrimaryInstance {
            get {
                try {
                    return (xlApp)Marshal.GetActiveObject("Excel.Application");
                }
                catch (COMException x)
                when (x.Message.StartsWith("Operation unavailable")) {
                    Debug.WriteLine("AppFactory: Primary instance unavailable.");
                    return null;
                }
            }
        }
    }
}