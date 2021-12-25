using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace server
{

    public partial class Form1 : Form
    {

        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            __ClientSockets = new List<SocketT2h>();
        }

        private void SetupServer()
        {
            label1.Text = "sunucu başlatıldı . . .";
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(1);

            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null); 

        }
        private void AppceptCallback(IAsyncResult ar)
        {
            
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));
            listBox1.Items.Add(socket.RemoteEndPoint.ToString());

            label1.Text = "Client bağlı. . .";
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
       static string sonlanan_clien = "";
       
        private void ReceiveCallback(IAsyncResult ar)
        {

            Socket socket = (Socket)ar.AsyncState;

            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    // 
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {

                            sonlanan_clien = __ClientSockets[i]._Name.Substring(1, __ClientSockets[i]._Name.Length - 1);
                            __ClientSockets.RemoveAt(i);
                            for (int j = 0; j < listBox1.Items.Count; j++)
                            {
                                if (listBox1.Items[j].Equals(sonlanan_clien))
                                {
                                    listBox1.Items.RemoveAt(j);

                                }
                            }
                        }
                    }
                    clientlerden_sil(sonlanan_clien);  
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];

                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    string reponse = string.Empty;

                    if (text.Contains("@@"))
                    {
                        for (int i = 0; i < listBox1.Items.Count; i++)
                        {
                            if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                            {
                                listBox1.Items.RemoveAt(i);
                                listBox1.Items.Insert(i, text.Substring(1, text.Length - 1));
                                __ClientSockets[i]._Name = text;
                                socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
                                isimleri_gonder();

                                return;
                            }
                        }
                    }
                    int index = text.IndexOf(" ");
                    string cli = text.Substring(0, index);

                    string mesaj = "";
                    int uzunluk = (text.Length) - (index + 2);
                    index = index + 2;
                    mesaj = text.Substring(index, uzunluk);
                    gonder_gelen_mesaji(cli, text, mesaj);
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            richTextBox1.AppendText("\n" + __ClientSockets[i]._Name + ": " + text);

                        }
                    }

                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        public void clientlerden_sil(string sonlanan_client)
        {
            string sil = "sil*" + sonlanan_clien;
            for (int j = 0; j < __ClientSockets.Count; j++)
            {
                if (__ClientSockets[j]._Socket.Connected)
                {
                        Sendata(__ClientSockets[j]._Socket, sil);
                        Thread.Sleep(20);
                }
            }
        }
        public void gonder_gelen_mesaji(string cli, string text, string mesaj)
        {

            string parcc = text.Substring(2, 2);
            cli = "@" + cli;

            int ind__ = (mesaj.IndexOf("*") + 1);
            string parcalanm = mesaj.Substring(ind__, mesaj.Length - ind__);
            string mess = mesaj.Substring(0, (ind__ - 1));
            string gonder_ = parcalanm + ": " + mess;
            try
            {
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    if (__ClientSockets[j]._Socket.Connected)
                    {
                        if (__ClientSockets[j]._Name.Equals(cli))
                        {

                            Sendata(__ClientSockets[j]._Socket, gonder_);
                            Thread.Sleep(20);
                        }

                    }
                }
            }
            catch (Exception e)
            {

            }
        }
        void Sendata(Socket socket, string mesajj)
        {
            byte[] data = Encoding.ASCII.GetBytes(mesajj);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }
        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

            CheckForIllegalCrossThreadCalls = false;
            SetupServer();

        }
        public void isimleri_gonder()
        {
            for (int j = 0; j < __ClientSockets.Count; j++)
            {
                if (__ClientSockets[j]._Socket.Connected)
                {
                    for (int i = 0; i < listBox1.Items.Count; i++)
                    {
                        Sendata(__ClientSockets[j]._Socket, listBox1.Items[i].ToString());
                        Thread.Sleep(20);
                    }

                }
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {

            for (int i = 0; i < listBox1.SelectedItems.Count; i++)
            {
                string t = listBox1.SelectedItems[i].ToString();
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    if (__ClientSockets[j]._Socket.Connected && __ClientSockets[j]._Name.Equals("@" + t))

                    {
                        Sendata(__ClientSockets[j]._Socket, textBox1.Text);
                    }
                }
            }
            richTextBox1.AppendText("\nServer: " + textBox1.Text);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
}

