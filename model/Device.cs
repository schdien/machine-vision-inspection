using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GxIAPINET;
using Emgu.CV;
using System.IO;
using Microsoft.Win32;

namespace IndustrialCamera
{

    class Device : INotifyPropertyChanged
    {
        public Device(IGXDeviceInfo deviceInfo)
        {
            Model = deviceInfo.GetModelName();
            SN = deviceInfo.GetSN();
            switch (deviceInfo.GetDeviceClass())
            {
                case GX_DEVICE_CLASS_LIST.GX_DEVICE_CLASS_U3V:
                    Type = "Assets/usb3.png";
                    break;
                case GX_DEVICE_CLASS_LIST.GX_DEVICE_CLASS_USB2:
                    Type = "Assets/usb2.png";
                    break;
                case GX_DEVICE_CLASS_LIST.GX_DEVICE_CLASS_GEV:
                    Type = "Assets/gige.png";
                    break;
            }
            //initImage = new BitmapImage();
            //initImage.BeginInit();
            //initImage.UriSource = new Uri(@"D:\ApplicationData\VS_Studio\IndustrialCamera\Assets\noSignal.png");
            //initImage.DecodePixelWidth = 400;
            //initImage.EndInit();
        }
        public Device()
        {
        }
        string model;
        string sn;
        string type;
        IGXDevice instance;
        IGXStream stream;
        IGXFeatureControl featureControl;
        IImageProcessConfig imageProcessConfig;
        WriteableBitmap image = null;
        IntPtr backBuffer;
        bool isThreshold;
        int thresholdParam = 127;
        bool isEdgeDetection;
        bool isDefectDetection;
 
        Emgu.CV.Util.VectorOfVectorOfPoint standardContour = new Emgu.CV.Util.VectorOfVectorOfPoint();
        double matchParam;

        bool isSizeDetection = false;
        bool isThresholdInv = false;
        string defectResult;
        string sizeResult;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, uint Length);

        public event PropertyChangedEventHandler PropertyChanged;
        public static event EventHandler ImageListViewChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        //输出图像属性
        public BitmapSource Image //直接用writeablebitmap绑定属性会造成内存泄漏，原因未知。这里使用writeablebitmap的基类型bitmapsource。
        {
            get
            { 
                //if(image == null) return initImage;
                //else return image;
                return image;
            }
            set 
            {
                //image = value;
                OnPropertyChanged("Image");
            }
        }
        //设备信息属性
        public int FrameWidth
        {
            get
            {
                return instance == null ? 0 : (int)featureControl.GetIntFeature("Width").GetValue();
            }
            set
            {
                featureControl.GetIntFeature("Width").SetValue(value);
                OnPropertyChanged("FrameWidth");
            }
        }
        public int FrameHeight
        {
            get
            {
                return instance == null ? 0 : (int)featureControl.GetIntFeature("Height").GetValue();
            }
            set
            {
                featureControl.GetIntFeature("Height").SetValue(value);
                OnPropertyChanged("FrameHeight");
            }
        }
        public string Model
        {
            get
            {
                return model;
            }
            set 
            {
                model = value;
                OnPropertyChanged("Model");
            }
            
        }
        public string SN
        {
            get
            {
                return sn;
            }
            set
            {
                sn = value;
                OnPropertyChanged("SN");
            }      
        }
        public string Type
        {
            get
            {

                return type;
            }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }
        
        public Visibility Visibility
        {
            get { return sn == null ? Visibility.Collapsed : Visibility.Visible; }
        }
        //设备控制属性
        public bool IsContinousAcquisition
        {
            set
            {
                if(value)
                {
                    AcquisitionStop();
                    featureControl.GetEnumFeature("AcquisitionMode").SetValue("Continuous");
                    AcquisitionStart();
                }
                else
                {
                    AcquisitionStop();
                }
            }
        }
        public bool IsSingleAcquisition
        {
            set
            {
                if (value)
                {
                    featureControl.GetEnumFeature("AcquisitionMode").SetValue("SingleFrame");
                    AcquisitionStart();
                }
                else
                {
                    AcquisitionStop();
                }
            }
        }

