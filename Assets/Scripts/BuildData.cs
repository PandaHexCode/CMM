using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildData", menuName = "UMM/BuildData", order = 0)]
public class BuildData : ScriptableObject{

    public string VERSION_STRING;
    public float VERSION_FLOAT;
    public bool isPBuild = false;
    public bool encryptLevels = false;
    public bool isAdminBuild = false;
    public string USERNAME = "NULL";
    public string levelKey;
    public string levelKey2;
    public static string hN = "4fhL5J0/PeuFBGgb7A2wMIm31S0WGbE6SzwKNoMMU3ubi8WgkrllM9QW5gbDoVFtG8H9s+uJOShnrf4pE9nva2f3DbcMvWUfL4wai8YDFyTe5vEwGMiqpd9qeiObLnv8";
    public static string hP = "nDR+h7Rt05q0RbLYawA56jIVDIUpxyibxOn+ekgWWFwHiqvtCzLpZUMlYJ0zWgWa9HmL0MdgRz5kTKjuBc2OreMpJUu9LPHgHnNOCACt3sB3JrkfpcK7VEPFYmSUs932";
    public static string hU = "40A1T234bNhD53oQ5Wh4Qd10lLgzplKE+KfPz0A7dtJmP2rEenSoqpi8utyLVUEsi+AMqOIn8zH4dfBgqCT2OcgpRAKnR3s4owMVitgrw5fw4v/rq0YuQ9smz6oO75Ix";

}
