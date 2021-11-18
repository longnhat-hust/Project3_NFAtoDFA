using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Project3_NFAtoDFA
{
    public partial class fMain : Form
    {
        Structure NFAStructure, DFAStructure;
        List<string> ListRead;

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
            [XmlElement(ElementName = "label")]
            public string Label { get; set; }

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
            NFAStructure = null;
            txbPreview.Text = null;

            if (File.Exists(link))
            {
                FileInfo fi = new FileInfo(link);
                if (fi.Exists && fi.Extension == ".jff")
                {                 
                    NFAStructure = ConvertXMLtoClass(link); //Chuyển nội dung tệp XML thành Lớp NFAStructure
                    if (NFAStructure != null)
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
        string ofdFileName;
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JFLAP files (*.jff)|*.jff";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ofdFileName = Path.GetFileNameWithoutExtension(ofd.FileName);
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
        public class DState
        {
            public string Id { get; set; }
            public List<State> List { get; set; }
        }
        public class DTran
        {
            public DState DFrom { get; set; }
            public DState DTo { get; set; }
            public string DRead { get; set; }
        }
        Structure ConvertNFAtoDFA(Structure NFA)
        {
            Structure DFA = new Structure();
            DFA.Type = "fa";
            DFA.Automaton = new Automaton();
            DFA.Automaton.ListState = new List<State>();
            DFA.Automaton.ListTransition = new List<Transition>();

            int Id = 0;

            // Xây dựng trạng thái của DFA
            State N_InitialState = InitialState(NFA);
            List<State> InStt = new List<State>();
            InStt.Add(N_InitialState);

            DState D_InitialState = new DState();
            D_InitialState.Id = Id.ToString();
            Id++;
            D_InitialState.List = eClosure(InStt, NFA);


            List<DState> ListDState = new List<DState>();
            List<DState> tmp1_ListDState = new List<DState>();
            List<DState> tmp2_ListDState = new List<DState>();
            tmp2_ListDState.Add(D_InitialState);
            ListDState.Add(D_InitialState);

            // Xây dựng hàm chuyển của DFA
            List<DTran> ListDTran = new List<DTran>();
            GetListRead(NFA);

            while (tmp2_ListDState.Count != 0)
            {
                tmp1_ListDState.Clear();
                tmp1_ListDState.AddRange(tmp2_ListDState);
                tmp2_ListDState.Clear();
                foreach (DState FromDStt in tmp1_ListDState)
                {
                    foreach (string Chr in ListRead)
                    {
                        DTran NewTran = new DTran();
                        NewTran.DFrom = FromDStt;
                        NewTran.DRead = Chr;

                        DState NewDState = new DState();
                        NewDState = DTranFunc(FromDStt, Chr, NFA);
                        bool SameDState = false;

                        foreach (DState DStt in ListDState)
                        {
                            if (DStt.List.Count == NewDState.List.Count)
                            {
                                foreach (State Stt in DStt.List)
                                {
                                    if (!NewDState.List.Contains(Stt))
                                    {
                                        break;
                                    }

                                    SameDState = true;
                                    NewDState.Id = DStt.Id;
                                    break;
                                }

                                if (SameDState) break;
                            }
                        }

                        if (!SameDState)
                        {
                            NewDState.Id = Id.ToString();
                            Id++;
                            ListDState.Add(NewDState);
                            tmp2_ListDState.Add(NewDState);
                        }

                        NewTran.DTo = NewDState;
                        ListDTran.Add(NewTran);
                    }
                }
            }

            //Xây dựng DFA
            foreach (DState DStt in ListDState)
            {
                State State = new State();
                State.Id = DStt.Id;
                State.Name = DStt.Id;
                foreach (State Stt in DStt.List)
                {
                    State.Label += Stt.Name + ",";
                    if (Stt.Final != null) State.Final = "";
                }
                State.Label.Substring(0, State.Label.Length - 1);
                DFA.Automaton.ListState.Add(State);
            }
            DFA.Automaton.ListState[0].Initial = "";
            foreach (DTran DTran in ListDTran)
            {
                Transition Tran = new Transition();
                Tran.From = DTran.DFrom.Id;
                Tran.To = DTran.DTo.Id;
                Tran.Read = DTran.DRead;
                DFA.Automaton.ListTransition.Add(Tran);
            }

            return DFA;
        }
        //
        // Trả về State từ 1 Id
        State StateFromId(string Id, Structure Struc)
        {
            foreach (State st in Struc.Automaton.ListState)
            {
                if (st.Id == Id)
                {
                    return st;
                }
            }
            return null;
        }
        //
        // InitialState
        State InitialState(Structure NFA)
        {
            State inSt = new State();
            foreach (State st in NFA.Automaton.ListState)
            {
                if (st.Initial != null)
                {
                    inSt = st;
                    break;
                }
            }
            return inSt;
        }
        //
        // eClosure
        List<State> eClosure(List<State> inListStt, Structure NFA)
        {
            List<State> outListStt = new List<State>();
            List<State> tmp1_ListStt = new List<State>();
            List<State> tmp2_ListStt = new List<State>();
            outListStt.AddRange(inListStt);
            tmp2_ListStt.AddRange(inListStt);


            while (tmp2_ListStt.Count != 0)
            {
                tmp1_ListStt.Clear();
                tmp1_ListStt.AddRange(tmp2_ListStt);
                tmp2_ListStt.Clear();
                foreach (State tmpStt in tmp1_ListStt)
                {
                    foreach (Transition tran in NFA.Automaton.ListTransition)
                    {
                        if (tran.From == tmpStt.Id && tran.Read == "")
                        {
                            State newState = StateFromId(tran.To, NFA);
                            if (!outListStt.Contains(newState))
                            {
                                tmp2_ListStt.Add(newState);
                                outListStt.Add(newState);
                            }
                        }
                    }
                }
            }
            return outListStt;
        }
        
        //
        // Hàm chuyển giữa 2 State
        State MoveFunc(State curState, string chr, Structure NFA) //Hàm chuyển
        {
            State nextState = new State();
            foreach (Transition tran in NFA.Automaton.ListTransition)
            {
                if (curState.Id == tran.From && chr == tran.Read)
                {
                    nextState = StateFromId(tran.To, NFA);
                    return nextState;
                }
            }
            return null;
        }
        //
        // Các kí tự đọc trong NFA
        void GetListRead(Structure NFA)
        {
            ListRead = new List<string>();
            foreach(Transition tran in NFA.Automaton.ListTransition)
            {
                if (tran.Read != "" && !ListRead.Contains(tran.Read) )
                {
                    ListRead.Add(tran.Read);
                }
            }
        }
        //
        // Hàm chuyển của DFA
        DState DTranFunc(DState FromListStt, string Chr, Structure NFA)
        {
            DState ToDStt = new DState();
            ToDStt.List = new List<State>();
            foreach (State Stt in FromListStt.List)
            {
                State ToStt = MoveFunc(Stt, Chr, NFA);
                if (!ToDStt.List.Contains(ToStt))
                {
                    if (ToStt != null) ToDStt.List.Add(ToStt);
                }
            }
            ToDStt.List = eClosure(ToDStt.List, NFA);
            return ToDStt;
        }
        //
        // Serialize thành định dạng XML
        //
        void SerializeToXML(object Obj, string Link)
        {
            FileStream fs = new FileStream(Link, FileMode.OpenOrCreate, FileAccess.Write);
            XmlSerializer sr = new XmlSerializer(typeof(Structure));
            sr.Serialize(fs, Obj);
            fs.Close();
        }
        //
        // Save File
        //
        void SaveFile(Structure Struc)
        {
            if (Struc != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JFLAP files (*.jff)|*.jff";
                sfd.FileName = ofdFileName + "_toDFA";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    {
                        SerializeToXML(Struc, sfd.FileName);
                        string content = null;
                        FileStream fs = new FileStream(sfd.FileName, FileMode.Open, FileAccess.Read);
                        StreamReader sr = new StreamReader(fs);
                        content = sr.ReadToEnd();
                        sr.Close();
                        fs.Close();

                        txbResult.Text = content;
                    }
                }
            }
            else MessageBox.Show("Invalid Output", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        //
        // Click button Convert
        //
        private void btnConvert_Click(object sender, EventArgs e)
        {
            DFAStructure = ConvertNFAtoDFA(NFAStructure);
            SaveFile(DFAStructure);
        }     
    }
}
