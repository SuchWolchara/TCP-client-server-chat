using System;
using System.Windows.Forms;
using static ClientApplication.ChatClient.ChatLogic;

namespace ClientApplication.ChatClient
{
    public partial class ClientForm : Form
    {
        public ConnectionEvent ButtonConnectClicked;

        public ChatEvent ButtonSendClicked;
        public ChatEvent ChatCommandClicked;

        private delegate void GuiDelegate();

        private bool _isByeClicked;

        public ClientForm()
        {
            InitializeComponent();

            listBox2.Items.Add("Users online:");
            listBox3.Items.Add("Chat commands:");
        }

        public void SetIsConnected(bool isConnected)
        {
            UpdateView(() => 
            {
                ListBoxesChangeState(isConnected);

                if (isConnected)
                {
                    button1.Text = "Disconnect";
                    button2.Enabled = true;
                    textBox1.Enabled = true;
                    textBox1.Focus();
                }
                else
                {
                    button1.Text = "Connect";
                    button2.Enabled = false;
                    textBox1.Enabled = false;
                    textBox1.Clear();
                }
            });
        }

        public void SetChatCommands(params object[] args)
        {
            UpdateView(() =>
            {
                listBox3.Items.AddRange(args);
            });
        }

        public void SetOnlineUsers(params object[] args)
        {
            UpdateView(() => 
            {
                listBox2.Items.AddRange(args);
            });
        }

        public void SetOnlineUser(string username)
        {
            UpdateView(() =>
            {
                if (listBox2.Items.Contains(username))
                    listBox2.Items.Remove(username);
                else
                    listBox2.Items.Add(username);
            });
        }

        public void SetMessage(string message)
        {
            UpdateView(() =>
            {
                listBox1.Items.Add(message);
            });
        }

        public void ClearMessages()
        {
            UpdateView(() =>
            {
                listBox1.Items.Clear();
                _isByeClicked = false;
            });
        }

        private void UpdateView(GuiDelegate func)
        {
            if (InvokeRequired)
                Invoke(func);
            else
                func();
        }

        private void ListBoxesChangeState(bool isConnected)
        {
            if (isConnected)
            {
                listBox3.SelectionMode = SelectionMode.One;
                listBox2.SelectionMode = SelectionMode.One;
                ClearMessages();
            }
            else
            {
                listBox3.Items.Clear();
                listBox3.Items.Add("Chat commands:");
                listBox3.SelectionMode = SelectionMode.None;

                listBox2.Items.Clear();
                listBox2.Items.Add("Users online:");
                listBox2.SelectionMode = SelectionMode.None;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ButtonConnectClicked?.Invoke();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_isByeClicked == false)
            {
                var message = textBox1.Text;
                textBox1.Clear();

                if (string.IsNullOrWhiteSpace(message))
                    return;

                if (string.Compare(message, "bye", true) == 0)
                    _isByeClicked = true;

                ButtonSendClicked?.Invoke(message, false);
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (_isByeClicked == false)
            {
                if (e.KeyChar != (char)Keys.Enter)
                    return;

                var message = textBox1.Text;
                textBox1.Clear();

                if (string.IsNullOrWhiteSpace(message))
                    return;

                ButtonSendClicked?.Invoke(message, false);
            }
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_isByeClicked == false)
            {
                var index = listBox3.SelectedIndex;

                if (index < 1)
                    return;

                var command = listBox3.SelectedItem.ToString();

                if (string.Compare(command, "bye", true) == 0)
                    _isByeClicked = true;

                ChatCommandClicked?.Invoke(command, false);
            }
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_isByeClicked == false)
            {
                var index = listBox2.SelectedIndex;

                if (index < 1)
                    return;

                textBox1.Text = listBox2.SelectedItem.ToString();
            }
        }
    }
}
