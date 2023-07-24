using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using DiscordRPC;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;
using Guna.UI2.WinForms;

namespace KeywordKing
{
    public partial class MainForm : Form
    {

        private const int cGrip = 16;
        private const int cCaption = 32;
        static DiscordRpcClient client;
        static bool running = false;
        List<string> dupe = new List<string>();
        delegate void Delegate(string line);
        List<string> prefixes = new List<string> { " ", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "y", "x", "y", "z", "how", "which", "why", "where", "who", "when", "are", "what" };
        List<string> suffixes = new List<string> { " ", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "y", "x", "y", "z", "like", "for", "without", "with", "versus", "vs", "to", "near", "except", "has" };
        bool prefixshow = false;
        bool suffixshow = false;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tooltip.SetToolTip(guna2ImageButton4, "Import From File");
            tooltip.SetToolTip(guna2ImageButton5, "Save Results To File");
            tooltip.SetToolTip(guna2ImageButton7, "Clear Results");
            tooltip.SetToolTip(guna2ImageButton6, "Clear Input");

            if (!File.Exists("prefixes.txt"))
            {
                File.WriteAllLines("prefixes.txt", prefixes);
            }
            else
            {
                prefixes.Clear();
                prefixes = File.ReadAllLines("prefixes.txt").ToList();
            }
            if (!File.Exists("suffixes.txt"))
            {
                File.WriteAllLines("suffixes.txt", suffixes);
            }
            else
            {
                suffixes.Clear();
                suffixes = File.ReadAllLines("suffixes.txt").ToList();
            }

