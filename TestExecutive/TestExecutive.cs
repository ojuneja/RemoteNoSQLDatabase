using System;
/////////////////////////////////////////////////////////////////////////
// TestExecutive.cs - Test Executive                                   //
// ver 1.0                                                             // 
// Author: Ojas Juneja, Syracuse University
/////////////////////////////////////////////////////////////////////////
// Purpose of this package is to execute n instances of reader and writer client which can send requests to server and get response in return
//----------------------Public Interfaces ------------------------------
/* processCommandLine - process Command Line
   makePath - make path for execution

    ver 1.0 - frist release
*/


namespace Project4Starter
    {
        using System.Diagnostics;
        class TestExecutive
        {
            int numR { get; set; } = 1;
            int numW { get; set; } = 1;
            string log { get; set; } = "N";

            static int serverPort = 8080;
            static int writerClientPort = 8085;
            static int readerClientPort = 8082;
            public void processCommandLine(string[] args)
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("  Invalid command line arguments.");
                }
                else if (args.Length == 3)
                {
                    try
                    {
                        numR = Int32.Parse(args[0]);
                        numW = Int32.Parse(args[1]);
                        if(args[2] == "Y")
                        {
                        log = "/O";
                        Console.Write("\n ====================================================\n");
                        Console.Write("\n Console Log Switch is On");
                        Console.Write("\n =============================\n");
                        }
                        else
                       {
                        log = "";
                        Console.Write("\n ====================================================\n");
                        Console.Write("\n Console Log Switch is Off");
                        Console.Write("\n =============================\n");
                       }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("processCommandLine Exception :" + e.StackTrace);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command line arguments.");
                }

            }

            public string makePath(string packagename)
            {
                string temp = AppDomain.CurrentDomain.BaseDirectory;
                int j = 0;
                while (j != 4)
                {
                    int i = temp.LastIndexOf("\\");
                    temp = temp.Substring(0, i);
                    j++;
                }
            if (packagename.Equals("WpfClient"))
                packagename = temp + "\\" + packagename + "\\bin\\debug\\WpfApplication1.exe";
            else
                packagename = temp + "\\" + packagename + "\\bin\\debug\\" + packagename + ".exe";
                return packagename;
            }

       public void demonstration()
        {
            Console.Write("\n ====================================================\n");
            Console.WriteLine("\nDemonstrating requirement 2 by using the noSQL database you implemented in Project #2 \n");
            Console.WriteLine("\nDemonstrating requirement 3 by making the WPF Client that can be used to send Messages to server \n");
            Console.WriteLine("\nDatabase can be persisted and augmented by using WPF Client\n");
            Console.WriteLine("\nDemonstrating requirement 6 by providing the logging switch option from command line \n");
            Console.WriteLine("\nDemonstrating requirements #9 and #10 because n number of multiple clients and n number of multiple servers\n");
            Console.WriteLine("\ncan be instantiated using test executive that will demonstrate all the requirements.\n");
            Console.Write("\n ====================================================\n");
            Console.Write("\n IMPORTANT NOTE REGARDING XML");
            Console.Write("\n =============================\n");
            Console.Write("\n Reader Client read XML Template from XMLReader.XML");
            Console.Write("\n Writer Client read XML Template from XMLWriter.XML");
            Console.Write("\n Server will persist XML to persist.xml");
            Console.Write("\n Server will augment XML from augment.xml");
            Console.Write("\n =============================\n");
        }

            static void Main(string[] args)
            {
                Console.Write("\n  starting Test-Executive  ");
                Console.Write("\n =============================\n");
               TestExecutive testExec = new TestExecutive();
            // Test if input arguments were supplied:
                testExec.demonstration();
                Process.Start(testExec.makePath("Server"));
                Process.Start(testExec.makePath("WpfClient"));
                if (args.Length == 3)
                {
                    string arg = "";
                    testExec.processCommandLine(args);
                    int i = 0;
                    while (i < testExec.numR)
                    {
                        arg = "/R http://localhost:" + TestExecutive.serverPort + "/CommService /L http://localhost:" + (TestExecutive.readerClientPort + i) + "/CommService";
                        //arg = "/R http://localhost:" + "8080" + "/CommService /O";

                        Process.Start(testExec.makePath("Client2"), arg);
                        i++;
                    }
                    i = 0;
                    while (i < testExec.numW)
                    {
                        arg = "/R http://localhost:" + TestExecutive.serverPort + "/CommService /L http://localhost:" + (TestExecutive.writerClientPort + i) + "/CommService " + testExec.log;
                        Process.Start(testExec.makePath("Client"), arg);
                        i++;
                    }
                    return;
                }
                else
                {
                    return;
                }
            }
        }
    }

