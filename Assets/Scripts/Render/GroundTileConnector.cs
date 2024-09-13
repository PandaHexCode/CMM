using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;

public class GroundTileConnector : MonoBehaviour{

    [System.NonSerialized]public int groundTileID = 208;
    [System.NonSerialized]public bool canSet = true;
    public LayerMask layer;
    public bool isFakeWall = false;
    public int blockFieldNumber = -1;

    public GameObject O = null;
    public GameObject U = null;
    public GameObject L = null;
    public GameObject R = null;

    private Sprite op;
    private Sprite up;
    private Sprite lp;
    private Sprite rp;

    private GroundTileConnector oa;
    private GroundTileConnector ua;
    private GroundTileConnector la;
    private GroundTileConnector ra;

    private SpriteRenderer mySpriteRenderer;

    public int fakeWallMaskSpriteID = 0;

    public void Start(){
        SceneManager.groundTileConnectors.Add(this);
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        GetBlocks();
    }

    public static void SetAllWithoutPos(){
        if (LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode)
            return;

        foreach (GroundTileConnector block in SceneManager.groundTileConnectors){
            if (block.canSet)
                block.Set();
        }
    }

    public static void SetAll(float fromPosX){
        if (LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode)
            return;

        foreach(GroundTileConnector block in SceneManager.groundTileConnectors){
            if(block.transform.position.x < (fromPosX + 5) && block.canSet)
                block.Set();
        }
    }

    private void OnDestroy(){
        SceneManager.groundTileConnectors.Remove(this);
    }

    private void GetBlocks(){
        BlockFieldManager blockFieldManager = LevelEditorManager.instance.blockFieldManager;
        BlockField blockField = blockFieldManager.GetBlockFieldAt(this.blockFieldNumber);
        if (blockField.blockFieldGameObject.transform.localPosition.x == 2.8f){
            L = this.gameObject;
            la = this;
            lp = this.mySpriteRenderer.sprite;
        }else
            GetRay(out L, out la, blockFieldManager.GetBlockFieldAt(this.blockFieldNumber - 1), out lp);

        GetRay(out O, out oa, blockFieldManager.GetBlockFieldOverBlockField(blockField), out op);
        GetRay(out R, out ra, blockFieldManager.GetBlockFieldAt(this.blockFieldNumber + 1), out rp);
        GetRay(out U, out ua, blockFieldManager.GetBlockFieldUnderBlockField(blockField), out up);
    }

    private void GetRay(out GameObject o, out GroundTileConnector setGroundTexture, BlockField blockField, out Sprite sp) {
        o = null;

        setGroundTexture = GameManager.instance.sceneManager.emptyGroundTileConnector;
        sp = null;
        GameObject block = blockField.currentBlock[0][LevelEditorManager.instance.currentArea];
        if(block == null |  (block != null && !GameManager.IsInLayerMask(block, this.layer)))
            block = blockField.currentBlock[1][LevelEditorManager.instance.currentArea];

        if (block != null && GameManager.IsInLayerMask(block, this.layer)){
            o = block;
            sp = o.GetComponent<SpriteRenderer>().sprite;
            setGroundTexture = o.GetComponent<GroundTileConnector>();
        }else{
            o = null;
        }
    }

    public void Set(){
        this.fakeWallMaskSpriteID = -1;
        GetBlocks();
        onNewSet();
    }

    private void sp(int i){
        mySpriteRenderer.sprite = TileManager.instance.currentTileset.mainTileset[i];
        groundTileID = i;
    }

    public void spWithoutStart(int i){
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.currentTileset.mainTileset[i];
        groundTileID = i;
    }

