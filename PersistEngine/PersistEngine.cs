///////////////////////////////////////////////////////////////
// PersistEngine.cs - Persist and retrieves dictionary data  //
//                    to XML                                 //
//                                                           //
// Ver 1.0                                                   //
// Application: Project#2                                    //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    HP Pavilion g series                         //
//Author    :   Ojas Juneja, Syracuse University             //
///////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package persists and retrieves dictionary data to XML
 * PUBLIC INTERFACES
 *  public void persistXML(DBEngine<Key, DBElement<Key, Data>> dbEngine, string inputFile) -  persists data into XML by taking DBengine and file as argument
 *  public void loadXmlFile() - creates XML file if not present otherwise loads its contents to object
 *  public bool retrieveDataFromXMLTypeOne(DBEngine<string, DBElement<string, ListOfStrings>> dbEngine, string inputFile)  - retrieves data from XMLTypeOne to dictionary
 *  void retrieveTypeOne(XElement elementRecord, DBElement<string, ListOfStrings> dbElement) - a subfunction of retrieveDataFromXMLTypeOne
 *  public bool retrieveDataFromXMLTypeTwo(DBEngine<int, DBElement<int, string>> dbEngine, string inputFile) - retrieves data from XMLTypeTwo to dictionary
 *  void retrieveTypeTwo(XElement elementRecord, DBElement<int, string> dbElement) - a subfunction of retrieveDataFromXMLTypeTwo
 *  public void addKeyAndPayloadType(string keyType, string payloadType) - add key type and payload type into XML
 *  bool keyPresent(Key key, DBElement<Key, Data> elem) - checking if the key is already present in XML to avoid duplication of data into XML
 *  bool addRecord(Key key, DBElement<Key, Data> data) - wrapper of addByrecord Function
 *  void addByRecord(Key key, DBElement<Key, Data> data) - adding the metdata and key into XML
 *  void handlingPayload(XElement xElementKey, XElement xPayload, bool payLoadFlag, string payloadString, ListOfStrings payload) - functions handles and adds both Type1(<string,ListOfStrings>) and Type2(<int,string>) payload
 */
/*
 * Maintenance:
 * ------------
 * Required Files: 
 *   Display.cs, UtilityExtensions.cs,DBElement.cs,DBFactory.cs,ItemEditor.cs,PayloadWrapper.cs,DBEngine.cs
 *
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 07 oct 15
 * - first release

 */

using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using System.Xml.Linq;
using System.IO;



namespace Project4Starter
{
    public class PersistEngine<Key, Data>
    {

        XDocument xDocument = new XDocument();
        string inputFile = "persist.xml";
        string fileName = "";

        //persists data into XML by taking DBengine and file as argument
        public bool persistXML(DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            fileName = @".\" + inputFile;
            string valueType = "ListOfStrings";
            string keyType = "string";
            loadXmlFile();
            addKeyAndPayloadType(keyType, valueType);
            IEnumerable<Key> enums = dbEngine.Keys();
            foreach (Key key in enums)
            {
                if (!addRecord(key, dbEngine.Dictionary[key]))
                    return false;
            }
            return true;
        }



        //creates XML file if not present otherwise loads its contents to object
        public void loadXmlFile()
        {
            if (!File.Exists(fileName))
            {
                xDocument.Declaration = new XDeclaration("1.0", "utf-8", "yes");
                XElement root = new XElement("noSqlDb");
                xDocument.Add(root);
                xDocument.Save(fileName);
            }
            else
            {
                xDocument = XDocument.Load(fileName);
            }
        }



        //retrieves data from XMLTypeOne to dictionary
        public bool retrieveDataFromXMLTypeOne(DBEngine<string, DBElement<string, ListOfStrings>> dbEngine, string inputFile)
        {
            fileName = inputFile;
            xDocument = XDocument.Load(fileName);
            var elem = from c in xDocument.Descendants("elements") select c;
            for (int i = 0; i < elem.Elements().Count(); i++)
            {
                DBElement<string, ListOfStrings> dbElement = new DBElement<string, ListOfStrings>();
                string key = elem.Elements().Attributes().ElementAt(i).Value;

                for (int count = 0; count < elem.Elements().Attributes().ElementAt(i).Parent.Descendants().Count(); count++)
                {
                    XElement elementRecord = elem.Elements().Attributes().ElementAt(i).Parent.Descendants().ElementAt(count);
                    retrieveTypeOne(elementRecord,dbElement);
                }
                //does not adds key to dictonary if already present to avoid exception
                if (!dbEngine.Dictionary.Keys.Contains(key))
                {
                    dbEngine.Dictionary.Add(key, dbElement);
                }
                else return false;
            }

            return true;
        }

