using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Script : MonoBehaviour
{

    // FUNCTIONS IMPORTED FROM DLL:
    //These functions given below were used earlier, but now only the last function AprilTagFunctionsCombined(~) does all the work!


    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetVerticalAngle")]
    //public static extern double GetVerticalAngle();

    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetHorizontalAngle")]
    //public static extern double GetHorizontalAngle();

    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApriltagDetect")]
    //public static extern bool ApriltagDetect(byte[] bitmap, int height, int width);

    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApriltagPoseTheta")]
    //public static extern void ApriltagPoseTheta(byte[] bitmap, int height, int width, double[] theta);

    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApriltagPoseTranslation")]
    //public static extern void ApriltagPoseTranslation(byte[] bitmap, int height, int width, double[] translation);

    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CenterCoord")]
    //public static extern void CenterCoord(byte[] bitmap, int height, int width, double[] coord);

    ////added for rotation
    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApriltagPoseHomogenous")]
    //public static extern void ApriltagPoseHomogenous(byte[] bitmap, int height, int width, double[] U, double[] V, double[] T);

    ////added for scaling of cube
    //[DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "ApriltagSize")]
    //public static extern void ApriltagSize(byte[] bitmap, int height, int width, double[] h, double[] w);


    //27_MAY: SINGLE FUNCTION TO SPEED UP
    [DllImport("ViSPUnity", CallingConvention = CallingConvention.Cdecl, EntryPoint = "AprilTagFunctionsCombined")]
    public static extern void AprilTagFunctionsCombined(byte[] bitmap, int height, int width, double[] coord, double[] U, double[] V, double[] T, double[] h, double[] w, double[] apr);

    //For storing U, V and T vectors
    double[] U = new double[4];
    double[] V = new double[4];
    double[] T = new double[4];

    WebCamTexture wct;
    Renderer rend;
    Texture2D tex;

    //For storing theta vector
    //double[] theta = new double[3];

    //For storing translation vector
    //double[] translation = new double[3];

    //For storing coordinates (pixel) of apriltag 
    double[] coord = new double[4];

    double[] h = new double[1];
    double[] w = new double[1];
    double[] apr = new double[6];

    [Header("Enter your Camera Resolution here")]
    public int X = 640;   // for example 640
    public int Y = 480;   // for example 480


    void Start()
    {

        // UPDATE (23 May 2018)
        // THE CAMERA WAS CHANGED FROM PERSPECTIVE TO ORTHOGRAPHIC
        // SO THE PART BELOW WAS COMMENTED OUT: PLANE WAS NOT SCALED USING CAMERA PARAMETERS
        // SIMPLY, THE PLANE ALONG X SHOULD BE 4/3 (1.33) TIMES ALONG Y


        //// Matching the camera parameters of Unity with the real camera parameters (PERSPECTIVE CAMERA)
        ////Get the vertical FOV angle from DLL function (in radians)
        //double vFOVangle = GetVerticalAngle();

        ////Convert into Degrees
        //vFOVangle = vFOVangle * Mathf.Rad2Deg;

        ////Set the vertical FOV of camera (in degrees), this also sets the horizontal FOV
        ////The horizontal angle is set according to the aspect ratio.
        ////In my case the aspect ratio was 16:9 (set this in the game view in Unity itself)
        //Camera.main.fieldOfView = (float)vFOVangle;


        //// Now scale the plane in front of camera so that it exactly matches the field of view of camera:

        ////Calculate half of it and stored it in half_v_FOV    
        //double half_v_FOV = vFOVangle / 2;

        ////Calculate the current height of plane, along Y-axis
        //float PlaneHeight = GetComponent<Collider>().bounds.size.y;
        //float oldHalfHeight = PlaneHeight / 2;

        ////Half of vertical angle in radians ( as requred by tan() )
        //double half_v_FOV_rads = half_v_FOV * Math.PI / 180;

        ////10 is the fixed distance from camera to plane, corresponding to the angle, new half-height is calculated
        //double newHalfHeight = 10 * (Mathf.Tan((float)half_v_FOV_rads));

        ////The plane has to be scaled along Z-axis (earlier for height, it was Y)
        //double ScalingFactorZ = newHalfHeight / oldHalfHeight;


        ////Following part is done for scaling the plane along its width (similar to above part)

        //double hFOVangle = GetHorizontalAngle();
        //hFOVangle = hFOVangle * 180.0 / Math.PI; //in degrees now
        //double half_h_FOV = hFOVangle / 2;
        ////calculate the current width of plane, width is along x axis
        //float PlaneWidth = GetComponent<Collider>().bounds.size.x;
        //float oldHalfWidth = PlaneWidth / 2;
        //double half_h_FOV_rads = half_h_FOV * Math.PI / 180;
        //double newHalfWidth = 10 * (Mathf.Tan((float)half_h_FOV_rads));
        //double ScalingFactorX = newHalfWidth / oldHalfWidth;


        ////Scaled along X and Z axes (this is done from plane point of view, hence, along Z, not Y)
        //transform.localScale = new Vector3((float)ScalingFactorX, 1, (float)ScalingFactorZ);


        //Debug.Log(wct.requestedHeight);
        //Debug.Log(wct.requestedWidth);

        wct = new WebCamTexture();
        //wct = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = wct;

        wct.Play(); //Start capturing image using webcam  
    }


    void Update()
    {

        //The function CenterCoord is called to get coordinates of aprilTag (stored in coord)
        //CenterCoord(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, coord);

        // The scaled width of plane = 11.29389
        // Transform the value in coord[2] such that ball's X coordinate becomes x
        //double x = (float)11.29389 / 640 * coord[2] - 11.293 / 2;

        // The scaled height of plane = 6.49303
        // Transform the value in coord[0] such that ball's Y coordinate becomes y
        //double y = (float)6.49303 / 480 * coord[0] - 6.493 / 2;

        //Reference for GameObject Sphere, that is to be moved on the plane (this was used before to test translation)
        //GameObject sphere = GameObject.Find("Sphere");

        //vec contains the new coordinates for sphere/cube/aprilTag in general
        //z-coordinate of sphere is fixed because distance from camera to plane is 10 (in -ve direction)
        //Vector3 vec = new Vector3((float)-x, (float)-y, -9);

        //change the coordinates of sphere by setting them equal to vector3 vec
        //sphere.GetComponent<Transform>().position = vec;

        // Added function to determine rotation and translation based on homogeneous matrix cMo
        //ApriltagPoseHomogenous(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, U, V, T);
        //Debug.Log(new Vector4((float)U[0], (float)U[1], (float)U[2], (float)U[3]));
        //Debug.Log(new Vector4((float)V[0], (float)V[1], (float)V[2], (float)V[3]));
        //Debug.Log(new Vector4((float)T[0], (float)T[1], (float)T[2], (float)T[3]));

        //ApriltagSize(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, h, w);
        //Debug.Log(area[0]);

        AprilTagFunctionsCombined(Color32ArrayToByteArray(wct.GetPixels32()), wct.height, wct.width, coord, U, V, T, h, w, apr);

        // UPDATE (23 May 2018) next 3 lines for orthographic 
        double x = (float)13.333 / X * coord[2] - 13.33 / 2;
        double y = (float)10 / Y * coord[0] - 10 / 2;
        Vector3 vec = new Vector3((float)-x, (float)-y, -9);

        //Reference for GameObject Cube, that is to be moved on the plane 
        GameObject cube = GameObject.Find("Cube");

        //Reference for GameObject Cube_pivot, that is to be rotated
        GameObject cube_pivot = GameObject.Find("Cube_pivot");


        //change the coordinates of Cube by setting them equal to vector3 vec
        //cube.GetComponent<Transform>().position = vec;
        cube_pivot.GetComponent<Transform>().position = vec;

        // Max value of side was <480. CHECKED USING apr[0]
        // Comparing the value of diagonals of polygon: this is more accurate than comparing sides of bounding box.
        double max_dim = System.Math.Max(apr[4], apr[5]);

        // Scaling factor for Cube: {scale the cube by a factor of 10 if the value of side (diagonal/sqrt(2)) is 480}
        float scale = (float)(10.0f / Y) * (float)max_dim / (float)Math.Sqrt(2);


        //Vector3 vector = new Vector3(0.5f * scale, 0f, 0f);
        cube.GetComponent<Transform>().localPosition = new Vector3(0.5f*scale,0,0);


        cube.transform.localScale = new Vector3((float)scale, (float)scale, (float)scale);
        //cube_pivot.transform.localScale = new Vector3((float)scale, (float)scale, (float)scale);

        //cube.transform.rotation = Quaternion.LookRotation(new Vector3((float)U[0], (float)U[1], (float)U[2]), new Vector3((float)V[0], (float)V[1], (float)V[2]));
        cube_pivot.transform.rotation = Quaternion.LookRotation(new Vector3((float)U[0], (float)U[1], (float)U[2]), new Vector3((float)V[0], (float)V[1], (float)V[2]));
    }


    //Function for converting into Byte Array to be sent to functions in DLL
    private static byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int length = colors.Length;
        byte[] bytes = new byte[length];
        int value = 0;

        for (int i = 0; i < colors.Length; i++)
        {
            value = (colors[i].r + colors[i].g + colors[i].b) / 3;
            bytes[colors.Length - i - 1] = (byte)value;
        }

        return bytes;
    }
}


//Few Debug Statements for testing purpose (can be put in Update() function)

//      Debug.Log(coord[0]);
//      Debug.Log(coord[2]);
//      Debug.Log(coord[1]);

//      Debug.Log(translation[0]);
//      Debug.Log(translation[1]);
//      Debug.Log(translation[2]);
//      Debug.Log(ApriltagDetect());
