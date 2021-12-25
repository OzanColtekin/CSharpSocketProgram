using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Form1()
        {
            InitializeComponent();
        }
        byte[] receivedBuf = new byte[1024];
        private void ReceiveData(IAsyncResult ar)
        {

            int listede_yok = 0;
            try
            {

                Socket socket = (Socket)ar.AsyncState;
                int received = socket.EndReceive(ar);
                byte[] dataBuf = new byte[received];
                Array.Copy(receivedBuf, dataBuf, received);
                string gelen = Encoding.ASCII.GetString(dataBuf).ToString();
                if (gelen.Contains("sil*"))
                {
                    string parcala = gelen.Substring(4, (gelen.Length - 4));
                    for (int j = 0; j < listBox1.Items.Count; j++)
                    {
                        if (listBox1.Items[j].Equals(parcala))
                        {
                            listBox1.Items.RemoveAt(j);

                        }
                    }
                }
                else if (gelen.Contains("@"))
                {

                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        if (listBox1.Items[i].ToString().Equals(gelen))
                        {
                            listede_yok = 1;
                        }
                    }
                    if (listede_yok == 0)
                    {
                        string ben = "@" + txName.Text;
                        if (ben.Equals(gelen))
                        {

                        }
                        else
                        {
                            listBox1.Items.Add(gelen);
                        }
                    }

                }
                else
                {
                    rb_chat.AppendText(gelen + "\n");
                }


                _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
              

            }
            catch (Exception e)
            {
                
            }

        }

        private void SendLoop()
        {
            while (true)
            {


                byte[] receivedBuf = new byte[1024];
                int rev = _clientSocket.Receive(receivedBuf);
                if (rev != 0)
                {
                    byte[] data = new byte[rev];
                    Array.Copy(receivedBuf, data, rev);
                    rb_chat.AppendText("\nServer: " + Encoding.ASCII.GetString(data) + "\n");
                }
                else _clientSocket.Close();

            }
        }

        private void LoopConnect()
        {
            int attempts = 0;
            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _clientSocket.Connect("37.123.97.112", 100);
                }
                catch (SocketException)
                {

                }
            }

            _clientSocket.BeginReceive(receivedBuf, 0, receivedBuf.Length, SocketFlags.None, new AsyncCallback(ReceiveData), _clientSocket);
            byte[] buffer = Encoding.ASCII.GetBytes("@@" + txName.Text);
            _clientSocket.Send(buffer);
            label3.Text = ("servere bağlandı!");
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            Thread t1 = new Thread(LoopConnect);
            t1.Start();

        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            if (_clientSocket.Connected)
            {
                string tmpStr = "";
                foreach (var item in listBox1.SelectedItems)
                {

                    tmpStr = listBox1.GetItemText(item);
                    byte[] buffer = Encoding.ASCII.GetBytes(tmpStr + " :" + txt_text.Text + "*" + txName.Text);
                    _clientSocket.Send(buffer);
                    Thread.Sleep(20);

                }
                if (tmpStr.Equals(""))
                {
                    MessageBox.Show("Listeden değer seçmelisiniz.");
                }
                else
                {
                    rb_chat.AppendText(txName.Text + ": " + txt_text.Text + "\n");
                }


            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
