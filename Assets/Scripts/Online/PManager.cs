using TMPro;
using UnityEngine;

public class PManager : MonoBehaviour {

    public TextMeshProUGUI t;

    private static bool isAllowed = false;

    private void Awake(){
        GetComponent<Patreon>().onConnect += OnC;
        GetComponent<Patreon>().onError += OnC;
    }

    public void OnLButton(){
        GetComponent<Patreon>().connect();
    }

    public void OnC(string text){
        t.text = "Loading";
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void OnE(string text){
        t.text = "Something went wrong.\n" + text;
    }

    public static bool IsAllowed(){
        return isAllowed;
    }
}
