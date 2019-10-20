using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EASEncoder.Models;
using EASEncoder.Models.SAME;
using NAudio.Wave;
using IniParser;
using IniParser.Model;
using System.IO;

namespace EASDemo
{
    public partial class Form1 : Form
    {
        
        private readonly List<AlertButton> AlertButtons = new List<AlertButton>();
        private string _length;
        private string _senderId;
        private DateTime _start;
        private WaveOutEvent player;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IniData configdata;
            try
            {
                configdata = (new FileIniDataParser()).ReadFile("alerts.ini");
                int lastbuttonbottom = 0;
                foreach (var section in configdata.Sections)
                {
                    List<SAMERegion> Regions = new List<SAMERegion>();
                    foreach (var key in section.Keys)
                    {
                        if (key.KeyName.IndexOf("FIPS") == 0)
                        {
                            int fipsState = Convert.ToInt32(key.Value.Substring(1, 2));
                            int fipsCounty = Convert.ToInt32(key.Value.Substring(3, 3));
                            Regions.Add(new SAMERegion(new SAMEState(fipsState, ""), new SAMECounty(fipsCounty, "", new SAMEState(fipsState, ""))));
                        }
                    }
                    string callsign;
                    if (section.Keys["Callsign"].Length == 8)
                    {
                        callsign = section.Keys["Callsign"];
                    }
                    if (section.Keys["Callsign"].Length >8)
                    {
                        callsign = section.Keys["Callsign"].Substring(0, 8);
                    }
                    else
                    {
                        callsign = section.Keys["Callsign"].PadRight(8);
                    }
                    AlertButton newbutton = new AlertButton(player, section.Keys["Originator"], section.Keys["Event"], Regions, section.Keys["Length"], callsign, (section.Keys["Audio"] == "wav"), section.Keys["Announcement"],
                        Convert.ToBoolean(section.Keys["Attn"]), Convert.ToBoolean(section.Keys["1050"]));

                    AlertButtons.Add(newbutton);
                    this.Controls.Add(newbutton.playbutton);
                    newbutton.playbutton.Text = section.SectionName;
                    newbutton.playbutton.Width = 255;
                    newbutton.playbutton.Top = lastbuttonbottom + 8;
                    lastbuttonbottom = newbutton.playbutton.Bottom;
                    this.Height = lastbuttonbottom + 48;
                    
                }
            }
            catch
            {
                MessageBox.Show("Problem reading alerts.ini");
            }
                     

        }

        class AlertButton
        {
            private WaveFileReader reader;
            private WaveOutEvent player;
            public Button playbutton;
            private readonly List<SAMERegion> Regions = new List<SAMERegion>();
            private string originator;
            private string eventCode;
            private string length;
            private string senderId;
            private string announcementText;
            private byte[] announcementStream;
            private bool announcementFromFile;
            private bool attn;
            private bool nwsTone;
            private MemoryStream messagestream;
            
            public AlertButton(WaveOutEvent player, string originator, string eventCode, List<SAMERegion> Regions, string length, string senderId, bool announcementFromFile = false, string announcement = "", bool attnTone = false, bool nwsTone = false)
            {
                this.originator = originator;
                this.eventCode = eventCode;
                this.Regions = Regions;
                this.length = length;
                this.senderId = senderId;
                this.announcementFromFile = announcementFromFile;
                this.player = player;
                this.attn = attnTone;
                this.nwsTone = nwsTone;

                if (announcementFromFile)
                {
                    reader = new WaveFileReader(announcement);
                    announcementStream = new byte[reader.Length];
                    int read = reader.Read(announcementStream, 0, (int)reader.Length);
                }
                else
                {
                    announcementText = announcement;
                }

                playbutton = new Button();
                playbutton.Click += Playbutton_Click;
                
            }

            private void Playbutton_Click(object sender, EventArgs e)
            {
                
                if (player != null)
                {
                    player.Stop();
                    return;
                }

                player = new WaveOutEvent();

                WaveStream mainOutputStream = new RawSourceWaveStream(this.GetMemoryStream(), new WaveFormat());
                var volumeStream = new WaveChannel32(mainOutputStream);
                volumeStream.PadWithZeroes = false;

                


                player.PlaybackStopped += (o, args) =>
                {
                    player.Dispose();
                    player = null;
                    
                };

                player.Init(volumeStream);

                player.Play();
            }

            public EASMessage message()
            {
                EASMessage newmessage = new EASMessage(originator, eventCode, Regions, length, DateTime.UtcNow, senderId);
                return newmessage;

            }

            public MemoryStream GetMemoryStream()
            {
                if (announcementFromFile)
                {
                    EASEncoder.EASEncoder encoder = new EASEncoder.EASEncoder(new WaveFormat());
                    return encoder.GetMemoryStreamFromNewMessage(message(), attn, nwsTone, announcementStream);
                }
                else
                {
                    if (announcementText == null)
                    {
                        announcementText = "";
                    }
                    EASEncoder.EASEncoder encoder = new EASEncoder.EASEncoder(new WaveFormat());
                    return encoder.GetMemoryStreamFromNewMessage(message(), attn, nwsTone, announcementText);
                }
            }
            
        }
    }
}
