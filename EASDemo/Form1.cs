using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EASEncoder.Models;
using EASEncoder.Models.SAME;
using NAudio.Wave;

namespace EASDemo
{
    public partial class Form1 : Form
    {
        private readonly List<SAMERegion> Regions = new List<SAMERegion>();
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
            Regions.Add(new SAMERegion(new SAMEState(36, "NY"), new SAMECounty(5, "Bronx", new SAMEState(36, "NY"))));
            Regions.Add(new SAMERegion(new SAMEState(36, "NY"), new SAMECounty(47, "Kings", new SAMEState(36, "NY"))));
            Regions.Add(new SAMERegion(new SAMEState(36, "NY"), new SAMECounty(61, "New York", new SAMEState(36, "NY"))));
            Regions.Add(new SAMERegion(new SAMEState(36, "NY"), new SAMECounty(81, "Queens", new SAMEState(36, "NY"))));
            Regions.Add(new SAMERegion(new SAMEState(36, "NY"), new SAMECounty(85, "Richmond", new SAMEState(36, "NY"))));


        }

        private void playDemo (string originator, string eventCode, string audioText, bool attn, bool nwsTone)
        {
            if (player != null)
            {
                player.Stop();
                return;
            }

            

            _start = DateTime.UtcNow;
            _senderId = "EAS DEMO";
            _length = "0500";

            var newMessage = new EASMessage(originator, eventCode,
                Regions, _length, _start, _senderId);


            var messageStream = EASEncoder.EASEncoder.GetMemoryStreamFromNewMessage(newMessage, attn,
                nwsTone, audioText);
            //btnGeneratePlay.Text = "Stop Playing";
            WaveStream mainOutputStream = new RawSourceWaveStream(messageStream, new WaveFormat());
            var volumeStream = new WaveChannel32(mainOutputStream);
            volumeStream.PadWithZeroes = false;

            player = new WaveOutEvent();
            player.PlaybackStopped += (o, args) =>
            {
                player.Dispose();
                player = null;
                //btnGeneratePlay.Text = "Generate && Play";
            };

            player.Init(volumeStream);

            player.Play();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            playDemo("EAS","RMT", "This is a coordinated monthly test of the broadcast stations of your area.  Equipment that can quickly warn you of emergencies is being tested. If this had been an actual emergency, an official message would have followed the alert tone.  This concludes this test of the Emergency Alert System.", true, false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            playDemo("WXR", "TOR", "This is serious", false, true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            playDemo("CIV", "CEM", "This is serious.", true, false);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            playDemo("PEP", "EAN", "President Obama.", true, false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            playDemo("EAS", "RWT", "", false, false);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            playDemo("WXR", "SVR", "This is serious", false, true);
            
        }
    }
}