        // a subfunction of retrieveDataFromXMLTypeOne
        void retrieveTypeOne(XElement elementRecord, DBElement<string, ListOfStrings> dbElement)
        {
            //retireving all the metadata values one by one into DBElement
            if (elementRecord.Name.ToString().Equals("name"))
            {
                dbElement.name = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("descr"))
            {
                dbElement.descr = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("timestamp"))
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
            //have to put payload data of type ListOFStrings into wrapper 
            else if (elementRecord.Name.ToString().Equals("payload"))
            {
                ListOfStrings payload = new ListOfStrings();
                for (int j = 0; j < elementRecord.Descendants().Count(); j++)
                {
                    payload.theWrappedData.Add(elementRecord.Descendants().ElementAt(j).Value);
                }
                dbElement.payload = payload;
            }
        }

        //retrieves data from XMLTypeTwo to dictionary
        public bool retrieveDataFromXMLTypeTwo(DBEngine<int, DBElement<int, string>> dbEngine, string inputFile,int limit)
        {
            fileName = inputFile;
            loadXmlFile();
            var elem = from c in xDocument.Descendants("elements") select c;
            if (limit > elem.Elements().Count())
            {
                limit = elem.Elements().Count();
            }
            for (int i = 0; i < limit; i++)
            {
                DBElement<int, string> dbElement = new DBElement<int, string>();
                int key = int.Parse(elem.Elements().Attributes().ElementAt(i).Value);
                for (int count = 0; count < elem.Elements().Attributes().ElementAt(i).Parent.Descendants().Count(); count++)
                {
                    XElement elementRecord = elem.Elements().Attributes().ElementAt(i).Parent.Descendants().ElementAt(count);
                    retrieveTypeTwo(elementRecord, dbElement);
                }
                //does not adds key to dictonary if already present to avoid exception
                if (!dbEngine.Dictionary.Keys.Contains(key))
                {
                    dbEngine.Dictionary.Add(key, dbElement);
                }
                else return false;
            }

            return true;
        }

        // a subfunction of retrieveDataFromXMLTypeTwo
        void retrieveTypeTwo(XElement elementRecord, DBElement<int, string> dbElement)
        {
            //retireving all the metadata values one by one into DBElement
            if (elementRecord.Name.ToString().Equals("name"))
            {
                dbElement.name = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("descr"))
            {
                dbElement.descr = elementRecord.Value;
            }
            else if (elementRecord.Name.ToString().Equals("timestamp"))
            {
                dbElement.timeStamp = DateTime.Parse(elementRecord.Value);
            }
            else if (elementRecord.Name.ToString().Equals("children"))
            {
                List<int> children = new List<int>();
                for (int j = 0; j < elementRecord.Descendants().Count(); j++)
                {
                    children.Add(int.Parse(elementRecord.Descendants().ElementAt(j).Value));
                }
                dbElement.children = children;
            }
            //no need to put string data into wrapper
            else if (elementRecord.Name.ToString().Equals("payload"))
            {
                dbElement.payload = elementRecord.Descendants().ElementAt(0).Value;
            }
        }

        //add key type and payload type into XML
        public void addKeyAndPayloadType(string keyType, string payloadType)
        {
            XElement xElementKeyType = new XElement("keytype", keyType);
            XElement xElementPayloadType = new XElement("payloadtype", payloadType);
            XElement root = xDocument.Root;
            if (xDocument == null)
            {
                root = new XElement("noSqlDb");
                xDocument.Add(root);
            }
            //if keytype or payloadtype already present then dont add
            if (!(xDocument.Element("noSqlDb").Elements("keytype").Any() || xDocument.Element("noSqlDb").Elements("payloadtype").Any()))
            {
                root.Add(xElementKeyType);
                root.Add(xElementPayloadType);
            }

        }

        //checking if the key is already present in XML to avoid duplication of data into XML
        bool keyPresent(Key key, DBElement<Key, Data> elem)
        {
            bool elementPresent = xDocument.Element("noSqlDb").Elements("elements").Any();
            if (elementPresent)
            {
                if (xDocument.Element("noSqlDb").Element("elements").Descendants().Any())
                {
                    //LINQ query to determine duplicate keys
                    bool keyPresent = xDocument.Descendants().Any(e => e.Name == "key" && e.Attributes().Any(a => a.Name == "id" && a.Value == key.ToString()));
                    if (!keyPresent)
                    {
                        return false;
                    }

                }
            }
            else return false;
            return true;
        }

