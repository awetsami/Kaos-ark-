using UnityEngine;
using TMPro;
using FishNet.Managing;
using System.Net;
using System.Net.Sockets;

public class MultiplayerMenu : MonoBehaviour
{
    [Header("FishNet Bağlantıları")]
    public NetworkManager networkManager;

    [Header("Arayüz Bağlantıları")]
    public GameObject anaMenuPaneli; // YENİ: Kapanmasını istediğimiz asıl Canvas/Panel
    public TMP_InputField ipInputField;
    public TextMeshProUGUI myIpText;

    void Start()
    {
        if (myIpText != null)
        {
            myIpText.text = "Senin Yerel IP Adresin: " + GetLocalIPAddress();
        }
    }

    public void KurVeOyna()
    {
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();

        // Boş objeyi değil, doğrudan arayüz panelini kapatıyoruz
        if (anaMenuPaneli != null) anaMenuPaneli.SetActive(false);
    }

    public void SadeceBaglan()
    {
        string girilenIP = ipInputField.text;

        if (!string.IsNullOrEmpty(girilenIP))
        {
            networkManager.TransportManager.Transport.SetClientAddress(girilenIP);
            networkManager.ClientManager.StartConnection();

            // Boş objeyi değil, doğrudan arayüz panelini kapatıyoruz
            if (anaMenuPaneli != null) anaMenuPaneli.SetActive(false);
        }
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "IP Bulunamadı (İnterneti Kontrol Et)";
    }
}