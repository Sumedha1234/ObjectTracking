using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Vision.Motion;
using AForge.Vision;

namespace ObjectTracking
{
    public partial class MultiColor : Form
    {

        Bitmap video0, video1, video2, video3, tmp1, tmp2;

        RGB rgb = new RGB();
        HSL hsl = new HSL();
        int[] HueMin = new int[3];
        int[] HueMax = new int[3];
        float[] SatMin = new float[3];
        float[] SatMax = new float[3];
        float[] LumMin = new float[3];
        float[] LumMax = new float[3];


        int choice = 0;
        bool isColorFilterng_Applied = false;

        List<Bitmap> list = new List<Bitmap>();

        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
   
        public MultiColor()
        {
            InitializeComponent();
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;

            //rdiobtnSingleTracking.Checked = true;
            checkBox1.Checked = true;
            FinalFrame = new VideoCaptureDevice();

            for (int i = 0; i < 3; i++)
            {
                HueMin[i] = 0;
                HueMax[i] = 359;
                SatMin[i] = LumMin[i] = 0f;
                SatMax[i] = LumMax[i] = 1f;
            }

            trackBar1.Value = HueMin[0];
            trackBar2.Value = HueMax[0];
            trackBar3.Value = (int)(SatMin[0] * 100f);
            trackBar4.Value = (int)(SatMax[0] * 100f);
            trackBar5.Value = (int)(LumMin[0] * 100f);
            trackBar6.Value = (int)(LumMax[0] * 100f);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FinalFrame.SignalToStop();
            FinalFrame.WaitForStop();
            FinalFrame.Stop();

        }

        private void button3_Click(object sender, EventArgs e)
        {
          
            FinalFrame.Stop();
            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
        }


        void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video0 = (Bitmap)eventArgs.Frame.Clone();
            video1 = (Bitmap)eventArgs.Frame.Clone();
            video2 = (Bitmap)eventArgs.Frame.Clone();
            video3 = (Bitmap)eventArgs.Frame.Clone();


            if (checkBox1.Checked == true)
            {
                
                HSLFiltering filter = new HSLFiltering();
                filter.Hue = new IntRange((int)HueMin[0], (int)HueMax[0]);
                filter.Saturation = new Range(SatMin[0], SatMax[0]);
                filter.Luminance = new Range(LumMin[0], LumMax[0]);
                filter.ApplyInPlace(video1);
                list.Add(video1);
                
            }

            if (checkBox2.Checked == true)
            {
                
                HSLFiltering filter = new HSLFiltering();
                filter.Hue = new IntRange((int)HueMin[1], (int)HueMax[1]);
                filter.Saturation = new Range(SatMin[1], SatMax[1]);
                filter.Luminance = new Range(LumMin[1], LumMax[1]);
                filter.ApplyInPlace(video2);
                list.Add(video2);
              
            }

            if (checkBox3.Checked == true)
            {
                
                HSLFiltering filter = new HSLFiltering();
                filter.Hue = new IntRange((int)HueMin[2], (int)HueMax[2]);
                filter.Saturation = new Range(SatMin[2], SatMax[2]);
                filter.Luminance = new Range(LumMin[2], LumMax[2]);
                filter.ApplyInPlace(video3);
                list.Add(video3);
            }

             

            if (list.Count != 0)
            {
                isColorFilterng_Applied = true;

                switch (list.Count)
                {
                    case 1: 
                            tmp2 = list[0];                        
                            break;

                    case 2:                        
                            Add add1 = new Add(list[0]);
                            tmp2 = add1.Apply(list[1]);
                            break;
                                               

                    case 3:                        
                            Add add2 = new Add(list[0]);
                            tmp1 = add2.Apply(list[1]);
                            Add add3 = new Add(tmp1);
                            tmp2 = add3.Apply(list[2]);
                            break;                        
                        
                }
            }
            else
            {
                tmp2 = (Bitmap) eventArgs.Frame.Clone();
                isColorFilterng_Applied = false;
            }


            list.Clear();

            if (isColorFilterng_Applied == true)
            {

                BlobCounter blobCounter = new BlobCounter();
                blobCounter.MinWidth = 20;
                blobCounter.MinHeight = 20;
                blobCounter.FilterBlobs = true;
                blobCounter.ObjectsOrder = ObjectsOrder.Area;

                blobCounter.ProcessImage(tmp2);
                Rectangle[] rects = blobCounter.GetObjectsRectangles();

                for (int i = 0; rects.Length > i; i++)
                {

                    Rectangle objectRect = rects[i];
                    Graphics g = Graphics.FromImage(video0);
                    using (Pen pen = new Pen(Color.Red, 5))
                    {
                        g.DrawRectangle(pen, objectRect);
                        g.DrawString((i + 1).ToString(), new Font("Arial", 20), Brushes.Red, objectRect);
                    }

                    g.Dispose();
                }
                

            }


            pictureBox1.Image = video0;

