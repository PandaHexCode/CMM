using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Goal : MonoBehaviour{

    public GoalType type = GoalType.BOX;
    public enum GoalType { BOX = 0, POINT = 1, SMW = 2};

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            if (collision.gameObject.GetComponent<PlayerController>().GetCanMove() == false)
                return;

            SceneManager.RemoveAllRespawnableEntities();
            foreach (PlayerController player in GameManager.instance.sceneManager.players)
                player.isInGoal = true;
            if (this.type == GoalType.BOX)
                StartCoroutine(TriggerGoalBox(collision.gameObject.GetComponent<PlayerController>()));
            else if (this.type == GoalType.POINT)
                StartCoroutine(TriggerGoalPoint(collision.gameObject.GetComponent<PlayerController>()));
        }
    }

    private IEnumerator TriggerGoalPoint(PlayerController player){
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        player.SetCanMove(false);
        player.BreakReturn();
        player.isFreeze = true;
        player.GetComponent<SpriteRenderer>().sprite = player.currentPlayerSprites.climb[1];
        player.transform.position = new Vector3(this.transform.position.x - 0.45f, player.transform.position.y, player.transform.position.z);
        player.GetComponent<Rigidbody2D>().gravityScale = 0.8f;
        player.GetComponent<Rigidbody2D>().isKinematic = true;
        player.GetComponent<Rigidbody2D>().simulated = false;
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);

        float targetY = -7.8f;
        while(this.transform.parent.GetChild(1).transform.localPosition.y > targetY){
            player.GetComponent<SpriteRenderer>().sprite = player.currentPlayerSprites.climb[1];
            this.transform.parent.GetChild(1).transform.Translate(0, -6 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(0.3f);

        player.GetComponent<Rigidbody2D>().isKinematic = false;
        player.GetComponent<Rigidbody2D>().simulated = true;
        player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -2f);

        float t = 0;
        while(player.GetComponent<Rigidbody2D>().velocity.y != 0 && t < 8){
            player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -2f);
            t = t + 2 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(0.1f);

        player.GetComponent<SpriteRenderer>().sprite = player.currentPlayerSprites.stand[0];

        StartCoroutine(FinishScreen(player));
    }

    public IEnumerator TriggerGoalBox(PlayerController player){
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        player.SetCanMove(false);
        player.BreakReturn();

        this.transform.GetChild(0).GetComponent<TileAnimator>().StopCurrentAnimation();
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play(0);

        float t = 0;
        while(player.GetComponent<Rigidbody2D>().velocity.y != 0 && t < 5){
            t = t + 2 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        player.GetComponent<SpriteRenderer>().sprite = player.currentPlayerSprites.stand[0];

        yield return new WaitForSeconds(1);
        StartCoroutine(FinishScreen(player));
    }

    private IEnumerator FinishScreen(PlayerController player){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.oneUp);
        GameManager.instance.sceneManager.TryToStopTimer();

        if (LevelEditorManager.instance != null && LevelEditorManager.isLevelEditor){

            LevelEditorManager.canSwitch = true;
            MenuManager.canOpenMenu = true;
            player.SetCanMove(true);
            player.isFreeze = false;
            LevelEditorManager.instance.SwitchMode();
        }else{

            GameManager.instance.sceneManager.finishScreen.SetActive(true);
            GameManager.instance.sceneManager.finishScreen.transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().text = GameManager.instance.sceneManager.finishScreen.transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().text.Replace("%", PlayerController.DEATHS.ToString());
            yield return new WaitForSeconds(1.2f);

            InputManager input = player.input;
            while (!input.UP && !input.DOWN && !input.LEFT && !input.RIGHT && !input.JUMP && !input.RUN)
            {
                yield return new WaitForSeconds(0);
            }

            if (GameManager.instance.isInPossibleCheck){
                GameManager.instance.isInPossibleCheck = false;
                string fileContent = GameManager.GetFileIn(GameManager.instance.currentLevelPath);
                if (!fileContent.Substring(1, 1).Contains(":")){
                    try{
                        fileContent = GameManager.Decrypt(fileContent, GameManager.Decrypt(GameManager.instance.buildData.levelKey, GameManager.instance.buildData.levelKey2));
                    }catch (Exception ex){
                        Debug.LogException(ex);
                    }
                }

                string[] lines = fileContent.Split('\n');
                string[] args1 = lines[0].Split(':');
                args1[12] = "1";
                string finalLine = string.Empty;
                foreach (string arg in args1)
                    finalLine = finalLine + arg + ":";
                lines[0] = finalLine;

                string content = string.Empty;
                foreach (string line in lines)
                    content = content +line + "\n";

                if (GameManager.instance.buildData.encryptLevels){
                    content = GameManager.Encrypt(content, GameManager.Decrypt(GameManager.instance.buildData.levelKey, GameManager.instance.buildData.levelKey2));
                }

                GameManager.SaveFile(GameManager.instance.currentLevelPath, content);
            }

            LevelEditorManager.canSwitch = true;
            MenuManager.canOpenMenu = true;
            GameManager.instance.sceneManager.finishScreen.transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().text = GameManager.instance.sceneManager.finishScreen.transform.GetChild(1).GetComponent<TMPro.TextMeshPro>().text.Replace(PlayerController.DEATHS.ToString(), "%");
            player.SetCanMove(true);
            player.isFreeze = false;
            GameManager.instance.LoadEditorScene("--style-" + (int)TileManager.instance.currentStyle.id);
        }
    }

}
