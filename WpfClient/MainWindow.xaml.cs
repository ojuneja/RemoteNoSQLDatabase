/////////////////////////////////////////////////////////////////////////
// MainWindows.xaml.cs - CommService GUI Client                        //
// ver 2.1                                                             //
// Source: Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4    //
// Author : Ojas Juneja, Syrcuse University
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# WPF Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, MakeMessage, Utilities
 * - Added using Project4Starter
 *
 * Note:
 * - This client receives and sends messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
*  ---------------------------- Public interfaces -----------------------
* sendMsgNotify - used to send notify messages
* sendExceptionNotify - used to send exception messages
* wpfWindow_Loaded - trim off leading and trailing white space
* parseMessage - parse message to show performance of reader and writer client
* postRcvMsg - WPF Client receive message and decides which operation needs to be done based on their content
* showServerResponse - show server performance
* start_Click_Persist - method used to send request to persist data
* start_Click_Augment - method used to send request to augment data
* setupChannel -  get Receiver and Sender running

 * Maintenance History:
 * --------------------
 * ver 2.1 : 24 Nov 2015
 * ver 2.0 : 29 Oct 2015
 * - changed Xaml to achieve more fluid design
 *   by embedding controls in grid columns as well as rows
 * - added derived sender, overridding notification methods
 *   to put notifications in status textbox
 * - added use of MessageMaker in send_click
 * ver 1.0 : 25 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project4Starter;
using System.Xml.Linq;

namespace WpfClient
{
  public partial class MainWindow : Window
  {
    static bool firstConnect = true;
    static Receiver rcvr = null;
    static wpfSender sndr = null;
    string localAddress = "localhost";
    string localPort = "8089";
    string remoteAddress = "localhost";
    string remotePort = "8080";

    /////////////////////////////////////////////////////////////////////
    // nested class wpfSender used to override Sender message handling
    // - routes messages to status textbox
    public class wpfSender : Sender
    {
      TextBox lStat_ = null;  // reference to UIs local status textbox
      System.Windows.Threading.Dispatcher dispatcher_ = null;

      public wpfSender(TextBox lStat, System.Windows.Threading.Dispatcher dispatcher)
      {
        dispatcher_ = dispatcher;  // use to send results action to main UI thread
        lStat_ = lStat;
      }

       //used to send notify messages
      public override void sendMsgNotify(string msg)
      {
        Action act = () => { lStat_.Text = msg; };
        dispatcher_.Invoke(act);

      }
            //used to send exception messages
            public override void sendExceptionNotify(Exception ex, string msg = "")
      {
        Action act = () => { lStat_.Text = ex.Message; };
        dispatcher_.Invoke(act);
      }
      public override void sendAttemptNotify(int attemptNumber)
      {
        Action act = null;
        act = () => { lStat_.Text = String.Format("attempt to send #{0}", attemptNumber); };
        dispatcher_.Invoke(act);
      }
    }
    public MainWindow()
    {
      InitializeComponent();
      lAddr.Text = localAddress;
      lPort.Text = localPort;
      rAddr.Text = remoteAddress;
      rPort.Text = remotePort;
      Title = "Prototype WPF Client";
      send.IsEnabled = false;
      Loaded += wpfWindow_Loaded;
    }
        //----< trim off leading and trailing white space >------------------

        private void wpfWindow_Loaded(object sender, RoutedEventArgs e)
        {
            localPort = lPort.Text;
            localAddress = lAddr.Text;
            remoteAddress = rAddr.Text;
            remotePort = rPort.Text;

            if (firstConnect)
            {
                firstConnect = false;
                if (rcvr != null)
                    rcvr.shutDown();
                setupChannel();
            }
            rStat.Text = "connect setup";
            send.IsEnabled = true;
            lPort.IsEnabled = false;
            lAddr.IsEnabled = false;
        }


     string trim(string msg)
    {
      StringBuilder sb = new StringBuilder(msg);
      for(int i=0; i<sb.Length; ++i)
        if (sb[i] == '\n')
          sb.Remove(i,1);
      return sb.ToString().Trim();
    }
        //----< indirectly used by child receive thread to post results >----

            //parse message to show performance of reader and writer client
        private string parseMessage(string content)
        {
            if (!content.Equals(""))
            {
                XDocument xdoc = XDocument.Parse(content);
                StringBuilder sb = new StringBuilder();
                string latency = "";
                string number = "";
                string fromUrl = "";
                string type = "";

                var elem = from c in xdoc.Descendants("message") select c;
                foreach (var item in elem.Elements())
                {
                    if (item.Name == "latency")
                        latency = item.Value.ToString();
                    if (item.Name == "port")
                        fromUrl = item.Value.ToString();
                    if (item.Name == "numMsgs")
                        number = item.Value.ToString();
                    if (item.Name == "client")
                        type = item.Value.ToString();
                }

                ulong timeint = ulong.Parse(latency);
                ulong numberInt = ulong.Parse(number);
                ulong avg = timeint / numberInt;
                sb.Append(String.Format("{0,-20} || ", fromUrl));
                sb.Append(String.Format("{0,-30} || ", type));
                sb.Append(String.Format("{0,-20} || ", number));
                sb.Append(String.Format("{0,-20} || ", latency));
                sb.Append(String.Format("{0,-20} || ", avg));

                return sb.ToString();
            }
            else return "";
        }


