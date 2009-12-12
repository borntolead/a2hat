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
        M_SERVER_ERROR = 0x03,//4: Невозможно подключиться к серверу. Проверьте правильность URL.
        M_CHAR_ABSENT = 0x0B,//12: Файл персонажа отсутствует на сервере. Возможно он удален.
        M_REPLICATION_ERROR = 0x0D,	//14: Ошибка при обновлении данных.
        M_TOO_STRONG = 0x0E, //15: Ваш персонаж слишком силен для карты, на которой происходит игра.
        M_TOO_WEAK = 0x0F, //16: Ваш персонаж слишком слаб для карты, на которой происходит игра.
        M_INVALID_LOGIN_PASSWORD = 0x14,//21: Неправильный логин или пароль.
        M_LOGIN_LOCKED = 0x15,	//22: Игрок с таким логином уже существует в игре.
        M_BANNED = 0x16,	//23: Ваш аккаунт заблокирован администратором.
        M_OFFLINE_SERVER = 0x17, //24: Сервер, на котором вы играли в прошлый раз, потерял связь с хэтом, и в данный момент ее восстанавливает. Подождите 30 секунд и попробуйте еще раз.
        M_SERVER_FULL = 0x19, //26: Сервер переполнен.
        M_DUPLICATE_NAME = 0x1A, //27:Персонаж с таким именем и кланом уже присутствует в игре.
        M_NICK_ERROR_CHAR = 0x1B, //28: "Имя вашего персонажа входит в список запрещенных."
        M_NICK_ERROR_LEN = 0x1C, //29: "Имя вашего персонажа слишком короткое. Минимальная разрешенная длина имени - 3 буквы."
        M_INVALID_DATA = 0x1D, //30: Ошибка в формате файла вашего персонажа.
        M_TEAMPLAY_STARTED = 0x20, //33: В командный турнир можно вступить только если он еще не начался.
        M_SHUTDOWN_INIT = 0x21, //34: Сервер, на котором вы решили играть, закрывается. Выберите другой сервер.
        M_BANNEDIP = 0x22, //35: Ваш IP адрес входит в список запрещенных.
        M_ALREADY_EXISTS = 0x23, // 36: Такое имя персонажа уже используется на хэте. Придумайте другое имя.
        M_WRONGVER = 0x24, //37: Ваша версия игры устарела. Загрузите свежее обновление для игры с www.allods.net
        M_HORROR = 0x65, //102: Ужас!
        M_ERROR_SERVER = 0x84, //133: Ошибка: плохой адрес сервера
        M_ERROR_PACKET = 0x8A, // 139: Ошибка: получен неправильный пакет
        //	M_DISCONNECT2 = 0xD6, //215: В данный момент на сервере проводятся профилактические мероприятия. В ближайшее время постоянная работа сервера возобновится
        M_BAN_FOREVER = 0xDB, //220: Ваш логин заблокирован администратором навечно
        M_TURNIR = 0xDC, //221: Данный режим игры в настоящее время недоступен
        M_HAT_NOT_LOAD = 0xDD, //222: В данный момент хет еще не до конца загрузился, повторите попытку создать персонажа чуть позже
        M_DIFF_GAME_TYPE = 0xDE, //223: Вам необходимо сменить режим игры, прежде чем продолжить играть
        M_LOGIN_LOCKED_TRY_AGAIN = 0x15,// M_LOGIN_LOCKED; // Добавить сообщение о повторной попытке 
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