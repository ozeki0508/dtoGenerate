using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DtoGenerate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.dtoView.RowCount = 30;
        }

        /// <summary>
        /// Dto作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            IWriteFile writter = new WriteDtoFile();
            this.CreateFile(writter);
        }

        /// <summary>
        /// Property作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            IWriteFile writter = new WritePropertyFile();
            this.CreateFile(writter);
        }

        /// <summary>
        /// ファイル作成
        /// </summary>
        /// <param name="writter"></param>
        private void CreateFile(IWriteFile writter)
        {
            try
            {
                // ダイアログ表示
                var dialog = new SaveFileDialog();
                dialog.Filter = "(*.txt)|*.txt";
                //dialog.DefaultExt = "txt";
                dialog.FileName = "Dto1.txt";
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog() != DialogResult.OK) return;

                // 保存
                Stream stream = null;

                using (stream = dialog.OpenFile())
                {
                    if (stream != null)
                    {
                        using (var sw = new StreamWriter(stream, Encoding.GetEncoding("UTF-8")))
                        {
                            for (int i = 0; i < this.dtoView.Rows.Count; i++)
                            {
                                var strType = (string)this.dtoView.Rows[i].Cells[0].Value;
                                var strPhygNm = (string)this.dtoView.Rows[i].Cells[1].Value;
                                var strLgNm = (string)this.dtoView.Rows[i].Cells[2].Value;

                                if (string.IsNullOrWhiteSpace(strType)) break;

                                strType = strType == "NUMERIC" || strType == "DECIMAL" ? "int" : "string";
                                var item = new WriteItem()
                                {
                                    TYPE = strType,
                                    PHYGIC_NM = strPhygNm,
                                    LOGIC_NM = strLgNm
                                };

                                writter.Write(sw, item);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "エラー");
            }
        }

        /// <summary>
        /// キーダウン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            // 編集中のカラム番号
            int _editingColumn;

            DataGridView dgv = (DataGridView)(sender);
            int x = dgv.CurrentCellAddress.X;
            int y = dgv.CurrentCellAddress.Y;

            if (e.Control && e.KeyCode == Keys.V)
            {
                // クリップボードの内容を取得
                string clipText = Clipboard.GetText();
                // 改行を変換
                clipText = clipText.Replace("\r\n", "\n");
                clipText = clipText.Replace("\r", "\n");
                // 改行で分割
                string[] lines = clipText.Split('\n');

                int r;
                bool nflag = true;
                for (r = 0; r <= lines.GetLength(0) - 1; r++)
                {
                    // 最後のNULL行をコピーするかどうか
                    if (r >= lines.GetLength(0) - 1 &&
                        "".Equals(lines[r]) && nflag == false) break;
                    if ("".Equals(lines[r]) == false) nflag = false;

                    // タブで分割
                    string[] vals = lines[r].Split('\t');

                    // 各セルの値を設定
                    int c = 0;
                    int c2 = 0;
                    for (c = 0; c <= vals.GetLength(0) - 1; c++)
                    {
                        // セルが存在しなければ貼り付けない
                        if (!(x + c2 >= 0 && x + c2 < dgv.ColumnCount &&
                            y + r >= 0 && y + r < dgv.RowCount))
                        {
                            continue;
                        }
                        // 非表示セルには貼り付けない
                        if (dgv[x + c2, y + r].Visible == false)
                        {
                            c = c - 1;
                            continue;
                        }
                        //// 貼り付け処理(入力可能文字チェック無しの時)------------
                        //// 行追加モード&(最終行の時は行追加)
                        //if (y + r == dgv.RowCount - 1 &&
                        //    dgv.AllowUserToAddRows == true)
                        //{
                        //    dgv.RowCount = dgv.RowCount + 1;
                        //}
                        //// 貼り付け
                        //dgv[x + c2, y + r].Value = vals[c];
                        //// ------------------------------------------------------
                        // 貼り付け処理(入力可能文字チェック有りの時)------------
                        string pststr = "";
                        for (int i = 0; i <= vals[c].Length - 1; i++)
                        {
                            _editingColumn = x + c2;
                            byte[] cc = Encoding.GetEncoding(
                                "SHIFT-JIS").GetBytes(vals[c].Substring(i, 1));
                            KeyPressEventArgs tmpe = new KeyPressEventArgs((char)cc[0]);
                            tmpe.Handled = false;
                            //DataGridViewPlus_CellKeyPress(sender, tmpe);
                            if (tmpe.Handled == false)
                            {
                                pststr = pststr + vals[c].Substring(i, 1);
                            }
                        }
                        // 行追加モード＆最終行の時は行追加
                        if (y + r == dgv.RowCount - 1 &&
                            dgv.AllowUserToAddRows == true)
                        {
                            dgv.RowCount = dgv.RowCount + 1;
                        }
                        // 貼り付け
                        dgv[x + c2, y + r].Value = pststr;
                        // ------------------------------------------------------
                        // 次のセルへ
                        c2 = c2 + 1;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dtoファイル書き込みクラス
    /// </summary>
    public class WriteDtoFile : IWriteFile
    {
        public void Write(StreamWriter sw, WriteItem item)
        {
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// " + item.LOGIC_NM);
            sw.WriteLine("/// <summary>");
            sw.WriteLine(string.Format("public {0} {1} ", item.TYPE, item.PHYGIC_NM) + "{ get; set; }\r\n");
        }
    }

    /// <summary>
    /// Propertyファイル書き込みクラス
    /// </summary>
    public class WritePropertyFile : IWriteFile
    {
        public void Write(StreamWriter sw, WriteItem item)
        {
            var returnVal = item.TYPE == "int" ? "0" : "\"\"";
            var p_lower = item.PHYGIC_NM.ToLower();

            sw.WriteLine(string.Format("private {0} m_{1} = {2};", item.TYPE, p_lower, returnVal));
            sw.WriteLine("/// <summary>");
            sw.WriteLine("/// " + item.LOGIC_NM);
            sw.WriteLine("/// <summary>");
            sw.WriteLine(string.Format("public {0} {1}", item.TYPE, item.PHYGIC_NM));
            sw.WriteLine("{");
            sw.WriteLine("  get { return this.m_" + p_lower + "; }");
            sw.WriteLine("  set");
            sw.WriteLine("  {");
            sw.WriteLine("      this.m_" + p_lower + " = value;");
            sw.WriteLine("      this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(this." + item.PHYGIC_NM + ")));");
            sw.WriteLine("  }");
            sw.WriteLine("}\r\n");
        }
    }

    /// <summary>
    /// 書込アイテム
    /// </summary>
    public class WriteItem
    {
        //型
        public string TYPE { get; set; }
        // 物理名
        public string PHYGIC_NM { get; set; }
        // 論理名
        public string LOGIC_NM { get; set; }
    }
}
