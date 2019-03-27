using System.Net.Sockets;
using System.Text;

namespace Tools
{
    /// <summary>
    /// The <c>MPLSSocket</c> is used to send and receive MPLS packet between nodes.
    /// </summary>
    public class MPLSSocket : Socket
    {
        /// <summary>
        /// Initializes a new instance of the <c>MPLSSocket</c> class using the specified parameters.
        /// </summary>
        /// <param name="addressFamily">Specifies the addressing scheme that an instance of the MPLSSocket class can use</param>
        /// <param name="socketType">Specifies the type of socket that an instance of the MPLSSocket class represents</param>
        /// <param name="protocolType">Specifies the type of the protocol that MPLSSocket class supports</param>
        public MPLSSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) :
            base(addressFamily, socketType, protocolType)
        {
            ReceiveTimeout = 5000000;
        }

        /// <summary>
        /// Converts <c>package</c> to bytes and sends it to connected socket
        /// </summary>
        /// <param name="package">Package that is about to be sent</param>
        /// <returns>The number of bytes sent</returns>
        public int Send(MPLSPackage package)
        {
            return Send(package.ToBytes());
        }

        /// <summary>
        /// Receives stream of bytes from the connected socket
        /// </summary>
        /// <returns>Package, that has been received</returns>
        public MPLSPackage Receive()
        {
            var buffer = new byte[256];
            int bytes = Receive(buffer);

            // probably the string KEEPALIVE
            if (Encoding.ASCII.GetString(buffer, 0, bytes).Substring(0, 9).Equals("KEEPALIVE"))
            {
                return null;
            }

            return MPLSPackage.FromBytes(buffer);
        }
    }
}