       //wrapper of addByrecord Function
        bool addRecord(Key key, DBElement<Key, Data> data)
        {
            if (!keyPresent(key, data))
            {
                addByRecord(key, data);
            }
            return true;
        }

        //adding the metdata and key into XML
        void addByRecord(Key key, DBElement<Key, Data> data)
        {
            bool childrenFlag = false, payLoadFlag = false;
            bool present = xDocument.Descendants("elements").Any();
            XElement root = xDocument.Root;
            if (!present)
            {
                XElement element = new XElement("elements");
                root.Add(element);
            }
            XElement xElementKey = new XElement("key", new XAttribute("id", key.ToString()));
            XElement xElementName = new XElement("name", data.name);
            XElement xElementDescr = new XElement("descr", data.descr);
            XElement xElementTimestamp = new XElement("timestamp", data.timeStamp.ToString());
            //childrens and paload will only be added if it is mentioned in dictionary of particular keyt
            List<Key> listChildren = data.children;
            XElement xChildrens = new XElement("children");
            if (listChildren.Count != 0)
            {
                foreach (var item in listChildren)
                {
                    XElement xChildren = new XElement("item", item);
                    xChildrens.Add(xChildren);
                }
                childrenFlag = true;
            }
            string payloadString = null;
            ListOfStrings payload = data.payload as ListOfStrings;
            if (payload == null)
                payloadString = data.payload as string;
            XElement xPayload = new XElement("payload");
            xElementKey.Add(xElementName);
            xElementKey.Add(xElementDescr);
            xElementKey.Add(xElementTimestamp);
            if (childrenFlag)
                xElementKey.Add(xChildrens);
            handlingPayload(xElementKey, xPayload, payLoadFlag, payloadString, payload);
            xDocument.Element("noSqlDb").Element("elements").Add(xElementKey);
            //saving object into XML after adding records
            xDocument.Save(fileName);
        }

        //functions handles and adds both Type1(<string,ListOfStrings>) and Type2(<int,string>) payload
        void handlingPayload(XElement xElementKey, XElement xPayload, bool payLoadFlag, string payloadString, ListOfStrings payload)
        {
            if (payload != null)
            {
                foreach (var item in payload.theWrappedData)
                {
                    XElement xPayloadItems = new XElement("item", item);
                    xPayload.Add(xPayloadItems);
                }
                payLoadFlag = true;
            }
            if (payloadString != null)
            {
                XElement xPayloadItems = new XElement("item", payloadString);
                xPayload.Add(xPayloadItems);
                payLoadFlag = true;
            }
            if (payLoadFlag)
                xElementKey.Add(xPayload);
        }


    }



#if (TEST_PERSISTENGINE)
    class TestPersistEngine
    {
        static void Main(string[] args)
        {
            //test stun for persist engine
            "Testing PersistEngine Package".title();
            WriteLine();
            PersistEngine<int, string> persistEngine = new PersistEngine<int, string>();
            "Testing Persist Engine Package".title('=');
            DBElement<int, string> elem = new DBElement<int, string>();
            DBEngine<int, DBElement<int, string>> dbEngine = new DBEngine<int, DBElement<int, string>>();
            //creating metdata
            "Metadata with data dictionary is ".title();
            elem.name = "X";
            elem.descr = "description";
            elem.timeStamp = DateTime.Parse("09/20/2015 11:36:58 PM");
            elem.children = new List<int> { 2, 3, 4 };
            elem.payload = "payload";
            dbEngine.insert(1, elem);
            elem = new DBElement<int, string>();
            elem.name = "XX";
            elem.descr = "description";
            elem.timeStamp = DateTime.Now;
            elem.children = new List<int> { 5, 6, 7 };
            elem.payload = "payload";
            dbEngine.insert(12, elem);
            dbEngine.showDB();
            //persisting metadata into XML
            "Write in-memory database in XML file, please check DataPersistTest.xml file in Path:./PersistEngine/bin/debug.".title('-');
            persistEngine.persistXML(dbEngine);
            "Deleting in-memory database".title('-');
            dbEngine.Dictionary.Clear();
            "Database restored or augmented from an existing XML (DataPersistTest.xml) file".title('-');
            persistEngine.retrieveDataFromXMLTypeTwo(dbEngine, "DataPersistTest.xml",10);
            dbEngine.showDB();
            WriteLine();
            Write("\n\n");
        }
    }
#endif
}
