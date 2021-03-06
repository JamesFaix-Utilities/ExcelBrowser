﻿using System.Collections.ObjectModel;

namespace ExcelBrowser.ViewModels {

    public class BookWindowsViewModel {

        public BookWindowsViewModel() {
            Windows = new ObservableCollection<BookWindowViewModel>();
        }

        public ObservableCollection<BookWindowViewModel> Windows { get; }

        public double Height => (Windows.Count > 1) ? 20 : 0;
    }
}
