using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DonutBlock : MonoBehaviour{

    private Coroutine cor = null;
    private bool canCancel = true;
    private Vector3 savedPos;
    private List<GameObject> list = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.transform.position.y < this.transform.position.y + 0.5f)
            return;

        if ((collision.gameObject.layer == 9 | GameManager.IsInLayerMask(collision.gameObject, GameManager.instance.entityMask) | (collision.gameObject.layer == 23 && collision.gameObject.GetComponent<EntityGravity>() != null))){
            if (collision.gameObject.layer == 23 && collision.gameObject.GetComponent<EntityGravity>() == null)
                return;
            if (this.cor == null)
                this.cor = StartCoroutine(DonutBlockHit());
            this.list.Add(collision.gameObject);
        }

        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(this.transform);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(null);

        this.list.Remove(collision.gameObject);

        if ((collision.gameObject.layer == 9 | GameManager.IsInLayerMask(collision.gameObject, GameManager.instance.entityMask) ) && this.cor != null && this.canCancel){
            if (collision.gameObject.layer == 23 && collision.gameObject.GetComponent<EntityGravity>() == null)
                return;
            if (GetComponentInChildren<PlayerController>() != null | this.list.Any())
                return;

            StopCoroutine(this.cor);
            this.cor = null;
            this.GetComponent<Animator>().StopPlayback();
            this.GetComponent<Animator>().enabled = false;
            this.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(64, TileManager.TilesetType.MainTileset);
        }
    }

    private IEnumerator DonutBlockHit(){
        this.canCancel = true;
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play("DonutBlockHit");
        this.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(65, TileManager.TilesetType.MainTileset);
        yield return new WaitForSeconds(1.3f);
        this.canCancel = false;
        Transform _transform = this.transform;
        this.savedPos = _transform.localPosition;
        while (_transform.position.y > 10){
            _transform.Translate(0, -8 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0f);
        }

        if (GetComponentInChildren<PlayerController>() != null)
            GetComponentInChildren<PlayerController>().Death();

        this.list.Clear();

        yield return new WaitForSeconds(1f);
        this.cor = null;
        this.GetComponent<Animator>().StopPlayback();
        this.GetComponent<Animator>().enabled = false;
        this.transform.GetChild(0).transform.localPosition = Vector3.zero;
        this.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(64, TileManager.TilesetType.MainTileset);
        _transform.localPosition = this.savedPos;
    }

}
