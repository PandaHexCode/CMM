using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;

public class PlatformSpawner : MonoBehaviour{

    public BlockID platformId;

    private List<GameObject> spawnedPlatforms = new List<GameObject>();

    private void OnEnable(){
        if (LevelEditorManager.isLevelEditor && !LevelEditorManager.instance.isPlayMode)
            Destroy(this);
        else
            StartCoroutine(Spawner());
    }

    private void OnDisable(){
        StopAllCoroutines();

        foreach(GameObject gm in this.spawnedPlatforms){
            Destroy(gm);
        }

        this.spawnedPlatforms.Clear();
    }

    private IEnumerator Spawner(){
        yield return new WaitForSeconds(0.2f);
        Transform clon = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)this.platformId].prefarb, this.transform.parent).transform;
        StartCoroutine(PlatformMove(clon));
        while(clon.position.y > 18){
            yield return new WaitForSeconds(0);
        }

        StartCoroutine(Spawner());
    }

    private IEnumerator PlatformMove(Transform clon, bool second = false){
        clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(49, TileManager.TilesetType.ObjectsTileset);
        clon.GetComponent<PlatformLift>().LoadLength();
        if (GameManager.instance.sceneManager.playerCamera.yCamera)
            clon.position = new Vector3(this.transform.position.x, 32, this.transform.position.z);
        else
            clon.position = new Vector3(this.transform.position.x, 27, this.transform.position.z);
        yield return new WaitForSeconds(0.1f);
        clon.GetComponent<PlatformLift>().direction = LiftHelper.Direction.NONE;
        this.spawnedPlatforms.Add(clon.gameObject);
        while (clon.position.y > 11){
            clon.Translate(0, -3 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0f);
        }

        this.spawnedPlatforms.Remove(clon.gameObject);
        Destroy(clon.gameObject);
    }

}
