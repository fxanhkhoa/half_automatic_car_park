using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Keras.Models;
using Keras.PreProcessing.Image;

using Emgu.CV;
using Emgu.CV.Structure;

using Numpy;
using Python;

namespace test_yolo_plate
{
    public partial class Form1 : Form
    {
        BaseModel model;
        int net_h = 416;
        int net_w = 416; // a multiple of 32, the smaller the faster
        double obj_thresh = 0.5, nms_thresh = 0.45;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            model = loadModel(); // Load H5
            predict();
        }

        private BaseModel loadModel()
        {
            var inferModel = Model.LoadModel("plate_weight.h5");
            return inferModel;
        }

        private void GetYoloBoxes(BaseModel model, Image<Bgr, Byte>[] img, int net_h, int net_w, int[] anchors, double obj_thresh, double nms_thresh)
        {
            int img_h = img[0].Height;
            int img_w = img[0].Width;
            int nb_images = img.Count();

            var batch_input = np.zeros(nb_images, net_h, net_w, 3);

            for (int i = 0; i < nb_images; i++)
            {
                batch_input[i] = preprocess_input(img[i], net_h, net_w);
            }

            var batch_output = model.PredictOnBatch(batch_input);

            for (int i = 0; i < nb_images; i++)
            {
                NDarray[] yolos = new NDarray[3] { batch_output[0][i], batch_output[1][i], batch_output[2][i] };
                foreach (NDarray n in yolos)
                {
                    Console.WriteLine(n);
                }
            }
        }
        
        private NDarray preprocess_input(Image<Bgr, Byte> img, int net_h, int net_w)
        {
            int new_h = img.Height;
            int new_w = img.Width;

            if (((double)(net_w) / new_w) < ((double)(net_h) / new_h))
            {
                new_h = ((new_h * net_w) / new_w);
                new_w = net_w;
            }
            else
            {
                new_w = (new_w * net_h) / new_h;
                new_h = net_h;
            }

            Image<Bgr, Byte> resized = img.Resize(new_w, new_h, Emgu.CV.CvEnum.Inter.Linear);
            var temp_resized = resized.Bytes;
            var new_image = np.ones(net_h, net_w, 3) * 0.5;
            new_image["(net_h-new_h)//2:(net_h+new_h)//2, (net_w-new_w)//2:(net_w+new_w)//2, :"] = temp_resized;
            new_image = np.expand_dims(new_image, 0);

            return new_image;
        }

        private void predict()
        {
            Image<Bgr, Byte> img = new Image<Bgr, Byte>("0000_02187_b.jpg");
            Image<Bgr, Byte>[] imgs = new Image<Bgr, byte>[1] { img };
              int[] anchors = new int[18] { 13, 18, 19, 28, 56, 74, 63, 85, 66, 91, 70, 94, 72, 99, 75, 105, 79, 112 };
            GetYoloBoxes(model, imgs, net_h, net_w, anchors, obj_thresh, nms_thresh);
        }
    }
}
