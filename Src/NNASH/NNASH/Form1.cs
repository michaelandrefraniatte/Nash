using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using EO.WebBrowser;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.Net;

namespace NNASH
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private static ProcessStartInfo startInfo;
        private void Form1_Shown(object sender, EventArgs e)
        {
            this.pictureBox1.Dock = DockStyle.Fill;
            EO.WebEngine.BrowserOptions options = new EO.WebEngine.BrowserOptions();
            options.EnableWebSecurity = false;
            EO.WebBrowser.Runtime.DefaultEngineOptions.SetDefaultBrowserOptions(options);
            EO.WebEngine.Engine.Default.Options.AllowProprietaryMediaFormats();
            EO.WebEngine.Engine.Default.Options.SetDefaultBrowserOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Create(pictureBox1.Handle);
            this.webView1.Engine.Options.AllowProprietaryMediaFormats();
            this.webView1.SetOptions(new EO.WebEngine.BrowserOptions
            {
                EnableWebSecurity = false
            });
            this.webView1.Engine.Options.DisableGPU = false;
            this.webView1.Engine.Options.DisableSpellChecker = true;
            this.webView1.Engine.Options.CustomUserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
            Navigate("");
            string folderpath = "file:///" + System.Reflection.Assembly.GetEntryAssembly().Location.Replace(@"file:\", "").Replace(Process.GetCurrentProcess().ProcessName + ".exe", "").Replace(@"\", "/").Replace(@"//", ""); 
            string path = @"nnash.html";
            string readText = File.ReadAllText(path).Replace("file:///C:/nnash/", folderpath);
            webView1.LoadHtml(readText);
            webView1.RegisterJSExtensionFunction("DownloadEmbed", new JSExtInvokeHandler(WebView_JSDownloadEmbed));
            webView1.RegisterJSExtensionFunction("DownloadAudio", new JSExtInvokeHandler(WebView_JSDownloadAudio));
            webView1.RegisterJSExtensionFunction("DownloadVideo", new JSExtInvokeHandler(WebView_JSDownloadVideo));
            webView1.RegisterJSExtensionFunction("OpenVideo", new JSExtInvokeHandler(WebView_JSOpenVideo));
        }
        private void webView1_LoadCompleted(object sender, EO.WebBrowser.LoadCompletedEventArgs e)
        {
            if (webView1.Url != "about:blank")
            {
                Task.Run(() => LoadPage());
            }
        }
        private void LoadPage()
        {
            string apikeytxt = "";
            using (StreamReader file = new StreamReader("api-key.txt"))
            {
                apikeytxt = file.ReadLine();
            }
            string stringinject;
            stringinject = @"
    <style>
body {
    font-family: sans-serif;
    background-color: #222222;
    color: #FFFFFF;
}

.row > .col-lg-4,
.col-6 {
    padding: 0;
}

#page-top, #title, #sub-title {
    font-family: sans-serif;
    background-color: #222222;
    color: #FFFFFF;
    background-color: #222222;
}

.button {
    color: #000000;
}

#csvFileInput {
    color: #FFFFFF;
}

input {
    margin-top: 5px;
    margin-bottom: 5px;
    display: inline-block;
    vertical-align: middle;
    color: #000000;
}
    </style>
".Replace("\r\n", " ");
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(" + stringinject + @" ).appendTo('head');";
            this.webView1.EvalScript(stringinject);
            stringinject = @"

    <form class='form-horizontal'>
        <input type='text' id='playlistID' value='playlist ID'>
        <input type='button' onClick='myFunction()' value='Check' class='button'>
    </form>

    <form class='form-horizontal'>
        <input type='button' onClick='saveAllEmbed()' value='Save All Embed' class='button'>
        <input type='button' onClick='saveAllAudio()' value='Save All Audio' class='button'>
        <input type='button' onClick='saveAllVideo()' value='Save All Video' class='button'>
    </form>

    <div id='contents'></div>

<script>

function saveAllEmbed() {
    $('.embedbutton').each(function(i) {
        var btn = $(this);
        setTimeout(btn.trigger.bind(btn, 'click'), i * 6000);
    });
}

function saveAllAudio() {
    $('.audiobutton').each(function(i) {
        var btn = $(this);
        setTimeout(btn.trigger.bind(btn, 'click'), i * 6000);
    });
}

function saveAllVideo() {
    $('.videobutton').each(function(i) {
        var btn = $(this);
        setTimeout(btn.trigger.bind(btn, 'click'), i * 6000);
    });
}

var nextpagetoken = 0;

var apikey = 'apikeytxt';

function handleEmbed(buttonid) {
    var filename = buttonid.id;
    document.getElementById(filename).value = 'ok';
    var videoid = filename.substring(filename.lastIndexOf('-ytbembed-') + 10, filename.lastIndexOf('.ytb'));
	DownloadEmbed(videoid, filename);
}

function handleAudio(buttonid) {
    var filename = buttonid.id;
    document.getElementById(filename).value = 'ok';
    filename = filename.replace('-audio', '');
	DownloadAudio(filename);
}

function handleVideo(buttonid) {
    var filename = buttonid.id;
    document.getElementById(filename).value = 'ok';
    filename = filename.replace('-video', '');
	DownloadVideo(filename);
}

function handleOpen(buttonid) {
    var filename = buttonid.id;
    filename = filename.replace('-open', '');
	OpenVideo(filename);
}

