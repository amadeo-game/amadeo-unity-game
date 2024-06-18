# amadeo-unity-game
Therapeutic Game.



# Architecture Overview

1. UDP Server
Purpose: Listens for incoming data from the Amadeo device or sends emulated data.
Components:
UDPServer Class: Manages UDP communication and processes incoming data.
Server Thread: Runs the server on a separate thread to handle incoming data asynchronously.
Data Handling: Parses incoming data and handles zeroing of force values.
2. Unity Client
Purpose: Receives, processes, and applies force data to the Unity application.
Components:
UDPClient Class: Listens for and processes UDP messages from the server.
BridgeAPI: Applies the received force data to the game objects or simulations in Unity.
GameManager: Manages the overall game state and integrates server interactions.
Detailed Data Flow and Architecture
Data Emission (UDP Server):

Data Source: The server receives data from the Amadeo device or reads from a file in emulation mode.
StartServer Method: Initializes the UDP listener on a specified port (e.g., port 4444).
HandleIncomingData Method: Manages data reception and processing:
Real Device Mode: Receives actual data from the device.
Emulation Mode: Reads and processes data from a file.
Zeroing Handling: If zeroing is required, calculates the zeroing forces from the initial set of data.
Data Sending: Sends parsed and possibly zeroed data to the Unity client.
Data Reception (Unity Client):

UDPClient Class: Listens for UDP messages on a specified port (e.g., port 8888).
ReceiveData Method: Asynchronously receives and processes incoming UDP messages.
HandleReceivedData Method:
Parses the received data into individual force values.
Applies zeroing offsets if necessary.
Logs and sends the processed data to the BridgeAPI.
Force Application (Unity):

BridgeAPI Class: Applies the processed force data to the Unity game objects.
ApplyForces Method: Updates the game state or animations based on the received force data.
GameManager Class: Oversees the interaction between the server and the Unity client, starting and stopping the server as needed, and managing game state transitions.



## Sequence Diagram

![image](https://github.com/amadeo-game/amadeo-unity-game/assets/74779722/05bb5e36-8e61-479b-a303-2f907d8b126e)


### Components Interaction
1. UDPServer:
* StartServer: Initializes UDP communication.
* HandleIncomingData: Continuously processes and sends data to the client.

2. UDPClient:
* ReceiveData: Listens for incoming UDP messages.
* HandleReceivedData: Parses and adjusts force data before passing it to the game.
 
3. Unity Game Components:
* BridgeAPI: Applies the parsed forces to affect game objects.
* GameManager: Manages game state, starting and stopping the server as needed.

## "Asynchronous Client-Server with Real-Time Event Processing"

* Asynchronous: Separate threads for handling data without blocking.
* Client-Server: Clear division between data provider (server) and data consumer (client).
* Real-Time Event Processing: Immediate response to data changes, crucial for real-time applications.
* This architecture is designed to handle dynamic, real-time data efficiently and effectively, supporting both live and emulated data sources.