        public bool IsAcquisiting
        {
            set
            {
                if (value) AcquisitionStart();
                else
                {
                    IsEdgeDetection = false;
                    IsSizeDetection = false;
                    IsDefectDetection = false;
                    IsThreshold = false;
                    AcquisitionStop();
                }
                OnPropertyChanged("IsAcquisiting");
            }
        }
        public bool IsAcquisitionModeSingleFrame
        {
            set
            {
                if (value) featureControl.GetEnumFeature("AcquisitionMode").SetValue("SingleFrame");
                OnPropertyChanged("IsAcquisitionModeSingleFrame");
            }
            get { return instance == null ? false : featureControl.GetEnumFeature("AcquisitionMode").GetValue() == "SingleFrame" ? true : false; }
        }
        public bool IsAcquisitionModeContinuous
        {
            set
            {
                if (value) featureControl.GetEnumFeature("AcquisitionMode").SetValue("Continuous");
                OnPropertyChanged("IsAcquisitionModeContinuous");
            }
            get { return instance == null ? false : featureControl.GetEnumFeature("AcquisitionMode").GetValue() == "Continuous" ? true : false; }
        }


        public int AcquisitionSpeedLevel
        {
            set
            {
                featureControl.GetIntFeature("AcquisitionSpeedLevel").SetValue(value);
                OnPropertyChanged("AcquisitionSpeedLevel");
            }
            get { return instance == null ? 0 : (int)featureControl.GetIntFeature("AcquisitionSpeedLevel").GetValue(); }
        }
        public double ExposureTime
        {
            set
            {
                if (value >= 50f && value <= 1000000f) featureControl.GetFloatFeature("ExposureTime").SetValue(value);
                OnPropertyChanged("ExposureTime");
            }
            get { return instance == null ? 0 : featureControl.GetFloatFeature("ExposureTime").GetValue(); }
        }
        public bool IsExposureAuto
        {
            set
            {
                if (value) featureControl.GetEnumFeature("ExposureAuto").SetValue("Continuous");
                else
                {
                    featureControl.GetEnumFeature("ExposureAuto").SetValue("Off");
                    ExposureTime = ExposureTime;
                }
            }
            get
            {
                return instance == null ? false : featureControl.GetEnumFeature("ExposureAuto").GetValue() == "Continuous" ? true : false;
            }
        }

        public double Gain
        {
            set
            {
                if (value >= 9f && value <= 63f) featureControl.GetFloatFeature("Gain").SetValue(value);
                OnPropertyChanged("Gain");
            }
            get { return instance == null ? 0 : featureControl.GetFloatFeature("Gain").GetValue(); }
        }
        public bool IsGainAuto
        {
            set
            {
                if (value) featureControl.GetEnumFeature("GainAuto").SetValue("Continuous");
                else
                {
                    featureControl.GetEnumFeature("GainAuto").SetValue("Off");
                    Gain = Gain;
                }
            }
            get
            {
                return instance == null ? false : featureControl.GetEnumFeature("GainAuto").GetValue() == "Continuous"? true : false;
            }
        }

