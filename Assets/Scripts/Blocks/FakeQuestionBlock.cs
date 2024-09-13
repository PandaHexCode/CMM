using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeQuestionBlock : TileAnimator{

    private bool wasUsed = false;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.wasUsed && collision.GetComponent<Rigidbody2D>().velocity.y > -5){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.booLaught);
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);

            /*BooEffect*/
            GameObject booEff1 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff1.transform.position = this.transform.position;
            booEff1.transform.rotation = Quaternion.Euler(0, 0, -40);
            GameObject booEff2 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff2.transform.position = this.transform.position;
            booEff2.transform.rotation = Quaternion.Euler(0, -180, -40);

            this.wasUsed = true;
            StartCoroutine(ColourIE());
            StartCoroutine(MoveIE(collision.gameObject.transform));
            GameObject mask = Instantiate(GameManager.instance.sceneManager.entityDarkMask, this.transform);
            mask.transform.localPosition = Vector3.zero;
            mask.transform.localScale = new Vector3(2, 2, 2);
        }
    }

    private IEnumerator ColourIE(){
        SpriteRenderer sp = GetComponentInChildren<SpriteRenderer>();
        while(sp.color.r > 246 | sp.color.g > 0){
            if (sp.color.r > 246)
                sp.color = new Color(sp.color.r - 5 * Time.deltaTime, sp.color.g, sp.color.b);
            if(sp.color.g > 0)
                sp.color = new Color(sp.color.r, sp.color.g - 5 * Time.deltaTime, sp.color.b);
            yield return new WaitForSeconds(0);
        }
    }

    private IEnumerator MoveIE(Transform player){
        Transform trans = this.transform;

        float targetY1 = trans.position.y + 1;
        float xSpeed1 = -3;
        if (player.position.x < trans.position.x)
            xSpeed1 = 3;
        while (trans.position.y < targetY1){
            trans.Translate(xSpeed1 * Time.deltaTime, 3 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(0.1f);
        this.gameObject.AddComponent<DamageBlock>();

        float xSpeed = 8;
        if (player.position.x < trans.position.x)
            xSpeed = -5;

        Transform childTrans = trans.GetChild(0).transform;
        while(trans.position.y > 10){
            trans.Translate(xSpeed * Time.deltaTime, -15 * Time.deltaTime, 0);
            childTrans.Rotate(0, 0, 500 * Time.deltaTime);

            RaycastHit2D ray = Physics2D.Raycast(trans.position, Vector2.down, 0.5f, GameManager.instance.entityGroundMask);
            if (!ray && xSpeed > 0)
                ray = Physics2D.Raycast(trans.position, Vector2.right, 0.5f, GameManager.instance.entityGroundMask);
            else if (!ray && xSpeed < 0)
                ray = Physics2D.Raycast(trans.position, Vector2.left, 0.5f, GameManager.instance.entityGroundMask);

            if (ray){
                if(SceneManager.CheckBlockToUseOrDestroy(ray.collider.gameObject, true) && !ray.collider.gameObject.GetComponent<BreakBlock>())
                    continue;
                GameObject eff = Instantiate(GameManager.instance.sceneManager.explodeEffect.GetComponent<ExplodeEffect>().standartBlockDestroyEffect);
                eff.transform.position = trans.position + new Vector3(4, 2, 0);
                foreach (Transform child in eff.transform)
                    child.GetComponent<ParticleSystem>().startColor = this.GetComponentInChildren<SpriteRenderer>().color;
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.breakBlock);
                Destroy(this.gameObject);
                StopAllCoroutines();
            }
            yield return new WaitForSeconds(0);
        }

        Destroy(this.gameObject);
    }

}
