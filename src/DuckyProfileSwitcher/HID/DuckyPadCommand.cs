namespace DuckyProfileSwitcher.HID
{
    public enum DuckyPadCommand : byte
    {
        /// <summary>
        /// The device will return its device information.
        /// </summary>
        Info = 0x00,
        /// <summary>
        /// The device will jump to a particular profile.
        /// </summary>
        Goto = 0x01,
        /// <summary>
        /// The device will go to the previous profile.
        /// </summary>
        PreviousProfile = 0x02,
        /// <summary>
        /// The device will go to the next profile.
        /// </summary>
        NextProfile = 0x03,
        /// <summary>
        /// Not implemented.
        /// </summary>
        ReloadCurrentProfile = 0x04,
        /// <summary>
        /// Not implemented.
        /// </summary>
        ChangeRgbLedColour = 0x05,
        /// <summary>
        /// Not implemented.
        /// </summary>
        PrintText = 0x06,
        /// <summary>
        /// Not implemented.
        /// </summary>
        PrintBitmap = 0x07,
        /// <summary>
        /// Not implemented.
        /// </summary>
        ClearScreen = 0x08,
        /// <summary>
        /// Not implemented.
        /// </summary>
        UpdateScreen = 0x09,
        ListFiles = 0x0A,
        ReadFile = 0x0B,
        Resume = 0x0C,
        Abort = 0x0D,
        OpenFileForWriting = 0x0E,
        WriteFile = 0x0F,
        CloseFile = 0x10,
        DeleteFile = 0x11,
        CreateDir = 0x12,
        DeleteDir = 0x13,
        Reset = 0x14,
        Sleep = 0x15,
    }
}
