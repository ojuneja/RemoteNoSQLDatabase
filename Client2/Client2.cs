/////////////////////////////////////////////////////////////////////////
// Client2.cs - CommService client sends and receives messages         //
// ver 2.2                                                             //
// Author:   Ojas Juneja, Syracuse University 
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added using System.Threading
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - in this incantation the client has Sender and now has Receiver to
 *   retrieve Server echo-back messages.
 * - If you provide command line arguments they should be ordered as:
 *   remotePort, remoteAddress, localPort, localAddress
 */
/*

/*--------------------------------------------Public Interfaces-------------------------------------------
* readMessageTemplates - retrieve urls from the CommandLine if there are any
* parseMessageTemplate - Function that decides which type of operation needs to be done
* sendTimestampSearchMessages - send request to query messages within particular timestamp
* sendValuePatternMessages - send request to query messages for particular patternv
* sendKeyPatternMessages - send request to query messages for particular pattern
* sendChildrenSearchMessages - send request to query messages for particular pattern for childrens
* sendValueSearchTypeMessages - send request to query messages for search type messages
* setMessageSize - set number of messages to be processed
* transportInformation - define XML for reader client to show that its operation is completed
 * Maintenance History:
 * --------------------
 * ver 2.2 : 24 Nov 2015
 * ver 2.1 : 29 Oct 2015
 * - fixed bug in processCommandLine(...)
 * - added rcvr.shutdown() and sndr.shutDown() 
 * ver 2.0 : 20 Oct 2015
 * - replaced almost all functionality with a Sender instance
 * - added Receiver to retrieve Server echo messages.
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 1.0 : 18 Oct 2015
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Project4Starter
{
    using System.Xml.Linq;
    using Util = Utilities;

    ///////////////////////////////////////////////////////////////////////
    // Client class sends and receives messages in this version
    // - commandline format: /L http://localhost:8085/CommService 
    //                       /R http://localhost:8080/CommService
    //   Either one or both may be ommitted

    class Client
    {
        string localUrl { get; set; } = "http://localhost:8082/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";

        List<MessageTemplate> listOfMsgs;
        private int messageSize = 0;
        HiResTimer hrt = new HiResTimer();
        //----< retrieve urls from the CommandLine if there are any >--------

        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
        }

        //----< retrieve urls from the CommandLine if there are any >--------
        public void readMessageTemplates(String fileName, Sender sndr)
        {
            MessageTemplate msgTemplate = new MessageTemplate();
            listOfMsgs = msgTemplate.GetMessageList(fileName);
            setMessageSize(listOfMsgs);
            if (listOfMsgs.Count > 0)
            {
                for (int i = 0; i < listOfMsgs.Count; i++)
                {
                    parseMessageTemplate(listOfMsgs[i], sndr);
                }
            }
            else
            {
                Console.WriteLine("message template list is empty");
            }
        }

        //Function that decides which type of operation needs to be done
        private void parseMessageTemplate(MessageTemplate msgTemplate, Sender sndr)
        {
            if (msgTemplate.messageType.Equals("Query1"))
            {
                sendValueSearchTypeMessages(msgTemplate, sndr);
            }
            if (msgTemplate.messageType.Equals("Query2"))
            {
                sendChildrenSearchMessages(msgTemplate, sndr);
            }
            if (msgTemplate.messageType.Equals("Query3"))
            {
                sendKeyPatternMessages(msgTemplate, sndr);
            }
            if (msgTemplate.messageType.Equals("Query4"))
            {
                sendValuePatternMessages(msgTemplate, sndr);
            }
            if (msgTemplate.messageType.Equals("Query5"))
            {
                sendTimestampSearchMessages(msgTemplate, sndr);
            }
        }

        //send request to query messages within particular timestamp
        private void sendTimestampSearchMessages(MessageTemplate msgTemplate, Sender sndr)
        {
            DateTime fromDate = new DateTime(2015, 11, 20);
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                DateTime toDate = DateTime.Now;
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "query5");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement startTime = new XElement("startTime", fromDate);
                XElement endTime = new XElement("endTime", toDate);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(startTime);
                messageNode.Add(endTime);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
               Console.WriteLine("Request Sent to Query key for timestamp from: " + fromDate + " to " + toDate);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }

        //send request to query messages for particular pattern
        private void sendValuePatternMessages(MessageTemplate msgTemplate, Sender sndr)
        {
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "query4");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement searchParaMeter;
                // the pattern should be different in order to send multiple messages
                if (i == 0)
                {
                    searchParaMeter = new XElement("searchParaMeter", "SMA");
                    Console.WriteLine("Request Sent to Query key for ValuePattern: SMA");
                }
                else
                {
                    searchParaMeter = new XElement("searchParaMeter", i);
                    Console.WriteLine("Request Sent to Query key for ValuePattern: " + i);
                }
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(searchParaMeter);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }


        //send request to query messages for particular pattern
        private void sendKeyPatternMessages(MessageTemplate msgTemplate, Sender sndr)
        {
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "query3");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement patternNode = new XElement("pattern", i);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(patternNode);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                Console.WriteLine("Request Sent to Query key for keyPattern: " + i);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }

        //send request to query messages for particular pattern for childrens
        private void sendChildrenSearchMessages(MessageTemplate msgTemplate, Sender sndr)
        {
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "query2");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement keynode = new XElement("key", "Key" +i);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(keynode);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                Console.WriteLine("Request Sent to Query childrens for key: " + "Key" + i);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }

        //send request to query messages for particular search type messages
        private void sendValueSearchTypeMessages(MessageTemplate msgTemplate, Sender sndr)
        {
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "query1");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement keynode = new XElement("key", "Key" + i);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(keynode);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                Console.WriteLine("Request Sent to Query value for key: " + "Key" + i);
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }

        //set number of messages to be processed
        public void setMessageSize(List<MessageTemplate> listOfMsgs)
        {
            if (listOfMsgs.Count > 0)
            {
                for (int i = 0; i < listOfMsgs.Count; i++)
                {
                    messageSize = messageSize + listOfMsgs[i].messageSize;
                }
            }
        }

        //define XML for reader client to show that its operation is completed
        public void transportInformation(ulong latency, Sender sndr)
        {
            Message msg = new Message();
            msg.fromUrl = localUrl;
            msg.toUrl = remoteUrl;
            XElement root = new XElement("message");
            XAttribute att = new XAttribute("commandType", "info");
            root.Add(att);
            XElement client = new XElement("client", "ReaderClient");
            XElement port = new XElement("port", localUrl);
            XElement xLatency = new XElement("latency", latency);
            XElement messages = new XElement("numMsgs", messageSize);
            root.Add(client);
            root.Add(port);
            root.Add(xLatency);
            root.Add(messages);
            msg.content = root.ToString();
            if (!sndr.sendMessage(msg))
                return;
            ulong uLongMessageSize = Convert.ToUInt64(messageSize);
            Console.WriteLine("\nTotal time taken to process all requests: " + latency);
            Console.WriteLine("\nTotal requests sent: " + messageSize);
            ulong avg = latency / uLongMessageSize;
            Console.WriteLine("\nAvg time: " + avg);
        }
        public int getMessageSize()
        {
            return messageSize;
        }

        static void Main(string[] args)
        {
            Console.Write("\n  starting CommService reader client");
            Console.Write("\n =======================================\n");
            Console.Write("\n XML for Reading template is XMLReader.xml");
            Console.Write("\n =======================================\n");
            Console.Write("\n If Verbose switch is off, then all logs are logged into WPF Client/GUI.");
            Console.Write("\n =====================================================================\n");
            Console.Title = "Client #2";
            Console.Write("\n Demonstrating Requirement #4 by querying all type of queries from reader client to server.");
            Console.Write("\n Demonstrating Requirement #7 and #8 by taking format of request from XML and showing performance on GUI as well as on Console.(If logging switch is on)");
            Client clnt = new Client();
            clnt.processCommandLine(args);

            string localPort = Util.urlPort(clnt.localUrl);
            string localAddr = Util.urlAddress(clnt.localUrl);
            Receiver rcvr = new Receiver(localPort, localAddr);
            if (rcvr.StartService())
            {
                rcvr.doService(rcvr.defaultServiceAction());
            }

            Sender sndr = new Sender(clnt.localUrl);  // Sender needs localUrl for start message

            Message msg = new Message();
            msg.fromUrl = clnt.localUrl;
            msg.toUrl = clnt.remoteUrl;

            Console.Write("\n  sender's url is {0}", msg.fromUrl);
            Console.Write("\n  attempting to connect to {0}\n", msg.toUrl);

            if (!sndr.Connect(msg.toUrl))
            {
                Console.Write("\n  could not connect in {0} attempts", sndr.MaxConnectAttempts);
                sndr.shutdown();
                rcvr.shutDown();
                return;
            }
            clnt.hrt.Start();
            clnt.readMessageTemplates("XMLReader.xml", sndr);
            rcvr.setTotalMessageSize(clnt.getMessageSize() + 1);
            while (true)
            {
                if (rcvr.getLastFlag())
                {
                    clnt.hrt.Stop();
                    break;
                }
            }
            ulong latency = clnt.hrt.ElapsedMicroseconds;
            int lat = Convert.ToInt32(latency);
            clnt.transportInformation(latency, sndr);
            msg.content = "done";
            sndr.sendMessage(msg);

            // Wait for user to press a key to quit.
            // Ensures that client has gotten all server replies.
            Util.waitForUser();

            // shut down this client's Receiver and Sender by sending close messages
            rcvr.shutDown();
            sndr.shutdown();

            Console.Write("\n\n");
        }
    }
}

