using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotConfig : MonoBehaviour
{
    public const string SlotPlacebet = Configuration.BaseSocketUrl + "/api/slotgame/place_bet";
    public const string SlotGetResult = Configuration.BaseSocketUrl + "/api/slotgame/get_result";
}
