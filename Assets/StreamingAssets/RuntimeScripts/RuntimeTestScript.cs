using UnityEngine;

public class RuntimeTestScript : MonoBehaviour{
	 
    public static RuntimeTestScript Init(){
        if (GameObject.Find("RuntimeTestScriptHost") != null)
            Destroy(GameObject.Find("RuntimeTestScriptHost"));

        GameObject host = new GameObject("RuntimeTestScriptHost");
        return host.AddComponent<RuntimeTestScript>();
    }

    private void Start(){
        Debug.Log("Hello World!");
    }

}
        