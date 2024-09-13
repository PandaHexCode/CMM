using UnityEngine;
using UnityEngine.Events;

public class LinkToEvent : MonoBehaviour{
    
    public UnityEvent unityEvent;

    public void StartEvent(){
        this.unityEvent.Invoke();
    }

}
