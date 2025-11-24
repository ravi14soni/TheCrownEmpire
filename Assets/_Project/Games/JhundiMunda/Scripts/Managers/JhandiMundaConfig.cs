using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JhandiMundaConfig : MonoBehaviour
{
    //public const string JhandimundaResult = Configuration.BaseUrl + "api/threeDice/get_result";
    public const string JhandimundaResult = Configuration.BaseSocketUrl + "/api/threeDice/get_result";
    public const string JhundiMunda_Prediction_PutBet =
        Configuration.BaseUrl + "/api/threeDice/place_bet";


    //     public const string JhundiMunda_Prediction_PutBet =
    // Configuration.BaseSocketUrl + "/api/threeDice/place_bet";

    public const string JhandimundaCancelBet = Configuration.BaseUrl + "/api/threeDice/cancel_bet";
}