function myFunction() {
    nextpagetoken = 0;
    (async () => {
        var playlistID = document.getElementById('playlistID').value;
        var htmlString = '';
        var param = '';
        param = 'playlistId=' + playlistID;
        var responsef = await fetch('https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&videoEmbeddable=true&maxResults=50&' + param + '&key=' + apikey);
        var files = await responsef.json();
        for (let file of files.items) {
            var videoid = file.snippet.resourceId.videoId;
            var title = file.snippet.title;
            var embedid = title + '-ytbembed-' + videoid + '.ytb';
            htmlString += `
                <div>` + title + `</div>
                <form class=\'form-horizontal\'>
                    <input type=\'text\' class=\'` + videoid + `\' value=\'` + videoid + `\'>
                    <input type=\'button\' id=\'` + embedid + `\' onClick=\'handleEmbed(this)\' value=\'Save Embed\' class=\'button embedbutton\'>
                    <input type=\'button\' id=\'` + videoid + `-audio\' onClick=\'handleAudio(this)\' value=\'Save Audio\' class=\'button audiobutton\'>
                    <input type=\'button\' id=\'` + videoid + `-video\' onClick=\'handleVideo(this)\' value=\'Save Video\' class=\'button videobutton\'>
                    <input type=\'button\' id=\'` + videoid + `-open\' onClick=\'handleOpen(this)\' value=\'Open Video\' class=\'button openbutton\'>
                </form></br>`;
        }
        try {
            nextpagetoken = files.nextPageToken;
            var nomore = false;
            do {
                try {
                    var responsenf = await fetch('https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&videoEmbeddable=true&maxResults=50&pageToken=' + nextpagetoken + '&' + param + '&key=' + apikey);
                    var nfiles = await responsenf.json();
                    for (let file of nfiles.items) {
                        var videoid = file.snippet.resourceId.videoId;
                        var title = file.snippet.title;
                        var embedid = title + '-ytbembed-' + videoid + '.ytb';
                        htmlString += `
                            <div>` + title + `</div>
                            <form class=\'form-horizontal\'>
                                <input type=\'text\' class=\'` + videoid + `\' value=\'` + videoid + `\'>
                                <input type=\'button\' id=\'` + embedid + `\' onClick=\'handleEmbed(this)\' value=\'Save Embed\' class=\'button embedbutton\'>
                                <input type=\'button\' id=\'` + videoid + `-audio\' onClick=\'handleAudio(this)\' value=\'Save Audio\' class=\'button audiobutton\'>
                                <input type=\'button\' id=\'` + videoid + `-video\' onClick=\'handleVideo(this)\' value=\'Save Video\' class=\'button videobutton\'>
                                <input type=\'button\' id=\'` + videoid + `-open\' onClick=\'handleOpen(this)\' value=\'Open Video\' class=\'button openbutton\'>
                            </form></br>`;morevideo = true;
                    }
                    nextpagetoken = nfiles.nextPageToken;
                }
                catch {
                    nomore = true;
                }
            }
            while (!nomore);
        }
        catch {}
        $('#contents').append(htmlString);
    })();
}
</script>
".Replace("\r\n", " ").Replace("apikeytxt", apikeytxt);
            stringinject = @"""" + stringinject + @"""";
            stringinject = @"$(document).ready(function(){$('body').append(" + stringinject + @");});";
            this.webView1.EvalScript(stringinject);
        }
        void WebView_JSDownloadEmbed(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string filename = e.Arguments[1] as string;
            string url = @"https://img.youtube.com/vi/" + videoid + @"/mqdefault.jpg";
            filename = filename.Replace("'", "").Replace("#", "").Replace(@"""", "");
            string path = filename.Replace(".ytb", ".jpg");
            if (!File.Exists(path))
                SaveImage(url, path, ImageFormat.Jpeg);
            File.Create(filename);
        }
        void WebView_JSDownloadAudio(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string url = @"https://img.youtube.com/vi/" + videoid + @"/mqdefault.jpg";
            string path = videoid + "-audio.jpg";
            if (!File.Exists(path))
                SaveImage(url, path, ImageFormat.Jpeg);
            File.WriteAllText("dla.cmd", @"yt-dlp --force-ipv4 --extract-audio --audio-format mp3 " + "https://www.youtube.com/watch?v=" + videoid);
            startInfo = new ProcessStartInfo("dla.cmd");
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(startInfo);
        }
        void WebView_JSDownloadVideo(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            string url = @"https://img.youtube.com/vi/" + videoid + @"/mqdefault.jpg";
            string path = videoid + "-video.jpg";
            if (!File.Exists(path))
                SaveImage(url, path, ImageFormat.Jpeg);
            File.WriteAllText("dlv.cmd", @"yt-dlp --force-ipv4 --format best " + "https://www.youtube.com/watch?v=" + videoid);
            startInfo = new ProcessStartInfo("dlv.cmd");
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            Process.Start(startInfo);
        }
        void WebView_JSOpenVideo(object sender, JSExtInvokeArgs e)
        {
            string videoid = e.Arguments[0] as string;
            Process.Start("https://www.youtube.com/watch?v=" + videoid);
        }
        public void SaveImage(string imageUrl, string filename, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap = new Bitmap(stream);
            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }
            stream.Flush();
            stream.Close();
            client.Dispose();
        }
        private void Navigate(string address)
        {
            webView1.Url = address;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.webView1.Dispose();
        }
    }
}
