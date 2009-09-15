using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libhat_ng.Helpers
{
    internal class PacketParser
    {
        ICommand Parse( byte[] data )
        {
            ICommand response = null;

            var pid = data[0];

            switch (pid)
            {
                case (int)PacketID.Login:
                    response = new LoginCommand();
                    break;
                case (int)PacketID.AcceptCharacter:
                case (int)PacketID.ArenaResult:
                case (int)PacketID.CharacterReturn:
                case (int)PacketID.CharacterUpdate:
                case (int)PacketID.GetCharacter:
                case (int)PacketID.GetServerData:
                case (int)PacketID.IdentServer:
                case (int)PacketID.NewCharacter:
                case (int)PacketID.NicknameCheck:
                case (int)PacketID.RejectCharacter:
                case (int)PacketID.ServerList:
                case (int)PacketID.ServerReconnect:
                default:
                    response = new UnknownCommand();
                    break;
            }
        

            return response;
        }
    }
}
