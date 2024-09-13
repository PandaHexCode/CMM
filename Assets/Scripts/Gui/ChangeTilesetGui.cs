using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTilesetGui : MonoBehaviour{

    public GameObject backgroundChooseMenuParent;
    public GameObject backgroundDefaultPage;
    public GameObject changeBackgroundPageArrowRight;
    public GameObject changeBackgroundPageArrowLeft;
    public BackgroundPage[] backgroundPages;

    [System.NonSerialized]public BackgroundPage[] currentBackgroundPages = null;
    private int currentBackgroundPage = -1;

    [System.Serializable]
    public class BackgroundPage{
        public GameObject pageParent;
        public TileManager.StyleID enableFor = TileManager.StyleID.SMB1;
    }

    public void OpenBackgroundChooseMenu(){
        if (this.backgroundChooseMenuParent.active){
            this.backgroundChooseMenuParent.SetActive(false);
        }

        this.currentBackgroundPage = -1;
        UpdateCurrentBackgroundPages();
        CloseAllPages();
        this.backgroundChooseMenuParent.SetActive(true);
        this.backgroundDefaultPage.SetActive(true);
    }

    public void CloseAllPages(){
        this.backgroundDefaultPage.SetActive(false);
        foreach (BackgroundPage page in this.backgroundPages){
            page.pageParent.SetActive(false);
        }
    }

    public void UpdateCurrentBackgroundPages(){
        List<BackgroundPage> pages = new List<BackgroundPage>();
        foreach (BackgroundPage page in this.backgroundPages){
            if (page.enableFor == TileManager.instance.currentStyle.id)
                pages.Add(page);
        }

        this.currentBackgroundPages = pages.ToArray();
        UpdateBackgroundArrowButtons();
    }

    public void UpdateBackgroundArrowButtons(){
        this.changeBackgroundPageArrowLeft.SetActive(false);
        this.changeBackgroundPageArrowRight.SetActive(false);
        
        if (this.currentBackgroundPage != -1)
            this.changeBackgroundPageArrowLeft.SetActive(true);

        if (this.currentBackgroundPage != (this.currentBackgroundPages.Length - 1) && this.currentBackgroundPages.Length != 0)
            this.changeBackgroundPageArrowRight.SetActive(true);
    }

    public void OnClickChangeBackgroundArrowButton(bool isLeft){
        if (isLeft)
            this.currentBackgroundPage = this.currentBackgroundPage - 1;
        else
            this.currentBackgroundPage = this.currentBackgroundPage + 1;

        CloseAllPages();
        if(this.currentBackgroundPage < 0){
            this.currentBackgroundPage = -1;
            this.backgroundDefaultPage.SetActive(true);
        }else
            this.currentBackgroundPages[this.currentBackgroundPage].pageParent.SetActive(true);
        UpdateBackgroundArrowButtons();
    }
}
