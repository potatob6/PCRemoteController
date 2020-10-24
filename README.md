# 客户端
用于接受服务端发送的信息
用于上传文件或图片或信息
### 服务器地址修改
可以通过修改*Program.cs*中119行IPAddress中修改服务器的ip
或者替换
`IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 4419);//服务器终端地址`
为
```
IPHostEntry host = Dns.GetHostByName("wkesports.xyz");
IPAddress ip = host.AddressList[0];
IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse(ip.ToString()), 4419);//服务器终端地址
```
其中*wkesports.xyz*为域名地址
