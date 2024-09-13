using UnityEngine;

public class LevelStartRespawnHelper : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 14 && collision.gameObject.CompareTag("R")){
            collision.GetComponent<WaitForRespawn>().respawnableEntity.Respawn();
            Destroy(collision.gameObject);
        }
    }

}