            switch (choice)
            {

                case 0:
                    pictureBox2.Image = video1;
                    break;

                case 1:
                    pictureBox2.Image = video2;
                    break;

                case 2:
                    pictureBox2.Image = video3;
                    break; 
 
                case 3:
                    pictureBox2.Image = tmp2;
                    break;  

                default:
                    pictureBox2.Image = video0;
                    break;

            }

        }



        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                choice = 0;
                trackBar1.Value = HueMin[0];
                trackBar2.Value = HueMax[0];
                trackBar3.Value = (int) (SatMin[0] * 100f);
                trackBar4.Value = (int) (SatMax[0] * 100f);
                trackBar5.Value = (int) (LumMin[0] * 100f);
                trackBar6.Value = (int) (LumMax[0] * 100f);
            }

            if (comboBox3.SelectedIndex == 1)
            {
                choice = 1;
                trackBar1.Value = HueMin[1];
                trackBar2.Value = HueMax[1];
                trackBar3.Value = (int)(SatMin[1] * 100f);
                trackBar4.Value = (int)(SatMax[1] * 100f);
                trackBar5.Value = (int)(LumMin[1] * 100f);
                trackBar6.Value = (int)(LumMax[1] * 100f);
            }

            if (comboBox3.SelectedIndex == 2)
            {
                choice = 2;
                trackBar1.Value = HueMin[2];
                trackBar2.Value = HueMax[2];
                trackBar3.Value = (int)(SatMin[2] * 100f);
                trackBar4.Value = (int)(SatMax[2] * 100f);
                trackBar5.Value = (int)(LumMin[2] * 100f);
                trackBar6.Value = (int)(LumMax[2] * 100f);
            }

            if (comboBox3.SelectedIndex == 3)
            {
                choice = 3;
            }
  

        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                HueMin[0] = trackBar1.Value;
                hsl.Hue = ((int)HueMin[0] + (int)HueMax[0]) / 2;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                HueMin[1] = trackBar1.Value;
                hsl.Hue = ((int)HueMin[1] + (int)HueMax[1]) / 2;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 2)
            {
                HueMin[2] = trackBar1.Value;
                hsl.Hue = ((int)HueMin[2] + (int)HueMax[2]) / 2;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                HueMax[0] = trackBar2.Value;
                hsl.Hue = ((int)HueMin[0] + (int)HueMax[0]) / 2;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                HueMax[1] = trackBar2.Value;
                hsl.Hue = ((int)HueMin[1] + (int)HueMax[1]) / 2;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 2)
            {
                HueMax[2] = trackBar2.Value;
                hsl.Hue = ((int)HueMin[2] + (int)HueMax[2]) / 2;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
           
            if (comboBox3.SelectedIndex == 0)
            {
                LumMin[0] = trackBar5.Value / 100f;
                hsl.Luminance = (LumMax[0] + LumMin[0]) / 2f;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                LumMin[1] = trackBar5.Value / 100f;
                hsl.Luminance = (LumMax[1] + LumMin[1]) / 2f;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 2)
            {
                LumMin[2] = trackBar5.Value / 100f;
                hsl.Luminance = (LumMax[2] + LumMin[2]) / 2f;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                LumMax[0] = trackBar6.Value / 100f;
                hsl.Luminance = (LumMax[0] + LumMin[0]) / 2f;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                LumMax[1] = trackBar6.Value / 100f;
                hsl.Luminance = (LumMax[1] + LumMin[1]) / 2f;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 2)
            {
                LumMax[2] = trackBar6.Value / 100f;
                hsl.Luminance = (LumMax[2] + LumMin[2]) / 2f;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                SatMin[0] = trackBar3.Value / 100f;
                hsl.Saturation = (SatMax[0] + SatMin[0]) / 2f;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                SatMin[1] = trackBar3.Value / 100f;
                hsl.Saturation = (SatMax[1] + SatMin[1]) / 2f;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 2)
            {
                SatMin[2] = trackBar3.Value / 100f;
                hsl.Saturation = (SatMax[2] + SatMin[2]) / 2f;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0)
            {
                SatMax[0] = trackBar4.Value / 100f;
                hsl.Saturation = (SatMax[0] + SatMin[0]) / 2f;
                rgb = hsl.ToRGB();
                panel1.BackColor = rgb.Color;
            }

            else if (comboBox3.SelectedIndex == 1)
            {
                SatMax[1] = trackBar4.Value / 100f;
                hsl.Saturation = (SatMax[1] + SatMin[1]) / 2f;
                rgb = hsl.ToRGB();
                panel3.BackColor = rgb.Color;
            }

            else if(comboBox3.SelectedIndex == 2)
            {
                SatMax[2] = trackBar4.Value / 100f;
                hsl.Saturation = (SatMax[2] + SatMin[2]) / 2f;
                rgb = hsl.ToRGB();
                panel4.BackColor = rgb.Color;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FinalFrame.SignalToStop();
            FinalFrame.Stop();     
            (new Main()).Show(); this.Hide();
        }

   
    }
}
