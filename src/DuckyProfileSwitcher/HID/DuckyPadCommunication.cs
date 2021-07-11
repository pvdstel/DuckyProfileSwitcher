using Device.Net;
using Hid.Net.Windows;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuckyProfileSwitcher.HID
{
    public static class DuckyPadCommunication
    {
        private const string VendorName = "dekuNukem";
        private const uint ProductID = 0xD11C; // 53532
        private const ushort CountedBufferUsage = 0x3a;
        private const byte DirectoryFlag = 0x01;
        private const int ProcessingDelayMS = 300;

        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private static IDevice? duckyHidDevice;

        public static void InvalidateDeviceCache()
        {
            duckyHidDevice?.Dispose();
            duckyHidDevice = null;
        }

        public static async Task<bool> IsConnected(CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                IDeviceFactory? hidFactory = new FilterDeviceDefinition()
                    .CreateWindowsHidDeviceFactory();

                var definitions = await hidFactory.GetConnectedDeviceDefinitionsAsync(cancellationToken);
                bool exists = definitions.Any(hid =>
                    hid.Usage == CountedBufferUsage
                    && hid.Manufacturer.Contains(VendorName));

                return exists;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task<ConnectedDeviceDefinition?> GetDeviceInfo(CancellationToken cancellationToken)
        {
            IDevice? dp = await FindDuckyPad(cancellationToken);
            return dp?.ConnectedDeviceDefinition;
        }

        public static async Task<DuckyPadInfo> GetDuckyPadInfo(CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                DuckyPadMessage message = new(0, DuckyPadCommand.Info);
                var response = await WriteReceive(message, cancellationToken);
                var result = new DuckyPadInfo(response.Payload[0],
                                              response.Payload[1],
                                              response.Payload[2],
                                              response.Payload[3],
                                              BitConverter.ToUInt32(response.Payload, 4),
                                              response.Payload[8]);
                return result;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task<DuckyPadStatus> GotoProfile(byte profile, CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                DuckyPadMessage message = new(0, DuckyPadCommand.Goto);
                message.Payload[0] = profile;
                var response = await WriteReceive(message, cancellationToken);
                await Task.Delay(ProcessingDelayMS);
                return response.Status;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task<DuckyPadStatus> PreviousProfile(CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                DuckyPadMessage message = new(0, DuckyPadCommand.PreviousProfile);
                var response = await WriteReceive(message, cancellationToken);
                await Task.Delay(ProcessingDelayMS);
                return response.Status;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task<DuckyPadStatus> NextProfile(CancellationToken cancellationToken)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                DuckyPadMessage message = new(0, DuckyPadCommand.NextProfile);
                var response = await WriteReceive(message, cancellationToken);
                await Task.Delay(ProcessingDelayMS);
                return response.Status;
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public static async Task<ImmutableList<(string name, DuckyPadFileType type)>> ListFiles(CancellationToken cancellationToken, string? rootDir = null)
        {
            try
            {
                await semaphoreSlim.WaitAsync(cancellationToken);
                DuckyPadMessage message = new(0, DuckyPadCommand.ListFiles);
                if (!string.IsNullOrEmpty(rootDir))
                {
                    byte[] rootDirBytes = Encoding.ASCII.GetBytes(rootDir);
                    Array.Copy(rootDirBytes, 0, message.Payload, 0, DuckyPadMessage.PayloadSize);
                }

                List<(string name, DuckyPadFileType type)> result = new();
                DuckyPadResponse duckyPadResponse = await WriteReceive(message, cancellationToken);
                while (true)
                {
                    ThrowIfError(duckyPadResponse);
                    if (duckyPadResponse.Status == DuckyPadStatus.EOF)
                    {
                        break;
                    }

                    byte[] stringBytes = duckyPadResponse.Payload.SkipWhile(b => b == 0)
                                                                 .TakeWhile(b => b != 0)
                                                                 .ToArray();

                    if (stringBytes.Length == 0)
                    {
                        break;
                    }

                    DuckyPadFileType type = DuckyPadFileType.File;
                    int startByte = 0;
                    if (stringBytes[0] == DirectoryFlag)
                    {
                        type = DuckyPadFileType.Directory;
                        startByte = 1;
                    }

                    string filename = Encoding.ASCII.GetString(stringBytes, startByte, stringBytes.Length - startByte);
                    result.Add((filename, type));

                    DuckyPadMessage resume = new(0, DuckyPadCommand.Resume);
                    duckyPadResponse = await WriteReceive(resume, cancellationToken);
                }

                return result.ToImmutableList();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Writes a message to the duckyPad device, and awaits a response.
        /// </summary>
        /// <param name="message">The message to send to the duckyPad.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The response from the device.</returns>
        private static async Task<DuckyPadResponse> WriteReceive(DuckyPadMessage message, CancellationToken cancellationToken)
        {
            IDevice? duckyPad = await FindDuckyPad(cancellationToken);
            byte[]? messageBytes = message.GetBytes();
            TransferResult hidResponse = await duckyPad.WriteAndReadAsync(messageBytes, cancellationToken);
            byte[]? responseBytes = hidResponse.Data;
            return DuckyPadResponse.FromBytes(responseBytes);
        }

        /// <summary>
        /// Throws an error if the HID response is unexpected, that is, ERROR or BUSY.
        /// </summary>
        /// <param name="response">The duckyPad response.</param>
        private static void ThrowIfError(DuckyPadResponse response)
        {
            if (response.Status == DuckyPadStatus.Error)
            {
                throw new DuckyPadException("The device returned an error status code.");
            }
            else if (response.Status == DuckyPadStatus.Busy)
            {
                throw new DuckyPadException("The device returned a busy status code.");
            }
        }

        /// <summary>
        /// Finds a connected duckyPad, or throws an exception if it cannot be found.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The duckyPad HID counted buffer device.</returns>
        private static async Task<IDevice> FindDuckyPad(CancellationToken cancellationToken)
        {
            if (duckyHidDevice != null)
            {
                return duckyHidDevice;
            }

            IDeviceFactory? hidFactory = new FilterDeviceDefinition(productId: ProductID)
                .CreateWindowsHidDeviceFactory(readBufferSize: DuckyPadResponse.TotalSize, writeBufferSize: DuckyPadMessage.TotalSize);

            ConnectedDeviceDefinition? deviceDefinition = (await hidFactory.GetConnectedDeviceDefinitionsAsync(cancellationToken))
                .FirstOrDefault(hid =>
                    hid.Usage == CountedBufferUsage
                    && hid.Manufacturer.Contains(VendorName));

            if (deviceDefinition == null)
            {
                throw new DuckyPadException("No connected duckyPad was found.");
            }

            IDevice? duckyPad = await hidFactory.GetDeviceAsync(deviceDefinition, cancellationToken);
            await duckyPad.InitializeAsync(cancellationToken);
            duckyHidDevice = duckyPad;

            return duckyPad;
        }
    }
}
