﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using ExcelBrowser.Interop;
using ExcelBrowser.Model;
using ExcelBrowser.Monitoring;

namespace ExcelBrowser.Controller {

    public sealed class SessionMonitor : IDisposable, INotifyPropertyChanged {

        public SessionMonitor(double refreshSeconds = 0.05) {
            Requires.Positive(refreshSeconds, nameof(refreshSeconds));
            Debug.WriteLine($"{nameof(SessionMonitor)}.{nameof(SessionMonitor)}");

            this.session = Interop.Session.Current;
            this.Session = TokenFactory.Session(session);

            this.detector = new ChangeDetector<SessionToken>(
                getValue: () => TokenFactory.Session(this.session),
                refreshSeconds: refreshSeconds);

            detector.Changed += DetectorChanged;
        }

        private readonly Session session;
        private readonly ChangeDetector<SessionToken> detector;

        public SessionToken Session { get; private set; }

        public string SessionSerialized => Session.ToString();

        private void DetectorChanged(object sender, EventArgs<ValueChange<SessionToken>> e) {
            Debug.WriteLine($"{nameof(SessionMonitor)}.{nameof(DetectorChanged)}");
            var change = e.Value;
            var modelChanges = SessionChangeAnalyzer.FindChanges(change);
            if (modelChanges.Any()) {
                Session = change.NewValue;
                OnSessionChanged(modelChanges);
            }
        }

        public event EventHandler<EventArgs<IEnumerable<ModelChange>>> SessionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnSessionChanged(IEnumerable<ModelChange> changes) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SessionSerialized"));
            SessionChanged?.Invoke(this, new EventArgs<IEnumerable<ModelChange>>(changes));
        }

        public void Dispose() {
            detector.Dispose();
        }
    }
}
