using System;
using System.Collections.Generic;
using Discord.WebSocket;

namespace PoopBot
{
    public struct MessageInformation
    {
        internal SocketUser Author;
        internal IReadOnlyCollection<SocketRole> PingedRoles;
        internal IReadOnlyCollection<SocketUser> PingedUsers;
        internal DateTime TimeAdded;
    }
}