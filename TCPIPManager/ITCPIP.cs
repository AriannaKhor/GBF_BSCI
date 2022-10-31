using GreatechApp.Core.Enums;
using System.Collections.Generic;

namespace TCPIPManager
{
    public interface ITCPIP
    {
        List<ClientSocket> clientSockets { get; set; }
        List<ServerSocket> serverSockets { get; set; }
    }
}