    public void onNewSet(){
        if (!this.isFakeWall){
            int t = -1;
            if(IsFakeWall(oa) | IsFakeWall(ua) | IsFakeWall(la) | IsFakeWall(ra) | (O != null && (IsFakeWall(oa.la) | IsFakeWall(oa.ra))) | (U != null && (IsFakeWall(ua.la) | IsFakeWall(ua.ra))))
                t = CheckFakeWallMaskSprite(O, U, L, R, oa,
                              ua, la, ra, op, up, lp, rp, this.fakeWallMaskSpriteID);

            if (t != -1)
                this.fakeWallMaskSpriteID = t;
        }

        if (O == null && U == null && L == null && R == null){
            sp(208);
            return;
        }

        if (O != null && U != null && L != null && R != null){
            if (oa.L == null && oa.R == null && ua.L == null && ua.R == null)
                sp(223);
            else{
                if (la.O == null && ra.O == null){
                    if (ua.R == null)
                        sp(233);
                    else{
                        if (ua.L == null)
                            sp(232);
                        else
                            sp(236);
                    }
                }else{
                    if (ua.L == null && ua.R == null){
                        if (oa.L == null)
                            sp(234);
                        else
                            sp(237);

                    }else{
                        if (la.U == null){
                            if (ra.O == null)
                                sp(240);
                            else{
                                if (oa.L == null){
                                    if (ra.U == null)
                                        sp(234);
                                    else
                                        sp(238);
                                }else
                                    sp(253);
                            }
                        }else{
                            if (ra.U == null){
                                if (oa.L == null)
                                    sp(241);
                                else{
                                    if (ra.O == null && ra.U == null)
                                        sp(239);
                                    else
                                        sp(254);
                                }
                            }else{
                                if (oa.L == null){
                                    if (ra.U == null)
                                        sp(241);
                                    else
                                        sp(251);
                                }else{
                                    if (ra.O == null){
                                        if (ra.O == null && ra.U == null)
                                            sp(239);
                                        else
                                            sp(252);
                                    }else{
                                        if (this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[196] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[197] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[198] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[246])
                                            return;
                                      
                                        int i = UnityEngine.Random.Range(196, 203);
                                        if(i == 198){
                                            int r = UnityEngine.Random.Range(0, 10);
                                            if (r > 5)
                                                i = 246;
                                        }
                                        if (i >= 199)
                                            i = 246;
                                        sp(i);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if(O == null){
            if(U != null){
                if(L != null && R != null && ua != null){
                    if (ua.L == null && ua.R == null)
                        sp(220);
                    else{
                        if (ra.O == null && ra.U == null)
                            sp(229);
                        else{
                            if (la.U == null)
                                sp(228);
                            else{
                                if (ra.U == null)
                                    sp(229);
                                else{
                                    if (this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[243] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[193] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[194] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[195])
                                        return;
                                    int i = UnityEngine.Random.Range(193, 197);
                                    if (i == 196)
                                        i = 243;
                                    sp(i);
                                }
                            }
                        }
                    }
                }
                if(L == null && R != null){
                    if (ra.U == null)
                        sp(218);
                    else{
                        if (ra.R == null && ra.O == null)
                            sp(184);
                        else
                            sp(242);
                    }
                }
                if(L != null && R == null){
                    if (la.U == null)
                        sp(217);
                    else{
                        if (la.L == null && la.O == null)
                            sp(185);
                        else
                            sp(244);
                    }
                }
            }else if(U == null){
                if (L == null && R != null)
                    sp(209);
                if (L != null && R == null)
                    sp(211);
                if (L != null && R != null){
                    if (this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[210] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[190] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[191] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[192])
                        return;

                    int i = UnityEngine.Random.Range(190, 199);
                    if (i >= 193)
                        i = 210;
                    sp(i);
                }
                if (R == null && L != null && (lp == TileManager.instance.currentTileset.mainTileset[245] | lp == TileManager.instance.currentTileset.mainTileset[186]))
                    sp(211);
                else if (R != null && L == null && lp == TileManager.instance.currentTileset.mainTileset[247])
                    sp(209);
            }
        }else if(O != null){
            if(U != null){
                if(R != null && L == null){
                    if (ra.R == null && ra.O == null && ra.U == null)
                        sp(222);
                    else{
                        if (ra.U == null && ra.O != null)
                            sp(226);
                        else if (ra.U != null && ra.O == null)
                            sp(224);
                        else{
                            if (ra.O == null && ra.U == null)
                                sp(222);
                            else{
                                if (ra.R != null)
                                    sp(245);
                                else
                                    sp(186);
                            }
                        }
                    }
                }
                if(R == null && L != null){
                    if (la.U == null && ua.L == null){
                        if (la.O == null && la.U == null)
                            sp(221);
                        else
                            sp(227);
                    }else{
                        if (O != null && la.O == null)
                            sp(225);
                        else{
                            if (la.L != null)
                                sp(247);
                            else
                                sp(187);
                        }
                    }
                }
            }else if(U == null){
                if(L == null && R != null){
                    if (oa.L == null && oa.R == null)
                        sp(216);
                    else{
                        if (ra.U == null && ra.O == null)
                            sp(216);
                        else{
                            if (ra.O == null)
                                sp(216);
                            else{
                                if (ra.R == null && ra.U == null)
                                    sp(188);
                                else
                                    sp(248);
                            }
                        }
                    }
                }
                if(L != null && R == null) {
                    if (oa.L == null && oa.R == null)
                        sp(215);
                    else{
                        if (la.O == null)
                            sp(215);
                        else{
                            if (la.L == null && la.U == null)
                                sp(189);
                            else
                                sp(250);
                        }
                    }
                }
                if(L != null && R != null){
                    if (oa.L == null && oa.R == null)
                        sp(219);
                    else{

                        if (op == TileManager.instance.currentTileset.mainTileset[184] | op == TileManager.instance.currentTileset.mainTileset[245] | lp == TileManager.instance.currentTileset.mainTileset[186])
                            sp(230);
                        else if (op == TileManager.instance.currentTileset.mainTileset[185] | op == TileManager.instance.currentTileset.mainTileset[247] | op == TileManager.instance.currentTileset.mainTileset[187])
                            sp(231);
                        else{
                            if (ra.O == null)
                                sp(231);
                            else{
                                if (oa.L == null)
                                    sp(230);
                                else{
                                    if (this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[199] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[200] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[201] | this.mySpriteRenderer.sprite == TileManager.instance.currentTileset.mainTileset[249])
                                        return;
                                    int i = UnityEngine.Random.Range(199, 205);
                                    if (i > 201)
                                        i = 249;

                                    sp(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (L != null && R != null){
            if (op == TileManager.instance.currentTileset.mainTileset[245] | op == TileManager.instance.currentTileset.mainTileset[184] | op == TileManager.instance.currentTileset.mainTileset[186]){
                if (la.U == null && U != null && ra.R == null && ua.R == null)
                    sp(234);
                else{
                    if (U == null)
                        sp(230);
                    else{
                        if (ua.L == null){
                            if (ra.U == null)
                                sp(234);
                            else
                                sp(238);
                        }else{
                            if (ra.U == null)
                                sp(241);
                            else
                                sp(251);
                        }
                    }
                }
            }
        }

        if (U != null && L != null && R != null){
            if (op == TileManager.instance.currentTileset.mainTileset[247] | op == TileManager.instance.currentTileset.mainTileset[185]){
                if (up == TileManager.instance.currentTileset.mainTileset[189] | up == TileManager.instance.currentTileset.mainTileset[247])
                    sp(239);
                else{
                    if (ra.U == null && ua.L == null && ua.R == null)
                        sp(235);
                    else{
                        if (la.U == null)
                            sp(240);
                        else{
                            if (ra.U == null && ra.O == null)
                                sp(239);
                            else
                                sp(252);
                        }
                    }
                }
            }
        }

        if (R == null && L == null){
            if (O != null && U == null)
                sp(214);
            if (O != null && U != null)
                sp(213);
            if (O == null && U != null)
                sp(212);
        }

    }

    public static bool IsFakeWall(GroundTileConnector groundTileConnector){
        if (groundTileConnector == null)
            return false;
        return groundTileConnector.isFakeWall;
    }

    public static int CheckFakeWallMaskSprite(GameObject O, GameObject U, GameObject L, GameObject R, GroundTileConnector oa, GroundTileConnector ua,
        GroundTileConnector la, GroundTileConnector ra, Sprite op, Sprite up, Sprite lp, Sprite rp, int current){
        int id = -1;
        if(O != null && O.layer == 30){
            O = null;
            oa = GameManager.instance.sceneManager.emptyGroundTileConnector;
            op = null;
        }else if(U != null && U.layer == 30){
            U = null;
            ua = GameManager.instance.sceneManager.emptyGroundTileConnector;
            up = null;
        }else if (L != null && L.layer == 30){
            L = null;
            la = GameManager.instance.sceneManager.emptyGroundTileConnector;
            lp = null;
        }else if (R != null && R.layer == 30){
            R = null;
            ra = GameManager.instance.sceneManager.emptyGroundTileConnector;
            rp = null;
        }

        if (O == null){
            if(U != null){
                if(L != null && R != null && ua != null){
                    if ((ua.L == null | IsFakeWall(ua.la)) && (ua.R == null | IsFakeWall(ua.ra)))
                        id = 220;
                    else{
                        if ((ra.O == null | IsFakeWall(ra.oa)) && (ra.U == null | IsFakeWall(ra.ua)))
                            id = 229;
                        else{
                            if (la.U == null | IsFakeWall(la.ua))
                                id = 228;
                            else{
                                if (ra.U == null | IsFakeWall(ra.ua))
                                    id = 229;
                                else{
                                    if (current == 243 | current == 193 | current == 194 | current == 195)
                                        id = -1;
                                    int i = UnityEngine.Random.Range(193, 197);
                                    if (i == 196)
                                        i = 243;
                                    id = i;
                                }
                            }
                        }
                    }
                }
                if(L == null && R != null){
                    if (ra.U == null | IsFakeWall(ra.ua))
                        id = 218;
                    else{
                        if ((ra.R == null | IsFakeWall(ra.ra)) && (ra.O == null | IsFakeWall(ra.oa)))
                            id = 184;
                        else
                            id = 242;
                    }
                }
                if(L != null && R == null){
                    if (la.U == null | IsFakeWall(la.ua))
                        id = 217;
                    else{
                        if ((la.L == null | IsFakeWall(la.la)) && (la.O == null | IsFakeWall(la.oa)))
                            id = 185;
                        else
                            id = 244;
                    }
                }
            }else if(U == null){
                if (L == null && R != null)
                    id = 209;
                if (L != null && R == null)
                    id = 211;
                if (L != null && R != null){
                    if (current == 210 | current == 190 | current == 191 | current == 192)
                        id = -1;

                    int i = UnityEngine.Random.Range(190, 199);
                    if (i >= 193)
                        i = 210;
                    id = i;
                }
                if (R == null && L != null && (la.fakeWallMaskSpriteID == 245 | la.fakeWallMaskSpriteID == 186))
                    id = 211;
                else if (R != null && L == null && la.fakeWallMaskSpriteID == 247)
                    id = 209;
            }
        }else if(O != null){
            if(U != null){
                if(R != null && L == null){
                    if ((ra.R == null | IsFakeWall(ra.ra)) && (ra.O == null | IsFakeWall(ra.oa)) && (ra.U == null | IsFakeWall(ra.ua)))
                        id = 222;
                    else{
                        if ((ra.O == null | IsFakeWall(ra.oa)) && (ra.U == null | IsFakeWall(ra.ua)))
                            id = 226;
                        else if (ra.U != null && (ra.O == null | IsFakeWall(ra.oa)))
                            id = 224;
                        else{
                            if ((ra.O == null | IsFakeWall(ra.oa)) && (ra.U == null | IsFakeWall(ra.ua)))
                                id = 222;
                            else{
                                if ((ra.R == null | IsFakeWall(ra.ra)))
                                    id = 245;
                                else
                                    id = 186;
                            }
                        }
                    }
                }
                if(R == null && L != null){
                    if ((la.U == null | IsFakeWall(la.ua)) && (ua.L == null | IsFakeWall(ua.la))){
                        if ((la.O == null | IsFakeWall(la.oa)) && (la.U == null | IsFakeWall(la.ua)))
                            id = 221;
                        else
                            id = 227;
                    }else{
                        if (O != null && (la.O == null | IsFakeWall(la.oa)))
                            id = 225;
                        else{
                            if (la.L != null)
                                id = 247;
                            else
                                id = 187;
                        }
                    }
                }
            }else if(U == null){
                if(L == null && R != null){
                    if ((oa.L == null | IsFakeWall(oa.la)) && (oa.R == null | IsFakeWall(oa.ra)))
                        id = 216;
                    else{
                        if ((ra.U == null | IsFakeWall(ra.ua)) && (ra.O == null | IsFakeWall(ra.oa)))
                            id = 216;
                        else{
                            if (ra.O == null | IsFakeWall(ra.oa))
                                id = 216;
                            else{
                                if ((ra.R == null | IsFakeWall(ra.ra)) && (ra.U == null | IsFakeWall(ra.ua)))
                                    id = 188;
                                else
                                    id = 248;
                            }
                        }
                    }
                }
                if(L != null && R == null) {
                    if ((oa.L == null | IsFakeWall(oa.la)) && (oa.R == null | IsFakeWall(oa.ra)))
                        id = 215;
                    else{
                        if (la.O == null | IsFakeWall(la.oa))
                            id = 215;
                        else{
                            if ((la.L == null | IsFakeWall(la.la)) && (la.U == null | IsFakeWall(la.ua)))
                                id = 189;
                            else
                                id = 250;
                        }
                    }
                }
                if(L != null && R != null){
                    if ((oa.L == null | IsFakeWall(oa.la)) && (oa.R == null | IsFakeWall(oa.ra)))
                        id = 219;
                    else{

                        if (oa.fakeWallMaskSpriteID == 184 | oa.fakeWallMaskSpriteID == 245 | la.fakeWallMaskSpriteID == 186)
                            id = 230;
                        else if (oa.fakeWallMaskSpriteID == 185 | oa.fakeWallMaskSpriteID == 247 | oa.fakeWallMaskSpriteID == 187)
                            id = 231;
                        else{
                            if (ra.O == null | IsFakeWall(ra.oa))
                                id = 231;
                            else{
                                if (oa.L == null | IsFakeWall(oa.la))
                                    id = 230;
                                else{
                                    if (current == 199 | current == 200 | current == 201 | current == 249)
                                        id = -1;
                                    int i = UnityEngine.Random.Range(199, 205);
                                    if (i > 201)
                                        i = 249;

                                    id = i;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (L != null && R != null){
            if (oa.fakeWallMaskSpriteID == 245 | oa.fakeWallMaskSpriteID == 184 | oa.fakeWallMaskSpriteID == 186){
                if ((la.U == null | IsFakeWall(la.ua)) && U != null && (ra.R == null | IsFakeWall(ra.ra)) && (ua.R == null | IsFakeWall(ua.ra)))
                    id = 23;
                else{
                    if (U == null)
                        id = 230;
                    else{
                        if (ua.L == null | IsFakeWall(ua.la)){
                            if (ra.U == null | IsFakeWall(ra.ua))
                                id = 234;
                            else
                                id = 238;
                        }else{
                            if (ra.U == null | IsFakeWall(ra.ua))
                                id = 241;
                            else
                                id = 251;
                        }
                    }
                }
            }
        }

        if (U != null && L != null && R != null){
            if (oa.fakeWallMaskSpriteID == 247 | oa.fakeWallMaskSpriteID == 185){
                if (ua.fakeWallMaskSpriteID == 189 | ua.fakeWallMaskSpriteID == 247)
                    id = 239;
                else{
                    if ((ra.U == null | IsFakeWall(ra.ua)) && (ua.L == null | IsFakeWall(ua.la)) && (ua.R == null | IsFakeWall(ua.ra)))
                        id = 235;
                    else{
                        if (la.U == null | IsFakeWall(la.ua))
                            id = 240;
                        else{
                            if ((ra.U == null | IsFakeWall(ra.ua)) && (ra.O == null | IsFakeWall(ra.oa)))
                                id = 239;
                            else
                                id = 252;
                        }
                    }
                }
            }
        }

        if (R == null && L == null){
            if (O != null && U == null)
                id = 214;
            if (O != null && U != null)
                id = 213;
            if (O == null && U != null)
                id = 212;
        }

        return id;
    }
}

