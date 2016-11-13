﻿using System;
using System.Runtime.Serialization;
using xlWin = Microsoft.Office.Interop.Excel.Window;

#pragma warning disable CS0659 //Does not need to override GetHashCode because base class implementation is sufficient.

namespace ExcelBrowser.Model {

    /// <summary>
    /// Represents a snapshot of an Excel window.
    /// </summary>
    [DataContract]
    public class WindowToken : Token<WindowId>, IEquatable<WindowToken> {

        public WindowToken(xlWin window) : base(window?.Id()) {
            //  Debug.WriteLine("WindowToken.Constructor");
            State = window.WindowState.Outer();
            IsVisible = window.Visible;
        }

        [DataMember(Order = 1)]
        public bool IsVisible { get; }

        [DataMember(Order = 2)]
        public WindowState State { get; }

        #region Equality

        public bool Matches(WindowToken other) => base.Equals(other);

        public bool Equals(WindowToken other) => base.Equals(other)
            && IsVisible == other.IsVisible
            && State == other.State;

        public override bool Equals(object obj) => Equals(obj as WindowToken);

        #endregion

        public override string ToString() => Serializer.Serialize(this);
    }
}