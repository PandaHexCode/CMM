using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;
using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Text;
using System.Threading;
using System.IO;

public class DevGameManager : MonoBehaviour{

    public GameObject devCanvas;
    public TMP_InputField devInputField;

    public string[] runtimeCompilerCodeTemplates;
    private int lastTemplate = -1;
    private bool isOn = false;
    private Thread thread = null;

    public static bool levelLoadDebug = false;

    public static bool SHOWDEBUGINFORMATIONS = true;
    public static void DrawBoxCastOnHit(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float hitInfoDistance, Color color)
    {
        origin = CastCenterOnCollision(origin, direction, hitInfoDistance);
        DrawBox(origin, halfExtents, orientation, color);
    }

    //Draws the full box from start of cast to its end distance. Can also pass in hitInfoDistance instead of full distance
    public static void DrawBoxCastBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Vector3 direction, float distance, Color color)
    {
        direction.Normalize();
        Box bottomBox = new Box(origin, halfExtents, orientation);
        Box topBox = new Box(origin + (direction * distance), halfExtents, orientation);

        Debug.DrawLine(bottomBox.backBottomLeft, topBox.backBottomLeft, color);
        Debug.DrawLine(bottomBox.backBottomRight, topBox.backBottomRight, color);
        Debug.DrawLine(bottomBox.backTopLeft, topBox.backTopLeft, color);
        Debug.DrawLine(bottomBox.backTopRight, topBox.backTopRight, color);
        Debug.DrawLine(bottomBox.frontTopLeft, topBox.frontTopLeft, color);
        Debug.DrawLine(bottomBox.frontTopRight, topBox.frontTopRight, color);
        Debug.DrawLine(bottomBox.frontBottomLeft, topBox.frontBottomLeft, color);
        Debug.DrawLine(bottomBox.frontBottomRight, topBox.frontBottomRight, color);

        DrawBox(bottomBox, color);
        DrawBox(topBox, color);
    }

    public static void DrawBox(Vector3 origin, Vector3 halfExtents, Quaternion orientation, Color color)
    {
        DrawBox(new Box(origin, halfExtents, orientation), color);
    }
    public static void DrawBox(Box box, Color color)
    {
        Debug.DrawLine(box.frontTopLeft, box.frontTopRight, color);
        Debug.DrawLine(box.frontTopRight, box.frontBottomRight, color);
        Debug.DrawLine(box.frontBottomRight, box.frontBottomLeft, color);
        Debug.DrawLine(box.frontBottomLeft, box.frontTopLeft, color);

        Debug.DrawLine(box.backTopLeft, box.backTopRight, color);
        Debug.DrawLine(box.backTopRight, box.backBottomRight, color);
        Debug.DrawLine(box.backBottomRight, box.backBottomLeft, color);
        Debug.DrawLine(box.backBottomLeft, box.backTopLeft, color);

        Debug.DrawLine(box.frontTopLeft, box.backTopLeft, color);
        Debug.DrawLine(box.frontTopRight, box.backTopRight, color);
        Debug.DrawLine(box.frontBottomRight, box.backBottomRight, color);
        Debug.DrawLine(box.frontBottomLeft, box.backBottomLeft, color);
    }

    public struct Box
    {
        public Vector3 localFrontTopLeft { get; private set; }
        public Vector3 localFrontTopRight { get; private set; }
        public Vector3 localFrontBottomLeft { get; private set; }
        public Vector3 localFrontBottomRight { get; private set; }
        public Vector3 localBackTopLeft { get { return -localFrontBottomRight; } }
        public Vector3 localBackTopRight { get { return -localFrontBottomLeft; } }
        public Vector3 localBackBottomLeft { get { return -localFrontTopRight; } }
        public Vector3 localBackBottomRight { get { return -localFrontTopLeft; } }

        public Vector3 frontTopLeft { get { return localFrontTopLeft + origin; } }
        public Vector3 frontTopRight { get { return localFrontTopRight + origin; } }
        public Vector3 frontBottomLeft { get { return localFrontBottomLeft + origin; } }
        public Vector3 frontBottomRight { get { return localFrontBottomRight + origin; } }
        public Vector3 backTopLeft { get { return localBackTopLeft + origin; } }
        public Vector3 backTopRight { get { return localBackTopRight + origin; } }
        public Vector3 backBottomLeft { get { return localBackBottomLeft + origin; } }
        public Vector3 backBottomRight { get { return localBackBottomRight + origin; } }

        public Vector3 origin { get; private set; }

        public Box(Vector3 origin, Vector3 halfExtents, Quaternion orientation) : this(origin, halfExtents)
        {
            Rotate(orientation);
        }
        public Box(Vector3 origin, Vector3 halfExtents)
        {
            this.localFrontTopLeft = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontTopRight = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
            this.localFrontBottomLeft = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
            this.localFrontBottomRight = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);

            this.origin = origin;
        }


        public void Rotate(Quaternion orientation)
        {
            localFrontTopLeft = RotatePointAroundPivot(localFrontTopLeft, Vector3.zero, orientation);
            localFrontTopRight = RotatePointAroundPivot(localFrontTopRight, Vector3.zero, orientation);
            localFrontBottomLeft = RotatePointAroundPivot(localFrontBottomLeft, Vector3.zero, orientation);
            localFrontBottomRight = RotatePointAroundPivot(localFrontBottomRight, Vector3.zero, orientation);
        }
    }

    //This should work for all cast types
    static Vector3 CastCenterOnCollision(Vector3 origin, Vector3 direction, float hitInfoDistance)
    {
        return origin + (direction.normalized * hitInfoDistance);
    }

    static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        Vector3 direction = point - pivot;
        return pivot + rotation * direction;
    }
    public static void SetMaxFpsTest(int maxFPS){
        Application.targetFrameRate = maxFPS;
        if (maxFPS == 999)
            QualitySettings.vSyncCount = 1;
        else
            QualitySettings.vSyncCount = 0;
    }

    private void Awake(){
        if (!Application.isEditor){
            DevGameManager.SHOWDEBUGINFORMATIONS = false;
            Destroy(this);
        }
        Debug.LogWarning("DevGameManager is active!");
        this.thread = new Thread(test);
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.I) && Time.timeScale != 0) {
            GameManager.instance.sceneManager.players[0].ChangeGravity();
        }


        if(Input.GetKeyDown(KeyCode.K) && Time.timeScale != 0){
            Debug.Log(GameManager.instance.GetRandomTitleLevel());
        }


        if (Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.Z)) {
            this.devCanvas.SetActive(!this.devCanvas.active);
            this.isOn = this.devCanvas.active;
            this.lastTemplate = -1;
            this.devInputField.SetTextWithoutNotify(string.Empty);
            InputManager.instances[0].allowInput = !this.isOn;
        }

        if (!this.isOn)
            return;

        if (Input.GetKeyDown(KeyCode.KeypadEnter)){
            test();
        }

        if(Input.GetKeyDown(KeyCode.UpArrow) && this.lastTemplate != this.runtimeCompilerCodeTemplates.Length - 1){
            this.lastTemplate++;
            this.devInputField.SetTextWithoutNotify(this.runtimeCompilerCodeTemplates[this.lastTemplate]);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow) && this.lastTemplate > 0){
            this.lastTemplate--;
            this.devInputField.SetTextWithoutNotify(this.runtimeCompilerCodeTemplates[this.lastTemplate]);
        }else if(Input.GetKeyDown(KeyCode.DownArrow) &&this.lastTemplate == 0){
            this.lastTemplate = -1;
            this.devInputField.SetTextWithoutNotify(string.Empty);
        }
    }

    public void test(){
        CompileCode(this.devInputField.text);
    }

    public void OnCompileButtonPress(TMP_InputField codeInput)
    {
        CompileScript(codeInput.text);
    }

    public void OnSaveButtonPress(TMP_InputField codeInput)
    {
        SaveFile(Application.streamingAssetsPath + "\\RuntimeScripts\\" + GetTypeName(codeInput.text) + ".cs", codeInput.text);
    }

    public void OnOpenButtonPress(TMP_InputField codeInput)
    {
        string path = Application.streamingAssetsPath + "\\RuntimeScripts\\" + GetTypeName(codeInput.text) + ".cs";
        if (File.Exists(path))
        {
            codeInput.SetTextWithoutNotify(GetFileIn(path));
        }
    }

    public void OnTemplateButtonPress(TMP_InputField codeInput)
    {
        string typeName = GetTypeName(codeInput.text);
        string temp = @"using UnityEngine;

public class " + typeName + @" : MonoBehaviour{
	 
    public static " + typeName + @" Init(){
        if (GameObject.Find(""" + typeName + @"Host"") != null)
            Destroy(GameObject.Find(""" + typeName + @"Host""));

        GameObject host = new GameObject(""" + typeName + @"Host"");
        return host.AddComponent<" + typeName + @">();
    }

    private void Start(){
        Debug.Log(""Hello World!"");
    }

}
        ";

        codeInput.SetTextWithoutNotify(temp);
    }

    public static void CompileScriptFromPath(string path)
    {
        if (!File.Exists(path))
            Debug.LogError(path + " not found!");

        CompileScript(GetFileIn(path));
    }

    public static void CompileScript(string sourceCode)
    {
        var assembly = CompileSource(sourceCode);

        string typeName = GetTypeName(sourceCode);

        var runtimeType = assembly.GetType(typeName);
        var method = runtimeType.GetMethod("Init");
        var del = (Func<MonoBehaviour>)
                      Delegate.CreateDelegate(
                          typeof(Func<MonoBehaviour>),
                          method
                  );

        var addedComponent = del.Invoke();
    }

    public static string GetTypeName(string sourceCode)
    {
        foreach (string str in sourceCode.Split('\n'))
        {
            if (str.StartsWith("public class "))
            {
                return str.Split(' ')[2];
            }
        }

        return string.Empty;
    }

    public static string GetFileIn(string path)
    {
        if (File.Exists(path))
        {
            StreamReader readStm2 = new StreamReader(path);
            string fileIn2 = readStm2.ReadToEnd();
            readStm2.Close();

            return fileIn2;
        }
        else
            return string.Empty;
    }

    public static void SaveFile(string path, string content)
    {
        if (File.Exists(path))
            File.Delete(path);

        StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
        sw.Write(content);
        sw.Close();
    }

    public void CompileCode(string code){
        try{
            string typeName = "CustomCode";
            string sourceCode = @"using UnityEngine;using System;using TMPro;using UMM.BlockData;using UMM.BlockField;using System.Collections;using System.Collections.Generic;

public class " + typeName + @" : MonoBehaviour{
	 
    public static " + typeName + @" Run(){
       " + code + @"
        return null;
    }

}
        ";
   
            var assembly = CompileSource(sourceCode);



            var runtimeType = assembly.GetType(typeName);
            var method = runtimeType.GetMethod("Run");
            var del = (Func<MonoBehaviour>)
                          Delegate.CreateDelegate(
                              typeof(Func<MonoBehaviour>),
                              method
                      );
            del.Invoke();
            this.thread.Abort();
        }catch(Exception e){
            Debug.LogError(e.Message + "|" + e.StackTrace);
            this.thread.Abort();
        }
    }

    private static Assembly CompileSource(string source){
        var provider = new CSharpCodeProvider();
        var param = new CompilerParameters();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()){
            param.ReferencedAssemblies.Add(assembly.Location);
        }
        param.GenerateExecutable = false;
        param.GenerateInMemory = true;

        var result = provider.CompileAssemblyFromSource(param, source);

        if (result.Errors.Count > 0){
            var msg = new StringBuilder();
            foreach (CompilerError error in result.Errors){
                msg.AppendFormat("Error ({0}): {1}\n",
                    error.ErrorNumber, error.ErrorText);
            }
            throw new Exception(msg.ToString());
        }

        return result.CompiledAssembly;
    }

    public void AddPositionOffsetToLevelFile(string path, Vector2 offset){
        string fileContent = GameManager.GetFileIn(path);
        if (1.4f.ToString().Contains(","))
            fileContent.Replace(".", ",");
        else
            fileContent.Replace(",", ".");

        string[] lines = fileContent.Split('\n');
        fileContent = string.Empty;
        fileContent = lines[0] + "\n" + lines[1];

        lines[0] = string.Empty;
        lines[1] = string.Empty;
        foreach (string line in lines){
            if (!string.IsNullOrEmpty(line)){
                string[] args = line.Split(':');
                float x = GameManager.StringToFloat(args[2]);
                float y = GameManager.StringToFloat(args[3]);
                float off = x + offset.x;
                string myLine = line;
                myLine = line.Replace(x.ToString(), off.ToString());
                off = y + offset.y;
                myLine = line.Replace(y.ToString(), off.ToString());
                
                fileContent = fileContent + "\n" + myLine;
            }
        }
        GameManager.SaveFile(path, fileContent);
    }
}
