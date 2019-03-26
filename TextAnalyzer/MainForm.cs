using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

namespace TextAnalyzer
{
    public partial class MainForm : Form
    {
        //Useful Variables
        Dictionary<String, Int32> Words = new Dictionary<String, Int32>(0);
        Int32 fx = 0, fy = 0, TextAmount = 1;
        Double MI = 0, fxy = 0, TS = 0, LL = 0, TF = 0, IDF = 0, CollectionLarge = 0;

        //Char Splitting
        static String[] SplitChars = new String[] { " ", ".", ",", "-", "\n", "\t", "\r" };
        
        //How to import Stopwords?
        String[] StopWords = new String[] { };

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button_Frequent_Click(object sender, EventArgs e)
        {
            if (InputTextRichTextBox.Text != "")
            {
                TextToDictionary(InputTextRichTextBox.Text).OutputDictionary(OutputRrichTextBox, OutputChart);
            }
            else
            {
                MessageBox.Show("Text is empty. Please, input some text and try again.", "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonBigrammAction_Click(object sender, EventArgs e)
        {
            if (InputTextRichTextBox.Text != "")
            {
                GetMutualInfo();
            }
            else
            {
                MessageBox.Show("Text is empty. Please, input some text and try again.", "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonTFIDF_Click(object sender, EventArgs e)
        {
            if (InputTextRichTextBox.Text != "")
            {
                GetTFxIDF();
            }
            else
            {
                MessageBox.Show("Text is empty. Please, input some text and try again.", "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
}

        //SampleText
        public MainForm()
        {
            InitializeComponent();
        }

        #region CaseMethodsFunctions
        public Dictionary<String, Int32> TextToDictionary(String InputString)
        {
            String Reduced = Regex.Replace(InputString, @"[^a-zA-Zа-яА-Я0-9 ]", "");
            String[] WordsArray = new String[] { };
            //proparsit k inp string
            WordsArray = Reduced.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            Words = new Dictionary<String, Int32>();
            foreach (String Element in WordsArray)
                if (!StopWords.Contains(Element.ToLower()))
                {
                    if (Words.ContainsKey(Element.ToLower()))
                    {
                        Words[Element.ToLower()]++;
                    }
                    else
                    {
                        Words.Add(Element.ToLower(), 1);
                    }
                }
            return Words;
        }      //Done
        private void GetMutualInfo()
        {
            if (BigramTextBox.Text != "" && BigramTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).Length == 2)
            {
                //What for?
                Dictionary<String, Int32> FreqDic = TextToDictionary(InputTextRichTextBox.Text.ToLower());

                //Preparing text Without Bigram
                String ReducedInput = InputTextRichTextBox.Text.ToLower().Replace(BigramTextBox.Text.ToLower().Trim(), "");

                //String Arrays
                String[] PrimalStrings = InputTextRichTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                PrimalStrings = Array.ConvertAll(PrimalStrings, d => d.ToLower());
                String[] ReducedStrings = ReducedInput.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                ReducedStrings = Array.ConvertAll(ReducedStrings, d => d.ToLower());

                fxy = (PrimalStrings.Length - ReducedStrings.Length) / 2;
 
                String[] BigramStrings = BigramTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                fx = FreqDic.Where(x => x.Key.Contains(BigramStrings[0].ToLower())).FirstOrDefault().Value;
                fy = FreqDic.Where(x => x.Key.Contains(BigramStrings[1].ToLower())).FirstOrDefault().Value;

                MI = Math.Log(Convert.ToDouble(fxy * PrimalStrings.Length) / Convert.ToDouble(fx * fy), 2);
                TS = Convert.ToDouble(Convert.ToDouble(fxy) - (Convert.ToDouble(fx * fy) / Convert.ToDouble(PrimalStrings.Length - 1))) / Convert.ToDouble(Math.Pow(fxy, 2));
                LL = 0;

                OutputRrichTextBox.Text = "Статистика:";
                OutputRrichTextBox.Text += $"\r\n FXY: {fxy}";
                OutputRrichTextBox.Text += $"\r\n FX: {fx} FY: {fy}";
                OutputRrichTextBox.Text += $"\r\n MI: {MI}";
                if (MI < 0) { OutputRrichTextBox.Text += "\r\n MI < 0"; }
                if (MI > 1) { OutputRrichTextBox.Text += "\r\n Значима"; }
                if ((MI > 0) && (MI < 1)) { OutputRrichTextBox.Text += "\r\n Не значима"; }
                OutputRrichTextBox.Text += $"\r\n \r\n TS: {TS}";
            }
            else
            {
                MessageBox.Show("Bigramm text  is Wrong. Please, input some proper one and try again.", "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }                                               //Done
        private void GetTFxIDF()
        {
            if (BigramTextBox.Text != "" && BigramTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).Length == 1)
            {
                //Creating Texts Corpus
                Dictionary<String, Int32> FreqDic = TextToDictionary(InputTextRichTextBox.Text.ToLower());
                String[] Corpus = InputTextRichTextBox.Text.Split('~');
                TextAmount = Corpus.Length;
                CollectionLarge = FreqDic.Count();

                String[] PrimalStrings = InputTextRichTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                PrimalStrings = Array.ConvertAll(PrimalStrings, d => d.ToLower());

                String[] WordString = BigramTextBox.Text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                fx = FreqDic.Where(x => x.Key.Contains(WordString[0].ToLower())).FirstOrDefault().Value;

                TF = Convert.ToDouble(fx) / Convert.ToDouble(PrimalStrings.Length);
                IDF = TF * Math.Log(((PrimalStrings.Length - TextAmount) / TextAmount), 10);
                OutputRrichTextBox.Text = "Статистика:";
                OutputRrichTextBox.Text += $"\r\n Объем корпуса: {Corpus.Length}";
                OutputRrichTextBox.Text += $"\r\n Объем коллекции для сравнения: {FreqDic.Count}";
                OutputRrichTextBox.Text += $"\r\n FX: {fx}";
                OutputRrichTextBox.Text += $"\r\n TF: {TF}";
                OutputRrichTextBox.Text += $"\r\n TFxIDF: {IDF}";
            }
            else
            {
                MessageBox.Show("Text is Wrong. Please, input some proper one and try again.", "Processing error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            OutputRrichTextBox.ReadOnly = true;
            InputTextRichTextBox.Text = "";

            var assembly = Assembly.GetExecutingAssembly();
            var list = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream("TextAnalyzer.Resources.SW.txt"))
            using (StreamReader reader = new StreamReader(stream, Encoding.Default))
            {
                string result = reader.ReadToEnd();
                StopWords = result.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        #region MenuStripFileOperations
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stream SampleStream = null;
            OpenFileDialog SampleDialog = new OpenFileDialog();
            SampleDialog.Title = "Open Text File";
            SampleDialog.Filter = "TXT files|*.txt";
            if (SampleDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((SampleStream = SampleDialog.OpenFile()) != null)
                    {
                        using (SampleStream)
                        {
                            using (var streamReader = new StreamReader(SampleStream, Encoding.Default))
                            {
                                InputTextRichTextBox.Text = streamReader.ReadToEnd();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var SampleDialog = new SaveFileDialog())
            {
                SampleDialog.Title = "Save Text File";
                SampleDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                SampleDialog.FilterIndex = 2;

                if (SampleDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var fileStream = new FileStream(String.Format($"{SampleDialog.FileName}.txt"), FileMode.OpenOrCreate))
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.Default))
                    {
                        streamWriter.Write(InputTextRichTextBox.Text);
                    }
                }
            }
        }
        #endregion
    }

    #region DictionaryOutput
    public static class SampleClass
    {
        public static void OutputDictionary(this Dictionary<String, Int32> SampleDictonary, RichTextBox SampleText, System.Windows.Forms.DataVisualization.Charting.Chart SampleChart)
        {
            SampleText.Text = "Текущий словарь: \r\n";
            Int32 Min = 0, Max = 0;
            IEnumerable<KeyValuePair<String, Int32>> SampleResult = SampleDictonary.OrderByDescending(SamplePair => SamplePair.Value);
            var ok = SampleResult.Take(10);
            SampleChart.Series.Clear();
            foreach (KeyValuePair<string, Int32> pair in ok)
            {
                SampleChart.Series.Add(pair.Key).Points.Add(pair.Value);
                if (pair.Value < Min)
                    { Min = pair.Value; }
                if (pair.Value > Max)
                    { Max = pair.Value; }
                SampleText.Text += $"\r\n {pair.Key} {pair.Value}";
            }

            SampleChart.ChartAreas[0].AxisY.Minimum = Min;
            SampleChart.ChartAreas[0].AxisY.Maximum = Max;
        }
    }
    #endregion      
    #region Shing
    public static class Shingles
    {
        static HashSet<string> stopWords = new HashSet<string> { "это", "как", "так", "и", "в", "над", "к", "до", "не", "на", "но", "за", "то", "с", "ли", "а", "во", "от", "со", "для", "о", "же", "ну", "вы", "бы", "что", "кто", "он", "она" };

        /// <summary>
        /// Получение сигнатуры текста
        /// </summary>
        public static HashSet<int> GetShingleHashes(string text, int shingleLen = 10)
        {
            var res = new HashSet<int>();

            var words = GetWordHashes(text).ToArray();

            if (words.Length < shingleLen)
            {
                res.Add(XOR(words, 0, words.Length));
            }
            else
            {
                for (int i = 0; i < words.Length - shingleLen + 1; i++)
                    res.Add(XOR(words, i, i + shingleLen));
            }

            return res;
        }


        public static float Compare(HashSet<int> signature1, HashSet<int> signature2)
        {
            var same = 0;
            foreach (var hash in signature1)
                if (signature2.Contains(hash))
                    same++;

            return 1f * same / (signature1.Count + signature2.Count - same);
        }

        static IEnumerable<int> GetWordHashes(string text)
        {
            return Regex.Matches(text, @"\w+").OfType<Match>()
                .Select(m => m.Value.ToLower())
                .Where(s => !stopWords.Contains(s))
                .Select(s => s.GetHashCode());
        }

        static int XOR(IList<int> vals, int from, int to)
        {
            var res = 0;
            to = Math.Min(vals.Count, to);
            for (int i = from; i < to; i++)
                res ^= vals[i];

            return res;
        }
    }
    #endregion
}
