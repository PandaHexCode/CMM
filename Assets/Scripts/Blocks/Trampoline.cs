using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : TileAnimator{

    public static List<GameObject> targetGameObjects = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision){
        if ((!collision.GetComponent<PowerupItem>() | collision.GetComponent<Trampoline>()) && collision.isTrigger | collision.gameObject.tag.Equals("CU"))
            return;

        if (collision.gameObject.layer == 9){
            if (collision.gameObject.transform.position.y < (this.transform.position.y + 0.5f))
                return;

            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.jumpTrampoline);
            StartAnimationClip(this.animationClips[0]);
            collision.gameObject.GetComponent<PlayerController>().Jump(23, true);
        }else if(GameManager.IsInLayerMask(collision.gameObject, GameManager.instance.entityMask) && !targetGameObjects.Contains(collision.gameObject)) {
            if (collision.gameObject.transform.parent.GetComponent<Entity>() != null)
                return;
            StartCoroutine(JumpEntity(collision.gameObject));
        }
    }

    private IEnumerator JumpEntity(GameObject en){
        targetGameObjects.Add(en);
        SoundManager.PlayAudioClipIfPlayerIsInNear(SoundManager.currentSoundEffects.jumpTrampoline, en.transform.position);
        StartAnimationClip(this.animationClips[0]);
        yield return new WaitForSeconds(0.1f);

        if (en.GetComponent<EntityGravity>())
            en.GetComponent<EntityGravity>().SetUseGravity(false);

        float speed = 15;
        float targetY = en.transform.position.y + 0.2f;
        while(en.transform.position.y < targetY){
            RaycastHit2D ray = Physics2D.Raycast(en.transform.position, Vector2.up, 0.5f, GameManager.instance.entityWandMask);
            if (ray && ray.collider.gameObject != null){
                if(ray.collider.gameObject.GetComponent<EntityGravity>() != null){
                    if(!targetGameObjects.Contains(ray.collider.gameObject))
                        StartCoroutine(JumpEntity(ray.collider.gameObject));
                }
                else if(ray.collider.gameObject.layer != 27 |(ray.collider.gameObject.layer == 27 && (ray.collider.gameObject.transform.eulerAngles.z == -90 | ray.collider.gameObject.transform.eulerAngles.z == 270))){
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
            en.transform.Translate(0, speed * Time.deltaTime, 0);
            if (speed < 20)
                speed = speed + 5 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        while(speed > 0.5f){
            RaycastHit2D ray = Physics2D.Raycast(en.transform.position, Vector2.up, 0.5f, GameManager.instance.entityWandMask);
            if (ray && ray.collider.gameObject != null){
                if (ray.collider.gameObject.GetComponent<EntityGravity>() != null){
                  if(!targetGameObjects.Contains(ray.collider.gameObject))
                        StartCoroutine(JumpEntity(ray.collider.gameObject));
                }else if (ray.collider.gameObject.layer != 27 |(ray.collider.gameObject.layer == 27 && (ray.collider.gameObject.transform.eulerAngles.z == -90 | ray.collider.gameObject.transform.eulerAngles.z == 270))){
                    yield return new WaitForSeconds(0.1f);
                    break;
                }
            }
            en.transform.Translate(0, speed * Time.deltaTime, 0);
            speed = speed - 25 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        while (speed < 5){
            en.transform.Translate(0, -speed * Time.deltaTime, 0);
            speed = speed + 25 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        if (en.GetComponent<EntityGravity>())
            en.GetComponent<EntityGravity>().SetUseGravity(true);

        targetGameObjects.Remove(en);
    }

}
