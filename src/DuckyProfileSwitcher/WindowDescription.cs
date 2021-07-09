using System;
using System.Collections.Generic;

namespace DuckyProfileSwitcher
{
    public class WindowDescription : IEquatable<WindowDescription?>
    {
        public WindowDescription(string title, string processName)
        {
            Title = title;
            ProcessName = processName;
        }

        public string Title { get; }

        public string ProcessName { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as WindowDescription);
        }

        public bool Equals(WindowDescription? other)
        {
            return other != null &&
                   Title == other.Title &&
                   ProcessName == other.ProcessName;
        }

        public override int GetHashCode()
        {
            int hashCode = 1512247762;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProcessName);
            return hashCode;
        }

        public static bool operator ==(WindowDescription? left, WindowDescription? right)
        {
            return EqualityComparer<WindowDescription>.Default.Equals(left!, right!);
        }

        public static bool operator !=(WindowDescription? left, WindowDescription? right)
        {
            return !(left == right);
        }
    }
}
