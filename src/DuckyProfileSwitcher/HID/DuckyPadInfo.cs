namespace DuckyProfileSwitcher.HID
{
    public record DuckyPadInfo
    {
        public DuckyPadInfo(byte major, byte minor, byte patch, byte model, uint serialNumber, byte profile)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Model = model;
            SerialNumber = serialNumber;
            Profile = profile;
        }

        public byte Major { get; }

        public byte Minor { get; }

        public byte Patch { get; }

        public byte Model { get; }

        public uint SerialNumber { get; }

        public byte Profile { get; }
    }
}
