﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xlApp = Microsoft.Office.Interop.Excel.Application;
using System;

namespace ExcelBrowser.Interop {

    public class Session {

        public static Session Current => new Session(Process.GetCurrentProcess().SessionId);

        public Session(int sessionId) {
            Debug.WriteLine("");
            Debug.WriteLine("Session.Constructor");

            SessionId = sessionId;
        }

        private IEnumerable<Process> Processes =>
            Process.GetProcessesByName("EXCEL")
            .Where(p => p.SessionId == this.SessionId);

        private static Fallible<xlApp> TryGetApp(Process process) {
            try {
                var app = process.AsExcelApp();
                if (app == null) throw new InvalidOperationException("Cannot get App from Process.");
                return new Fallible<xlApp>(app);
            }
            catch (Exception x) {
                return new Fallible<xlApp>(x);
            }
        }

        public int SessionId { get; }

        public IEnumerable<int> ProcessIds =>
            Processes
            .Select(p => p.Id)
            .ToArray();

        public IEnumerable<int> UnreachableProcessIds =>
            ProcessIds
            .Except(Apps.Select(a => a.AsProcess().Id))
            .ToArray();

        public IEnumerable<xlApp> Apps =>
            Processes
            .Select(TryGetApp)
            .Values()
            .Where(a => a.AsProcess().IsVisible())
            .ToArray();

        public xlApp TopMost {
            get {
                var dict = Apps.ToDictionary(
                    keySelector: a => a.AsProcess(),
                    elementSelector: a => a);

                var topProcess = dict.Keys.TopMost();

                if (topProcess == null) {
                    return null;
                }
                else {
                    try {
                        return dict[topProcess];
                    }
                    catch {
                        return null;
                    }
                }
            }
        }

        public xlApp Primary => AppFactory.PrimaryInstance;
    }
}
