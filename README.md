# RemoteNoSQLDatabase
• Designed and Developed a No SQL database that can persist, query, add, delete and edit data using key value pairs.     
• Developed a facility in C# for WPF application where multiple clients uses WCF to remotely send and query data from No SQL database residing on server.    

#####Requirements can be found [HERE](https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/Requirements.pdf)

####Running The Project
Project can be instaniated by running [compile.bat](https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/compile.bat) first and then [run.bat] (https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/run.bat) in administrator mode


#####INPUT
1. Format is: <Path> n1 n2 Y
where n1 is number of read clients
and n2 is number of write clients
2. Y is logging on, So, N would be logging off
<Path> is the path of executable code

#####Output
<img src=https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/Screenshots/Screenshot1.png width="700" height="500"/>
<img src=https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/Screenshots/Screen2.png width="700" height="500"/>
<img src=https://github.com/ojuneja/RemoteNoSQLDatabase/blob/master/Screenshots/Screen3.png width="700" height="500"/>

#####IMPORTANT NOTES

1. Server will augment XML from augment.xml which is present in current directory
2. Server will persist XML to persist.xml which is present in current 
3. Read Client will read XMLTemplate from XMLReader.xml which is present in current directory
4. Write Client will read XMLTemplate from XMLWriter.xml which is present in current directory
5. Reader Client,Writer Client,Server,WPF will get port numbers automatically
6. If you want to run project from viual studio then you can see all the XMLFiles in .\TestExecutive\bin\Debug
7. Everytime you make changes to any file, you have to re-build the project using compile.bat and then run using run.bat
