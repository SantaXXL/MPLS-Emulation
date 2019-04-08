# About

This project is meant to show how the MPLS networks work the - it emulates  data plane. The structure of the project is as follows:
- we have "client nodes", also called "hosts" - their role is to send (and receive) IP packages. Each host is connected to LER (Label Edge Router)
- "network nodes" - every node in the network that is not a host. It represents a Network Element in an MPLS network (that is, either LER or LSR). LER receives an IP package from a host and then forwards it to another node (might be either host or LSR)
- "cable cloud" - to better emulate a real network, we shall not send packages directly between nodes, but also simulate a cable. Cable might get broken, hence connection between nodes might get broken as well.
- "management system" - this emulates control plane and management plane (we can set, add/remove/change rules - entires in IP/MPLS FIBs)

The point of this project is to show how the tunneling in MPLS networks work and what shall be done to reroute a broken connection. The most important parts are implemented just like it is written in RFC 3031.

This project was written by three Warsaw University of Technology, Faculty of Electronics and Information Technology, students. Course name: TSST. 

# How to run it

Download the files, unzip them, open TSST.sln file in Visual Studio (we were using 2017 version), Build -> Build all.
And then run run_simulation.bat. 

Note: the program used to manipulate program windows, resources/cmdow.exe, might be classified as a virus by some security programs.

# Gallery

![Imgur](https://i.imgur.com/fBjXOC8.png?1)
![Imgur](https://i.imgur.com/6iZvah4.png?1)
![Imgur](https://i.imgur.com/5aLEMMR.png)
