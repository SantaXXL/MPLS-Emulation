using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Tools
{
    /// <summary>
    /// Represents the package that is sent between nodes in MPLS network
    /// </summary>
    public class MPLSPackage
    {
        /// <summary>
        /// A stack of MPLS labels.
        /// </summary>
        public LabelStack LabelStack { get; set; }

        /// <summary>
        /// Message ID. First sent message has ID = 0.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Packet length (in bytes).
        /// </summary>
        public int PacketLength { get; set; }

        /// <summary>
        /// Time To Live field. Decremented by 1 with every hop.
        /// </summary>
        public ushort TTL { get; set; }

        /// <summary>
        /// Sending message host IP address.
        /// </summary>
        public IPAddress SourceAddress { get; set; }

        /// <summary>
        /// Receving message host IP address.
        /// </summary>
        public IPAddress DestAddress { get; set; }

        /// <summary>
        /// Port at which package was sent/received (this value is changed by cable cloud).
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Message's payload, typed be user.
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// Header's length in bytes.
        /// LabelStack + ID + PacketLength + TTL + SourceAddress + DestAddress + Port
        /// </summary>
        public const int HeaderLength = 2 + 4 + 4 + 2 + 4 + 4 + 2;

        /// <summary>
        /// Default Time To Live value. First hop - from host to LER.
        /// </summary>
        private const int DefaultTTL = 255;

        /// <summary>
        /// Initalizes MPLS package and sets default value of TTL.
        /// </summary>
        public MPLSPackage()
        {
            LabelStack = new LabelStack();
            TTL = DefaultTTL;
        }

        /// <summary>
        /// Converts <c>MPLSPackage</c> to bytes
        /// </summary>
        /// <returns>Converted package</returns>
        public byte[] ToBytes()
        {
            List<byte> result = new List<byte>();
            PacketLength = HeaderLength + Payload.Length;

            result.AddRange(LabelStack.GetBytes());
            result.AddRange(BitConverter.GetBytes(ID));
            result.AddRange(BitConverter.GetBytes(PacketLength));
            result.AddRange(BitConverter.GetBytes(TTL));
            result.AddRange(SourceAddress.GetAddressBytes());
            result.AddRange(DestAddress.GetAddressBytes());
            result.AddRange(BitConverter.GetBytes(Port));
            result.AddRange(Encoding.ASCII.GetBytes(Payload ?? ""));

            return result.ToArray();
        }

        /// <summary>
        /// Converts bytes to <c>MPLSPackage</c>
        /// </summary>
        /// <param name="bytes">Received stream of bytes</param>
        /// <returns>Converted package</returns>
        /// <exception cref="InvalidMPLSPackageException">Thrown when a stream of bytes is not a valid package</exception>
        public static MPLSPackage FromBytes(byte[] bytes)
        {
            try
            {
                MPLSPackage package = new MPLSPackage();
                package.LabelStack = LabelStack.FromBytes(bytes);
                var stackLength = package.LabelStack.GetLength();

                package.ID = BitConverter.ToInt32(bytes, stackLength);
                package.PacketLength = BitConverter.ToInt32(bytes, stackLength + 4);
                package.TTL = (ushort)((bytes[stackLength + 9] << 8) + bytes[stackLength + 8]);

                package.SourceAddress = new IPAddress(new byte[]
                    {bytes[stackLength + 10], bytes[stackLength + 11], bytes[stackLength + 12], bytes[stackLength + 13]});
                package.DestAddress = new IPAddress(new byte[]
                {
                    bytes[stackLength + 14], bytes[stackLength + 15], bytes[stackLength + 16], bytes[stackLength + 17]
                });

                package.Port = (ushort)((bytes[stackLength + 19] << 8) + bytes[stackLength + 18]);

                var usefulPayload = new List<byte>();
                usefulPayload.AddRange(bytes.ToList()
                    .GetRange(stackLength + 20, package.PacketLength - HeaderLength));

                package.Payload = Encoding.ASCII.GetString(usefulPayload.ToArray());
                return package;
            }
            catch (Exception)
            {
                throw new InvalidMPLSPackageException();
            }
        }

        /// <summary>
        /// Generates a message to be shown by a host or node, after receiving/sending package.
        /// </summary>
        /// <returns>Generated string</returns>
        public override string ToString()
        {
            if (LabelStack.IsEmpty())
            {
                return $"[ID={ID}, LabelStack={LabelStack}, Payload={Payload}, TTLStack={TTL}, Source={SourceAddress}, Destination={DestAddress}]";
            }
            else
            {
                var TTLStack = string.Join(", ", LabelStack.Labels.ToList().Select(label => label.TTL)) + ", " + TTL;
                return $"[ID={ID}, LabelStack={LabelStack}, Payload={Payload}, TTLStack={TTLStack}, Source={SourceAddress}, Destination={DestAddress}]";
            }
        }

        /// <summary>
        /// Pushes a new label on top of the <c>LabelStack</c>
        /// </summary>
        /// <param name="label">A label to be pushed at the top of the stack.</param>
        public void PushLabel(Label label)
        {
            LabelStack.Labels.Push(label);
        }

        /// <summary>
        /// Peeks a top label from <c>LabelStack</c> without popping it.
        /// </summary>
        /// <returns>Peeked top labeled or null, if <c>LabelStack</c> is empty</returns>
        public Label PeekTopLabel()
        {
            if (LabelStack.IsEmpty())
            {
                return null;
            }
            return LabelStack.Labels.Peek();
        }

        /// <summary>
        /// Pops the outermost label from the top of the <c>LabelStack</c>.
        /// </summary>
        /// <returns>Popped label</returns>
        public Label PopTopLabel()
        {
            return LabelStack.Labels.Pop();
        }
    }
}
