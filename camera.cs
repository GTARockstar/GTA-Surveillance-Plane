//*
//You are free to copy, use, distribute, tweek and modify any of this code as you see fit
//*

using System;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;



public class CreateCamera : Script
{


    private Ped Pilot;
    private Vehicle SurveillancePlane;
    public Camera cam;
    public double z_cam = 0.0;
    public double y_cam = 0.0;
    public double x_cam = 0.0;
    public int zoomlvl = 70;
    public int camshake = 0;
    public int nvg = 0;
    public int ScriptStatus = 0;
    public double mouseX = 20.0;
    public double mouseY = 20.0;
    public double icon_x = 0.500;
    public double icon_y = 0.500;





    public CreateCamera()
    {
        this.Tick += onTick;
        this.KeyUp += onKeyUp;
        this.KeyDown += onKeyDown;


    }




    public void onTick(object sender, EventArgs e)
    {

        if (ScriptStatus == 1)
        {


            //Load target hud     
            if (!Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, "helicopterhud"))
            {
                Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "helicopterhud", 0);
            }

            //Set game is render near plane's location
            Function.Call(Hash._SET_FOCUS_AREA, SurveillancePlane.Position.X, SurveillancePlane.Position.Y, 0.0, 0.0, 0.0, 0.0);

            //Targeting screen
            Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "hud_dest", 0.500, 0.500, 0.1, 0.1, 0.0, 255, 255, 255, 255);
            Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "hud_target", 0.500, 0.500, 0.075, 0.075, 0.0, 255, 0, 0, 255);


            //Obtaining cursor X/Y data to control camera
            var mouseX = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.CursorX);
            var mouseY = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.CursorY);

            // Left/Right camera pan limit (Based on screen's width)
            mouseX *= -UI.WIDTH;

            // Up/Down camera tilt limit (This prevents the camera from tilting up into the plane)
            mouseY *= -135;

            //Is Mouse Being Used?
            bool mouseUD = Function.Call<bool>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.LookUpDown);
            bool mouseLR = Function.Call<bool>(Hash.GET_CONTROL_NORMAL, 0, (int)GTA.Control.LookLeftRight);

            //Obtaining mouse scroll data to control zoom
            bool zoomup = Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 2, 14); //Scroll up
            bool zoomdown = Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 2, 15); //Scroll down

            //Night Vision
            if (nvg == 1)
            {


                TrackerLocation();
                TrackerHeading();
                TrackerSpeed();

                //Flimsy Camera Filter (Works best at night)
                Function.Call(Hash.DRAW_RECT, 0.500, 0.500, 1.1, 1.1, 96, 96, 96, 50);

            }


            //Move camera (Checks of any mouse movements are true)
            if (mouseUD || mouseLR || zoomup || zoomdown)
            {


                //If zoom up is activated (Anything past 60-70 zoom tends to make the camera distorted)
                if (zoomup)
                {

                    if (zoomlvl < 60)
                    {

                        //Zoom Increments
                        zoomlvl += 4;


                    }


                }

                //If zoom down is activated (Anything past 1 doesnt work)
                if (zoomdown)
                {
                    if (zoomlvl > 1)
                    {

                        //Zoom decrement
                        zoomlvl -= 4;

                    }
                }

                //Shows an icon in the top right port of the screen everytime camra is moved
                Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "targetlost", 0.9, 0.1, 0.030, 0.030, 0.0, 255, 255, 255, 255);

                //It seems the camera has to be destroyed everytime a change is made or the camera won't work
                World.DestroyAllCameras();
                Camera cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
                Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 0, cam.Handle, 0, 0);
                World.RenderingCamera = cam;
                Function.Call(Hash.ATTACH_CAM_TO_ENTITY, cam, SurveillancePlane, 0.0, 0.0, -2.5, true);
                Function.Call(Hash.GET_RENDERING_CAM, cam);

                //Rotate Camera based on Cursor X and Y position
                Function.Call(Hash.SET_CAM_ROT, cam, mouseY, 0.0, mouseX, 2);

                //Set camera's zoom level
                cam.FieldOfView = zoomlvl;



            }
        }


    }


    void onKeyUp(object sender, KeyEventArgs e)
    {


        //PRESS NumPad0 to CREATE PLANE AND START CAMERA
        if (e.KeyCode == Keys.NumPad0 && ScriptStatus == 0)
        {
            //Enable script components
            ScriptStatus = 1;

            //Set widescreen, teleport player to desert airport, set player ignored
            Function.Call(Hash.SET_WIDESCREEN_BORDERS, 1, 500);
            Function.Call(Hash.SET_ENTITY_COORDS, Game.Player.Character, 1747.0f, 3273.7f, 41.1f, 0, 0, 0, 1);
            Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player.Character, true);
            Function.Call(Hash.CAN_CREATE_RANDOM_COPS, true);
            Game.Player.IgnoredByEveryone = true;

            //Create Titan
            SurveillancePlane = World.CreateVehicle(VehicleHash.Titan, Game.Player.Character.GetOffsetInWorldCoords(new GTA.Math.Vector3(0, 0, 150)));
            SurveillancePlane.CustomPrimaryColor = Color.FromArgb(255, 255, 255);
            SurveillancePlane.MaxSpeed = 200;
            SurveillancePlane.Speed = 100;
            SurveillancePlane.EngineRunning = true;


            //Create Pilot
            Pilot = World.CreatePed(PedHash.Pilot02SMM, SurveillancePlane.Position);
            Pilot.SetIntoVehicle(SurveillancePlane, VehicleSeat.Driver);

            Wait(500);
            Function.Call(Hash._SET_VEHICLE_LANDING_GEAR, SurveillancePlane, 1);

            //Create Initial Camera
            Camera cam = World.CreateCamera(new Vector3(1956.543f, 3699.894f, 35), Vector3.Zero, 60);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, 1, 1, cam.Handle, 0, 0);
            World.RenderingCamera = cam;
            Function.Call(Hash.ATTACH_CAM_TO_ENTITY, cam, SurveillancePlane, 0.0, 0.0, -2.5, true);
            cam.FieldOfView = zoomlvl;
            Function.Call(Hash.SET_CAM_ROT, cam, Game.Player.Character.ForwardVector.X, Game.Player.Character.ForwardVector.Y, Game.Player.Character.ForwardVector.Z, 2);
            Function.Call(Hash.SHAKE_CAM, cam, "ROAD_VIBRATION_SHAKE", 1.0);



            //Set Pilot to fly plane to point
            Function.Call(Hash.TASK_PLANE_MISSION, Pilot, SurveillancePlane, 0, 0, 1492.318, 1472.318, 700.0, 4, 100f, 0f, 90f, 700f, 500f);

            //Set Pilot to fly helicopter to point (Disabled)
            //Function.Call(Hash.TASK_HELI_MISSION, Pilot, SurveillancePlane, 0, 0, 1492.318, 1472.318, 200.0, 9, 200.0, 200.0, 30.0, 0, 150, 0.0, 0);


        }

        //FLIR EFFECT ON

        if (e.KeyCode == Keys.NumPad9 && nvg == 0)
        {
            Function.Call(Hash._START_SCREEN_EFFECT, "DeathFailMPIn", 60000, 2);
            Function.Call(Hash._SET_BLACKOUT, true);
            Function.Call(Hash._SET_FAR_SHADOWS_SUPPRESSED, true);
            nvg = 1;
        }

        //FLIR EFFECT OFF

        else if (e.KeyCode == Keys.NumPad9 && nvg == 1)
        {
            Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
            Function.Call(Hash._SET_BLACKOUT, false);
            Function.Call(Hash._SET_FAR_SHADOWS_SUPPRESSED, false);
            nvg = 0;
        }
    }



    void onKeyDown(object sender, KeyEventArgs e)
    {
        if (ScriptStatus == 1)
        {
            //PRESS END TO EXIT CAMERA MODE AND RETURN PLAYER
            if (e.KeyCode == Keys.End || Game.Player.IsDead || ScriptStatus == 0)
            {
                Function.Call(Hash.SET_ENTITY_COORDS, Game.Player.Character, 1747.0f, 3273.7f, 41.1f, 0, 0, 0, 1);
                Function.Call(Hash.SET_WIDESCREEN_BORDERS, false, 0);

                ScriptStatus = 0;
                nvg = 0;
                Pilot.Delete();
                SurveillancePlane.Delete();
                World.RenderingCamera = null;
                World.DestroyAllCameras();
                Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS);
                Function.Call(Hash._SET_BLACKOUT, false);
                Function.Call(Hash.CLEAR_FOCUS);
                cam.Destroy();

                Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player.Character, false);
                Game.Player.IgnoredByEveryone = false;

                Wait(500);
            }
        }
    }



    //Display plane's location in IR mode relative to coordinates
    public void TrackerLocation()
    {
        if (ScriptStatus == 1)
        {
            OutputArgument Street = new OutputArgument();
            OutputArgument Cross = new OutputArgument();
            Function.Call(Hash.GET_STREET_NAME_AT_COORD, SurveillancePlane.Position.X, SurveillancePlane.Position.Y, SurveillancePlane.Position.Z, Street, Cross);
            string Street1 = Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, Street.GetResult<int>());
            string Street2 = Function.Call<string>(Hash.GET_STREET_NAME_FROM_HASH_KEY, Cross.GetResult<int>());

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.3f, 0.3f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_CENTRE, 0);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            if (Street2 == "") { Street2 = ""; } else { Street2 = " (crossing) " + Street2; }
            string latlondetails = "Tracker: " + Street1 + Street2;
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, latlondetails);
            Function.Call(Hash._DRAW_TEXT, 0.004999, 0.014999);
            Function.Call(Hash.DRAW_RECT, 0.074999, 0.02999, 0.365, 0.02999f, 96, 96, 96, 50);
        }
    }


    //Display plane's heading in IR mode
    public void TrackerHeading()
    {
        if (ScriptStatus == 1)
        {

            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.3f, 0.3f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_CENTRE, 0);
            //Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            int vheading = (int)Math.Ceiling(SurveillancePlane.Heading);
            string heading = "Heading: " + vheading;
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, heading);
            Function.Call(Hash._DRAW_TEXT, 0.004999, 0.064999);
            Function.Call(Hash.DRAW_RECT, 0.074999, 0.07999, 0.15f, 0.02999f, 96, 96, 96, 50);
        }
    }

    //Display plane's speed
    public void TrackerSpeed()
    {
        if (ScriptStatus == 1)
        {
            string vcolor = SurveillancePlane.DisplayName;
            Function.Call(Hash.SET_TEXT_FONT, 0);
            Function.Call(Hash.SET_TEXT_SCALE, 0.3f, 0.3f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 255);
            Function.Call(Hash.SET_TEXT_CENTRE, 0);
            //Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            int vspeed = (int)Math.Ceiling(SurveillancePlane.Speed);
            string heading = "Speed: " + vspeed;
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, heading);
            Function.Call(Hash._DRAW_TEXT, 0.004999, 0.12);
            Function.Call(Hash.DRAW_RECT, 0.074999, 0.135, 0.15f, 0.02999f, 96, 96, 96, 50);
        }
    }
}
