/** @file FileItem.cs
 *  @brief 影像檔案項目

 *  這個類別包裝了來自 XML 檔案所指定的 sprite 影像項目，包括它的檔名以及在 sheet 中的位置。

 *  @author Shu-Kai Yang (skyang@csie.nctu.edu.tw)
 *  @date 2019/8/30 */

using System;

namespace TexImgSheet
{
    public class FileItem
    {
        /// <summary>
        ///  從 XML 檔案中讀取到的項目。
        /// </summary>
        public Int32 Row {  get; set;  }
        public Int32 Col {  get; set;  }
        public String FileName {  get; set;  }
    }
}
