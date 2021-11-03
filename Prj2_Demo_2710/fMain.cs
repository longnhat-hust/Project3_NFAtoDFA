using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Project2_Recognition
{
    public partial class fMain : Form
    {
        Structure MainStructure;
        State CurrentState;

        public fMain()
        {
            InitializeComponent();
        }
        //
        // Định nghĩa các Lớp
        //
        [XmlRoot(ElementName = "state")]
        public class State
        {
            [XmlElement(ElementName = "x")]
            public string X { get; set; }
            [XmlElement(ElementName = "y")]
            public string Y { get; set; }
            [XmlElement(ElementName = "initial")]
            public string Initial { get; set; }
            [XmlAttribute(AttributeName = "id")]
            public string Id { get; set; }
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }
            [XmlElement(ElementName = "final")]
            public string Final { get; set; }
        }

        [XmlRoot(ElementName = "transition")]
        public class Transition
        {
            [XmlElement(ElementName = "from")]
            public string From { get; set; }
            [XmlElement(ElementName = "to")]
            public string To { get; set; }
            [XmlElement(ElementName = "read")]
            public string Read { get; set; }
        }

        [XmlRoot(ElementName = "automaton")]
        public class Automaton
        {
            [XmlElement(ElementName = "state")]
            public List<State> ListState { get; set; }
            [XmlElement(ElementName = "transition")]
            public List<Transition> ListTransition { get; set; }
        }

        [XmlRoot(ElementName = "structure")]
        public class Structure
        {
            [XmlElement(ElementName = "type")]
            public string Type { get; set; }
            [XmlElement(ElementName = "automaton")]
            public Automaton Automaton { get; set; }
        }
        //
        // HÀM Chuyển nội dung tệp XML thành các Lớp
        //
        Structure ConvertXMLtoClass(string LinkXML)
        {
            Structure struc = new Structure();
            try
            {              
                XmlSerializer serializer = new XmlSerializer(typeof(Structure));
                using (TextReader reader = new StringReader(File.ReadAllText(LinkXML)))
                {
                    struc = (Structure)serializer.Deserialize(reader);
                }
                return struc;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //
        // Lấy các thông tin của tệp Jff
        //
        void GetInfoFileJff(string link)
        {
            MainStructure = null;
            txbPreview.Text = null;

            if (File.Exists(link))
            {
                FileInfo fi = new FileInfo(link);
                if (fi.Exists && fi.Extension == ".jff")
                {                 
                    MainStructure = ConvertXMLtoClass(link); //Chuyển nội dung tệp XML thành Lớp MainStructure
                    if (MainStructure != null)
                    {
                        string content = null;
                        FileStream fs = new FileStream(link, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs);
                        content = sr.ReadToEnd();
                        sr.Close();
                        fs.Close();

                        txbLink.Text = link;
                        txbPreview.Text = content;
                    }
                }
            }
        }
        //
        // Chọn file
        //
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JFLAP files (*.jff)|*.jff";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                GetInfoFileJff(ofd.FileName);
            }
        }
        private void txbLink_TextChanged(object sender, EventArgs e)
        {        
            GetInfoFileJff(txbLink.Text);           
        }
        //
        // 
        //
        State InitialState() //Hàm tìm Initial State
        {
            State inSt = new State();
            foreach (State st in MainStructure.Automaton.ListState)
            {
                if (st.Initial != null)
                {
                    inSt = st;
                    break;
                }
            }
            return inSt;
        }
        State NextState(State curState, string chr) //Hàm trả về Next State với input là (Current State và kí tự tiếp theo)
        {
            State nextState = new State();          
            foreach (Transition tran in MainStructure.Automaton.ListTransition)
            {
                if (curState.Id == tran.From && chr == tran.Read)
                {
                    foreach (State st in MainStructure.Automaton.ListState)
                    {
                        if (st.Id == tran.To)
                        {
                            nextState = st;
                            return nextState;
                        }
                    }
                }
            }
            return null;
        }
        //
        // Hàm kiểm tra xâu vào có thuộc hay không
        //
        string Path;
        bool CheckWord(string word)
        {
            Path = null;

            CurrentState = InitialState();
            Path = CurrentState.Name;

            State tmpState;

            char[] chrWord = word.ToCharArray();

            foreach(char chr in chrWord)
            {
                tmpState = NextState(CurrentState, chr.ToString());
                if (tmpState != null)
                {
                    CurrentState = tmpState;
                    Path += "→" + CurrentState.Name;
                }
                else return false;
            }

            if (CurrentState.Final != null) //Kiểm tra trạng thái cuối cùng có phải Final không
            {
                return true;
            }
            else return false;
        }
        //
        // Click button Check
        //
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (MainStructure != null)
            {
                if (CheckWord(txbWord.Text)) Accept();
                else Reject();
            }
            else MessageBox.Show("Invalid input", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //
        //
        //
        void Accept()
        {
            txbWord.Text += "\r\n" + Path;
            txbWord.ForeColor = Color.Lime;
            MessageBox.Show("Input is accepted","Result",MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        void Reject()
        {
            txbWord.Text += "\r\n" + Path;
            txbWord.ForeColor = Color.Red;
            MessageBox.Show("Input is rejected", "Result", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void txbWord_TextChanged(object sender, EventArgs e)
        {
            txbWord.ForeColor = Color.White;
        }
    }
}
