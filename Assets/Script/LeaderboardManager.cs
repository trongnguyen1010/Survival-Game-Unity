using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using TMPro;

[System.Serializable]
public class ScoreData
{
    public string name;
    public int score;
}

public class LeaderboardManager : MonoBehaviour
{
    // Nếu chạy trên máy tính thì là localhost
    // Nếu build ra điện thoại Android thật thì phải thay bằng IP máy tính của bạn (VD: http://192.168.1.5:5000)

    // Mặc định là localhost (phòng hờ)
    public static string serverURL = "http://localhost:5000";

    [SerializeField] private TMP_InputField ipInputField;

    void Start()
    {
        // Khi game bật lên, tự load lại cái IP cũ đã nhập lần trước
        string savedIP = PlayerPrefs.GetString("ServerIP", "192.168.2.176");
        
        // Cập nhật URL
        serverURL = "http://" + savedIP + ":5000";
        
        // Hiển thị lên ô nhập để biết đang dùng IP nào
        if (ipInputField != null)
        {
            ipInputField.text = savedIP;
            // Lắng nghe sự kiện khi gõ xong
            ipInputField.onEndEdit.AddListener(UpdateServerIP);
        }
    }

    // Hàm này gọi khi bạn gõ xong IP và bấm Enter hoặc bấm ra ngoài
    public void UpdateServerIP(string newIP)
    {
        if (string.IsNullOrEmpty(newIP)) return;

        // Lưu lại vào bộ nhớ máy
        PlayerPrefs.SetString("ServerIP", newIP);
        PlayerPrefs.Save();

        // Cập nhật đường dẫn server
        serverURL = "http://" + newIP + ":5000";
        Debug.Log("Đã đổi Server sang: " + serverURL);
    }

    public void SendScore(string playerName, int score)
    {
        StartCoroutine(PostScore(playerName, score));
    }

    IEnumerator PostScore(string name, int score)
    {
        // Tạo cục dữ liệu JSON
        ScoreData data = new ScoreData { name = name, score = score };
        string json = JsonUtility.ToJson(data);

        // Gửi yêu cầu POST
        UnityWebRequest request = new UnityWebRequest(serverURL + "/submit", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Gửi điểm thành công: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Lỗi gửi điểm: " + request.error);
        }
    }
}