        //WPF Client receive message and decides which operation needs to be done based on their content
        public void postRcvMsg(string content)
        {
            if (content.Contains("identifier"))
            {
                TextBlock item = new TextBlock();
                string result = parseMessage(content);
                item.Text = trim(result);
                item.FontSize = 14;
                lst_perfromance.Items.Insert(0, item);
            }
            else if (content.Contains("readerClient"))
            {
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 12;
                rcvmsgs.Items.Insert(0, item);
            }
            else if (content.Contains("message"))
            {
                TextBlock item = new TextBlock();
                item.Text = trim(content);
                item.FontSize = 12;
                sndmsgs.Items.Insert(0, item);
            }
            else if (content.Contains("response"))
            {
                TextBlock item = new TextBlock();
                XDocument xdoc = XDocument.Parse(content);
                var elem = from c in xdoc.Descendants("response") select c;
                foreach (var elems in elem.Elements())
                {
                    if (elems.Name == "item")
                    {
                        item.Text = trim("Database " + elems.Value + " succesfully");
                    }
                }
                item.FontSize = 14;
                lst_response.Items.Insert(0, item);
            }
            else if (content.Contains("serverIdentifier"))
            {
                showServerResponse(content);
            }
        }

        //show server performance
        void showServerResponse(string content)
        {
            string time = "", numberOfQueries = "";
            StringBuilder sb = new StringBuilder();
            XDocument xDoc = XDocument.Parse(content);
            XElement root = xDoc.Root;
            foreach(var elem in root.Elements())
            {
                if(elem.Name == "time")
                {
                    time = elem.Value;
                }
                else if(elem.Name == "noOfQueries")
                {
                    numberOfQueries = elem.Value;
                }
            }
            lst_Server_perfromance.Items.Clear();
            sb.Append(String.Format("{0,-10} ||", "Time Taken By Server to Process Queries: " + time));
            sb.Append(String.Format("{0,-10} ||", "Total Queries Processed By Server: " + numberOfQueries));
            sb.Append(String.Format("{0,-10} ||", "Avg. Time: " + int.Parse(time) / int.Parse(numberOfQueries)));
            TextBlock item = new TextBlock();
            item.Text = trim(sb.ToString());
            item.FontSize = 14;
            lst_Server_perfromance.Items.Insert(0, item);
           
        }
    //----< used by main thread >----------------------------------------

    public void postSndMsg(string content)
    {
      TextBlock item = new TextBlock();
      item.Text = trim(content);
      item.FontSize = 16;
      sndmsgswpf.Items.Insert(0, item);
    }
    //----< get Receiver and Sender running >----------------------------

    void setupChannel()
    {
      rcvr = new Receiver(localPort, localAddress);
      Action serviceAction = () =>
      {
        try
        {
          Message rmsg = null;
          while (true)
          {
            rmsg = rcvr.getMessage();
            Action act = () => { postRcvMsg(rmsg.content); };
            Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Background);
          }
        }
        catch(Exception ex)
        {
          Action act = () => { lStat.Text = ex.Message; };
          Dispatcher.Invoke(act);
        }
      };
      if (rcvr.StartService())
      {
        rcvr.doService(serviceAction);
      }

      sndr = new wpfSender(lStat, this.Dispatcher);
    }
    //----< set up channel after entering ports and addresses >----------

    
        //method used to send request to persist data
        private void start_Click_Persist(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            XElement messageNode = new XElement("message");
            XAttribute att = new XAttribute("commandType", "persistWPF");
            messageNode.Add(att);
            Message msg = new Message();
            msg.fromUrl = "http://localhost:" +  localPort + "/CommService";
            msg.toUrl = "http://localhost:" + remotePort + "/CommService";
            msg.content = messageNode.ToString();
            if (!sndr.sendMessage(msg))
                return;
        }

        //method used to send request to augement data
        private void start_Click_Augment(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            XElement messageNode = new XElement("message");
            XAttribute att = new XAttribute("commandType", "augmentWPF");
            messageNode.Add(att);
            Message msg = new Message();
            msg.fromUrl = "http://localhost:" + localPort + "/CommService";
            msg.toUrl = "http://localhost:" + remotePort + "/CommService";
            msg.content = messageNode.ToString();
            if (!sndr.sendMessage(msg))
                return;
        }
        //----< send a demonstraton message >--------------------------------

        private void send_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        #region
        #endregion
        if (!remoteAddress.Equals(rAddr.Text) || !remotePort.Equals(rPort.Text))
        {
          remoteAddress = rAddr.Text;
          remotePort = rPort.Text;
        }
        // - Make a demo message to send
        // - You will need to change MessageMaker.makeMessage
        //   to make messages appropriate for your application design
        // - You might include a message maker tab on the UI
        //   to do this.

        MessageMaker maker = new MessageMaker();
        Message msg = maker.makeMessage(
          Utilities.makeUrl(lAddr.Text, lPort.Text), 
          Utilities.makeUrl(rAddr.Text, rPort.Text)
        );
        lStat.Text = "sending to" + msg.toUrl;
        sndr.localUrl = msg.fromUrl;
        sndr.remoteUrl = msg.toUrl;
        lStat.Text = "attempting to connect";
        if (sndr.sendMessage(msg))
          lStat.Text = "connected";
        else
          lStat.Text = "connect failed";
        postSndMsg(msg.content);
      }
      catch(Exception ex)
      {
        lStat.Text = ex.Message;
      }
    }

    }
}
