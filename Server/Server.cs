/////////////////////////////////////////////////////////////////////////
// Server.cs - CommService server                                      //
// ver 2.4                                                             //  
// Author: Ojas Juneja, Syracuse University
/////////////////////////////////////////////////////////////////////////
/*
 * Additions to C# Console Wizard generated code:
 * - Added reference to ICommService, Sender, Receiver, Utilities
 *
 * Note:
 * - This server now receives and then sends back received messages.
 */
/*
 * Plans:
 * - Add message decoding and NoSqlDb calls in performanceServiceAction.
 * - Provide requirements testing in requirementsServiceAction, perhaps
 *   used in a console client application separate from Performance 
 *   Testing GUI.
 */
/*
* ------------------------------------- Public Intefaces -------------------------------
* ProcessCommandLine - quick way to grab ports and addresses from commandline
* identifyOperation -  identifies operation 
* getKeyValueType - returns the signal to indicate datatype of key 
* getKey - gets the key
* getMetaPattern - gets the search parameter
* getPattern - gets the pattern
* getToDate - gets to date
* getFromDate - gets from date
* getNumberOfKeys - gets the size of keys
* createAddDBElement - creates DBElement from XML
* processDeleteMessage - process delete message from No SQL Database
* processEditMessage - process edits message from No SQL Database
* processAddMessage - process add message from No SQL Database
* getSize - get number fo requests
* responseXML - generates response XML fro writer client
* responseXMLSingle- generates specific response XML fro writer client
* processTimeStampQueryMessage - process timestamp query from No SQL database
* sendReponseTimeStampQuery - send response for timestamp query to reader client
* processValuePatternQuery - process value pattern query from No SQL database
* processKeyPatternQuery- process key pattern query from No SQL database
* sendReponsePatternQuery- send response for pattern query to reader client
* processChildrenSearchQuery- process children search query from No SQL database
* sendReponseChildrenQuery- send response for child search query to reader client
* processValueSearchQuery- process value search query from No SQL database
* constructResponseValueSearch -send response for value search to reader client
* processPersistMessage - process persist query from No SQL database
* processPersistMessageForWPF - process persist query for WPF from No SQL database
* processAugementMessage - process augement query from No SQL database
* processAugementMessageForWPF - process augement query for WPF from No SQL database
* sendToWPF - send message to WPF
* makeDataForDB - make data for DB
* preLoadData - generate data for querying purposes
* makeServerXMLString- make XML for server response to WPF
 * Maintenance History:
 * --------------------
 * ver 2.4: 24 Nov 2015
 * ver 2.3 : 29 Oct 2015
 * - added handling of special messages: 
 *   "connection start message", "done", "closeServer"
 * ver 2.2 : 25 Oct 2015
 * - minor changes to display
 * ver 2.1 : 24 Oct 2015
 * - added Sender so Server can echo back messages it receives
 * - added verbose mode to support debugging and learning
 * - to see more detail about what is going on in Sender and Receiver
 *   set Utilities.verbose = true
 * ver 2.0 : 20 Oct 2015
 * - Defined Receiver and used that to replace almost all of the
 *   original Server's functionality.
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

    class Server
    {
        static int varWriteClient = 0, varReadclient = 0;
        private int serverRequestProcessed = 0;
        private int varCountData = 20;
        List<int> Ttime = new List<int>();
        private string wpfURL = "http://localhost:8089/CommService";


        DBEngine<string, DBElement<string, List<string>>> db = new DBEngine<string, DBElement<string, List<string>>>();

        string address { get; set; } = "localhost";
        string port { get; set; } = "8080";

        string getWpfUrl()
        {
            return wpfURL;
        }

        //----< quick way to grab ports and addresses from commandline >-----

        public void ProcessCommandLine(string[] args)
        {
            if (args.Length > 0)
            {
                port = args[0];
            }
            if (args.Length > 1)
            {
                address = args[1];
            }
        }

        //identifies operation 
        public static string identifyOperation(XDocument xdoc)
        {
            var elem = xdoc.Root;
            return elem.Attribute("commandType").Value;
        }

        //returns the signal to indicate datatype of key 
        public static int getKeyValueType(XDocument xdoc)
        {

            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "keyType")
                {
                    if (item.Value.Equals("string"))
                    {
                        return 1;
                    }
                    if (item.Value.Equals("Int"))
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }

        //gets the key
        public string getKey(XDocument xdoc)
        {
            string key = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "key")
                    key = item.Value.ToString();
            }
            return key;
        }

        //gets the search parameter
        public string getMetaPattern(XDocument xdoc)
        {
            string pattern = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "searchParaMeter")
                    pattern = item.Value.ToString();
            }
            return pattern;
        }

        //gets the pattern
        public string getPattern(XDocument xdoc)
        {
            string pattern = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "pattern")
                    pattern = item.Value.ToString();
            }
            return pattern;
        }

        //gets to date
        public DateTime getToDate(XDocument xdoc)
        {
            DateTime toDate = new DateTime();
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "endTime")
                    toDate = Convert.ToDateTime(item.Value);
            }
            return toDate;
        }

        //gets from date
        public DateTime getFromDate(XDocument xdoc)
        {
            DateTime fromDate = new DateTime();
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "startTime")
                    fromDate = Convert.ToDateTime(item.Value);
            }
            return fromDate;
        }

        //gets the number of keys present in the XML Message
        public string getNumberOfKeys(XDocument xdoc)
        {
            string numKeys = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "size")
                    numKeys = item.Value.ToString();
            }
            return numKeys;
        }

        //creates the DBElement from XML
        public static string createAddDBElement(XDocument xdoc, out DBElement<string, List<string>> element)
        {
            element = new DBElement<string, List<string>>();
            string key = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name == "key")
                    key = item.Value.ToString();
                else if (item.Name == "value")
                {
                    foreach (var valueItem in item.Elements())
                    {
                        createDBElement(valueItem,ref element);
                    }
                }
            }
            return key;
        }

        public static void createDBElement(XElement valueItem,ref DBElement<string, List<string>> element)
        {
            if (valueItem.Name == "name")
                element.name = valueItem.Value;
            else if (valueItem.Name == "desc")
                element.descr = valueItem.Value;
            else if (valueItem.Name == "time")
                element.timeStamp = Convert.ToDateTime(valueItem.Value);
            else if (valueItem.Name == "payload")
            {
                element.payload = new List<string>();
                foreach (var payloadItem in valueItem.Elements())
                {
                    if (payloadItem.Name == "item")
                        element.payload.Add(payloadItem.Value);
                }
            }
            else if (valueItem.Name == "children")
            {
                element.children = new List<string>();
                foreach (var childs in valueItem.Elements())
                {
                    if (childs.Name == "item")
                        element.children.Add(childs.Value);
                }
            }
        }
    
    //process delete message and calls delete function of No SQL Database
    public void processDeleteMessage(XDocument xdoc, Sender sndr, Message msg)
    {
        int type = getKeyValueType(xdoc);
        serverRequestProcessed = serverRequestProcessed + getSize(xdoc);
        if (type == 1)
        {
            IEnumerable<string> keys = db.Keys();

            // string ListString
            int numKeys = Convert.ToInt32(getNumberOfKeys(xdoc));
            if (numKeys >= keys.Count())
            {
                numKeys = keys.Count() - 1;
            }

            List<string> keyList = keys.ToList();
            for (int i = 0; i < numKeys; i++)
            {
                string keyToDeleted = keyList.ElementAt(i);
                if (db.delete(keyToDeleted))
                {
                    msg.content = keyToDeleted + " record is deleted.";
                }
                else
                {
                    msg.content = keyToDeleted + " record is  not deleted.";
                }
                Console.WriteLine(msg.content);
            }
        }
    }
        //process edit message and calls edit function of No SQL Database
        public void processEditMessage(XDocument xdoc, Sender sndr, Message msg)
        {
            IEnumerable<string> keys = db.Keys();
            List<string> keyList = keys.ToList();

            string keyToEdit = getKey(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (!keyList.Contains(keyToEdit))
            {
                Console.WriteLine("Key {0} is not present in the DB", keyToEdit);
            }

            else
            {
                int type = getKeyValueType(xdoc);
                if (type == 1)
                {
                    DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                    string key = createAddDBElement(xdoc, out element);
                    Message testMsg = new Message();
                    testMsg.toUrl = msg.toUrl;
                    testMsg.fromUrl = msg.fromUrl;

                    if (db.edit(key, element))
                    {
                        testMsg.content = key + " record is edited Successfully.";
                    }
                    else
                    {
                        testMsg.content = key + " record is not edited.";
                    }
                    Console.WriteLine(testMsg.content);
                    sndr.sendMessage(testMsg);
                }
            }
        }

        //process add message and calls add function of No SQL Database
        public void processAddMessage(XDocument xdoc, Sender sndr, Message msg)
        {
            int type = getKeyValueType(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (type == 1)
            {
                DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                string key = createAddDBElement(xdoc, out element);
                Message testMsg = new Message();
                testMsg.toUrl = msg.toUrl;
                testMsg.fromUrl = msg.fromUrl;
                if (db.insert(key, element))
                {
                    testMsg.content = key + " record is inserted Successfully.";
                }
                else
                {
                    testMsg.content = key + " record is not inserted.";
                }
                Console.WriteLine(testMsg.content);
            }
        }

        //get number of requests contained in single message
        public int getSize(XDocument xdoc)
        {
            int size = 0;
            XElement root = xdoc.Root;
            foreach (var elem in root.Elements())
            {
                if (elem.Name == "size")
                {
                    size = int.Parse(elem.Value);
                }
            }
            return size;
        }

        //generates response for writer client queries
        XElement responseXML(XDocument xdoc, string type, Message msg, Sender sndr)
        {

            XElement messageNode = new XElement("message");
            XElement client = new XElement("writerClient", msg.toUrl);
            XAttribute att = new XAttribute("commandTypePerformed", type);
            messageNode.Add(att);
            messageNode.Add(client);
            string key = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name.ToString().Equals("key"))
                {
                    key = item.Value.ToString();
                }
            }
            XElement sequenceElement = new XElement("key", key);
            messageNode.Add(sequenceElement);
            return messageNode;
        }



        //generates specific response for writer client queries
        XElement responseXMLSingle(XDocument xdoc, string operation, Message msg, Sender sndr)
        {
            XElement messageNode = new XElement("message");
            XElement client = new XElement("writerClient", msg.toUrl);
            XAttribute att = new XAttribute("commandTypePerformed", operation);
            messageNode.Add(att);
            messageNode.Add(client);
            string size = "";
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name.ToString().Equals("size"))
                {
                    size = item.Value.ToString();
                }
            }
            XElement sequenceElement = new XElement("numKeys", size);
            messageNode.Add(sequenceElement);
            return messageNode;
        }

        //process timestamp query by parsing and then calling No SQL database
        public void processTimeStampQueryMessage(XDocument xdoc, Sender sndr, Message msg)
        {
            DateTime fromDate = getFromDate(xdoc);
            DateTime toDate = getToDate(xdoc);

            int type = getKeyValueType(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (type == 1)
            {
                DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                QueryProcessing<string, List<string>> queryP5 = new QueryProcessing<string, List<string>>();
                Message testMsg = new Message();
                testMsg.toUrl = msg.toUrl;
                testMsg.fromUrl = msg.fromUrl;
                List<string> keysList = queryP5.processMatchQuery(queryP5.defineQueryTimestamp(fromDate, toDate, db), db);
                if (keysList.Count != 0)
                {
                    testMsg.content = "\nKeys within Date pattern from : " + fromDate + " to " + toDate + " are found\n";
                    Util.swapUrls(ref msg);
                    sendReponseTimeStampQuery(fromDate.ToString(), toDate.ToString(), keysList, sndr, msg);
                }
                else
                {
                    testMsg.content = "\nKeys within Date pattern not found\n";
                }
                Console.WriteLine(testMsg.content);
            }
        }

        //generates reponse for timestamp query
        public void sendReponseTimeStampQuery(string from, string to, List<string> listKeys, Sender sndr, Message msg)
        {
            XElement root = new XElement("keysWithinTimestamp");
            XElement client = new XElement("readerClient", msg.toUrl);
            XElement xFrom = new XElement("from", from);
            XElement xTo = new XElement("to", to);
            root.Add(client);
            root.Add(xFrom);
            root.Add(xTo);
            XElement matchingKeys = new XElement("matchingkeys");
            root.Add(matchingKeys);
            foreach (var key in listKeys)
            {
                XElement item = new XElement("item", key);
                matchingKeys.Add(item);
            }
            msg.content = root.ToString();
            sndr.sendMessage(msg);
            sendToWPF(msg, root, sndr);
        }

        //process valuePattern query by parsing and then calling No SQL database
        public void processValuePatternQuery(XDocument xdoc, Sender sndr, Message msg)
        {
            string pattern = getMetaPattern(xdoc);
            int type = getKeyValueType(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (type == 1)
            {
                DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                QueryProcessing<string, List<string>> queryP4 = new QueryProcessing<string, List<string>>();
                Message testMsg = new Message();
                testMsg.toUrl = msg.toUrl;
                testMsg.fromUrl = msg.fromUrl;
                List<string> listKeys = queryP4.processMatchQuery(queryP4.defineQueryValueSearch(pattern, db), db);
                if (listKeys.Count != 0)
                {
                    testMsg.content = "\nKeys within pattern= " + pattern + " found\n";
                    Util.swapUrls(ref msg);
                    sendReponsePatternQuery(pattern, listKeys, sndr, msg);
                }
                else
                {
                    testMsg.content = "\nKeys with pattern= " + pattern + " not found\n";
                }
                Console.WriteLine(testMsg.content);
            }
        }

        //process keyPattern query by parsing and then calling No SQL database
        public void processKeyPatternQuery(XDocument xdoc, Sender sndr, Message msg)
        {
            string pattern = getPattern(xdoc);

            int type = getKeyValueType(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (type == 1)
            {
                DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                QueryProcessing<string, List<string>> QueryP3 = new QueryProcessing<string, List<string>>();
                Message testMsg = new Message();
                testMsg.toUrl = msg.toUrl;
                testMsg.fromUrl = msg.fromUrl;
                List<string> listKeys = QueryP3.processMatchQuery(QueryP3.defineQueryKeySearch(pattern, db), db);
                if (listKeys.Count != 0)
                {
                    testMsg.content = "\nKeys with pattern= " + pattern + " found\n";
                    Util.swapUrls(ref msg);
                    sendReponsePatternQuery(pattern, listKeys, sndr, msg);
                }
                else
                {
                    testMsg.content = "\nKeys with pattern= " + pattern + " not found\n";
                }
                Console.WriteLine(testMsg.content);
            }
        }

        //generates response for pattern query
        public void sendReponsePatternQuery(string patt, List<string> listKeys, Sender sndr, Message msg)
        {
            XElement root = new XElement("keysReturned");
            XElement client = new XElement("readerClient", msg.toUrl);
            XElement xKey = new XElement("pattern", patt);
            XElement matchingKeys = new XElement("matchingkeys");
            foreach (var key in listKeys)
            {
                XElement item = new XElement("item", key);
                matchingKeys.Add(item);
            }
            root.Add(client);
            root.Add(xKey);
            root.Add(matchingKeys);
            msg.content = root.ToString();
            sndr.sendMessage(msg);
            sendToWPF(msg, root, sndr);
        }

        //process children search query by parsing and then calling No SQL database
        public void processChildrenSearchQuery(XDocument xdoc, Sender sndr, Message msg)
        {
            IEnumerable<string> keys = db.Keys();
            List<string> keyList = keys.ToList();
            string keyToSearch = getKey(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (!keyList.Contains(keyToSearch))
            {
                Console.WriteLine("Key {0} is not present in the DB", keyToSearch);
            }
            else
            {
                int type = getKeyValueType(xdoc);
                if (type == 1)
                {
                    DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                    QueryProcessing<string, List<string>> QueryP2 = new QueryProcessing<string, List<string>>();
                    Message testMsg = new Message();
                    testMsg.toUrl = msg.toUrl;
                    testMsg.fromUrl = msg.fromUrl;
                    List<string> listKeys = new List<string>();
                    if (QueryP2.childrensQuery(keyToSearch, out listKeys, db))
                    {
                        testMsg.content = "\n Children of " + keyToSearch + " are found\n";
                        Util.swapUrls(ref msg);
                        sendReponseChildrenQuery(keyToSearch, listKeys, sndr, msg);
                    }
                    else
                    {
                        testMsg.content = "\nchilderen of " + keyToSearch + " not found\n";
                    }
                    Console.WriteLine(testMsg.content);
                }
            }
        }

        //generates response for children query
        public void sendReponseChildrenQuery(string key, List<string> listKeys, Sender sndr, Message msg)
        {
            XElement root = new XElement("childrensReturned");
            XElement client = new XElement("readerClient", msg.toUrl);
            XElement xKey = new XElement("key", key);
            XElement children = new XElement("childrens");
            foreach (var keys in listKeys)
            {
                XElement item = new XElement("item", keys);
                children.Add(item);
            }
            root.Add(client);
            root.Add(xKey);
            root.Add(children);
            msg.content = root.ToString();
            sndr.sendMessage(msg);
            sendToWPF(msg, root, sndr);
        }

        //process vallue search query by parsing and then calling No SQL database
        public void processValueSearchQuery(XDocument xdoc, Sender sndr, Message msg)
        {
            IEnumerable<string> keys = db.Dictionary.Keys;
            List<string> keyList = keys.ToList();
            string keyToSearch = getKey(xdoc);
            serverRequestProcessed = serverRequestProcessed + 1;
            if (!keyList.Contains(keyToSearch))
            {
                Console.WriteLine("Key {0} is not present in the DB", keyToSearch);
            }
            else
            {
                int type = getKeyValueType(xdoc);
                if (type == 1)
                {
                    DBElement<string, List<string>> element = new DBElement<string, List<string>>();
                    QueryProcessing<string, List<string>> queryP1 = new QueryProcessing<string, List<string>>();
                    Message testMsg = new Message();
                    testMsg.toUrl = msg.toUrl;
                    testMsg.fromUrl = msg.fromUrl;
                    if (queryP1.valueQuery(keyToSearch, out element, db))
                    {
                        testMsg.content = "\nValue of " + keyToSearch + " returned\n";
                        Util.swapUrls(ref msg);
                        constructResponseValueSearch(keyToSearch, element, sndr, msg);
                    }
                    else
                    {
                        testMsg.content = "\nValue of " + keyToSearch + " not found\n";
                    }
                    Console.WriteLine(testMsg.content);
                }
            }
        }

        //construct response for value search query
        void constructResponseValueSearch(string key, DBElement<string, List<string>> dbElement, Sender sndr, Message msg)
        {
            XElement root = new XElement("valueFetched");
            XElement client = new XElement("readerClient", msg.toUrl);
            XElement xKey = new XElement("keyToBeSearched", key);
            XElement name = new XElement("name", dbElement.name);
            XElement descr = new XElement("description", dbElement.descr);
            XElement children = new XElement("chidlren");
            foreach (var child in dbElement.children)
            {
                XElement item = new XElement("item", child);
                children.Add(item);
            }
            XElement payload = new XElement("payload");
            foreach (var payloadItem in dbElement.payload)
            {
                XElement item = new XElement("item", payloadItem);
                payload.Add(item);
            }
            XElement timestamp = new XElement("timestamp", dbElement.timeStamp);
            root.Add(client);
            root.Add(xKey);
            root.Add(name);
            root.Add(descr);
            root.Add(timestamp);
            root.Add(children);
            root.Add(payload);
            msg.content = root.ToString();
            sndr.sendMessage(msg);
            sendToWPF(msg, root, sndr);
        }

        //send message to WPF
        void sendToWPF(Message msg, XElement root, Sender sndr)
        {
            Message msgWpf = new Message();
            msgWpf.fromUrl = msg.fromUrl;
            msgWpf.toUrl = wpfURL;
            msgWpf.content = root.ToString();
            sndr.sendMessage(msgWpf);
        }

        void sendToWPFWriter(Message msg, XElement root, Sender sndr)
        {
            Message msgWpf = new Message();
            msgWpf.fromUrl = msg.toUrl;
            msgWpf.toUrl = wpfURL;
            msgWpf.content = root.ToString();
            sndr.sendMessage(msgWpf);
        }

        //process Persist query by parsing and then calling No SQL database
        public void processPersistMessage(XDocument xdoc, Sender sndr, Message msg)
        {
            serverRequestProcessed = serverRequestProcessed + getSize(xdoc);
            PersistEngine<string, List<string>> pe = new PersistEngine<string, List<string>>();
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            if (pe.persistXML(db))
            {
                testMsg.content = "\nDatabase persisted successfully";
            }
            else
            {
                testMsg.content = "\nDatabase persistance failed";
            }
            Console.WriteLine(testMsg.content);
        }

        //process Persist query  from WPF by parsing and then calling No SQL database
        public void processPersistMessageWPF(XDocument xdoc, Sender sndr, Message msg)
        {
            PersistEngine<string, List<string>> pe = new PersistEngine<string, List<string>>();
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            if (pe.persistXML(db))
            {
                testMsg.content = "\nDatabase persisted successfully";
            }
            else
            {
                testMsg.content = "\nDatabase persistance failed";
            }
            Console.WriteLine(testMsg.content);
            Util.swapUrls(ref msg);
            XElement root = new XElement("response");
            XElement message = new XElement("item", "Persisted");
            root.Add(message);
            msg.content = root.ToString();
            sndr.sendMessage(msg);
        }

        //process augment query by parsing and then calling No SQL database
        public void processAugementMessageWPF(XDocument xdoc, Sender sndr, Message msg)
        {
            retrieveDataFromXML(db, "augment.xml", int.MaxValue);
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            if (db.Keys().Count() > 0)
            {
                testMsg.content = "\nDatabase augmented successfully";
            }
            else
            {
                testMsg.content = "\nDatabase augmentation failed";
            }
            Console.WriteLine(testMsg.content);
            Util.swapUrls(ref msg);
            XElement root = new XElement("response");
            XElement message = new XElement("item", "Augmented");
            root.Add(message);
            msg.content = root.ToString();
            sndr.sendMessage(msg);
        }

        //retrieves data from XML
        public void retrieveDataFromXML(DBEngine<string, DBElement<string, List<string>>> dbEngine, String inputFile, int limit)
        {
            XDocument document = new XDocument();
            String fileName = "";
            fileName = inputFile;
            document = XDocument.Load(fileName);
            IEnumerable<XElement> elem = document.Descendants("elements");
            if (limit > elem.Elements().Count())
            {
                limit = elem.Elements().Count();
            }
            for (int i = 0; i < limit; i++)
            {
                DBElement<string, List<string>> dbElement = new DBElement<string, List<string>>();
                string key = elem.Elements().Attributes().ElementAt(i).Value;

                for (int count = 0; count < elem.Elements().Attributes().ElementAt(i).Parent.Descendants().Count(); count++)
                {
                    XElement elementRecord = elem.Elements().Attributes().ElementAt(i).Parent.Descendants().ElementAt(count);
                    dbElement = retrieve(elementRecord, dbElement);
                    dbEngine.insert(key, dbElement);
                }
            }

        }

        DBElement<string, List<string>> retrieve(XElement elementRecord, DBElement<string, List<string>> dbElement)
        {
            if (elementRecord.Name.ToString().Equals("name"))
            {
                dbElement.name = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("desc"))
            {
                dbElement.descr = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("time"))
            {
                dbElement.timeStamp = DateTime.Parse(elementRecord.Value);
            }
            else if (elementRecord.Name.ToString().Equals("children"))
            {
                List<string> children = new List<string>();
                for (int j = 0; j < elementRecord.Descendants().Count(); j++)
                {
                    children.Add(elementRecord.Descendants().ElementAt(j).Value);
                }
                dbElement.children = children;
            }
            else if (elementRecord.Name.ToString().Equals("payload"))
            {
                List<string> payload = new List<string>();
                for (int j = 0; j < elementRecord.Descendants().Count(); j++)
                {
                    payload.Add(elementRecord.Descendants().ElementAt(j).Value);
                }
                dbElement.payload = payload;
            }
            return dbElement;
        }
        

        //process augment message query by parsing and calling retrieve function
        public void processAugmentMessage(XDocument xdoc, Sender sndr, Message msg)
        {
            string size = "";
            serverRequestProcessed = serverRequestProcessed + getSize(xdoc);
            var elem = from c in xdoc.Descendants("message") select c;
            foreach (var item in elem.Elements())
            {
                if (item.Name.ToString().Equals("size"))
                {
                    size = item.Value.ToString();
                }
            }
            retrieveDataFromXML(db,"augment.xml",int.Parse(size));
            Message testMsg = new Message();
            testMsg.toUrl = msg.toUrl;
            testMsg.fromUrl = msg.fromUrl;
            if (db.Keys().Count() > 0)
            {
                testMsg.content = "\nDatabase augmented successfully";
            }
            else
            {
                testMsg.content = "\nDatabase augmentation failed";
            }
            Console.WriteLine(testMsg.content);
        }

        //make data for No SQL database
        public void makeDataForDB(DBElement<string, List<string>> elem, List<string> childrens, List<string> payload, string name = "unnamed", string descr = "no description")
        {
            elem.name = name;
            elem.descr = descr;
            elem.timeStamp = DateTime.Now;
            elem.children = childrens;
            elem.payload = payload;
        }

        //generate random data
        void preLoadData()
        {
            string child1 = "child1";
            string child2 = "child2";
            string child3 = "child3";
            for (int key = 0; key < varCountData;  key++)
            {
                DBElement<string, List<string>> elem = new DBElement<string, List<string>>();
                makeDataForDB(elem, new List<string> { child1 + key, child2 + key, child3 + key }, new List<string> { "SMA" + 1, "681" + 1 }, "name" + key, "description" + 1);
                db.insert("Key" + key, elem);
            }
        }

        //make WPF message
        string makeWPFMessage(string clientXML)
        {
            if (!clientXML.Equals(""))
            {
                XDocument xDoc = XDocument.Parse(clientXML);
                XElement root = xDoc.Root;
                XElement identifier = new XElement("identifier");
                root.Add(identifier);
                return root.ToString();
            }
            else return "";
        }

        //make server response for WPF
        public string makeServerXMLString(int time,int noOfQueries)
        {
            XElement root = new XElement("serverIdentifier");
            XElement xTime = new XElement("time",time);
            XElement xNoOfQueries = new XElement("noOfQueries", noOfQueries);
            root.Add(xTime);
            root.Add(xNoOfQueries);
            return root.ToString();
        }

        //send server response to wpf client
        public void sendToWPFForServer(string message,Sender sndr,Message msg)
        {
            Message msgWPF = new Message();
            msgWPF.toUrl = wpfURL;
            msgWPF.fromUrl = msg.fromUrl;
            msgWPF.content = message.ToString();
            sndr.sendMessage(msgWPF);
        }

        //get number of queries processed by server
        public int getServerProcessedQueries()
        {
            return serverRequestProcessed;
        }

        static void Main(string[] args)
        {
            //Thread.Sleep(1000);
            Util.verbose = false;
            Server srvr = new Server();
            string writeClientXML = "";
            string readClientXML = "";
            srvr.preLoadData();
            srvr.ProcessCommandLine(args);
            Console.Title = "Server";
            Console.Write(String.Format("\n  Starting CommService server listening on port {0}", srvr.port));
            Console.Write("\n ====================================================\n");
            Console.WriteLine("\nDemonstrating requirement 2 by using the noSQL database you implemented in Project #2 \n");
            Console.WriteLine("\nDemonstrating requirement 3 by making the WPF Client that can be used to send Messages to server \n");
            Console.WriteLine("\nDatabase can be persisted and augmented by using WPF Client\n");
            Console.WriteLine("\nDemonstrating requirement 6 by providing the logging switch option from command line \n");
            Console.WriteLine("\nDemonstrating requirements #9 and #10 because n number of multiple clients(both reader and writer)\n");
            Console.WriteLine("can be instantiated using test executive that will demonstrate all the requirements.\n");
            Console.Write("\n ====================================================\n");
            Console.Write("\n IMPORTANT NOTE REGARDING XML");
            Console.Write("\n =============================\n");
            Console.Write("\n Reader Client read XML Template from XMLReader.XML");
            Console.Write("\n Writer Client read XML Template from XMLWriter.XML");
            Console.Write("\n Server will persist XML to persist.xml");
            Console.Write("\n Server will augment XML from augment.xml");
            Console.Write("\n =============================\n");
            Sender sndr = new Sender(Util.makeUrl(srvr.address, srvr.port));
            //Sender sndr = new Sender();
            Receiver rcvr = new Receiver(srvr.port, srvr.address);

            // - serviceAction defines what the server does with received messages
            // - This serviceAction just announces incoming messages and echos them
            //   back to the sender.  
            // - Note that demonstrates sender routing works if you run more than
            //   one client.

            Action serviceAction = () =>
            {
                Message msg = null;
                while (true)
                {
                    msg = rcvr.getMessage();   // note use of non-service method to deQ messages
                   
                    if (msg.content == "connection start message")
                    {
                        continue; // don't send back start message
                    }
                    if (msg.content == "done")
                    {
                            if (varWriteClient == 0)
                            {
                            if (!writeClientXML.Equals(""))
                                varWriteClient++;
                                Message msgToWpf = new Message();
                                msgToWpf.fromUrl = Util.makeUrl(srvr.address, srvr.port);
                                msgToWpf.toUrl = srvr.getWpfUrl();
                                msgToWpf.content = srvr.makeWPFMessage(writeClientXML);
                                sndr.sendMessage(msgToWpf);
                            }
                            if (varReadclient == 0)
                            {
                            if(!readClientXML.Equals(""))
                                varReadclient++;
                                Message msgToWpf1 = new Message();
                                msgToWpf1.fromUrl = Util.makeUrl(srvr.address, srvr.port);
                                msgToWpf1.toUrl = srvr.getWpfUrl();
                                msgToWpf1.content = srvr.makeWPFMessage(readClientXML);
                                sndr.sendMessage(msgToWpf1);
                            }
                        continue;

                    }
                    else if (msg.content.Contains("message #"))
                    {
                        Console.WriteLine("\n Received from WPF: " + msg.content);
                        continue;
                    }
                    if (msg.content == "closeServer")
                    {
                        Console.Write("received closeServer");
                        break;
                    }
                    try
                    {
                        XDocument xdoc = XDocument.Parse(msg.content);
                        string operationCalled = identifyOperation(xdoc);
                        // swap urls for outgoing message

                        if (operationCalled == "add")
                           {
                               srvr.sendToWPFWriter(msg, xdoc.Root, sndr);
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processAddMessage(xdoc, sndr, msg);
                               Util.swapUrls(ref msg);
                               XElement response = srvr.responseXML(xdoc, operationCalled,msg, sndr);
                               msg.content = response.ToString();
                               sndr.sendMessage(msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()),sndr,msg);
                           }
                           else if (operationCalled == "delete")
                           {
                               srvr.sendToWPFWriter(msg, xdoc.Root, sndr);
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processDeleteMessage(xdoc, sndr, msg);
                               Util.swapUrls(ref msg);
                               XElement response = srvr.responseXMLSingle(xdoc,operationCalled,msg, sndr);
                               msg.content = response.ToString();
                               sndr.sendMessage(msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }

                           else if (operationCalled == "edit")
                           {
                               srvr.sendToWPFWriter(msg, xdoc.Root, sndr);
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processEditMessage(xdoc, sndr, msg);
                               Util.swapUrls(ref msg);
                               XElement response = srvr.responseXML(xdoc, operationCalled,msg, sndr);
                               msg.content = response.ToString();
                               sndr.sendMessage(msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }

                           else if (operationCalled == "query1")
                           {
                               Console.WriteLine("Value Search Operation Called\n");
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processValueSearchQuery(xdoc, sndr, msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if (operationCalled == "query2")
                           {
                               Console.WriteLine("Children Search Operation Called\n");
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processChildrenSearchQuery(xdoc, sndr, msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if (operationCalled == "query3")
                           {
                               Console.WriteLine("Key Pattern Search Operation Called\n");
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processKeyPatternQuery(xdoc, sndr, msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if (operationCalled == "query4")
                           {
                               Console.WriteLine("Value Pattern Search Operation Called\n");
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processValuePatternQuery(xdoc, sndr, msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if (operationCalled == "query5")
                           {
                               Console.WriteLine("Timestamp Search Operation Called\n");
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processTimeStampQueryMessage(xdoc, sndr, msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if (operationCalled == "persist")
                           {
                               srvr.sendToWPFWriter(msg, xdoc.Root, sndr);
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processPersistMessage(xdoc, sndr, msg);
                               Util.swapUrls(ref msg);
                               XElement response = srvr.responseXMLSingle(xdoc, operationCalled,msg, sndr);
                               msg.content = response.ToString();
                               sndr.sendMessage(msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }                        
                           else if (operationCalled == "augment")
                           {
                               srvr.sendToWPFWriter(msg, xdoc.Root, sndr);
                               HiResTimer hrt = new HiResTimer();
                               hrt.Start();
                               srvr.processAugmentMessage(xdoc, sndr, msg);
                               Util.swapUrls(ref msg);
                               XElement response = srvr.responseXMLSingle(xdoc, operationCalled,msg,sndr);
                               msg.content = response.ToString();
                               sndr.sendMessage(msg);
                               hrt.Stop();
                               srvr.Ttime.Add(Convert.ToInt32(hrt.ElapsedMicroseconds));
                               srvr.sendToWPFForServer(srvr.makeServerXMLString(srvr.Ttime.Sum(), srvr.getServerProcessedQueries()), sndr, msg);
                           }
                           else if(operationCalled == "info")
                           {
                               var elem = from c in xdoc.Descendants("message") select c;
                               foreach (var item in elem.Elements())
                               {
                                   if(item.Name == "client")
                                   {
                                       if(item.Value == "WriterClient")
                                       {
                                           writeClientXML = msg.content;
                                           break;
                                       }
                                       else
                                       {
                                           readClientXML = msg.content;
                                           break;
                                       }
                                   }
                               }
                           }
                           else if(operationCalled == "persistWPF")
                           {
                               srvr.processPersistMessageWPF(xdoc, sndr, msg);
                           }
                           else if (operationCalled == "augmentWPF")
                           {
                               srvr.processAugementMessageWPF(xdoc, sndr, msg);
                           }
                    }

                    catch { }



#if (TEST_WPFCLIENT)
                    /////////////////////////////////////////////////
                    // The statements below support testing the
                    // WpfClient as it receives a stream of messages
                    // - for each message received the Server
                    //   sends back 1000 messages
                    //
                    int count = 0;
                    for (int i = 0; i < 2; ++i)
                    {
                        Message testMsg = new Message();
                        testMsg.toUrl = msg.toUrl;
                        testMsg.fromUrl = msg.fromUrl;
                        testMsg.content = String.Format("test message #{0}", ++count);
                        Console.Write("\n  sending testMsg: {0}", testMsg.content);
                        sndr.sendMessage(testMsg);
                    }
#else
                    /////////////////////////////////////////////////
                    // Use the statement below for normal operation

#endif
                    

                }
                
            };

            if (rcvr.StartService())
            {
                rcvr.doService(serviceAction); // This serviceAction is asynchronous,
            }                                // so the call doesn't block.
            Util.waitForUser();
        }
    }
}
