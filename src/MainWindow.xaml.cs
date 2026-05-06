/** @file MainWindow.xaml.cs
 *  @brief 縮放影像檔案

 *  這個程式開啟檔案對話框選擇一個 XML 檔案，當中指定了 sprite sheet 的尺寸與分格、以及每一格應該載
    入的影像檔案名稱，然後將它合併成一張 PNG 影像並儲存檔案。

 *  @author Shu-Kai Yang (skyang@csie.nctu.edu.tw)
 *  @date 2019/8/30 */

using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows;
using Dialogs = System.Windows.Forms;

namespace TexImgSheet
{
    public partial class MainWindow : Window
    {
        /// <summary>
        ///  來自 XML 檔案的設定。
        /// </summary>
        public Int32 CellWidth = 64;
        public Int32 CellHeight = 64;
        public Int32 TexRows = 1;
        public Int32 TexCols = 1;

        public List<FileItem> TexItems = new List<FileItem>();
        public String TexDirectory = String.Empty;

        /// <summary>
        ///  產出的影像。
        /// </summary>
        public Int32 PngWidth = 64;
        public Int32 PngHeight = 64;
        public String PngFileTitle = "untitled";
        public Bitmap PngBitmap = null;

        public MainWindow()
        {   InitializeComponent();  }

        #region Parsing the XML file...
        private void PickXmlButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.OpenFileDialog dialog = new Dialogs.OpenFileDialog();
            dialog.Filter = "XML files (*.xml)|*.xml";
            dialog.RestoreDirectory = true;
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;

            /// 開啟檔案對話框，選取 XML 檔案並讀取之:
            Dialogs.DialogResult result = dialog.ShowDialog();
            if (result == Dialogs.DialogResult.OK)
            {  LoadXmlFile(dialog.FileName);  }
        }

        private void FileItemListView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                String[] droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as String[];
                if (droppedFiles.Length > 0)
                {   LoadXmlFile(droppedFiles[0]);  }
            }
        }

        /// <summary>
        ///  選取 XML 檔案並且載入項目與設定。
        /// </summary>
        private void LoadXmlFile(String pathName)
        {
            /// 解析 XML 檔案:
            XmlDocument doc = new XmlDocument();
            try {  doc.Load(pathName);  }
            catch (Exception ex)
            {   MessageBox.Show(ex.Message);  return;  }

            /// 重置內部變數:
            CellWidth = 64;
            CellHeight = 64;
            TexRows = 1;
            TexCols = 1;
            TexItems.Clear();

            PngWidth = 64;
            PngHeight = 64;
            if (PngBitmap != null)
            {   PngBitmap.Dispose(); PngBitmap = null;  }

            this.Title = pathName + " - TexImage Sheet Creator";
            TexDirectory = Path.GetDirectoryName(pathName);
            PngFileTitle = Path.GetFileNameWithoutExtension(pathName);

            /// 解析 XML 內容:
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    XmlElement element = node as XmlElement;
                    if (element.Name.Equals("canvas"))
                    {
                        if (element.HasAttribute("rows"))
                        {   TexRows = Int32.Parse(element.Attributes["rows"].Value);  }

                        if (element.HasAttribute("cols"))
                        {   TexCols = Int32.Parse(element.Attributes["cols"].Value);  }

                        if (element.HasAttribute("cellwidth"))
                        {   CellWidth = Int32.Parse(element.Attributes["cellwidth"].Value);  }

                        if (element.HasAttribute("cellheight"))
                        {   CellHeight = Int32.Parse(element.Attributes["cellheight"].Value);  }
                    }
                    else if (element.Name.Equals("sprites"))
                    {
                        foreach (XmlNode child in element.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Element)
                            {
                                XmlElement el = child as XmlElement;
                                if (el.Name.Equals("img"))
                                {
                                    FileItem item = new FileItem();
                                    item.Row = Int32.Parse(el.Attributes["row"].Value);
                                    item.Col = Int32.Parse(el.Attributes["col"].Value);
                                    item.FileName = el.Attributes["src"].Value;
                                    TexItems.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            /// 更新到畫面上:
            CellWidthTextBlock.Text = "Cell Width: " + CellWidth.ToString() + "px";
            CellHeightTextBlock.Text = "Cell Height: " + CellHeight.ToString() + "px";
            DimTextBlock.Text = TexRows.ToString() +  " row(s) x " + TexCols.ToString() + " col(s)";
            FileItemListView.ItemsSource = TexItems;
            ExportProgress.Value = 0;
        }
        #endregion

        /// <summary>
        ///  產生 sprite sheet 影像並輸出為 PNG 檔案。
        /// </summary>
        private void ExportPngButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.SaveFileDialog dialog = new Dialogs.SaveFileDialog();
            dialog.Filter = "PNG files (*.png)|*.png";
            dialog.FileName = PngFileTitle;
            dialog.DefaultExt = "png";
            dialog.OverwritePrompt = true;

            if (String.IsNullOrEmpty(TexDirectory) == false)
            {   dialog.InitialDirectory = TexDirectory;  }

            Dialogs.DialogResult result = dialog.ShowDialog();
            if (result == Dialogs.DialogResult.Cancel) {  return;  }

         /* if (String.IsNullOrEmpty(TexDirectory) == false)
            {   Directory.SetCurrentDirectory(TexDirectory);  } */

            if (PngBitmap == null)
            {
                /// 計算輸出的影像尺寸，並建立空白影像:
                PngWidth = CellWidth * TexCols;
                PngHeight = CellHeight * TexRows;
                PngBitmap = new Bitmap(PngWidth, PngHeight, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(PngBitmap))
                {
                    int doneFiles = 0;

                    foreach (FileItem item in TexItems)
                    {
                        /// 載入 FileItem 所指定的影像為 Bitmap 物件:
                        Bitmap bmp = null;
                        String pathName = Path.Combine(TexDirectory, item.FileName);
                        using (FileStream stream = new FileStream(pathName, FileMode.Open, FileAccess.Read))
                        {   bmp = (Bitmap)Image.FromStream(stream);  }

                        /// 將 Bitmap 畫在指定的位置:
                        /// 再於 outImg 中央畫上 m_bitmap:
                        int x = item.Col * CellWidth;
                        if (bmp.Width < CellWidth)
                        {   x += ((CellWidth - bmp.Width) / 2);  }

                        int y = item.Row * CellHeight;
                        if (bmp.Height < CellHeight)
                        {   y += ((CellHeight - bmp.Height) / 2);  }

                        g.DrawImage(bmp, x, y, bmp.Width, bmp.Height);

                        ++doneFiles;
                        ExportProgress.Value = (doneFiles * 100) / TexItems.Count;
                    }
                }
            }

            /// 繪製結束，輸出為 PNG 檔案:
            try {  PngBitmap.Save(dialog.FileName, ImageFormat.Png);  }
            catch (Exception ex) {  MessageBox.Show(ex.Message);  }
        }
    }
}
