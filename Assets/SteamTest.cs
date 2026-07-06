using UnityEngine;
using Steamworks;

public class SteamTest : MonoBehaviour
{
    void Start()
    {
        // SteamManager sorunsuz çalıştı mı kontrolü
        if (SteamManager.Initialized)
        {
            // Steam profil adını çekiyoruz
            string userName = SteamFriends.GetPersonaName();
            Debug.Log("<color=green><b>Steam Bağlantısı Başarılı!</b></color> Hoş geldin geliştirici: " + userName);
        }
        else
        {
            Debug.LogError("Steam başlatılamadı! Arkada Steam'in açık olduğundan ve steam_appid.txt dosyasının doğru yerde olduğundan emin ol.");
        }
    }
}