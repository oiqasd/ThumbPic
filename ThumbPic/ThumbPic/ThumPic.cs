using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO; 
using System.Text; 
using System.Windows.Forms;

namespace ThumbPic
{
    public partial class FrmThumPic : Form
    {

        string TrasforPath = AppDomain.CurrentDomain.BaseDirectory + "\\ThunbPic\\";

        public FrmThumPic()
        {
            if (!Directory.Exists(TrasforPath))
            {
                Directory.CreateDirectory(TrasforPath);
            }
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFile();

        }

        private void btnTrans_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFrom.Text))
            {
                OpenFile();
                return;
            }
            btnTrans.Enabled = false;
            if (IsDictionary(txtFrom.Text))
            {
                FindFile(txtFrom.Text);
            }
            else
            {
                DoThum(txtFrom.Text);
            }
            btnTrans.Enabled = true;
        }

        void FindFile(string folder)
        {
            DirectoryInfo TheFolder = new DirectoryInfo(folder);
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                FindFile(NextFolder.FullName);
            }

            FileInfo[] fileInfo = TheFolder.GetFiles();
            foreach (FileInfo NextFile in fileInfo)
            { 
                DoThum(NextFile.FullName);
            }
        }
        void DoThum(string fileFullName)
        {
            int dWidth = 0, dHeight = 0;
            string dFile = "";

            FileInfo fileinfo = new FileInfo(fileFullName);
            if (fileinfo.Attributes == FileAttributes.SparseFile)
            {
                LogHelper.Error(fileFullName, "是无效文件");
                return;
            }

            var fileExt = CheckFile(fileFullName);
            if (fileExt != FileExtension.GIF && fileExt != FileExtension.JPG && fileExt != FileExtension.PNG)
            {
                LogHelper.Error(fileFullName, "是无效文件(GIF,JPG,PNG)");
                return;
            }
            dFile = TrasforPath + fileinfo.Name ;

            long maxlength = Convert.ToInt64(txtWhere.Text) * 1024 * 1024;
            if(fileinfo.Length<=maxlength)
            {
                File.Copy(fileFullName, dFile, true);
                return;
            }


            int flag = Convert.ToInt32(txtFlag.Text);
            
            Image iSource = Image.FromFile(fileFullName);
            ImageFormat tFormat = iSource.RawFormat;

            int sW = 0, sH = 0;

            //按比例缩放

            Size tem_size = new Size(iSource.Width, iSource.Height);

            //if (tem_size.Width > dHeight || tem_size.Width > dWidth) //将**改成c#中的或者操作符号
            //{

            //    if ((tem_size.Width * dHeight) > (tem_size.Height * dWidth))
            //    {

            //        sW = dWidth;

            //        sH = (dWidth * tem_size.Height) / tem_size.Width;

            //    }

            //    else
            //    {

            //        sH = dHeight;

            //        sW = (tem_size.Width * dHeight) / tem_size.Height;

            //    }

            //}

            //else
            //{

            sW = tem_size.Width;

            sH = tem_size.Height;

            //}
            //保持原大小
            dWidth = tem_size.Width; dHeight = tem_size.Height;

            //Bitmap ob = new Bitmap(tem_size, dHeight);
            Bitmap ob = new Bitmap(iSource.Width, iSource.Height);

            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);

            g.CompositingQuality = CompositingQuality.HighQuality;

            g.SmoothingMode = SmoothingMode.HighQuality;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();

            //以下代码为保存图片时，设置压缩质量

            EncoderParameters ep = new EncoderParameters();

            long[] qy = new long[1];

            qy[0] = flag;//设置压缩的比例1-100

            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);

            ep.Param[0] = eParam;

            try
            {

                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();

                ImageCodecInfo jpegICIinfo = null;

                for (int x = 0; x < arrayICI.Length; x++)
                {

                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {

                        jpegICIinfo = arrayICI[x];

                        break;

                    }

                }

                if (jpegICIinfo != null)
                {
                    ob.Save(dFile, jpegICIinfo, ep);//dFile是压缩后的新路径 
                }

                else
                {
                    ob.Save(dFile, tFormat);
                }

            }

            catch
            {
                LogHelper.Error("转换失败，", fileFullName);

            }

            finally
            {

                iSource.Dispose();

                ob.Dispose();

            }


        }

        /// <summary>
        /// 选择文件
        /// </summary>
        void OpenFile()
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "选择图片地址";
            fdlg.InitialDirectory = @"C:\";  //@是取消转义字符的意思  
            //fdlg.Filter = "All files（*.*）|*.*|All files(*.*)|*.* ";

            /* 
            * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项 
            * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个. 
            */
            //fdlg.FilterIndex = 2;
            /* 
             *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录， 
             *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的   
             */
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txtFrom.Text = fdlg.FileName;// System.IO.Path.GetFileNameWithoutExtension(fdlg.FileName);

            }
        }

        /// <summary>
        /// 判断是否路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsDictionary(string path)
        {
            FileInfo file = new FileInfo(txtFrom.Text);
            if (file.Attributes == FileAttributes.Directory)
            {
                return true;
            }

            return false;
        }

        public enum FileExtension
        {
            JPG = 255216,
            GIF = 7173,
            PNG = 13780,
            SWF = 6787,
            RAR = 8297,
            ZIP = 8075,
            _7Z = 55122,
            TEXT = 99999999,
            VALIDFILE = 999999999
        }

        /// <summary>
        /// 图片类型
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        FileExtension CheckFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            string fileType = string.Empty; ;
            try
            {
                byte data = br.ReadByte();
                fileType += data.ToString();
                data = br.ReadByte();
                fileType += data.ToString();
                FileExtension extension;
                try
                {
                    extension = (FileExtension)Enum.Parse(typeof(FileExtension), fileType);
                }
                catch
                {
                    try
                    {
                        bool isTextFile = true;
                        int i = 0;
                        int length = (int)fs.Length;
                        data = new byte();
                        while (i < length && isTextFile)
                        {
                            data = (byte)fs.ReadByte();
                            isTextFile = (data != 0);
                            i++;
                        }
                        if (isTextFile)
                        {
                            extension = FileExtension.TEXT;
                        }
                        else
                            extension = FileExtension.VALIDFILE;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("ThumPic", "CheckFile", ex);
                        extension = FileExtension.VALIDFILE;
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Close();
                            br.Close();
                        }
                    }
                }
                return extension;
            }
            catch (Exception ex)
            {
                LogHelper.Error("ThumPic", "CheckFile", ex);
                return FileExtension.VALIDFILE;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    br.Close();
                }
            }
        }


    }
}
