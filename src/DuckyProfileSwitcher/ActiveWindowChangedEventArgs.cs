using System;
using System.Collections.Generic;

namespace DuckyProfileSwitcher
{
    public class ActiveWindowChangedEventArgs : EventArgs, IEquatable<ActiveWindowChangedEventArgs?>
    {
        public ActiveWindowChangedEventArgs(IntPtr handle, string title, string processName, string className)
        {
            Handle = handle;
            Title = title;
            ProcessName = processName;
            ClassName = className;
        }

        public IntPtr Handle { get; }
        public string Title { get; }
        public string ProcessName { get; }
        public string ClassName { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ActiveWindowChangedEventArgs);
        }

        public bool Equals(ActiveWindowChangedEventArgs? other)
        {
            return other != null &&
                   EqualityComparer<IntPtr>.Default.Equals(Handle, other.Handle) &&
                   Title == other.Title &&
                   ProcessName == other.ProcessName &&
                   ClassName == other.ClassName;
        }

        public override int GetHashCode()
        {
            int hashCode = -1308394375;
            hashCode = hashCode * -1521134295 + Handle.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProcessName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
            return hashCode;
        }

        public override string ToString()
        {
            return $"{{Handle: {Handle}, Title: '{Title}', ProcessName: '{ProcessName}', ClassName: '{ClassName}'}}";
        }

        public static bool operator ==(ActiveWindowChangedEventArgs? left, ActiveWindowChangedEventArgs? right)
        {
            return EqualityComparer<ActiveWindowChangedEventArgs>.Default.Equals(left!, right!);
        }

        public static bool operator !=(ActiveWindowChangedEventArgs? left, ActiveWindowChangedEventArgs? right)
        {
            return !(left == right);
        }
    }
}