        public bool IsBalanceRatioSelectorRed
        {
            set
            {
                try { 
                if (value)
                {
                    featureControl.GetEnumFeature("BalanceRatioSelector").SetValue("Red");
                    BalanceRatio = BalanceRatio;
                }
                OnPropertyChanged("IsBalanceRatioSelectorRed");
                }
                catch { }
            }
            get
            {
                try { 
                return instance == null ? false : featureControl.GetEnumFeature("BalanceRatioSelector").GetValue() == "Red" ? true : false;
                }
                catch { return false; }
            }
        }
        public bool IsBalanceRatioSelectorGreen
        {
            set
            {
                try { 
                if (value)
                {
                    featureControl.GetEnumFeature("BalanceRatioSelector").SetValue("Green");
                    BalanceRatio = BalanceRatio;
                }
                OnPropertyChanged("IsBalanceRatioSelectorGreen");
                }
                catch { }
            }
            get
            {
                try { 
                return instance == null ? false : featureControl.GetEnumFeature("BalanceRatioSelector").GetValue() == "Green" ? true : false;
                }
                catch { return false; }
            }
        }
        public bool IsBalanceRatioSelectorBlue
        {
            set
            {
                if (value)
                {
                    try
                    {
                        featureControl.GetEnumFeature("BalanceRatioSelector").SetValue("Blue");
                        BalanceRatio = BalanceRatio;
                    }
                    catch { }
                }
                OnPropertyChanged("IsBalanceRatioSelectorBlue");
            }
            get
            { try { return instance == null ? false : featureControl.GetEnumFeature("BalanceRatioSelector").GetValue() == "Blue" ? true : false; }
                catch { return false; }
            }
        }
        public double BalanceRatio
        {
            set
            {
                try { 
                if (value >= 0.1f && value <= 5f) featureControl.GetFloatFeature("BalanceRatio").SetValue(value);
                OnPropertyChanged("BalanceRatio");
                }
                catch { }
            }
            get {
                try {
                    return instance == null ? 0 : featureControl.GetFloatFeature("BalanceRatio").GetValue();
                }
                catch { return 0.1; }
            }
        }

        public bool IsBalanceAuto
        {
            set
            {
                if (value)
                {
                    featureControl.GetEnumFeature("BalanceWhiteAuto").SetValue("Continuous");
                }
                else
                {
                    try
                    {
                        featureControl.GetEnumFeature("BalanceWhiteAuto").SetValue("Off");
                        BalanceRatio = BalanceRatio;
                    }
                    catch { }
                }
            }
            get
            {
                try { 
                return instance == null ? false : featureControl.GetEnumFeature("BalanceWhiteAuto").GetValue() == "Continuous"? true : false;
                }
                catch { return false; }
            }
        }
 


