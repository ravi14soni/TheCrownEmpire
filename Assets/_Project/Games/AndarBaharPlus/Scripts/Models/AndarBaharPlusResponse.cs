using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndarBaharPlusResponse : MonoBehaviour { }

[Serializable]
public class ABPBotUser
{
    public string id;
    public string name;
    public string gender;
    public string coin;
    public string avatar;
    public string added_date;
    public string updated_date;
    public string isDeleted;
}

[Serializable]
public class GameData
{
    public string id;
    public string room_id;
    public string main_card;
    public string winning;
    public string winning_red_black;
    public string winning_shape;
    public string winning_ak;
    public string winning_up_down;
    public string status;
    public string added_date;
    public int time_remaining;
    public string end_datetime;
    public string updated_date;
}

[Serializable]
public class ABPGameCard
{
    public string id;
    public string ander_baher_plus_id;
    public string card;
    public string added_date;
}

[Serializable]
public class ABPLastWinning
{
    public string id;
    public string room_id;
    public string main_card;
    public string card1;
    public string card2;
    public string card3;
    public string winning;
    public string winning_red_black;
    public string winning_shape;
    public string winning_ak;
    public string winning_up_down;
    public string status;
    public string winning_amount;
    public string user_amount;
    public string comission_amount;
    public string total_amount;
    public string admin_profit;
    public string end_datetime;
    public string random;
    public string added_date;
    public string updated_date;
}

[Serializable]
public class Root
{
    public List<ABPBotUser> bot_user;
    public string message;
    public List<GameData> game_data;
    public List<ABPGameCard> game_cards;
    public List<object> online_users;
    public int online;
    public List<object> last_bet;
    public int ander_bet;
    public int bahar_bet;
    public List<ABPLastWinning> last_winning;
    public int code;
}
