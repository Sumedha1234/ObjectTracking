using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Vision.Motion;
using AForge.Vision;


namespace ObjectTracking
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        Graphics g;
        Bitmap video,video2;
        int minheight=15,minwidth=15, trackchoice=0;
        short range=80;
        private Color color;
        int HueMax, HueMin;
        float SatMax, SatMin, LumMax, LumMin;
        RGB rgb = new RGB();
        HSL hsl= new HSL();
    

         Difference differenceFilter = new Difference();
         Threshold thresholdFilter = new Threshold(15);
         IFilter erosionFilter = new Erosion();
         Merge mergeFilter = new Merge();

         IFilter extrachChannel = new ExtractChannel(RGB.R);

         Bitmap backgroundImage = null;
         Bitmap currentImage;

         int x1, y1, x2, y2,wdth,hgth;
         bool track;
         int counter;



        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }


        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;

        private void Form1_Load(object sender, EventArgs e)
        {
            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
            {
                comboBox1.Items.Add(Device.Name);
            }

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            radioButton1.Checked = true;
            rdiobtnSingleTracking.Checked = true;
            numericUpDown1.Value = range;

            FinalFrame = new VideoCaptureDevice();

            HueMin= 0;
            HueMax= 359;
            SatMin= LumMin = 0f;
            SatMax= LumMax = 1f;

            trackBar1.Value = HueMin;
            trackBar2.Value = HueMax;
            trackBar3.Value = (int)(SatMin * 100f);
            trackBar4.Value = (int)(SatMax * 100f);
            trackBar5.Value = (int)(LumMin * 100f);
            trackBar6.Value = (int)(LumMax * 100f);
        }


        private void button1_Click(object sender, EventArgs e)
        {
           FinalFrame.SignalToStop();
           FinalFrame.Stop();

            FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
            FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
            FinalFrame.Start();
            
        }

       
        void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            video = (Bitmap)eventArgs.Frame.Clone();
            video2 = (Bitmap)eventArgs.Frame.Clone();     
                  
            //Color Tracking
            if (trackchoice==0)
            {
                if (radioButton1.Checked == true)
                {
                    HSLFiltering filter = new HSLFiltering();
                    filter.Hue = new IntRange((int)HueMin, (int)HueMax);
                    filter.Saturation = new Range(SatMin, SatMax);
                    filter.Luminance = new Range(LumMin, LumMax);
                    filter.ApplyInPlace(video2);
                    objec(video2);
                }
                if (radioButton2.Checked == true)
                {
                    // create filter
                    EuclideanColorFiltering filter = new EuclideanColorFiltering();
                    // set center colol and radius
                    filter.CenterColor = rgb;
                    filter.Radius = range;
                    // apply the filter
                    filter.ApplyInPlace(video2);
                    objec(video2);
                    
                }
            }


           // motion Tracking
            if (trackchoice==1)
            {
               
                currentImage = (Bitmap)eventArgs.Frame.Clone();
                Bitmap tmpimage = Grayscale.CommonAlgorithms.BT709.Apply(currentImage), copyCurrentImage = currentImage;

                if (backgroundImage != null)
                {

                    ThresholdedEuclideanDifference thd = new ThresholdedEuclideanDifference(30);
                    thd.OverlayImage = backgroundImage;
                    tmpimage = thd.Apply(tmpimage);
                }

                BlobCounter blobcounter = new BlobCounter();
                blobcounter.MinHeight = minheight;
                blobcounter.MinWidth = minwidth;
                blobcounter.FilterBlobs = true;
                blobcounter.ObjectsOrder = ObjectsOrder.Area;
                blobcounter.ProcessImage(tmpimage);

                Rectangle[] rects = blobcounter.GetObjectsRectangles();
                foreach (Rectangle rect in rects)
                {
                    if (rects.Length>=5)
                    {
                        Rectangle objectRect = rects[0];
                        g = Graphics.FromImage(currentImage);
                        using (Pen pen = new Pen(Color.Red, 6))
                        {
                            g.DrawRectangle(pen, objectRect);
                        }
                        g.Dispose();
   
                    }
                }


                backgroundImage = Grayscale.CommonAlgorithms.BT709.Apply(copyCurrentImage);

                pictureBox1.Image = copyCurrentImage;
                pictureBox2.Image = tmpimage;
            }


            if (trackchoice==2)
            {

                if (radioButton1.Checked == true)
                {
                    HSLFiltering filter = new HSLFiltering();
                    filter.Hue = new IntRange((int)HueMin, (int)HueMax);
                    filter.Saturation = new Range(SatMin, SatMax);
                    filter.Luminance = new Range(LumMin, LumMax);
                    filter.ApplyInPlace(video2);
                    //objec(video2);
                }
                if (radioButton2.Checked == true)
                {
                    // create filter
                    EuclideanColorFiltering filter = new EuclideanColorFiltering();
                    // set center colol and radius
                    filter.CenterColor = rgb;
                    filter.Radius = range;       //filter.Radius = 100;
                    // apply the filter
                    filter.ApplyInPlace(video2);
                   // objec(video2);

                }
                
                BlobCounter blobCounter = new BlobCounter();
                blobCounter.MinWidth = minwidth;
                blobCounter.MinHeight = minheight;
                blobCounter.FilterBlobs = true;
                blobCounter.ObjectsOrder = ObjectsOrder.Size;


                blobCounter.ProcessImage(video2);
                Rectangle[] recs = blobCounter.GetObjectsRectangles();

                pictureBox2.Image = video2;

                foreach (Rectangle rect in recs)
                {
                    if (recs.Length >= 0)
                    {
                        Rectangle objectRect = recs[0];

                        x1 = objectRect.X;
                        y1 = objectRect.Y;
                        wdth = objectRect.Width;
                        hgth = objectRect.Height;

                    }
                }


                currentImage = (Bitmap)eventArgs.Frame.Clone();
                Bitmap tmpimage = Grayscale.CommonAlgorithms.BT709.Apply(currentImage), copyCurrentImage = currentImage;

                if (backgroundImage != null)
                {

                    ThresholdedEuclideanDifference thd = new ThresholdedEuclideanDifference(50);
                    thd.OverlayImage = backgroundImage;
                    tmpimage = thd.Apply(tmpimage);


                    BlobCounter blobcounter = new BlobCounter();
                    blobcounter.MinHeight = minheight;
                    blobcounter.MinWidth = minwidth;
                    blobcounter.FilterBlobs = true;
                    blobcounter.ObjectsOrder = ObjectsOrder.Area;
                    blobcounter.ProcessImage(tmpimage);

                    Rectangle[] rects = blobcounter.GetObjectsRectangles();
                    foreach (Rectangle rect in rects)
                    {
                        if (rects.Length >= 5) //
                        {
                            Rectangle objectRect = rects[0];

                            x2 = objectRect.X;
                            y2 = objectRect.Y;


                            if ((x1 - 50 < x2) && (x2 < x1 + 50))
                            {
                                if ((y1 - 50 < y2) && (y2 < y1 + 50))
                                {
                                    track = true;
                                    counter = 25;
                                }
                            }
                            else
                            {
                                counter--;
                            }
                                                         

                            if (counter == 0)
                                track = false;

                            if(track==true && counter>0)
                            {
                                g = Graphics.FromImage(copyCurrentImage);
                                using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 6))
                                {
                                    g.DrawRectangle(pen, x1, y1, wdth, hgth);
                                }
                             
                                g.Dispose();

                            }

                            if (CheckCordinates.Checked)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    richTextBox1.Text = "X=["+ x1 + "], Y=["+ y1+"]" +"\n" + richTextBox1.Text + "\n"; ;
                                });
                            }
                        }
                    }

                }

                backgroundImage = Grayscale.CommonAlgorithms.BT709.Apply(copyCurrentImage);
                pictureBox1.Image = copyCurrentImage;
            }
        }


        public void objec(Bitmap video2)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.MinWidth = minwidth;
            blobCounter.MinHeight = minheight;
            blobCounter.FilterBlobs = true;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;

            blobCounter.ProcessImage(video2);
            Rectangle[] rects = blobCounter.GetObjectsRectangles();
            pictureBox2.Image = video2;



            if (rdiobtnSingleTracking.Checked)
            {

                //  Single Object Tracking
                foreach (Rectangle rect in rects)
                {
                    if (rects.Length >= 0)
                    {
                        Rectangle objectRect = rects[0];
                       
                        g = Graphics.FromImage(video);
                        using (Pen pen = new Pen(Color.Red, 7))
                        {
                            g.DrawRectangle(pen, objectRect);
                        }
                        
                        g.Dispose();

                        x1 = objectRect.X + (objectRect.Width / 2);
                        y1 = objectRect.Y + (objectRect.Height / 2);

                        if (CheckCordinates.Checked)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                richTextBox1.Text = objectRect.Location.ToString() + "\n" + richTextBox1.Text + "\n"; ;
                            });
                        }
                    }
                    
                }

            }



            if (rdiobtnMultiTracking.Checked)
            {
                //Multi Object tracking

                for (int i = 0; rects.Length > i; i++)
                {
                    Rectangle objectRect = rects[i];
                   
                    g = Graphics.FromImage(video);
                    using (Pen pen = new Pen(Color.Red, 7))
                    {
                        g.DrawRectangle(pen, objectRect);
                        g.DrawString((i + 1).ToString(), new Font("Arial", 28), Brushes.Red, objectRect);
                    }
                    g.Dispose();

                    if (CheckCordinates.Checked)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            richTextBox1.Text = objectRect.Location.ToString() + "\n" + richTextBox1.Text + "\n"; ;
                        });
                    }

                }
            }
            pictureBox1.Image = video;
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (FinalFrame.IsRunning == true)
            {
                FinalFrame.SignalToStop();
                FinalFrame.WaitForStop();
                FinalFrame.Stop();
            }
        }


        //Save
        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                pictureBox2.Image.Save("Filtering.bmp", ImageFormat.Jpeg);
                pictureBox1.Image.Save("Tracking.bmp", ImageFormat.Jpeg);
            }
            else
            {
                MessageBox.Show("Start Capture First");
            }
        }


        public Pen pen { get; set; }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            color = colorDialog1.Color;
            textBox1.BackColor = color;    
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            HueMin = trackBar1.Value;
            hsl.Hue = ((int)HueMin + (int)HueMax) / 2;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            HueMax = trackBar2.Value;
            hsl.Hue = ((int)HueMin + (int)HueMax) / 2;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            SatMin = trackBar3.Value/100f;
            hsl.Saturation = (SatMax + SatMin) / 2f;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            SatMax = trackBar4.Value/100f;
            hsl.Saturation = (SatMax + SatMin) / 2f;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            LumMin = trackBar5.Value/100f;
            hsl.Luminance = (LumMax + LumMin) / 2f;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }


        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            LumMax = trackBar6.Value/100f;
            hsl.Luminance = (LumMax + LumMin) / 2f;
            rgb = hsl.ToRGB();
            textBox1.BackColor = rgb.Color;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (FinalFrame.IsRunning)
            {
                FinalFrame.SignalToStop();
                FinalFrame.WaitForStop();
                FinalFrame.Stop();
            }
        }

        private void minheightnumeric_ValueChanged(object sender, EventArgs e)
        {
            minheight =(int)minheightnumeric.Value;
        }

        private void minwidthnumeric_ValueChanged(object sender, EventArgs e)
        {
            minwidth = (int)minwidthnumeric.Value;
        }

        private void CheckCordinates_CheckedChanged(object sender, EventArgs e)
        {
            if (!CheckCordinates.Checked) 
            {
                richTextBox1.Text = "";
            }
        }


        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == 0)
                trackchoice = 0;
            else if (comboBox2.SelectedIndex == 1)
                trackchoice = 1;
            else
                trackchoice = 2;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Hide();
            groupBox1.Show();
            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Hide();
            groupBox3.Show();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            rgb.Red = (byte)trackBar7.Value;
            textBox1.BackColor = rgb.Color;
            
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            rgb.Green =(byte)trackBar8.Value;
            textBox1.BackColor = rgb.Color;
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            rgb.Blue =(byte) trackBar9.Value;
            textBox1.BackColor = rgb.Color;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            range = (short)numericUpDown1.Value;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            
            FinalFrame.SignalToStop();
            FinalFrame.Stop();
            (new MultiColor()).Show(); this.Hide();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            rgb.Color = colorDialog1.Color;
            trackBar7.Value = rgb.Red;
            trackBar8.Value = rgb.Green;
            trackBar9.Value = rgb.Blue;
            textBox1.BackColor = rgb.Color;
        }

        
    } 
}