        public void AcquisitionStart()
        {
            //Image = Image;
            Image = new WriteableBitmap(FrameWidth, FrameHeight, 96, 96, PixelFormats.Bgr24, null);//这里不通知Image控件更新，在开始采集后通知
            backBuffer = image.BackBuffer;
            stream.StartGrab();
            featureControl.GetCommandFeature("AcquisitionStart").Execute();
        }
        public void AcquisitionStop()
        {
            Image = null;
            featureControl.GetCommandFeature("AcquisitionStop").Execute();
            stream.StopGrab();
        }
        //设备连接属性
        public bool IsConnected
        {
            set
            {
                if(value)
                {
                    instance = IGXFactory.GetInstance().OpenDeviceBySN(SN, GX_ACCESS_MODE.GX_ACCESS_CONTROL);
                    stream = instance.OpenStream(0);
                    featureControl = instance.GetRemoteFeatureControl();
                    stream.RegisterCaptureCallback(instance, AcquisitionHandler);                 //注册采集回调函数
                    imageProcessConfig = instance.CreateImageProcessConfig();

                    //image = new WriteableBitmap(2592, 1944, 96, 96, PixelFormats.Bgr24, null);//这里不通知Image控件更新，在开始采集后通知
                    IsAcquisitionModeContinuous = IsAcquisitionModeContinuous;
                    IsAcquisitionModeSingleFrame = IsAcquisitionModeSingleFrame;
                    AcquisitionSpeedLevel = AcquisitionSpeedLevel;
                    IsBalanceAuto = IsBalanceAuto;
                    IsGainAuto = IsGainAuto;
                    IsExposureAuto = IsExposureAuto;

                    IsBalanceRatioSelectorBlue = IsBalanceRatioSelectorBlue;
                    IsBalanceRatioSelectorGreen = IsBalanceRatioSelectorGreen;
                    IsBalanceRatioSelectorRed = IsBalanceRatioSelectorRed;
                    BalanceRatio = BalanceRatio;
                    Gain = Gain;
                    ExposureTime = ExposureTime;
                    ContrastParam = ContrastParam;
                    GammaParam = GammaParam;
                    LightnessParam = LightnessParam;
                    SaturationParam = SaturationParam;
                    imageProcessConfig.EnableColorCorrection(false);
                }
                else
                {
                    IsAcquisiting = false;
                    stream.Close();
                    imageProcessConfig.Destory();
                    instance.Close();
                    stream = null;
                    instance = null;
                    imageProcessConfig = null;
                    featureControl = null;
                    backBuffer = IntPtr.Zero;
                    //image = null;
                    OnPropertyChanged("Image");
                    GC.Collect();
                }
            }
            get { return instance != null; }
         }

        
        public int ContrastParam
        {
            get { return instance == null ? 0 : imageProcessConfig.GetContrastParam(); }
            set
            {
                imageProcessConfig.SetContrastParam(value);
                OnPropertyChanged("ContrastParam");
            }
        }
        public double GammaParam
        {
            get { return instance == null ? 0 : imageProcessConfig.GetGammaParam(); }
            set
            {
                imageProcessConfig.SetGammaParam(value);
                OnPropertyChanged("GammaParam");
            }
        }
        public int LightnessParam
        {
            get { return instance == null ? 0 : imageProcessConfig.GetLightnessParam(); }
            set
            {
                imageProcessConfig.SetLightnessParam(value);
                OnPropertyChanged("LightnessParam");
            }
        }
        /*
        public bool IsDenoise
        {
            get { return instance == null ? false : imageProcessConfig.IsDenoise(); }
            set
            {
                imageProcessConfig.EnableDenoise(value);
                OnPropertyChanged("IsDenoise");
            }
        }
        */
        public int SaturationParam
        {
            get { return instance == null ? 0 : imageProcessConfig.GetSaturationParam(); }
            set
            {
                imageProcessConfig.SetSaturationParam(value);
                OnPropertyChanged("SaturationParam");
            }
        }
        /*
        public bool IsColorCorrection
        {
            get { return instance == null ? false : imageProcessConfig.IsColorCorrection(); }
            set
            {
                imageProcessConfig.EnableColorCorrection(value);
                OnPropertyChanged("IsColorCorrection");
            }
        }
        
        public bool IsConvertFlip
        {
            get { return instance == null ? false : imageProcessConfig.IsConvertFlip(); }
            set
            {
                imageProcessConfig.EnableConvertFlip(value);
                OnPropertyChanged("IsConvertFlip");
            }
        }
        */
        public bool IsThreshold
        {
            get { return isThreshold; }
            set 
            { 
                if(value)
                    SaturationParam = 0;
                else
                    SaturationParam = 64;
                isThreshold = value;
                OnPropertyChanged("IsThreshold");
            }
        }
        public bool IsThresholdInv
        {
            get { return isThresholdInv; }
            set
            {
                isThresholdInv = value;
                OnPropertyChanged("IsThresholdInv");
            }
        }
        public int ThresholdParam
        {
            get { return thresholdParam; }
            set 
            { 
                thresholdParam = value;
                OnPropertyChanged("ThresholdParam");
            }
        }
        public bool IsEdgeDetection
        {
            get { return isEdgeDetection; }
            set
            {
                if (value)
                    SaturationParam = 0;
                else
                    SaturationParam = 64;
                isEdgeDetection = value;
                OnPropertyChanged("IsEdgeDetection");
            }
        }
        public bool IsDefectDetection
        {
            get { return isDefectDetection; }
            set
            {
                IsThreshold = value;
                isDefectDetection = value;
                OnPropertyChanged("IsDefectDetection");
            }
        }
 
        public double MatchParam
        {
            get { return matchParam; }
            set { matchParam = value; OnPropertyChanged("MatchParam"); }
        }

        public bool IsSizeDetection
        {
            get { return isSizeDetection; }
            set 
            {
                IsThreshold = value;
                isSizeDetection = value; 
                OnPropertyChanged("IsSizeDetection"); 
            }
        }

