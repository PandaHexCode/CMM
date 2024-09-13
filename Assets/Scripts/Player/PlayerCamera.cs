using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour{

     public Transform playerTransform;
     public BackgroundParallax background;
     private Transform myTransform;
     private Camera camera;

    public float lerpt = 0.1f;

     public int minX = 0;
     public float maxX = 10000;
     public float yOffset = 0;
     public float minY;
     public float startY;
     public float maxY;
     public float fractionOfJourneyY;
     private bool freeze = false;private float freezeCameraY;

    [System.NonSerialized]public bool yCamera = true;
    private bool multiplayerCam = false;private Transform[] multiplayerPlayers;

    /**/
    public enum ScaleMethod { Aspect_Ratio = 0, Stretch = 1};
    private ScaleMethod scaleMethod;

    private void Awake(){
        this.camera = this.GetComponent<Camera>();
      //  this.camera.aspect = 1.778589f;

        this.myTransform = this.transform;

        if (this.playerTransform == null)
            this.playerTransform = GameObject.Find("Player").transform;
    }
   
    private void LateUpdate(){
        if (multiplayerCam)
            MultiplayerPlayerFinder();

        Vector3 newPos = playerTransform.position;

        newPos.z = -10;

        if (freeze)
        {
            newPos = myTransform.position;
            newPos.y = freezeCameraY + yOffset;
        }

        if (newPos.x < minX)
            newPos.x = minX;

        if (newPos.x > maxX)
            newPos.x = maxX;

        if (yCamera && !freeze)
        {
            if (newPos.y < minY | newPos.y < startY)
                newPos.y = minY + yOffset;
            else if (newPos.y > maxY)
                newPos.y = maxY + yOffset;
            else
                newPos.y = newPos.y + yOffset;

            newPos.y = Mathf.Lerp(myTransform.position.y, newPos.y, fractionOfJourneyY);
        }
        else if (!freeze)
            newPos.y = 19 + yOffset;

        myTransform.position = Vector3.Lerp(myTransform.position, newPos, lerpt * Time.deltaTime);
        if (myTransform.position.x < minX)
            myTransform.position = new Vector3(minX, myTransform.position.y, myTransform.position.z);
    }

    private void MultiplayerPlayerFinder(){
        foreach(Transform tr in this.multiplayerPlayers){
            bool smaler = false;
            foreach (Transform tr2 in this.multiplayerPlayers)
                if (tr.position.x < tr2.position.x)
                    smaler = true;

            if (!smaler && !tr.GetComponent<PlayerController>().GetIsInBubble()){
                this.playerTransform = tr;
                return;
            }
        }
    }

    public void StartMultiplayerCam(){
        this.multiplayerCam = true;
        this.multiplayerPlayers = new Transform[GameManager.instance.sceneManager.players.Count];
        for (int i = 0; i < this.multiplayerPlayers.Length; i++){
            this.multiplayerPlayers[i] = GameManager.instance.sceneManager.players[i].transform;
        }
    }

    public void StartSinglePlayerCam(){
        this.playerTransform = GameManager.instance.sceneManager.players[0].transform;
        this.multiplayerCam = false;
    }

    public void FreezeCamera(){
        this.freezeCameraY = this.myTransform.position.y;
        this.freeze = true;
    }

    public void UnfreezeCamera(){
        this.freeze = false;
    }

    public bool IsFreeze(){
        return this.freeze;
    }

    public void ChangeScaleMethod(ScaleMethod newScaleMethod){
        this.scaleMethod = newScaleMethod;
        this.camera = GetComponent<Camera>();

        /*Delete old if exits*/
        if (GetComponent<SetCamFit>() != null)
            Destroy(GetComponent<SetCamFit>());
        if (GetComponentInChildren<MenuManager>().menuParent.transform.parent.GetComponent<UnityEngine.UI.AspectRatioFitter>() != null)
            Destroy(GetComponentInChildren<MenuManager>().menuParent.transform.parent.GetComponent<UnityEngine.UI.AspectRatioFitter>());
        this.camera.rect = new Rect(0, 0, 1, 1);
        this.camera.ResetAspect();

        if(this.scaleMethod == ScaleMethod.Aspect_Ratio){
            this.gameObject.AddComponent<SetCamFit>();
            UnityEngine.UI.AspectRatioFitter rt = GetComponentInChildren<MenuManager>().menuParent.transform.parent.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            rt.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth;
            rt.aspectRatio = 1.778509f;
        }else if(this.scaleMethod == ScaleMethod.Stretch){
            this.camera.aspect = 1.777778f;
        }
    }

    public ScaleMethod GetScaleMethod(){
        return this.scaleMethod;
    }

    public void SetVSync(bool state){
        if (state)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }

}
