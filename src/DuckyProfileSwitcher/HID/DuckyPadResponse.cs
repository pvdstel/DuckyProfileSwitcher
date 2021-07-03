using System;

namespace DuckyProfileSwitcher.HID
{
    public class DuckyPadResponse
    {
        public const byte HidUsageId = 0x04;
        public const byte TotalSize = 64;
        public const byte HeaderSize = 3;
        public const byte PayloadSize = TotalSize - HeaderSize;

        private DuckyPadResponse(byte sequenceNumber, DuckyPadStatus status, byte[] payload)
        {
            SequenceNumber = sequenceNumber;
            Status = status;
            Payload = payload;
        }

        public byte SequenceNumber { get; set; }

        public DuckyPadStatus Status { get; set; }

        public byte[] Payload { get; } = new byte[PayloadSize];

        public static DuckyPadResponse FromBytes(byte[] response)
        {
            if (response.Length != TotalSize)
            {
                throw new FormatException("The response size from duckyPad is not exactly 64 bytes.");
            }

            if (response[0] != HidUsageId)
            {
                throw new FormatException("The response from duckyPad does not have the correct HID usage ID.");
            }

            if (!Enum.IsDefined(typeof(DuckyPadStatus), response[2]))
            {
                throw new FormatException("The response status from duckyPad is not recognized.");
            }

            byte sequenceNumber = response[1];
            DuckyPadStatus status = (DuckyPadStatus)response[2];
            byte[] payload = new byte[PayloadSize];
            Array.Copy(response, HeaderSize, payload, 0, PayloadSize);

            return new DuckyPadResponse(sequenceNumber, status, payload);
        }
    }
}