            client = new DiscordRpcClient("807890633930309654");
            client.Initialize();
            var presence = new RichPresence()
            {
                Details = "Generating Keywords",
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                },
                Assets = new Assets()
                {
                    LargeImageKey = "king",
                    LargeImageText = "github.com/MachineKillin/keywordkings"
                }
            };
            client.SetPresence(presence);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(ClientSize.Width - cGrip, ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, BackColor, rc);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;
                    return;
                }
                if (pos.X >= ClientSize.Width - cGrip && pos.Y >= ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized) 
            {
                WindowState = FormWindowState.Maximized;
                guna2ImageButton2.Image = pictureBox1.Image;
                guna2ImageButton2.HoverState.Image = pictureBox1.Image;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                guna2ImageButton2.Image = pictureBox2.Image;
                guna2ImageButton2.HoverState.Image = pictureBox2.Image;
            }
            
        }

        private void guna2ImageButton3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void guna2VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            
        }

        private void guna2ImageButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    guna2TextBox1.Lines = File.ReadAllLines(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error while reading the file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            linestxt.Text = "lines: " + guna2TextBox1.Lines.Count().ToString();
        }

        private void SafeWrite(string line)
        {
            if (guna2TextBox2.InvokeRequired)
            {
                guna2TextBox2.Invoke(new Delegate(SafeWrite), line);
            }
            else
            {
                guna2TextBox2.AppendText(line + Environment.NewLine);
            }
        }

        private void googlekws(string furl, string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync(furl + keyword).Result;
                                string json = response.Content.ReadAsStringAsync().Result;
                                JArray jsonArray = JArray.Parse(json);
                                JArray suggestionsArray = jsonArray[1].ToObject<JArray>();
                                string[] results = suggestionsArray.Select(suggestion => suggestion.ToString()).ToArray();
                                foreach (string output in results)
                                {
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void ask(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://amg-ss.ask.com/query?q=" + keyword).Result;
                                dynamic jsonObject = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                                foreach (string output in jsonObject[1])
                                {
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void bing(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://api.bing.com/osjson.aspx?query=" + keyword).Result;
                                JArray jsonArray = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                                foreach (string output in (JArray)jsonArray[1])
                                {
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void duck(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://duckduckgo.com/ac/?q=" + keyword).Result;
                                List<dynamic> phraseList = JsonConvert.DeserializeObject<List<dynamic>>(response.Content.ReadAsStringAsync().Result);
                                foreach (var out_ in phraseList)
                                {
                                    string output = out_.phrase.ToString();
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void yahoo(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://search.yahoo.com/sugg/ff?output=json&command=" + keyword).Result;
                                dynamic jsonData = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
                                foreach (var result in jsonData.gossip.results)
                                {
                                    string output = result.key.ToString();
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void ebay(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://autosug.ebay.com/autosug?kwd=" + keyword).Result;
                                string json = response.Content.ReadAsStringAsync().Result;
                                int startIndex = json.IndexOf("(") + 1;
                                int endIndex = json.LastIndexOf(")");
                                string jsonData = json.Substring(startIndex, endIndex - startIndex);
                                dynamic suggestions = JsonConvert.DeserializeObject<dynamic>(jsonData);
                                foreach (string output in suggestions.res.sug)
                                {
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void amazon(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://completion.amazon.com/api/2017/suggestions?alias=aps&plain-mid=1&prefix=" + keyword).Result;
                                JObject json = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                                JArray suggestions = (JArray)json["suggestions"];
                                foreach (var suggestion in suggestions)
                                {
                                    string output = (string)suggestion["value"];
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch { }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private void yandex(string[] kw)
        {
            HttpClient client = new HttpClient();
            foreach (string kwrd in kw)
            {
                List<string> kwrds = new List<string> { kwrd };
                if (guna2ToggleSwitch1.Checked)
                {
                    foreach (string prefix in prefixes) { kwrds.Add(prefix + " " + kwrd); }
                }
                if (guna2ToggleSwitch2.Checked)
                {
                    foreach (string suffix in suffixes) { kwrds.Add(kwrd + " " + suffix); }
                }
                kwrds.RemoveAll(s => s == "");
                if (running == true)
                {
                    foreach (string keyword in kwrds)
                    {
                        if (running == true)
                        {
                            try
                            {
                                HttpResponseMessage response = client.GetAsync("https://yandex.com/suggest/suggest-ya.cgi?n=1000&part=" + keyword).Result;
                                string json = response.Content.ReadAsStringAsync().Result;
                                int startIndex = json.IndexOf('[');
                                int endIndex = json.LastIndexOf(']');
                                string jsonArrayString = json.Substring(startIndex, endIndex - startIndex - 6);
                                JArray jsonArray = JArray.Parse(jsonArrayString + "]");
                                JArray innerList = (JArray)jsonArray[1];
                                var list = innerList.ToObject<List<string>>();
                                foreach (string output in list)
                                {
                                    if (!dupe.Contains(output))
                                    {
                                        dupe.Add(output);
                                        SafeWrite(output);
                                    }
                                }
                            }
                            catch {  }
                        }
                        else { break; }
                    }
                }
                else { break; }
            }
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            if (guna2TextBox1.Text == "")
            {
                MessageBox.Show("Please add base keywords!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (guna2Button1.Text == "Stop")
                {
                    running = false;
                    guna2Button1.Text = "Start";
                }
                else
                {
                    guna2Button1.Text = "Stop";
                    running = true;
                    string[] lines = guna2TextBox1.Lines;
                    if (askbox.Checked) { await Task.Run(() => ask(lines)); }
                    if (bingbox.Checked) { await Task.Run(() => bing(lines)); }
                    if (googlebox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&q=", lines)); }
                    if (googlenewsbox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=n&callback=?&q=", lines)); }
                    if (googleshopbox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=sh&callback=?&q=", lines)); }
                    if (googlebookbox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=bo&callback=?&q=", lines)); }
                    if (googlevidbox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=v&callback=?&q=", lines)); }
                    if (googleimagebox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=i&callback=?&q=", lines)); }
                    if (youtubebox.Checked) { await Task.Run(() => googlekws("https://suggestqueries.google.com/complete/search?client=chrome&ds=yt&callback=?&q=", lines)); }
                    if (duckgobox.Checked) { await Task.Run(() => duck(lines)); }
                    if (yahoobox.Checked) { await Task.Run(() => yahoo(lines)); }
                    if (ebaybox.Checked) { await Task.Run(() => ebay(lines)); }
                    if (amazonbox.Checked) { await Task.Run(() => amazon(lines)); }
                    if (yandexbox.Checked) { await Task.Run(() => yandex(lines)); }
                    running = false;
                    guna2Button1.Text = "Start";
                }
            }

        }

        private void guna2ImageButton5_Click(object sender, EventArgs e)
        {
            File.AppendAllLines(DateTime.Now.ToString("dd-MM-yy HH-mm") + ".txt", guna2TextBox2.Lines);
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {
            label6.Text = "keywords: " + guna2TextBox2.Lines.Count().ToString();
        }

        private void guna2ImageButton6_Click(object sender, EventArgs e)
        {
            guna2TextBox1.Text = "";
        }

        private void guna2ImageButton7_Click(object sender, EventArgs e)
        {
            guna2TextBox2.Text = "";
            dupe.Clear();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (!prefixshow)
            {
                guna2Button2.Enabled = false;
                guna2DataGridView1.Columns.Clear();
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = "Prefixes";
                column.Name = "Prefixes";
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                guna2DataGridView1.Columns.Add(column);
                foreach (string prefix in prefixes)
                {
                    guna2DataGridView1.Rows.Add(prefix);
                }
                guna2Button3.Text = "Save";
                guna2DataGridView1.Visible = true;
                prefixshow = true;
            }
            else
            {
                if (File.Exists("prefixes.txt")) { File.WriteAllText("prefixes.txt", string.Empty); }
                prefixes.Clear();
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (!(cell.Value.ToString() == ""))
                            {
                                File.AppendAllText("prefixes.txt", cell.Value?.ToString() + Environment.NewLine ?? "");
                                prefixes.Add(cell.Value?.ToString() ?? "");
                            }
                        }
                    }
                }
                guna2Button3.Text = "Edit";
                guna2DataGridView1.Visible = false;
                prefixshow = false;
                guna2Button2.Enabled = true;
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (!suffixshow)
            {
                guna2Button3.Enabled = false;
                guna2DataGridView1.Columns.Clear();
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.HeaderText = "Suffixes";
                column.Name = "Suffixes";
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                guna2DataGridView1.Columns.Add(column);
                foreach (string suffix in suffixes)
                {
                    guna2DataGridView1.Rows.Add(suffix);
                }
                guna2Button2.Text = "Save";
                guna2DataGridView1.Visible = true;
                suffixshow = true;
            }
            else
            {
                if (File.Exists("suffixes.txt")) { File.WriteAllText("suffixes.txt", string.Empty); }
                suffixes.Clear();
                foreach (DataGridViewRow row in guna2DataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (!(cell.Value.ToString() == ""))
                            {
                                File.AppendAllText("suffixes.txt", cell.Value?.ToString() + Environment.NewLine ?? "");
                                suffixes.Add(cell.Value?.ToString() ?? "");
                            }
                        }
                    }
                }
                guna2Button2.Text = "Edit";
                guna2DataGridView1.Visible = false;
                suffixshow = false;
                guna2Button3.Enabled = true;
            }
        }
        bool all = false;
        private void allbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!all)
            {
                amazonbox.Checked = true;
                askbox.Checked = true;
                bingbox.Checked = true;
                duckgobox.Checked = true;
                ebaybox.Checked = true;
                googlebox.Checked = true;
                googlebookbox.Checked = true;
                googleimagebox.Checked = true;
                googlenewsbox.Checked = true;
                googleshopbox.Checked = true;
                googlevidbox.Checked = true;
                yahoobox.Checked = true;
                yandexbox.Checked = true;
                youtubebox.Checked = true;
                all = true;
            }
            else
            {
                amazonbox.Checked = false;
                askbox.Checked = false;
                bingbox.Checked = false;
                duckgobox.Checked = false;
                ebaybox.Checked = false;
                googlebox.Checked = false;
                googlebookbox.Checked = false;
                googleimagebox.Checked = false;
                googlenewsbox.Checked = false;
                googleshopbox.Checked = false;
                googlevidbox.Checked = false;
                yahoobox.Checked = false;
                yandexbox.Checked = false;
                youtubebox.Checked = false;
                all = false;
            }
        }
    }
}
