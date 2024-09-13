using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class CustomText : MonoBehaviour{

    [System.Serializable]
    public class CustomTextRenderSettings{
        public bool setColor = true; public Color32 color;
    }

    public CustomTextRenderSettings[] customTextRenderSettings;

    private TMP_Text m_TextComponent;

    private void Awake(){
        this.m_TextComponent = GetComponent<TMP_Text>();
        this.LoadText();
    }

    public void LoadText(){
        List<int> vertBegins = new List<int>();
        List<int> vertEnds = new List<int>();
        string text = m_TextComponent.text;
        string[] startSplits = text.Split(new string[] { "%c" }, StringSplitOptions.None);
        m_TextComponent.text = m_TextComponent.text.Replace("%c", "");
        m_TextComponent.text = m_TextComponent.text.Replace("%e", "");
        text = m_TextComponent.text;
        for (int i = 1; i < startSplits.Length; i++){
            string endSplit = startSplits[i].Split(new string[] { "%e" }, StringSplitOptions.None)[0];
            for (int f = 0; f <= text.Length; f++){
                string ctext = text.Substring(0, f);
                if (ctext.EndsWith(endSplit)){
                    int begin = ctext.Length - endSplit.Length;
                    int end = ctext.Length;
                    vertBegins.Add(begin);
                    vertEnds.Add(end);
                }
            }
        }

        m_TextComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = m_TextComponent.textInfo;

        Color32[] newVertexColors;

        for (int t = 0; t < vertBegins.Count; t++){
            for (int i = vertBegins[t]; i < vertEnds[t]; i++){
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;
                if (this.customTextRenderSettings[t].setColor){
                    if (textInfo.characterInfo[i].isVisible){
                        newVertexColors[vertexIndex + 0] = this.customTextRenderSettings[t].color;
                        newVertexColors[vertexIndex + 1] = this.customTextRenderSettings[t].color;
                        newVertexColors[vertexIndex + 2] = this.customTextRenderSettings[t].color;
                        newVertexColors[vertexIndex + 3] = this.customTextRenderSettings[t].color;
                        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                        m_TextComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                    }
                }
            }
        }
    }
}
