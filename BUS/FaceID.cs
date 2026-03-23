using Emgu.CV;
using Emgu.CV.Structure;
using FaceRecognitionDotNet;
using System;
using System.Drawing;
using System.Linq;

namespace ql_nhanSW.BUS
{
    internal class FaceID
    {
        private CascadeClassifier _faceDetector;
        private FaceRecognition _faceRecognition;

        public FaceID()
        {
            // Model detect face (OpenCV)
            _faceDetector = new CascadeClassifier("AI/haarcascade_frontalface_default.xml");

            // Model tạo vector khuôn mặt
            _faceRecognition = FaceRecognition.Create("AI");
        }

        // 1. Detect khuôn mặt
        public Rectangle[] DetectFaces(Mat frame)
        {
            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
            Image<Gray, byte> gray = image.Convert<Gray, byte>();

            return _faceDetector.DetectMultiScale(gray, 1.1, 5);
        }


        // 2. Vẽ khung xanh quanh mặt

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


        // 3. Lưu ảnh khuôn mặt

        public void SaveFace(Mat frame, string path)
        {
            frame.Save(path);
        }


        // 4. Tạo vector khuôn mặt

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


        // 5. So sánh vector khuôn mặt

        public double CompareFace(float[] v1, float[] v2)
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


        // 6. Kiểm tra cùng người

        public bool IsSamePerson(float[] inputVector, float[] dbVector)
        {
            double distance = CompareFace(inputVector, dbVector);

            return distance < 0.6; // ngưỡng nhận diện
        }
    }
}