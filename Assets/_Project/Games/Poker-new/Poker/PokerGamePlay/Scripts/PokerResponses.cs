using System;
using System.Collections.Generic;
using UnityEngine;

public class PokerResponses : MonoBehaviour
{ }
#region join Table

[System.Serializable]
public class PokerJoinTableResponse
{
    public string message;
    public List<PokerTable> table_data;
    public int code;
}

#endregion

#region start game json
[System.Serializable]
public class PokerStartGame
{
    public string user_id;
    public string token;
    public string blind_1;
    public string table_id;
}
#endregion

#region Leave Table JSON

[System.Serializable]
public class PokerTableData
{
    public string user_id;
    public string token;
    public string blind_1;
}

[System.Serializable]
public class PokerTable
{
    public string id;
    public string poker_table_id;
    public string user_id;
    public string seat_position;
    public string role;
    public string game_wallet;
    public string added_date;
    public string updated_date;
    public string isDeleted;
    public string user_type;
    public string name;
    public string mobile;
    public string profile_pic;
    public string wallet;
    public string master_boot_value;
}

[System.Serializable]
public class PokerLeaveTableResponse
{
    public string message;
    public PokerTable[] table_data;
    public int code;
}

#endregion

#region GameData

[System.Serializable]
public class PokerGameDataTableUser
{
    public string id;
    public string poker_table_id;
    public string user_id;
    public int seat_position;
    public int role;
    public string game_wallet;
    public string added_date;
    public string updated_date;
    public string isDeleted;
    public string user_type;
    public string name;
    public string mobile;
    public string profile_pic;
    public string wallet;
    public string master_boot_value;
}

[System.Serializable]
public class PokerGameDataTableDetail
{
    public string id;
    public string master_table_id;
    public string boot_value;
    public string maximum_blind;
    public string chaal_limit;
    public string pot_limit;
    public string private_table;
    public string added_date;
    public string updated_date;
    public string isDeleted;
}

[Serializable]
public class PokerGameDataLastGame
{
    public string game_id;
}

[Serializable]
public class PokerGameDataGameLog
{
    public string id;
    public string game_id;
    public string user_id;
    public string action;
    public string chaal_type;
    public string round;
    public string seen;
    public string amount;
    public string left_amount;
    public string points;
    public string timeout;
    public string added_date;
    public string name;
    public string role;
}

[Serializable]
public class PokerGameDataAllUser
{
    public string id;
    public string poker_table_id;
    public string user_id;
    public string seat_position;
    public string role;
    public string game_wallet;
    public string added_date;
    public string updated_date;
    public string isDeleted;
    public string user_type;
    public string name;
    public string mobile;
    public string profile_pic;
    public string wallet;
    public string master_boot_value;
}

[Serializable]
public class PokerGameDataGameUser
{
    public string id;
    public string game_id;
    public string user_id;
    public string role;
    public string card1;
    public string card2;
    public string packed;
    public string all_in;
    public string total_amount;
    public string seen;
    public string rule;
    public string value;
    public string added_date;
    public string updated_date;
    public string name;
}

[Serializable]
public class PokerCard
{
    public string card1;
    public string card2;
}

[System.Serializable]
public class PokerMiddleCard
{
    public string card;
    public string round;
    public string pot_amount;
    public string added_date;
}

[Serializable]
public class PokerGameDataRabbitCard
{
    public string cards;
}

[System.Serializable]
public class PokerGameData
{
    public PokerGameDataTableUser[] table_users;
    public PokerGameDataTableDetail table_detail;
    public int active_game_id;
    public int game_status;
    public int table_amount;
    public List<PokerGameDataLastGame> last_games;
    public List<PokerGameDataGameLog> game_log;
    public List<PokerGameDataAllUser> all_users;
    public List<PokerGameDataGameUser> game_users;
    public string chaal;
    public List<PokerMiddleCard> middle_card;
    public string game_amount;
    public List<PokerCard> cards;
    public List<List<PokerGameDataRabbitCard>> rabbit_cards;
    public int check;
    public string round;
    public List<object> game_gifts;
    public string message;
    public string winner_user_id;
    public int code;
}

#endregion

#region chaal and raise data

[System.Serializable]
public class PokerUserChaalData
{
    public string user_id;
    public string plus;
    public string token;
    public string rule;
    public string value;
    public string game_id;
    public string chaal_type;
    public string amount;
    public string raise;
}

#endregion

#region Pack Game Class

[System.Serializable]
public class PokerPackData
{
    public string user_id;
    public string token;
    public string timeout;
}

#endregion


