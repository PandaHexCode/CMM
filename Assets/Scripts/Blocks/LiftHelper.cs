using System.Collections.Generic;
using UnityEngine;

public class LiftHelper : MonoBehaviour{

    public enum Direction { UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3, BLUE = 4, NONE = 5 };
    [System.NonSerialized] public List<Transform> entityFollowers = new List<Transform>();

    public void MoveLift(float x, float y, float z, Direction direction){
        this.transform.Translate(x, y, z);
        foreach (Transform entity in this.entityFollowers){
            Vector2 vt = Vector2.left;
            if (direction == LiftHelper.Direction.RIGHT)
                vt = Vector2.right;
            else if (direction == LiftHelper.Direction.UP)
                vt = Vector2.up;
            if (direction == LiftHelper.Direction.DOWN)
                vt = Vector2.down;
            bool ray1 = SceneManager.EntityWallCheckRay(entity, vt);
            if (!ray1)
                entity.Translate(x, y, z);
        }
    }

}
