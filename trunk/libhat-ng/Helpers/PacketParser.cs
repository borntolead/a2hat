using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace libhat_ng.Helpers
{
    internal class PacketParser
    {
        public static ICommand Parse( byte[] data )
        {
            ICommand response = null;

            var pid = data[0];

            switch (pid)
            {
                case (int)PacketID.Login:
                    response = new LoginCommand();
                    break;
                case (int)PacketID.AcceptCharacter:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.ArenaResult:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.CharacterReturn:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.CharacterUpdate:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.GetCharacter:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.GetServerData:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.IdentServer:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.NewCharacter:
                    response = new NewCharacterCommand();
                    break;
                case (int)PacketID.NicknameCheck:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.RejectCharacter:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.ServerList:
                    response = new UnknownCommand();
                    break;
                case (int)PacketID.ServerReconnect:
                    response = new UnknownCommand();
                    break;
                default:
                    response = new UnknownCommand();
                    break;
            }
        

            return response;
        }
    }

    
}
