start .\CableCloud\bin\Debug\CableCloud.exe "./Resources/cloud.cloudconfig"
start .\Host\bin\Debug\Host.exe "./Resources/host1.hostconfig"
start .\Host\bin\Debug\Host.exe "./Resources/host2.hostconfig"
start .\Host\bin\Debug\Host.exe "./Resources/host3.hostconfig"
start .\Host\bin\Debug\Host.exe "./Resources/host4.hostconfig"
start .\Host\bin\Debug\Host.exe "./Resources/host5.hostconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node1.nodeconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node2.nodeconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node3.nodeconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node4.nodeconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node5.nodeconfig"
start .\NetworkNode\bin\Debug\NetworkNode.exe "./Resources/node6.nodeconfig"
start .\ManagementSystem\bin\Debug\ManagementSystem.exe "./Resources/ms.msconfig"
timeout 1
start ./Resources/cmdow.exe R1 /mov 0 0
start ./Resources/cmdow.exe R2 /mov 0 351
start ./Resources/cmdow.exe R3 /mov 0 702
start ./Resources/cmdow.exe R4 /mov 501 0
start ./Resources/cmdow.exe R5 /mov 501 351
start ./Resources/cmdow.exe R6 /mov 501 702
start ./Resources/cmdow.exe H1 /mov 1002 0
start ./Resources/cmdow.exe H2 /mov 1002 351
start ./Resources/cmdow.exe H3 /mov 1002 702
start ./Resources/cmdow.exe H4 /mov 1503 0
start ./Resources/cmdow.exe H5 /mov 1503 351
start ./Resources/cmdow.exe CableCloud /mov 1503 702
start ./Resources/cmdow.exe ManagementSystem /mov 200 200 /act
