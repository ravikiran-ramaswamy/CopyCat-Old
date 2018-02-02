using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.Kinect;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text;


public class KinectDataCollection : MonoBehaviour
{
    //declare devices
    private KinectSensor kinectSensor;

    //declare helper classes
    private JointDataWriter jointDataWriter;
    //private ColorFrameWriter colorFrameWriter;
    //private DepthFrameWriter depthFrameWriter;

    //declare Kinect objects
    private Body[] bodies = null;
    private BodyFrameReader bodyFrameReader = null;
    private List<GestureDetector> gestureDetectorList = null;
    private MultiSourceFrameReader _reader;

    //declare class variables
    private bool currentRunRecorded;
    private bool startMode = false;
    private bool leftHanded = false;
    public bool startRecording = false;
    private int runState = 0;               //0 = not running; 1 = recording; -1 = writing
    bool raisedLeftHand = false;
    private string mainDir;
    public GameObject level;
    private int session_number;
    private int framesCapturedInPhrase;
    private int totalCapturedFrames_joints;
    private int totalCapturedFrames_color;
    private int totalCapturedFrames_depth;
    private static int msgCount = 0;
    private string currentPhrase;

    //write variables
    private Queue<byte[]> colorQueue = new Queue<byte[]>();
    private Queue<ushort[]> depthQueue = new Queue<ushort[]>();
    int dimension = 0, widthD = 0, heightD = 0;
    ushort minDepth = 0, maxDepth = 0;

    // Use this for initialization
    void Start()
    {
        currentRunRecorded = false;
        currentPhrase = level.GetComponent<RoomLoader>().currentPhrase;
        mainDir = @"C:\PhraseData\" + currentPhrase + @"\";

        this.kinectSensor = KinectSensor.GetDefault();

        // set IsAvailableChanged event notifier
        //this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

        // open the sensor
        this.kinectSensor.Open();

        this._reader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
        //this._reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

        // set the status text
        //this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
        //                                                : Properties.Resources.NoSensorStatusText;

        // open the reader for the body frames
        this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

        // set the BodyFramedArrived event notifier
        this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;

        // initialize the BodyViewer object for displaying tracked bodies in the UI
        //this.kinectBodyView = new KinectBodyView(this.kinectSensor);

        // initialize the gesture detection objects for our gestures
        this.gestureDetectorList = new List<GestureDetector>();

        // initialize the MainWindow
        //this.InitializeComponent();

        // set our data context objects for display in UI
        //this.DataContext = this;
        //this.kinectBodyViewbox.DataContext = this.kinectBodyView;

        // connect to htk server via tcpClient
        //this.clientInterface = ClientInterface.getClientInstance();
        //clientInterface.connect();

        //Console.WriteLine("connect to the client interface \n " + clientInterface.GetHashCode() + "\n");            
        //clientInterface.disconnect();

        // create a gesture detector for each body (6 bodies => 6 detectors) and create content controls to display results in the UI
        //int col0Row = 0, col1Row = 0;

        //this.colorFrameWriter = new ColorFrameWriter();
        //this.depthFrameWriter = new DepthFrameWriter();
        this.jointDataWriter = new JointDataWriter();
        this.totalCapturedFrames_joints = 0;
        this.totalCapturedFrames_color = 0;
        this.totalCapturedFrames_depth = 0;
        this.framesCapturedInPhrase = 0;
        //this.phrase_indices = new int[phrase_list.Length];

        session_number = 1;

        int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
        for (int i = 0; i < maxBodies; ++i)
        {
            //GestureResultView result = new GestureResultView(i, false, false, 0.0f);
            GestureDetector detector = new GestureDetector(this.kinectSensor);
            this.gestureDetectorList.Add(detector);

            // split gesture results across the first two columns of the content grid
            //ContentControl contentControl = new ContentControl();
            //contentControl.Content = this.gestureDetectorList[i].GestureResultView;
            /*
            if (i % 2 == 0)
            {
                // Gesture results for bodies: 0, 2, 4
                Grid.SetColumn(contentControl, 0);
                Grid.SetRow(contentControl, col0Row);
                ++col0Row;
            }
            else
            {
                // Gesture results for bodies: 1, 3, 5
                Grid.SetColumn(contentControl, 1);
                Grid.SetRow(contentControl, col1Row);
                ++col1Row;
            }

            this.contentGrid.Children.Add(contentControl);*/
        }

        //prevDeleteButton.Click += deletePreviousSample;
        //mainDir = System.IO.Path.Combine(dataWritePath, phrase_name);
        //String colorDir = System.IO.Path.Combine(mainDir, "color");
        //String depthDir = System.IO.Path.Combine(mainDir, "depth");
        //System.IO.Directory.CreateDirectory(mainDir);
        //System.IO.Directory.CreateDirectory(colorDir);
        //System.IO.Directory.CreateDirectory(depthDir);

        //System.IO.Directory.CreateDirectory(mainDir);
        //System.IO.Directory.CreateDirectory(colorDir);
        //System.IO.Directory.CreateDirectory(depthDir);

        //colorFrameWriter.setCurrentPhrase(currentPhrase);
        //depthFrameWriter.setCurrentPhrase(currentPhrase);
        //jointDataWriter.setCurrentPhrase(currentPhrase);

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        //this.bodyFrameReader.FrameArrived += this.Reader_BodyFrameArrived;
    }

