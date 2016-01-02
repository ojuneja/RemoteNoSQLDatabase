===================== IMPORTANT NOTE REGRDING XML ==========================

1. Server will augment XML from augment.xml which is present in current directory
2. Server will persist XML to persist.xml which is present in current 
3. Read Client will read XMLTemplate from XMLReader.xml which is present in current directory
4. Write Client will read XMLTemplate from XMLWriter.xml which is present in current directory


===================== COMMAND LINE ARGUMENTS ================================
Format is: <Path> n1 n2 Y
where n1 is number of read clients
n2 is number of write clients
Y is logging on, So, N would be logging off
<Path> is the path of executable code


==================== Reader Client,Writer Client,Server,WPF will get port numbers automatically====================


If you want to run project from viual studio then you can see all the XMLFiles in .\TestExecutive\bin\Debug

Everytime you make changes to any file, yo have to re-build the project using compile.bat and then run using run.bat