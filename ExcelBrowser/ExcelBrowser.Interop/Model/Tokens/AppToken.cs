﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using xlApp = Microsoft.Office.Interop.Excel.Application;
using xlBook = Microsoft.Office.Interop.Excel.Workbook;
using xlWin = Microsoft.Office.Interop.Excel.Window;
using System.Runtime.InteropServices;
using System.Diagnostics;

#pragma warning disable CS0659 //Does not need to override GetHashCode because base class implementation is sufficient.

namespace ExcelBrowser.Model {

    /// <summary>
    /// Represents a snapshot of an Excel application instance.
    /// </summary>
    public class AppToken : Token<AppId> {

        public AppToken(xlApp app) : this(app?.Id()) {
            //    Debug.WriteLine("AppToken.Constructor");

            try {
                //Test if reachable
                IsReachable = true;
                IsVisible = app.Visible;

                Books = app.Workbooks.OfType<xlBook>()
                    .Select(wb => new BookToken(wb))
                    .ToImmutableArray();

                xlBook activeBook = app.ActiveWorkbook;
                if (activeBook != null) {
                    var id = activeBook.Id();
                    ActiveBook = Books.Single(b => Equals(b.Id, id));
                }

                xlWin activeWindow = app.ActiveWindow;
                if (activeWindow != null) {
                    var id = activeWindow.Id();
                    ActiveWindow = Books.Single(b => Equals(b.Id.BookName, id.BookName))
                        .Windows.Single(w => Equals(w.Id, id));
                }
            }
            catch (COMException x) 
            when (x.Message.StartsWith("The message filter indicated that the application is busy.")) {
                Debug.WriteLine($"Busy @ {Id}");
                IsReachable = false;
                IsVisible = false;
                Books = null;
                ActiveBook = null;
                ActiveWindow = null;
            }            
        }

        private AppToken(AppId id) : base(id) { }

        public static AppToken Unreachable(int processId) {
            return new AppToken(new AppId(processId)) {
                Books = new BookToken[0].ToImmutableArray()
            };
        }

        public bool IsVisible { get; }
        public bool IsReachable { get; }

        public IEnumerable<BookToken> Books { get; private set; }

        public BookToken ActiveBook { get; }

        public WindowToken ActiveWindow { get; }

        #region Equality

        public bool Equals(AppToken other) => base.Equals(other)
            && IsReachable == other.IsReachable
            && IsVisible == other.IsVisible
            && Books.SequenceEqual(other.Books)
            && Equals(ActiveBook, other.ActiveBook)
            && Equals(ActiveWindow, other.ActiveWindow);

        public override bool Equals(object obj) => Equals(obj as AppToken);

        public bool Matches(AppToken other) => base.Equals(other);

        #endregion
    }
}
