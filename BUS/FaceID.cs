using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognitionDotNet;
using System;
using System.Linq;
using System.Drawing;


namespace ql_nhanSW.BUS
{
    internal class FaceID
    {
        private VideoCapture _camera;
        private CascadeClassifier _faceDetector;
        private FaceRecognition _faceRecognition;

        public FaceID()
        {
            // Load AI model detect face
            _faceDetector = new CascadeClassifier("AI/haarcascade_frontalface_default.xml");

            // Load model tạo vector khuôn mặt
            _faceRecognition = FaceRecognition.Create("AI");

            // mở camera
            _camera = new VideoCapture(0);
        }

        //1. Lấy frame từ camera

        public Mat GetCameraFrame()
        {
            Mat frame = new Mat();
            _camera.Read(frame);
            return frame;
        }


        // 2. Detect khuôn mặt


        public Rectangle[] DetectFaces(Mat frame)
        {
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            Image<Gray, byte> gray = image.Convert<Gray, byte>();

            var faces = _faceDetector.DetectMultiScale(
                gray,
                1.1,
                5
            );

            return faces;
        }


        // 3. Vẽ khung khuôn mặt


        public Mat DrawFaceBox(Mat frame)
        {
            var faces = DetectFaces(frame);

            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();

            foreach (var face in faces)
            {
                image.Draw(face, new Bgr(0, 255, 0), 2);
            }

            return image.Mat;
        }


        // 4. Lưu ảnh khuôn mặt


        public void SaveFaceImage(Mat frame, string path)
        {
            frame.Save(path);
        }


        // 5. Tạo vector khuôn mặt

        public float[] GetFaceVector(string imagePath)
        {
            var image = FaceRecognition.LoadImageFile(imagePath);

            var encoding = _faceRecognition
                            .FaceEncodings(image)
                            .FirstOrDefault();

            if (encoding == null)
                return null;

            return encoding
                    .GetRawEncoding()
                    .Select(x => (float)x)
                    .ToArray();
        }


        // 6. So sánh vector khuôn mặt


        public double CompareFaceVector(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null)
                return 999;

            double sum = 0;

            for (int i = 0; i < v1.Length; i++)
            {
                sum += Math.Pow(v1[i] - v2[i], 2);
            }

            return Math.Sqrt(sum);
        }


        // 7. Kiểm tra cùng người

        public bool IsSamePerson(float[] inputVector, float[] dbVector)
        {
            double distance = CompareFaceVector(inputVector, dbVector);

            // ngưỡng nhận diện
            if (distance < 0.6)
                return true;

            return false;
        }

        // 8. Logic Check-In / Check-Out
        //---------------------------------------

        public bool CheckEmployee(string inputImage, float[] dbVector)
        {
            float[] inputVector = GetFaceVector(inputImage);

            if (inputVector == null)
                return false;

            return IsSamePerson(inputVector, dbVector);
        }
        public Mat ScanFace()
        {
            Mat frame = new Mat();
            _camera.Read(frame);

            var image = frame.ToImage<Bgr, byte>();
            var gray = image.Convert<Gray, byte>();

            var faces = _faceDetector.DetectMultiScale(gray, 1.1, 5);

            foreach (var face in faces)
            {
                image.Draw(face, new Bgr(0, 255, 0), 2);
            }

            return image.Mat;
        }
    }
}
