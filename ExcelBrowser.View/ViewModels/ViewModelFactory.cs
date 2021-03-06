﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExcelBrowser.Model;

namespace ExcelBrowser.ViewModels {

    public class ViewModelFactory {

        public static SessionViewModel ConvertSession(SessionToken token) {
            Requires.NotNull(token, nameof(token));

            var result = new SessionViewModel();
            foreach (var appToken in token.Apps) {
                result.Apps.Add(ConvertApp(appToken));
            }
            return result;
        }

        public static AppViewModel ConvertApp(AppToken token) {
            Requires.NotNull(token, nameof(token));

            var result = new AppViewModel(token);
            foreach (var bookToken in token.Books) {
                result.Books.Add(ConvertBook(bookToken));
            }
            return result;
        }

        public static BookViewModel ConvertBook(BookToken token) {
            Requires.NotNull(token, nameof(token));

            var result = new BookViewModel(token) {
                Windows = GetBookWindows(token)
            };

            var activeSheets = token.Windows
                .Select(w => new ActiveSheet(w.Id.WindowIndex, w.ActiveSheetId?.SheetName))
                .ToArray();

            foreach (var sheetToken in token.Sheets) {
                result.Sheets.Add(GetSheet(sheetToken, activeSheets));
            }

            return result;
        }

        public static BookWindowsViewModel GetBookWindows(BookToken token) {
            Requires.NotNull(token, nameof(token));

            var result = new BookWindowsViewModel();
            foreach (var windowToken in token.Windows) {
                result.Windows.Add(ConvertWindow(windowToken));
            }
            return result;
        }

        public static BookWindowViewModel ConvertWindow(WindowToken token) {
            Requires.NotNull(token, nameof(token));
            return new BookWindowViewModel(token);
        }

        private static SheetViewModel GetSheet(SheetToken token, IEnumerable<ActiveSheet> activeSheets) {
            var result = new SheetViewModel(token) {
                TabColor = token.TabColor
            };
            var i = 1;
            foreach (var a in activeSheets) {
                var win = new SheetWindowViewModel(token, i) {
                    IsActive = token.Id.SheetName == a.SheetName,
                };
                result.Windows.Add(win);
                i++;
            }
            return result;
        }

        private class ActiveSheet {
            public ActiveSheet(int windowIndex, string sheetName) {
                Debug.Assert(sheetName != null);
                WindowIndex = windowIndex;
                SheetName = sheetName;
            }

            public int WindowIndex { get; }
            public string SheetName { get; }
        }
    }
}
