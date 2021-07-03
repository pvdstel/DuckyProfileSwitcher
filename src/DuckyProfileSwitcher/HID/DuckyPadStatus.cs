namespace DuckyProfileSwitcher.HID
{
    public enum DuckyPadStatus : byte
    {
        /// <summary>
        /// The command succeeded.
        /// </summary>
        Success = 0,
        /// <summary>
        /// The command failed.
        /// </summary>
        Error = 1,
        /// <summary>
        /// The device is executing a script, or in a menu.
        /// </summary>
        Busy = 2,
        EOF = 3,
    }
}
