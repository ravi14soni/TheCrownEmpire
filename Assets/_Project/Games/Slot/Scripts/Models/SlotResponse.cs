using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotResponse : MonoBehaviour
{

}

[Serializable]
public class Slot_GameData
{
    public int id;
    public int room_id;
    public string main_card;
    public string winning;
    public int status;
    public string added_date;
    public int time_remaining;
    public string end_datetime;
    public string updated_date;
    public List<string> reel_grid;
    public List<Slot_Winning> winnings;
}

[Serializable]
public class Slot_GameData_payline
{
    public int id;
    public int room_id;
    public string main_card;
    public string winning;
    public int status;
    public string added_date;
    public int time_remaining;
    public string end_datetime;
    public string updated_date;
    //public List<string> reel_grid;
    public List<Slot_Winning> winnings;
}

[Serializable]
public class Slot_GameCard
{
    public int id;
    public int slot_game_id;
    public string card;
    public string added_date;
}

[Serializable]
public class Slot_LastWinning
{
    public int id;
    public int room_id;
    public string main_card;
    public string winning;
    public int status;
    public int winning_amount;
    public int user_amount;
    public int comission_amount;
    public int random_amount;
    public int total_amount;
    public int admin_profit;
    public string end_datetime;
    public int random;
    public List<string> reel_grid;
    public List<Slot_Winning> winnings;
    public string added_date;
    public string updated_date;
}

[Serializable]
public class Slot_Winning
{
    public int payline;
    public string symbol;
    public int multiply;
    //public int amount;
}

[Serializable]
public class Slot_RootObject
{
    public List<List<int>> paylines;
    public Dictionary<string, Dictionary<string, int>> paytable;
    public List<Slot_GameData> game_data;
    public List<Slot_GameCard> game_cards;
    public int online;
    public List<Slot_LastWinning> last_winning;
}

[Serializable]
public class Slot_RootObject_ForPayline
{
    public List<List<int>> paylines;
    public Dictionary<string, Dictionary<string, int>> paytable;
    public List<Slot_GameData_payline> game_data;
    public List<Slot_GameCard> game_cards;
    public int online;
    public List<Slot_LastWinning> last_winning;
}


[System.Serializable]
public class SlotPlaceBetResponse
{
    public string message;
    public int bet_id;
    public float wallet;
    public int code;
}
[System.Serializable]
public class SlotGameResult
{
    public float win_amount;
    public float bet_amount;
    public float diff_amount;
    public string message;
    public int code;

    public string wallet;
}