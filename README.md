# Pipelines.TcpEcho.Protobuf
This is an extension of System.IO.Pipelines sample TcpEcho (https://github.com/davidfowl/TcpEcho) with Protobuf ReadOnlySequenceReader.


## Test Environment
* .Net Framework 4.7.2
* protobuf-net v3.0.0-alpha.32
* Commandline:
  Client.exe <MessageSize>
  PipelinesServer.exe <MessageSize>
  SocketServer.exe <MessageSize>
* Please use message sizes - 32, 128, 512, 1024, 2048, 4096, 8192, 10000
* Used Release AnyCPU for taking readings by sending same protobuf buffer 1_000_00 times.


| Message Size|Stream|Pipeline|
|-------------|------|----|
| 32 |3.71|4.80|
|128 |3.81|4.70|
|512 |4.11|5.03|
|1024|4.09|5.12|
|2048|4.46|5.68|
|4096|4.78|5.75|
|8192|5.36|5.98|
|10000|5.64|6.60|

* Time taken for sending same protobuf buffer 1_000_00 times which includes deserialization at server side.
These observations are little different from the sample TcpEcho (https://github.com/davidfowl/TcpEcho).
