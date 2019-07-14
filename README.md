# Pipelines.TcpEcho.Protobuf
This is an extension of System.IO.Pipelines sample TcpEcho (https://github.com/davidfowl/TcpEcho) with Protobuf ReadOnlySequenceReader.


## Test Environment
* .Net Framework 4.7.2



| Message Size|Socket|Pipe|
|-------------|------|----|
| 32 |3.71|4.80|
|128 |3.81|4.70|
|512 |4.11|5.03|
|1024|4.09|5.12|
|2048|4.46|5.68|
|4096|4.78|5.75|
|8192|5.36|5.98|
|10000|5.64|6.60|