    public void recordButtonPressed()
    {
        startRecording = !startRecording;
    }

    private void Reader_BodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
    {
        bool dataReceived = false;
        //checkTrial();
        using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
        {
            if (bodyFrame != null)
            {
                if (this.bodies == null)
                {
                    // creates an array of 6 bodies, which is the max number of bodies that Kinect can track simultaneously
                    this.bodies = new Body[bodyFrame.BodyCount];
                }

                // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                // As long as those body objects are not disposed and not set to null in the array,
                // those body objects will be re-used.
                bodyFrame.GetAndRefreshBodyData(this.bodies);
                dataReceived = true;
            }
        }

        if (this.bodies != null)
        {
            int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                Body body = this.bodies[i];

                Windows.Kinect.Joint handr = body.Joints[JointType.HandRight];         //11
                Windows.Kinect.Joint handl = body.Joints[JointType.HandLeft];          //7
                Windows.Kinect.Joint thumbr = body.Joints[JointType.ThumbRight];       //24
                Windows.Kinect.Joint thumbl = body.Joints[JointType.ThumbLeft];        //22
                Windows.Kinect.Joint tipr = body.Joints[JointType.HandTipRight];       //23
                Windows.Kinect.Joint tipl = body.Joints[JointType.HandTipLeft];        //21

                Windows.Kinect.Joint hipr = body.Joints[JointType.HipRight];           //16
                Windows.Kinect.Joint hipl = body.Joints[JointType.HipLeft];            //12
                Windows.Kinect.Joint spinebase = body.Joints[JointType.SpineBase];     //0
                Windows.Kinect.Joint spinemid = body.Joints[JointType.SpineMid];

                //if (!paused)
                if (true)
                {
                    double spineDifferenceY = Math.Abs(spinebase.Position.Y - spinemid.Position.Y);
                    double distFromBase = (spineDifferenceY * 2.0) / 3.0; //Take 2/3rds the distance from the spine base.
                    double threshold = spinebase.Position.Y + distFromBase;

                    double handlY = handl.Position.Y;
                    double handrY = handr.Position.Y;



                    //bool value = (bool)dominantHand.IsChecked;
                    //if (value)
                    //{
                    //    leftHanded = true;
                    //    dominantHandText.Text = "Left Handed.";
                    //}
                    //else
                    //{
                    //    leftHanded = false;
                    //    dominantHandText.Text = "Right Handed.";
                    //}



                    if (!startRecording)
                    {
                        if (runState == 1)
                        {   
                            if (!raisedLeftHand)
                            {
                                //Erase the session data
                                this.jointDataWriter.deleteLastSample(session_number, mainDir); //clientInterface.sendData("delete");
                                //phrase_indices[current_phrase_index]--;
                                colorQueue.Clear();
                                depthQueue.Clear();
                                startMode = false;
                                //textFlag.Text = "Erased data, and ready!";
                                runState = 0;
                                raisedLeftHand = false;
                            }
                            else if (raisedLeftHand)
                            {
                                //Save the session data
                                startMode = false;
                                this.jointDataWriter.endPhrase(); //clientInterface.sendData("end");
                                //Console.WriteLine("\n" + this.framesCapturedInPhrase + "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE\n");
                                //saveData(colorQueue, depthQueue, dimension, minDepth, maxDepth, widthD, heightD);
                                //textFlag.Text = "Saved data, and ready!";
                                runState = 0;
                                raisedLeftHand = false;
                            }

                        }
                    }
                    else if (startRecording)
                    {
                        //Begin the data collection.
                        if (!startMode)
                        {
                            //textFlag.Text = "Started!";
                            runState = 1;
                            startMode = true;
                            //phrase_indices[current_phrase_index]++;
                            session_number++;
                            String filePath = mainDir + "\\" + session_number;
                            System.IO.Directory.CreateDirectory(filePath);
                            //clientInterface.sendData("start");
                            level.GetComponent<RoomLoader>().SetGestureDataPath(this.jointDataWriter.startNewPhrase(session_number, mainDir));
                            this.framesCapturedInPhrase = 0;
                            //clientInterface.sendData(phrase_name);
                        }
                        //textFlag.Text = "Started!";
                        runState = 1;
                        if (startRecording)
                        {
                            raisedLeftHand = true;
                        }
                    }

                }
                else
                {
                    this.jointDataWriter.pause(); //clientInterface.sendData("paused...");
                }
            }
        }

