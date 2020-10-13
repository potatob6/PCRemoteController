# 服务端
+ 可用命令
+ upload 上传文件
+ scp下载文件
+ clients 查看所有客户端
+ scrshot 截图
+ listen监听
+ runcmd 执行命令
+ cam 调用摄像头
+ key 模拟按键  
全部命令用?分隔  
例:```1?runcmd?cd ../..&dir```  
例2:```all?scp?C:\123.txt?C:\a.txt```  

_注意:服务端需要开启4419 4418 4417 4416端口供数据传输_

## 命令功能
### upload上传文件
>用法:```1?upload?C:\123.txt?C:\a.txt``` C:\123.txt为本地文件位置,C:\a.txt为客户端文件存放位置  
### scp下载文件
>用法:```1?upload?C:\123.txt?C:\a.txt``` C:\123.txt为客户端文件位置,C:\a.txt为本地文件存放位置
### clients查看所有客户端
>用法:```clients```查看所有连接的客户端
### scrshot截图
>用法:```1?scrshot```截图并且传输到android客户端
### listen监听
>用法:```1?listen?csgo.exe```监听xxx.exe程序是否*开启*或*关闭*
### runcmd运行命令
>用法:```1?runcmd?cd ../..&dir```运行客户端上的cmd命令
### cam调用摄像头
>用法:```1?cam?5?0```调用客户端上的摄像头，其中5为最大等待时长， 0为摄像头序号
### key模拟按键
>用法:```1?key?%{TAB}```模拟客户端上的按键，详情查看```Sendkeys```
