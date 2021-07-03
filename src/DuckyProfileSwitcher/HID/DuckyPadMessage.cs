using System;

namespace DuckyProfileSwitcher.HID
{
    public class DuckyPadMessage
    {
        public const byte HidUsageId = 0x05;
        public const byte TotalSize = 64;
        public const byte HeaderSize = 3;
        public const byte PayloadSize = TotalSize - HeaderSize;

        public DuckyPadMessage()
        {
        }

        public DuckyPadMessage(byte sequenceNumber, DuckyPadCommand command)
        {
            SequenceNumber = sequenceNumber;
            Command = command;
        }

        public DuckyPadMessage(byte sequenceNumber, DuckyPadCommand command, byte[] payload)
        {
            SequenceNumber = sequenceNumber;
            Command = command;
            Payload = payload;
        }

        public byte SequenceNumber { get; set; }

        public DuckyPadCommand Command { get; set; } = DuckyPadCommand.Info;

        public byte[] Payload { get; } = new byte[PayloadSize];

        public byte[] GetBytes()
        {
            byte[] result = new byte[TotalSize];
            result[0] = HidUsageId;
            result[1] = SequenceNumber;
            result[2] = (byte)Command;
            Array.Copy(Payload, 0, result, HeaderSize, PayloadSize);
            return result;
        }
    }
}
