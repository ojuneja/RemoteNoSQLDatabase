///////////////////////////////////////////////////////////////
// QueryProcessing.cs - Queries diff kinds of data from dictionary
//                                                           //
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
 * This package queries diff kinds of data from dictionary
 * PUBLIC INTERFACES
 * public Func<Key, bool> defineQueryTimestamp(DateTime beg, DateTime end) - defining delegate for querying using timestamp
 *  public Func<Key, bool> defineQueryKeySearch(int searchKey) - defining delegate for querying key patterns
 *  public Func<Key, bool> defineQueryValueSearch(string search) - defining delegate for querying value patterns which serves as wrapper for value search engine
 *  bool valueSearchEngine(Key key, string search) - defining delegate for querying value patterns
 *  public bool childrensQuery(Key key, out List<Key> valueCollection) - querying value using specifying key
 *  public bool valueQuery(Key key, out DBElement<Key, Data> valueCollection) - querying childrens using specifying key
 * public List<Key> processMatchQuery(Func<Key, bool> queryPredicate) - this function is generic for querying values or keys or timestamps
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
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
    public class QueryProcessing<Key, Data>
    {
       

        //defining delegate for querying using timestamp
        public Func<Key, bool> defineQueryTimestamp(DateTime beg, DateTime end, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            Func<Key, bool> queryPredicate = null;
            if (end == null)
            {
                end = DateTime.Now;
            }
            //lambda that checks whether the beginning and ending date is in range
            queryPredicate = (Key key) =>
            {
                if (dbEngine.Dictionary != null)
                {
                    if (DateTime.Compare(dbEngine.Dictionary[key].timeStamp, beg) >= 0
                    && DateTime.Compare(dbEngine.Dictionary[key].timeStamp, end) <= 0)
                    {
                        return true;
                    }
                }
                return false;
            };
            return queryPredicate;
        }

        //defining delegate for querying key patterns
        public Func<Key, bool> defineQueryKeySearch(string search, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            Func<Key, bool> queryPredicate = null;
            //lambda that checks whether the key pattern matches
            queryPredicate = (Key key) =>
            {
                if (dbEngine.Dictionary != null)
                {
                    if (key.ToString().Contains(search))
                    {
                        return true;
                    }
                }
                return false;
            };
            return queryPredicate;
        }

        //defining delegate for querying value patterns which serves as wrapper for value search engine
        public Func<Key, bool> defineQueryValueSearch(string search, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            Func<Key, bool> queryPredicate = null;
            queryPredicate = (Key key) =>
            {
                if (dbEngine.Dictionary != null)
                {
                    return valueSearchEngine(key, search, dbEngine);
                }
                return false;
            };
            return queryPredicate;
        }

        //defining delegate for querying value patterns
        bool valueSearchEngine(Key key, string search, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            if (dbEngine.Dictionary[key].name.Contains(search))
            {
                return true;
            }
            if (dbEngine.Dictionary[key].descr.Contains(search))
            {
                return true;
            }
            if (dbEngine.Dictionary[key].timeStamp.ToString().Contains(search))
            {
                return true;
            }
            //checking whether children or payload is present before querying
            if (dbEngine.Dictionary[key].children != null)
            {
                List<Key> childrens = dbEngine.Dictionary[key].children;
                foreach (var item in childrens)
                {
                    if (item.ToString().Contains(search))
                    {
                        return true;
                    }
                }
            }
            if (dbEngine.Dictionary[key].payload != null)
            {
                List<string> payload = dbEngine.Dictionary[key].payload as List<string>;
                foreach (var item in payload)
                {
                    if (item.ToString().Contains(search))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //querying value using specifying key
        public bool valueQuery(Key key, out DBElement<Key, Data> valueCollection, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            valueCollection = new DBElement<Key, Data>();
            if (dbEngine.Dictionary.Keys.Count() > 0)
            {
                if (dbEngine.Dictionary.Keys.Contains(key))
                {
                    valueCollection = dbEngine.Dictionary[key];
                    return true;
                }
            }
            return false;
        }

        //querying childrens using specifying key
        public bool childrensQuery(Key key, out List<Key> valueCollection, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
            valueCollection = new List<Key>();
            if (dbEngine.Dictionary.Keys.Count() > 0)
            {
                if (dbEngine.Dictionary.Keys.Contains(key))
                {
                    valueCollection = dbEngine.Dictionary[key].children;
                    return true;
                }
            }
            return false;
        }

        //this function is generic for querying values or keys or timestamps
        //you just need to define a rule or predicate
        public List<Key> processMatchQuery(Func<Key, bool> queryPredicate, DBEngine<Key, DBElement<Key, Data>> dbEngine)
        {
         List<Key> keys = new List<Key>();
        IEnumerable<Key> keysCollection = dbEngine.Keys();
            foreach (var item in keysCollection)
            {
                if (queryPredicate(item))
                {
                    keys.Add(item);
                }
            }
            return keys;
        }


    }

#if (TEST_QUERYPROCESSING)
    class TestQueryProcessing
    {
        static void Main(string[] args)
        {
            //test stub for Query Processing
           // "Testing QueryProcessing Package".title('=');
            WriteLine();
            QueryProcessing<int, string> query = new QueryProcessing<int, string>();
            DBElement<int, string> elem = new DBElement<int, string>();
            DBElement<int, string> elemResults;
            List<int> childrens;
            DBEngine<int, DBElement<int, string>> dbEngine = new DBEngine<int, DBElement<int, string>>();
          //  "Metadata with data dictionary is ".subTitle('-');
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
            /*query.DBEngine = dbEngine;
            Write("\n\n");
            //testing different kinds of function by querying different kinds of data
            "Testing func: valueQuery() by getting following metdata from key = 1".subTitle();
            query.valueQuery(1, out elemResults);
            elemResults.showElement();
            Write("\n\n");
            "Testing func: childrenQuery() by getting following childrens from key = 1".subTitle();
            query.childrensQuery(1, out childrens);
            childrens.showList();
            Write("\n\n");
            "Testing func: processKeyMatchQuery() and defineQueryKeySearch() by getting keys with pattern 1".subTitle();
            List<int> keys1 = query.processMatchQuery(query.defineQueryKeySearch("1"));
            keys1.showList();
            Write("\n\n");
            "Testing func: processKeyMatchQuery() and defineQueryValueSearch() by getting keys with values having 'X' in their pattern".subTitle();
            List<int> keys2 = query.processMatchQuery(query.defineQueryValueSearch("X"));
            keys2.showList();
            Write("\n\n");
            "Testing func: processKeyMatchQuery() and defineQueryTimestampSearch() by getting keys with timestamp between 09/20/2015 11:36:58 PM to 09/22/2015 11:36:58 PM".subTitle();
            List<int> keys3 = query.processMatchQuery(query.defineQueryTimestamp(DateTime.Parse("09/20/2015 11:36:58 PM"), DateTime.Parse("09/22/2015 11:36:58 PM")));
            keys3.showList();
            Write("\n\n");*/
        }
    }
#endif
}

