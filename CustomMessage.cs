using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CusMsg
{
    public enum MSGTYPE
    {
        //当类型为_FILE_时:strValue的值为命令本身内容,imgValue的值为文件字节数组
        _STRING_,
        _IMAGE_,
        _FILE_,
    }
}
