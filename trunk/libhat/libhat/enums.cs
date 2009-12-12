using System;
using System.Collections.Generic;
using System.Text;

namespace libhat {
    public enum GameType {
          Normal      = 0x0000
        , Arena       = 0x0300
        , Cooperative = 0x0200
    }

    public enum ServerType {
        DEATHMATCH  = 0x01,
        TEAMPLAY    = 0x00,
        COOPERATIVE = 0x02
    }

    enum Client_message {
        M_ALL_OK = 0x00,
        M_SERVER_ERROR = 0x03,//4: ���������� ������������ � �������. ��������� ������������ URL.
        M_CHAR_ABSENT = 0x0B,//12: ���� ��������� ����������� �� �������. �������� �� ������.
        M_REPLICATION_ERROR = 0x0D,	//14: ������ ��� ���������� ������.
        M_TOO_STRONG = 0x0E, //15: ��� �������� ������� ����� ��� �����, �� ������� ���������� ����.
        M_TOO_WEAK = 0x0F, //16: ��� �������� ������� ���� ��� �����, �� ������� ���������� ����.
        M_INVALID_LOGIN_PASSWORD = 0x14,//21: ������������ ����� ��� ������.
        M_LOGIN_LOCKED = 0x15,	//22: ����� � ����� ������� ��� ���������� � ����.
        M_BANNED = 0x16,	//23: ��� ������� ������������ ���������������.
        M_OFFLINE_SERVER = 0x17, //24: ������, �� ������� �� ������ � ������� ���, ������� ����� � �����, � � ������ ������ �� ���������������. ��������� 30 ������ � ���������� ��� ���.
        M_SERVER_FULL = 0x19, //26: ������ ����������.
        M_DUPLICATE_NAME = 0x1A, //27:�������� � ����� ������ � ������ ��� ������������ � ����.
        M_NICK_ERROR_CHAR = 0x1B, //28: "��� ������ ��������� ������ � ������ �����������."
        M_NICK_ERROR_LEN = 0x1C, //29: "��� ������ ��������� ������� ��������. ����������� ����������� ����� ����� - 3 �����."
        M_INVALID_DATA = 0x1D, //30: ������ � ������� ����� ������ ���������.
        M_TEAMPLAY_STARTED = 0x20, //33: � ��������� ������ ����� �������� ������ ���� �� ��� �� �������.
        M_SHUTDOWN_INIT = 0x21, //34: ������, �� ������� �� ������ ������, �����������. �������� ������ ������.
        M_BANNEDIP = 0x22, //35: ��� IP ����� ������ � ������ �����������.
        M_ALREADY_EXISTS = 0x23, // 36: ����� ��� ��������� ��� ������������ �� ����. ���������� ������ ���.
        M_WRONGVER = 0x24, //37: ���� ������ ���� ��������. ��������� ������ ���������� ��� ���� � www.allods.net
        M_HORROR = 0x65, //102: ����!
        M_ERROR_SERVER = 0x84, //133: ������: ������ ����� �������
        M_ERROR_PACKET = 0x8A, // 139: ������: ������� ������������ �����
        //	M_DISCONNECT2 = 0xD6, //215: � ������ ������ �� ������� ���������� ���������������� �����������. � ��������� ����� ���������� ������ ������� ������������
        M_BAN_FOREVER = 0xDB, //220: ��� ����� ������������ ��������������� �������
        M_TURNIR = 0xDC, //221: ������ ����� ���� � ��������� ����� ����������
        M_HAT_NOT_LOAD = 0xDD, //222: � ������ ������ ��� ��� �� �� ����� ����������, ��������� ������� ������� ��������� ���� �����
        M_DIFF_GAME_TYPE = 0xDE, //223: ��� ���������� ������� ����� ����, ������ ��� ���������� ������
        M_LOGIN_LOCKED_TRY_AGAIN = 0x15,// M_LOGIN_LOCKED; // �������� ��������� � ��������� ������� 
    };

    enum Client_operation {
        M_CHECK_NICKNAME  = 0xDF,
        M_SEND_MESSAGE    = 0x0B,
        M_GET_SERVER_LIST = 0xC8
    }

    public enum Difficulty {
          Easy   = 1
        , Medium = 2
        , Hard   = 3
        , Horror = 4
    }

    public enum ParseState {
        UNKNOWN_PACKET,
        SERVER_CONNECTED
    }

    public enum HatMode {
        COMPATIBLE, 
        NEW_CHARACTER_MANAGEMENT
    }
}