        //保存属性
        public bool IsSaving
        {
            get
            {
                return false;
            }
            set
            {
                if(value)
                {
                    string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    //保存到本地
                    string path = SaveImageToLocal();
                    if (path == "") 
                        return;
                    string name = Path.GetFileName(path);
                    string deviceName = model + "(" + sn + ")";
                    //插入信息到数据库
                    if(IsDefectDetection)
                        ImageDatabase.InsertToTable(name,date,deviceName, path, "Defect Detection:",defectResult);
                    else if(IsSizeDetection)
                        ImageDatabase.InsertToTable(name, date, deviceName, path, "Size Detection:", sizeResult);
                    else
                        ImageDatabase.InsertToTable(name, date, deviceName, path);
                    ImageListViewChanged(null,null);

                    //通知视图更新
                    OnPropertyChanged("IsSaving");
                }
            }
        }

        //采集回调函数
        private void AcquisitionHandler(object obj, IFrameData frameData)
        {
            if (frameData.GetStatus() == GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS)
            {

                int width = (int)frameData.GetWidth();
                int height = (int)frameData.GetHeight();
                
                /*
                Mat rawImg = new Mat(height ,width ,Emgu.CV.CvEnum.DepthType.Cv8U, 1, frameData.GetBuffer(), width);
                Mat rgbImg = new Mat();
                CvInvoke.CvtColor(rawImg, rgbImg,Emgu.CV.CvEnum.ColorConversion.BayerGr2Rgb);
                rawImg.Dispose();
                CopyMemory(backBuffer, rgbImg.DataPointer, (uint)(width * height * 3));
                rgbImg.Dispose();
                */
                
                IntPtr processedImageBuffer = frameData.ImageProcess(imageProcessConfig);
                int length = width * height * 3;
                /*
                if (isThreshold)
                {
                    
                    unsafe
                    {
                        byte *bufferPtr = (byte*)processedImageBuffer.ToPointer();
                        for(int i = 0; i < length; i++)
                        {
                            if (bufferPtr[i] < thresholdParam) bufferPtr[i] = 0;
                            else bufferPtr[i] = 255;
                        }
                    }
                }
                */
                /*

         */
                
                Mat cvImg = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 3, processedImageBuffer, width * 3);
                if(isThreshold)
                {
                    if(IsThresholdInv)
                        CvInvoke.Threshold(cvImg, cvImg, ThresholdParam, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
                    else
                        CvInvoke.Threshold(cvImg, cvImg, ThresholdParam, 255, Emgu.CV.CvEnum.ThresholdType.Binary);
                }
                if (isEdgeDetection)
                {
                    Mat matX = new Mat();
                    Mat matY = new Mat();
                    CvInvoke.Sobel(cvImg, matY, Emgu.CV.CvEnum.DepthType.Default, 1, 0);
                    CvInvoke.Sobel(cvImg, matX, Emgu.CV.CvEnum.DepthType.Default, 0, 1);
                    cvImg = matX + matY;
                    matX.Dispose();
                    matY.Dispose();
                }
                if (isDefectDetection)
                {
                    Emgu.CV.Util.VectorOfVectorOfPoint contour = new Emgu.CV.Util.VectorOfVectorOfPoint();
                    //Mat hierarchy = new Mat();
                    Mat grayImg = new Mat();
                    CvInvoke.CvtColor(cvImg, grayImg, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
                    CvInvoke.FindContours(grayImg, contour, null, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    grayImg.Dispose();
                    
                    if (contour.Size !=0)
                    {
                        
                        Emgu.CV.Structure.MCvScalar red = new Emgu.CV.Structure.MCvScalar(0, 0, 255);
                        defectResult = "Pass";
                        for (int i = 0; i < contour.Size; i++)
                        {
                            if (CvInvoke.ArcLength(contour[i], true) > 100)
                            {
                                defectResult = "Fail";
                                CvInvoke.DrawContours(cvImg, contour, i, red, 2);
                            }
                        }
                    }
                    contour.Dispose();
                }
                if(isSizeDetection)
                {
                    Emgu.CV.Util.VectorOfVectorOfPoint contour = new Emgu.CV.Util.VectorOfVectorOfPoint();
                    //Mat hierarchy = new Mat();
                    Mat grayImg = new Mat();
                    CvInvoke.CvtColor(cvImg, grayImg, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
                    CvInvoke.FindContours(grayImg, contour, null, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
                    grayImg.Dispose();
                    Emgu.CV.Structure.RotatedRect rect = new Emgu.CV.Structure.RotatedRect();
                    
                    if (contour.Size != 0)
                    {
                        Emgu.CV.Structure.MCvScalar green = new Emgu.CV.Structure.MCvScalar(0, 255, 0);
                        Emgu.CV.Structure.MCvScalar red = new Emgu.CV.Structure.MCvScalar(0, 0, 255);
                        int maxCount = 0;
                        double maxArea = 0;
                        for (int i = 0; i < contour.Size; i++)
                        {
                            double area = CvInvoke.ContourArea(contour[i], false);
                            if (area>1000)
                            {
                                rect = CvInvoke.MinAreaRect(contour[i]);
                                System.Drawing.PointF[] vertices = rect.GetVertices();

                                System.Drawing.Point[] intVertices = new System.Drawing.Point[4];
                                for (int j = 0; j < 4; j++)
                                {
                                    intVertices[j].X = (int)vertices[j].X;
                                    intVertices[j].Y = (int)vertices[j].Y;
                                }
                                CvInvoke.Polylines(cvImg, intVertices, true, green, 2);
                                CvInvoke.PutText(cvImg, ((int)rect.Size.Width).ToString() + "Px", new System.Drawing.Point((intVertices[0].X + intVertices[3].X) / 2, (intVertices[0].Y + intVertices[3].Y) / 2), Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.2, red, 2);
                                CvInvoke.PutText(cvImg, ((int)rect.Size.Height).ToString() + "Px", new System.Drawing.Point((intVertices[0].X + intVertices[1].X) / 2, (intVertices[0].Y + intVertices[1].Y) / 2), Emgu.CV.CvEnum.FontFace.HersheySimplex, 1.2, red, 2);
                                if (area>maxArea)
                                {
                                    maxArea = area;
                                    maxCount = i;
                                }
                            }
                        }
                        rect = CvInvoke.MinAreaRect(contour[maxCount]);
                        sizeResult = ((int)rect.Size.Width).ToString() + "*" + ((int)rect.Size.Height).ToString();
                    }
                    contour.Dispose();
                }
                
                CopyMemory(backBuffer, cvImg.DataPointer, (uint)length);
                cvImg.Dispose();

                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    image.Lock();
                    image.AddDirtyRect(new Int32Rect(0, 0, width, height));
                    image.Unlock();
                });
              
            }
        }
        //其他私有函数
        private string SaveImageToLocal()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                InitialDirectory = Directory.GetParent(Environment.CurrentDirectory.ToString()).ToString(),
                Filter = ".bmp| *.bmp|.jpg|*.jpg|.png|*.png"
            };
            dialog.ShowDialog();
            string path = dialog.FileName;
            BitmapEncoder encoder;
            switch (Path.GetExtension(path).ToLower())
            {
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    encoder= new PngBitmapEncoder();
                    break;
                default:
                    return "";
            }
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(stream);
            }
            return path;
        }
    }



    class DevicesInfo
    {
        public DevicesInfo()
        {
            IGXFactory.GetInstance().Init();
            DeviceList = new ObservableCollection<Device>();
        }
        public ObservableCollection<Device> DeviceList { get; set; }
        

        public bool UpdateDeviceInfoList()
        { 
            if (DeviceList.Count != 0)
            {
                foreach (var device in DeviceList)
                {
                    if (device.IsConnected == true)
                        device.IsConnected = false;
                }
                DeviceList.Clear();
            }
              
            List<IGXDeviceInfo> DeviceInfoList = new List<IGXDeviceInfo>();
            IGXFactory.GetInstance().UpdateAllDeviceList(200, DeviceInfoList);
            
            if (DeviceInfoList.Count != 0)
            {
                foreach (IGXDeviceInfo deviceInfo in DeviceInfoList)
                {
                    Device device = new Device(deviceInfo);
                    DeviceList.Add(device);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