        if (dataReceived)
        {

            // we may have lost/acquired bodies, so update the corresponding gesture detectors
            if (this.bodies != null)
            {
                // loop through all bodies to see if any of the gesture detectors need to be updated
                int maxBodies = this.kinectSensor.BodyFrameSource.BodyCount;
                for (int i = 0; i < maxBodies; ++i)
                {
                    Body body = this.bodies[i];
                    ulong trackingId = body.TrackingId;

                    if (trackingId != 0)
                    {

                        String msg = prepareTcpMessage(body);
                        if (startMode)
                        {
                            //Console.WriteLine("Joints.........." + totalCapturedFrames_joints++);
                            totalCapturedFrames_joints++;
                            this.framesCapturedInPhrase++;
                        }
                        this.jointDataWriter.writeData(msg + "\n"); //clientInterface.sendData(msg);

                    }

                    // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                    if (trackingId != this.gestureDetectorList[i].TrackingId)
                    {
                        this.gestureDetectorList[i].TrackingId = trackingId;

                        // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                        // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                        this.gestureDetectorList[i].IsPaused = trackingId == 0;
                    }
                }
            }
        }
    }
    /*
    public void saveData(Queue<byte[]> colorQueue, Queue<ushort[]> depthQueue, int depthArrDimension, ushort minDepth, ushort maxDepth, int widthD, int heightD)
    {
        if (this.framesCapturedInPhrase < 25)
        {
            this.jointDataWriter.deleteLastSample(session_number, mainDir); //clientInterface.sendData("delete");
            //phrase_indices[current_phrase_index]--;

        }
        else
        {

            String filePathColor = mainDir + "\\" + session_number + "\\color\\";
            System.IO.Directory.CreateDirectory(filePathColor);

            String filePathDepth = mainDir + "\\" + session_number + "\\depth\\";
            System.IO.Directory.CreateDirectory(filePathDepth);

            int size = colorQueue.Count;
            for (int x = 0; x < size; x++)
            {
                byte[] pixels = colorQueue.Dequeue();
                //this.colorFrameWriter.ProcessWrite(pixels, session_number, mainDir);
            }

            int size2 = depthQueue.Count;
            for (int x = 0; x < size2; x++)
            {
                ushort[] pixelData = depthQueue.Dequeue();
                byte[] pixels = new byte[depthArrDimension];
                int colorIndex = 0;
                for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
                {
                    ushort depth = pixelData[depthIndex];

                    byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                    pixels[colorIndex++] = intensity; // Blue
                    pixels[colorIndex++] = intensity; // Green
                    pixels[colorIndex++] = intensity; // Red

                    ++colorIndex;
                }

                //PixelFormat format = PixelFormats.Bgr32;
                //int stride = widthD * format.BitsPerPixel / 8;
                //this.depthFrameWriter.ProcessWrite(pixels, session_number, mainDir);
            }
            //session_number++;
        }
    }
    */
    private String prepareTcpMessage(Body body)
    {
        String msg = "";

        Windows.Kinect.Joint head = body.Joints[JointType.Head];               //3
        Windows.Kinect.Joint neck = body.Joints[JointType.Neck];               //2
        Windows.Kinect.Joint shoulderr = body.Joints[JointType.ShoulderRight]; //8
        Windows.Kinect.Joint shoulderl = body.Joints[JointType.ShoulderLeft];  //4
        Windows.Kinect.Joint spinesh = body.Joints[JointType.SpineShoulder];   //20

        Windows.Kinect.Joint elbowr = body.Joints[JointType.ElbowRight];       //9
        Windows.Kinect.Joint elbowl = body.Joints[JointType.ElbowLeft];        //5
        Windows.Kinect.Joint wristr = body.Joints[JointType.WristRight];       //10
        Windows.Kinect.Joint wristl = body.Joints[JointType.WristLeft];        //6
        Windows.Kinect.Joint handr = body.Joints[JointType.HandRight];         //11
        Windows.Kinect.Joint handl = body.Joints[JointType.HandLeft];          //7
        Windows.Kinect.Joint thumbr = body.Joints[JointType.ThumbRight];       //24
        Windows.Kinect.Joint thumbl = body.Joints[JointType.ThumbLeft];        //22
        Windows.Kinect.Joint tipr = body.Joints[JointType.HandTipRight];       //23
        Windows.Kinect.Joint tipl = body.Joints[JointType.HandTipLeft];        //21

        Windows.Kinect.Joint hipr = body.Joints[JointType.HipRight];           //16
        Windows.Kinect.Joint hipl = body.Joints[JointType.HipLeft];            //12
        Windows.Kinect.Joint spinebase = body.Joints[JointType.SpineBase];     //0
        Windows.Kinect.Joint kneer = body.Joints[JointType.KneeRight];         //17
        Windows.Kinect.Joint kneel = body.Joints[JointType.KneeLeft];          //13

        double l0 = Math.Round(Math.Sqrt(Math.Pow((neck.Position.X - shoulderl.Position.X), 2) + Math.Pow((neck.Position.Y - shoulderl.Position.Y), 2) + Math.Pow((neck.Position.Z - shoulderl.Position.Z), 2)), 5);
        double r0 = Math.Round(Math.Sqrt(Math.Pow((neck.Position.X - shoulderr.Position.X), 2) + Math.Pow((neck.Position.Y - shoulderr.Position.Y), 2) + Math.Pow((neck.Position.Z - shoulderr.Position.Z), 2)), 5);
        double l1 = Math.Round(Math.Sqrt(Math.Pow((shoulderl.Position.X - elbowl.Position.X), 2) + Math.Pow((shoulderl.Position.Y - elbowl.Position.Y), 2) + Math.Pow((shoulderl.Position.Z - elbowl.Position.Z), 2)), 5);
        double r1 = Math.Round(Math.Sqrt(Math.Pow((shoulderr.Position.X - elbowr.Position.X), 2) + Math.Pow((shoulderr.Position.Y - elbowr.Position.Y), 2) + Math.Pow((shoulderr.Position.Z - elbowr.Position.Z), 2)), 5);
        double l2 = Math.Round(Math.Sqrt(Math.Pow((elbowl.Position.X - wristl.Position.X), 2) + Math.Pow((elbowl.Position.Y - wristl.Position.Y), 2) + Math.Pow((elbowl.Position.Z - wristl.Position.Z), 2)), 4);
        double r2 = Math.Round(Math.Sqrt(Math.Pow((elbowr.Position.X - wristr.Position.X), 2) + Math.Pow((elbowr.Position.Y - wristr.Position.Y), 2) + Math.Pow((elbowr.Position.Z - wristr.Position.Z), 2)), 4);

        double norm = (l0 + l1 + l2 + r0 + r1 + r2) / 2.0;

        Windows.Kinect.Joint[] joints = { head, neck, shoulderr, shoulderl, spinesh, elbowr, elbowl, wristr, wristl, handr, handl, thumbr, thumbl, tipr, tipl, hipr, hipl, spinebase, kneer, kneel };
        String msg_points = "";
        foreach (Windows.Kinect.Joint j in joints)
        {
            msg_points += "" + Math.Round(j.Position.X / norm, 5) + " " + Math.Round(j.Position.Y / norm, 5) + " " + Math.Round(j.Position.Z / norm, 5) + " ";
        }
        Console.WriteLine(msgCount++ + " | " + msg.Length);

        //------------------------------------------------------------------------------------------------------------------------------------
        //------------------------------------------------------------------------------------------------------------------------------------
        JointType[] joint_types = {
                JointType.Head,
                JointType.Neck,
                JointType.ShoulderRight,
                JointType.ShoulderLeft,
                JointType.SpineShoulder,
                JointType.ElbowRight,
                JointType.ElbowLeft,
                JointType.WristRight,
                JointType.WristLeft,
                JointType.HandRight,
                JointType.HandLeft,
                JointType.ThumbRight,
                JointType.ThumbLeft,
                JointType.HandTipRight,
                JointType.HandTipLeft,
                JointType.HipRight,
                JointType.HipLeft,
                JointType.SpineBase
            };//, JointType.KneeRight, JointType.KneeLeft };

        int joint_count = 0;
        foreach (JointType j in joint_types)
        {
            Windows.Kinect.Vector4 quat = body.JointOrientations[j].Orientation;
            double msg_w = Math.Round(quat.W, 7);
            double msg_x = Math.Round(quat.X, 7);
            double msg_y = Math.Round(quat.Y, 7);
            double msg_z = Math.Round(quat.Z, 7);
            //double msg_x = Math.Round((j.Position.X - neck.Position.X) / norm, 5);double msg_y = Math.Round((j.Position.Y - neck.Position.Y) / norm, 5);double msg_z = Math.Round((j.Position.Z - neck.Position.Z) / norm, 5);
            msg += "" + msg_w + " " + msg_x + " " + msg_y + " " + msg_z + " ";
            joint_count++;
        }
        //Console.WriteLine(msgCount++ +" | " + msg.Length + " | " + joint_count);

        msg = msg + " ||| " + msg_points;
        return msg;
    }

    private void deletePreviousSample()
    {
        this.jointDataWriter.deleteLastSample(session_number, mainDir); //clientInterface.sendData("delete");
        startMode = false;
        runState = 1;
    }
}




