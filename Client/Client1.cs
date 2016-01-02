/////////////////////////////////////////////////////////////////////////
// Client1.cs - CommService client sends and receives messages         //
// ver 2.2                                                             //
// Source: Jim Fawcett, CSE681 - Software Modeling and Analysis, Project #4   
// Author:   Ojas Juneja, Syracuse University                            // 
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
/*--------------------------------------------Public Interfaces-------------------------------------------
* readMessageTemplates - retrieve urls from the CommandLine if there are any
* parseMessageTemplate - Function that decides which type of operation needs to be done
* getMessageSize - Function which calculates and returns the number of messages performed by writer client
* sendAugmentMessage - Function that send request to augment data from XML
* sendEditMessage - Function that sends request to edit data
* sendDeleteMessage - Function that sends request to delete data
* processCommandLine - Function to process command line
* transportInformation - Function to send message that writer client operation is done

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
using System.Collections;


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
        List<MessageTemplate> listOfMsgs;
        List<string> listOfKeys = new List<string>();
        private int messageProcessed = 0;
        string localUrl { get; set; } = "http://localhost:8081/CommService";
        string remoteUrl { get; set; } = "http://localhost:8080/CommService";
        HiResTimer hrt = new HiResTimer();

        //----< retrieve urls from the CommandLine if there are any >--------

        public void readMessageTemplates(String fileName, Sender sndr)
        {
            MessageTemplate msgTemplate = new MessageTemplate();
            listOfMsgs = msgTemplate.GetMessageList(fileName);
                if (listOfMsgs.Count > 0)
            {
                for (int i = 0; i < listOfMsgs.Count ; i++)
                {
                    parseMessageTemplate(listOfMsgs[i],sndr);
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
            
            if (msgTemplate.messageType.Equals("largeAdd"))
            {
               Console.WriteLine("\nADD OPERATION");
                sendLargeAddMessage(msgTemplate, sndr);
              
            }
            if (msgTemplate.messageType.Equals("delete"))
            {
                Console.WriteLine("\nDELETE OPERATION");
                sendDeleteMessage(msgTemplate, sndr);
                
            }
            if (msgTemplate.messageType.Equals("edit"))
            {
               Console.WriteLine("\nEDIT OPERATION");
               sendEditMessage(msgTemplate, sndr);                
            }
            if (msgTemplate.messageType.Equals("persist"))
            {
                Console.WriteLine("\nPERSIST OPERATION");
                sendPersistMessage(msgTemplate, sndr);                
            }
            if (msgTemplate.messageType.Equals("augment"))
            {
                Console.WriteLine("\nAUGEMENT OPERATION");
                sendAugmentMessage(msgTemplate, sndr);
            }
            
        }

        //Function which calculates and returns the number of messages performed by writer client
        public int getMessageSize(String fileName)
        {
        int messageSize = 0;
        MessageTemplate msgTemplate = new MessageTemplate();
            listOfMsgs = msgTemplate.GetMessageList(fileName);
            if (listOfMsgs.Count > 0)
            {
                for (int i = 0; i < listOfMsgs.Count; i++)
                {
                    if (listOfMsgs[i].messageType.Equals("delete") || listOfMsgs[i].messageType.Equals("persist") || listOfMsgs[i].messageType.Equals("augment"))
                        messageSize = messageSize + 1;
                    else
                    {
                        messageSize = messageSize + listOfMsgs[i].messageSize;
                    }
                    messageProcessed = messageProcessed + listOfMsgs[i].messageSize;
                }
            }
            return messageSize;
        }

        //Function that send request to augment data from XML
        private void sendAugmentMessage(MessageTemplate msgTemplate, Sender sndr)
        {
            Random random = new Random();
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "augment");
                messageNode.Add(att);
                XElement numKeys = new XElement("size");
                numKeys.Add(msgTemplate.messageSize);
                messageNode.Add(numKeys);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                if (!sndr.sendMessage(msg))
                    return;
            if (Util.verbose)
            {
                Console.WriteLine("For Augementation, Request Sent to augement " + msgTemplate.messageSize + " messages");
            }
        }

        //Function that send request to persist data to XML
        private void sendPersistMessage(MessageTemplate msgTemplate, Sender sndr)
        {
            Random random = new Random();
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "persist");
                messageNode.Add(att);
                XElement numKeys = new XElement("size");
                numKeys.Add(msgTemplate.messageSize);
                messageNode.Add(numKeys);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                if (!sndr.sendMessage(msg))
                    return;
            if (Util.verbose)
            {
                Console.WriteLine("For Persistance, Request Sent to persist " + msgTemplate.messageSize + " messages");
            }
        }

        //Function that sends request to edit data
        private void sendEditMessage(MessageTemplate msgTemplate, Sender sndr)
        {
            Random random = new Random(); //generated data as well as XML to send request to edit data one by one
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "edit");
                messageNode.Add(att);
                string rndKey = listOfKeys[random.Next(listOfKeys.Count)];
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement keynode = new XElement("key", rndKey.ToString());
                XElement valueNode = new XElement("value");
                XElement name = new XElement("name", "Updated-name : " + i.ToString());
                XElement desc = new XElement("desc", "Updated-desc : " + i.ToString());
                XElement timestamp = new XElement("time", DateTime.Now);
                XElement payload = new XElement("payload");
                XElement payloadItem = new XElement("item", "Updated SMA of " + rndKey);
                payload.Add(payloadItem);
                payloadItem = new XElement("item", "Updated OOD of " + rndKey);
                payload.Add(payloadItem);
                XElement children = new XElement("children");
                int childrenSize = random.Next(3, 5);
                for (int j = 1; j < childrenSize; j++)
                {
                    XElement item = new XElement("item", "updated-child"+j.ToString());
                    children.Add(item);
                }
                valueNode.Add(name);
                valueNode.Add(desc);
                valueNode.Add(timestamp);
                valueNode.Add(payload);
                valueNode.Add(children);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(keynode);
                messageNode.Add(valueNode);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                if (Util.verbose)
                {
                    Console.WriteLine("For Edition, Msg Sent with key= " + rndKey.ToString());
                }
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }

        //Function that sends request to delete data
        private void sendDeleteMessage(MessageTemplate msgTemplate, Sender sndr)
        {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "delete");
                messageNode.Add(att);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement numKeys = new XElement("size", msgTemplate.messageSize.ToString());
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(numKeys);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
            if (Util.verbose)
            {
                Console.WriteLine("For Deletion, Request sent to delete " + msgTemplate.messageSize.ToString() + " messages");
            }
            if (!sndr.sendMessage(msg))
                    return;
        }

        //Function that sends request to add large data
        private void sendLargeAddMessage(MessageTemplate msgTemplate, Sender sndr)
        {
            Random random = new Random();
            for (int i = 0; i < msgTemplate.messageSize; i++)
            {
                XElement messageNode = new XElement("message");
                XAttribute att = new XAttribute("commandType", "add");
                messageNode.Add(att);
                int rndKey = random.Next(1, 10000);
                //Console.WriteLine("key : " + rndKey);
                XElement keyTypenode = new XElement("keyType", "string");
                XElement valueTypeNode = new XElement("valueType", "ListOfString");
                XElement keynode = new XElement("key", rndKey.ToString());
                listOfKeys.Add(rndKey.ToString());
                XElement valueNode = new XElement("value");
                XElement name = new XElement("name", "name : " + i.ToString());
                XElement desc = new XElement("desc", "desc : " + i.ToString());
                XElement timestamp = new XElement("time", DateTime.Now);
                XElement payload = new XElement("payload");
                XElement payloadItem = new XElement("item", "SMA of " + rndKey);
                payload.Add(payloadItem);
                payloadItem = new XElement("item", "OOD of " + rndKey);
                payload.Add(payloadItem);
                XElement children = new XElement("children");
                int childrenSize = random.Next(13, 15);
                for (int j = 1; j < childrenSize; j++)
                {
                    XElement item = new XElement("item", j.ToString());
                    children.Add(item);
                }
                valueNode.Add(name);
                valueNode.Add(desc);
                valueNode.Add(timestamp);
                valueNode.Add(payload);
                valueNode.Add(children);
                messageNode.Add(keyTypenode);
                messageNode.Add(valueTypeNode);
                messageNode.Add(keynode);
                messageNode.Add(valueNode);
                Message msg = new Message();
                msg.fromUrl = localUrl;
                msg.toUrl = remoteUrl;
                msg.content = messageNode.ToString();
                if (Util.verbose) Console.WriteLine("For Addition, Msg Sent with key= " + rndKey.ToString());
                if (!sndr.sendMessage(msg))
                    return;
                Thread.Sleep(10);
            }
        }


        //Function to process command line
        public void processCommandLine(string[] args)
        {
            if (args.Length == 0)
                return;
            localUrl = Util.processCommandLineForLocal(args, localUrl);
            remoteUrl = Util.processCommandLineForRemote(args, remoteUrl);
            if(args.Length > 4)
            {
                if(args[4] == "/O" || args[4] == "/o")
                {
                    Util.verbose = true;
                }
                else
                {
                    Util.verbose = false;
                }
            }
        }

        //Function to send message that writer client operation is done
        public void transportInformation(ulong latency, Sender sndr)
        {
            Message msg = new Message();
            msg.fromUrl = localUrl;
            msg.toUrl = remoteUrl;
            XElement root = new XElement("message");
            XAttribute att = new XAttribute("commandType", "info");
            root.Add(att);
            XElement client = new XElement("client", "WriterClient");
            XElement port = new XElement("port", localUrl);
            XElement xLatency = new XElement("latency",latency);
            XElement messages = new XElement("numMsgs",messageProcessed);
            root.Add(client);
            root.Add(port);
            root.Add(xLatency);
            root.Add(messages);
            msg.content = root.ToString();
            if (!sndr.sendMessage(msg))
                return;
            ulong uLongmessageProcessed = Convert.ToUInt64(messageProcessed);
            if (Util.verbose)
            {
                Console.WriteLine("\nTotal time taken to process all requests: " + latency);
                Console.WriteLine("\nTotal requests sent: " + messageProcessed);
                ulong avg = latency / uLongmessageProcessed;
                Console.WriteLine("\nAvg time: " + avg);
            }
        }



        static void Main(string[] args)
        {
            //Thread.Sleep(1500);
            Console.Write("\n  starting CommService writer client");
            Console.Write("\n =======================================\n");
            Console.Write("\n XML for Reading template is XMLWriter.xml");
            Console.Write("\n =======================================\n");
            Console.Write("\n If Verbose switch is off, then all logs are logged into WPF Client/GUI.");
            Console.Write("\n =====================================================================\n");
            Console.Write("\n Demonstrating Requirement #4 by sending add/delete/edit/persist/augment messages from writer client to server.");
            Console.Write("\n Demonstrating Requirement #5 by taking format of request from XML and showing performance on GUI as well as on Console(If logging switch is on).");
            Console.Title = "Client #1";
           
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
            rcvr.setTotalMessageSize(clnt.getMessageSize("XMLWriter.xml"));
            clnt.readMessageTemplates("XMLWriter.xml", sndr);
             while(true)
            {
                if (rcvr.getLastFlag())
                {
                    clnt.hrt.Stop();
                    break;
                }
            }
            ulong latency = clnt.hrt.ElapsedMicroseconds;
            int lat = Convert.ToInt32(latency);
            clnt.transportInformation(latency,sndr);
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
