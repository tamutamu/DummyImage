﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DummyImg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.rdoImage.Checked = true;
        }

        private async Task doGenerate(string type)
        {
            await Task.Run(() =>
            {
                this.Invoke(new Action(() =>
                    {
                        progressBar2.Value = 0;
                        progressBar2.Minimum = 0;
                        progressBar2.Maximum = txtImageList.Lines.Length;

                        foreach (var text in txtImageList.Lines)
                        {
                            if (text.Length > 0 && !text.Equals("\\N"))
                            {
                                if (type == "image")
                                {
                                    generateImage(text, text);
                                }
                                else
                                {
                                    generatePdf(text, text);
                                }
                                progressBar2.Value = progressBar2.Value + 1;
                            }
                        }
                        progressBar2.Value = progressBar2.Maximum;
                    }
                    ));
            });
        }

        private void generateImage(string fileName, string drawText)
        {
            const int fontSize = 90;
            const string fontFamily = "MS UI Gothic";
            const int width = 300;  // 画像の幅
            const int height = 300; // 画像の高さ
            const int margin = 0; // マージン

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (Font fnt = new Font(fontFamily, fontSize))
                using (Pen bluePen = new Pen(Color.DeepSkyBlue, margin))
                using (Pen grayPen = new Pen(Color.Gainsboro, margin))
                {
                    // 背景
                    Rectangle bgRect = new Rectangle(0, 0, width, height);
                    g.FillRectangle(Brushes.White, bgRect);

                    // 枠
                    grayPen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    Rectangle bdsRect = new Rectangle(margin + 6, margin + 6, width - margin * 2, height - margin * 2);
                    g.DrawRectangle(grayPen, bdsRect);
                    bluePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    Rectangle bdRect = new Rectangle(margin, margin, width - margin * 2, height - margin * 2);
                    g.DrawRectangle(bluePen, bdRect);
                    g.FillRectangle(Brushes.DeepSkyBlue, bdRect);

                    // テキスト
                    g.DrawString(drawText, fnt, Brushes.White, bdRect);
                }

                bmp.Save(System.IO.Path.Combine(txtOutput.Text, fileName), System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void generatePdf(string fileName, string drawText)
        {
            var md = $@"
# {drawText}
";
            var byteArray = Encoding.UTF8.GetBytes(md);
            var stream = new MemoryStream(byteArray);

            using (var reader = new System.IO.StreamReader(stream))
            using (var writer = new System.IO.StreamWriter("result.html"))
            {
                CommonMark.CommonMarkConverter.Convert(reader, writer);
            }

            var html = CommonMark.CommonMarkConverter.Convert(md);
            //Console.WriteLine(html);
            //Console.ReadLine();

            Byte[] result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, PdfSharp.PageSize.A4);
                pdf.Save(ms);
                result = ms.ToArray();
            }
            using (BinaryWriter w = new BinaryWriter(File.OpenWrite(System.IO.Path.Combine(txtOutput.Text, fileName))))
            {
                w.Write(result);
            }
        }


        private void btnFldOutput_Click(object sender, EventArgs e)
        {
            if (fldOutput.ShowDialog(this) == DialogResult.OK)
            {
                txtOutput.Text = fldOutput.SelectedPath;
            }
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            if (this.rdoImage.Checked)
            {
                await doGenerate("image");
            }
            else
            {
                await doGenerate("pdf");
            }

            MessageBox.Show("完了");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
