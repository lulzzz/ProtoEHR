ProtoEHR
=====
### How to run the system
The Orleans project of two parts the silo and the client, and both have to run to get the system running. We are using the Dotnet core for Linux, so these instructions have only been tested for the versions and software mentioned in the tools sections, but they should work for OS's if the Dotnet core is installed. All the instructions and commands assume a position in the root project folder.

First, we have to download the required packages and build the project.

>dotnet restore

>dotnet build

After that, it is possible to run the silo. It is done with the following command.

>dotnet run --project ./Silo

This command also starts the Orleans Dashboard, which can be found when opening the browser and navigating to http://localhost:8080. Open a new terminal window, and we can begin to start the client with the following command.

>dotnet run --project ./Client

This starts the client, and the instructions are shown on what functionality can be run. 
